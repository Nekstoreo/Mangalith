import { IsEnum, IsOptional, IsString, IsUUID } from 'class-validator';
import { FileType } from '@/file/entities/file.entity';

export class UploadFileDto {
  @IsUUID()
  @IsOptional()
  chapterId?: string;

  @IsString()
  @IsOptional()
  title?: string;

  @IsString()
  @IsOptional()
  description?: string;

  @IsEnum(FileType)
  @IsOptional()
  fileType?: FileType = FileType.COMPRESSED;
}

export class UploadResponseDto {
  id: string;
  filename: string;
  originalFilename: string;
  fileSize: number;
  mimeType: string;
  path: string;
  status: string;
  createdAt: Date;
  metadata?: unknown;
  pages?: unknown;
  thumbnails?: Record<number, string>;
}