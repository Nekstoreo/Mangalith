import { Injectable, BadRequestException, InternalServerErrorException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { ConfigService } from '@nestjs/config';
import { WinstonLoggerService } from '@/common/logger.service';
import { File, FileType, FileFormat, FileStatus } from '@/file/entities/file.entity';
import { UploadFileDto, UploadResponseDto } from './dto/upload-file.dto';
import * as fs from 'fs/promises';
import * as path from 'path';
import { ArchiveProcessorService } from '@/processing/services/archive-processor.service';

@Injectable()
export class UploadsService {
  private readonly uploadPath: string;
  private readonly maxFileSize: number;
  private readonly allowedMimeTypes = [
    'application/zip',
    'application/x-zip-compressed',
    'application/x-cbr',
    'application/x-cbz',
    'application/x-rar-compressed',
  ];

  constructor(
    @InjectRepository(File)
    private readonly fileRepository: Repository<File>,
    private readonly configService: ConfigService,
    private readonly logger: WinstonLoggerService,
    private readonly archiveProcessor: ArchiveProcessorService,
  ) {
  this.uploadPath = this.configService.get('uploads.destination') || './storage/uploads';
    this.maxFileSize = this.configService.get('uploads.maxFileSize') || 104857600; // 100MB
    
    // Ensure upload directory exists
    this.ensureUploadDirectory();
  }

  private async ensureUploadDirectory() {
    try {
      await fs.access(this.uploadPath);
    } catch {
      await fs.mkdir(this.uploadPath, { recursive: true });
      this.logger.log(`Created upload directory: ${this.uploadPath}`, 'UploadsService');
    }
  }

  async uploadFile(
    file: Express.Multer.File,
    uploadDto: UploadFileDto,
    userId: string,
  ): Promise<UploadResponseDto> {
    this.logger.log(
      `Starting file upload for user ${userId}: ${file.originalname}`,
      'UploadsService',
    );

    try {
      // Validar tipo de archivo
      this.validateFile(file);

      // Crear estructura de directorios por usuario
      const userUploadPath = path.join(this.uploadPath, userId);
      await this.ensureDirectory(userUploadPath);

      // Generar nombre único para el archivo
      const timestamp = Date.now();
      const sanitizedName = this.sanitizeFilename(file.originalname);
      const filename = `${timestamp}-${sanitizedName}`;
      const filePath = path.join(userUploadPath, filename);

      // Mover archivo del temporal al destino final
      await fs.writeFile(filePath, file.buffer);

      // Determinar formato del archivo
      const format = this.determineFileFormat(file.originalname);

      // Crear registro en base de datos
      const fileEntity = this.fileRepository.create({
        filename,
        originalFilename: file.originalname,
        path: filePath,
        fileType: uploadDto.fileType || FileType.COMPRESSED,
        format,
        mimeType: file.mimetype,
        fileSizeBytes: file.size,
        status: FileStatus.UPLOADED,
        uploadedAt: new Date(),
      });

      const savedFile = await this.fileRepository.save(fileEntity);

      this.logger.log(
        `File uploaded successfully: ${savedFile.id}`,
        'UploadsService',
      );

      const processingResult = await this.archiveProcessor.process(savedFile);

      savedFile.status = FileStatus.PROCESSED;
      savedFile.processedAt = new Date();
      await this.fileRepository.save(savedFile);

      return {
        id: savedFile.id,
        filename: savedFile.filename,
        originalFilename: savedFile.originalFilename,
        fileSize: savedFile.fileSizeBytes,
        mimeType: savedFile.mimeType,
        path: savedFile.path,
        status: savedFile.status,
        createdAt: savedFile.createdAt,
        metadata: processingResult.metadata,
        pages: processingResult.pages,
        thumbnails: processingResult.thumbnailPaths,
      };
    } catch (error) {
      this.logger.error(
        `File upload failed for user ${userId}: ${error.message}`,
        error.stack,
        'UploadsService',
      );

      // Cleanup temporal file if exists
      if (file.buffer) {
        await this.cleanupTempFile(file);
      }

      if (error instanceof BadRequestException) {
        throw error;
      }

      throw new InternalServerErrorException('File upload failed');
    }
  }

  private validateFile(file: Express.Multer.File) {
    // Validar tamaño
    if (file.size > this.maxFileSize) {
      throw new BadRequestException(
        `File too large. Maximum size is ${this.maxFileSize / 1024 / 1024}MB`,
      );
    }

    // Validar tipo MIME
    if (!this.allowedMimeTypes.includes(file.mimetype)) {
      throw new BadRequestException(
        `Invalid file type. Allowed types: CBZ, CBR, ZIP, RAR`,
      );
    }

    // Validar extensión
    const fileExt = path.extname(file.originalname).toLowerCase();
    const allowedExtensions = ['.cbz', '.cbr', '.zip', '.rar'];
    
    if (!allowedExtensions.includes(fileExt)) {
      throw new BadRequestException(
        `Invalid file extension. Allowed extensions: ${allowedExtensions.join(', ')}`,
      );
    }
  }

  private async ensureDirectory(dirPath: string) {
    try {
      await fs.access(dirPath);
    } catch {
      await fs.mkdir(dirPath, { recursive: true });
    }
  }

  private sanitizeFilename(filename: string): string {
    // Remover caracteres peligrosos y mantener solo alfanuméricos, guiones y puntos
    return filename.replace(/[^a-zA-Z0-9.-]/g, '_');
  }

  private determineFileFormat(filename: string): FileFormat {
    const ext = path.extname(filename).toLowerCase();
    
    switch (ext) {
      case '.cbz':
        return FileFormat.CBZ;
      case '.cbr':
        return FileFormat.CBR;
      case '.zip':
        return FileFormat.ZIP;
      case '.rar':
        return FileFormat.RAR;
      default:
        return FileFormat.ZIP; // Default fallback
    }
  }

  private async cleanupTempFile(file: Express.Multer.File) {
    try {
      // Since we're using memory storage, no temp file to cleanup
      // This is a placeholder for future disk storage implementation
      this.logger.log('Cleaned up temporary file resources', 'UploadsService');
    } catch (error) {
      this.logger.warn(
        `Failed to cleanup temp file: ${error.message}`,
        'UploadsService',
      );
    }
  }

  async getFile(fileId: string, userId: string): Promise<File> {
    const file = await this.fileRepository.findOne({
      where: { id: fileId },
    });

    if (!file) {
      throw new BadRequestException('File not found');
    }

    // Verificar que el archivo pertenece al usuario (básico)
    // En una implementación más completa, se verificaría a través de la relación con Chapter/Manga/User
    if (!file.path.includes(userId)) {
      throw new BadRequestException('Access denied');
    }

    return file;
  }

  async deleteFile(fileId: string, userId: string): Promise<void> {
    const file = await this.getFile(fileId, userId);

    try {
      // Eliminar archivo físico
      await fs.unlink(file.path);
      
      // Marcar como eliminado en base de datos
      file.status = FileStatus.DELETED;
      await this.fileRepository.save(file);

      this.logger.log(`File deleted successfully: ${fileId}`, 'UploadsService');
    } catch (error) {
      this.logger.error(
        `Failed to delete file ${fileId}: ${error.message}`,
        error.stack,
        'UploadsService',
      );
      throw new InternalServerErrorException('Failed to delete file');
    }
  }
}