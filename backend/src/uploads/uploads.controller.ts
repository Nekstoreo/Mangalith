import {
  Controller,
  Post,
  Get,
  Delete,
  Param,
  UseInterceptors,
  UploadedFile,
  Body,
  HttpCode,
  HttpStatus,
  ParseUUIDPipe,
  BadRequestException,
} from '@nestjs/common';
import { FileInterceptor } from '@nestjs/platform-express';
import { UploadsService } from './uploads.service';
import { UploadFileDto, UploadResponseDto } from './dto/upload-file.dto';
import { WinstonLoggerService } from '@/common/logger.service';

// TODO: Implementar guards de autenticación cuando estén disponibles
// import { AuthGuard } from '@/auth/guards/auth.guard';
// import { CurrentUser } from '@/auth/decorators/current-user.decorator';
// import { User } from '@/users/entities/user.entity';

@Controller('uploads')
export class UploadsController {
  constructor(
    private readonly uploadsService: UploadsService,
    private readonly logger: WinstonLoggerService,
  ) {}

  @Post()
  @HttpCode(HttpStatus.CREATED)
  @UseInterceptors(
    FileInterceptor('file', {
      limits: {
        fileSize: 104857600, // 100MB - se puede configurar desde config
      },
      fileFilter: (req, file, cb) => {
        const allowedMimeTypes = [
          'application/zip',
          'application/x-zip-compressed',
          'application/x-cbr',
          'application/x-cbz',
          'application/x-rar-compressed',
        ];

        if (allowedMimeTypes.includes(file.mimetype)) {
          cb(null, true);
        } else {
          cb(
            new BadRequestException(
              'Invalid file type. Only CBZ, CBR, ZIP, and RAR files are allowed.',
            ),
            false,
          );
        }
      },
    }),
  )
  // TODO: Activar cuando el sistema de auth esté listo
  // @UseGuards(AuthGuard)
  async uploadFile(
    @UploadedFile() file: Express.Multer.File,
    @Body() uploadDto: UploadFileDto,
    // @CurrentUser() user: User,
  ): Promise<{ success: boolean; data: UploadResponseDto; message: string }> {
    if (!file) {
      throw new BadRequestException('No file uploaded');
    }

    this.logger.log(
      `Upload request received: ${file.originalname} (${file.size} bytes)`,
      'UploadsController',
    );

    // TODO: Usar el usuario real cuando la autenticación esté disponible
    const mockUserId = 'temp-user-id'; // Temporal para testing

    try {
      const result = await this.uploadsService.uploadFile(
        file,
        uploadDto,
        mockUserId,
      );

      return {
        success: true,
        data: result,
        message: 'File uploaded successfully',
      };
    } catch (error) {
      this.logger.error(
        `Upload failed: ${error.message}`,
        error.stack,
        'UploadsController',
      );
      throw error;
    }
  }

  @Get(':id')
  // TODO: Activar cuando el sistema de auth esté listo
  // @UseGuards(AuthGuard)
  async getFile(
    @Param('id', ParseUUIDPipe) id: string,
    // @CurrentUser() user: User,
  ) {
    // TODO: Usar el usuario real cuando la autenticación esté disponible
    const mockUserId = 'temp-user-id';

    try {
      const file = await this.uploadsService.getFile(id, mockUserId);
      return {
        success: true,
        data: {
          id: file.id,
          filename: file.filename,
          originalFilename: file.originalFilename,
          fileSize: file.fileSizeBytes,
          mimeType: file.mimeType,
          status: file.status,
          createdAt: file.createdAt,
          updatedAt: file.updatedAt,
        },
        message: 'File retrieved successfully',
      };
    } catch (error) {
      this.logger.error(
        `Get file failed: ${error.message}`,
        error.stack,
        'UploadsController',
      );
      throw error;
    }
  }

  @Delete(':id')
  @HttpCode(HttpStatus.NO_CONTENT)
  // TODO: Activar cuando el sistema de auth esté listo
  // @UseGuards(AuthGuard)
  async deleteFile(
    @Param('id', ParseUUIDPipe) id: string,
    // @CurrentUser() user: User,
  ): Promise<void> {
    // TODO: Usar el usuario real cuando la autenticación esté disponible
    const mockUserId = 'temp-user-id';

    try {
      await this.uploadsService.deleteFile(id, mockUserId);
      this.logger.log(`File deleted: ${id}`, 'UploadsController');
    } catch (error) {
      this.logger.error(
        `Delete file failed: ${error.message}`,
        error.stack,
        'UploadsController',
      );
      throw error;
    }
  }
}
