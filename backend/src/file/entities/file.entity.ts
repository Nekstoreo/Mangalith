import {
  Entity,
  PrimaryGeneratedColumn,
  Column,
  CreateDateColumn,
  UpdateDateColumn,
  ManyToOne,
  JoinColumn,
  Index,
} from 'typeorm';
import { Chapter } from '@/chapter/entities/chapter.entity';

export enum FileType {
  IMAGE = 'image',
  COMPRESSED = 'compressed',
  THUMBNAIL = 'thumbnail',
}

export enum FileFormat {
  JPG = 'jpg',
  JPEG = 'jpeg',
  PNG = 'png',
  WEBP = 'webp',
  CBZ = 'cbz',
  CBR = 'cbr',
  ZIP = 'zip',
  RAR = 'rar',
}

export enum FileStatus {
  UPLOADED = 'uploaded',
  PROCESSED = 'processed',
  ERROR = 'error',
  DELETED = 'deleted',
}

@Entity('files')
@Index(['chapter', 'fileType']) // Índice compuesto para filtrado
@Index(['status']) // Índice para estado
export class File {
  @PrimaryGeneratedColumn('uuid')
  id: string;

  @Column({ length: 255 })
  filename: string;

  @Column({ name: 'original_filename', length: 255 })
  originalFilename: string;

  @Column({ length: 500 })
  path: string;

  @Column({ name: 'public_url', length: 500, nullable: true })
  publicUrl: string;

  @Column({
    name: 'file_type',
    type: 'enum',
    enum: FileType,
  })
  fileType: FileType;

  @Column({
    type: 'enum',
    enum: FileFormat,
  })
  format: FileFormat;

  @Column({ name: 'mime_type', length: 100 })
  mimeType: string;

  @Column({ name: 'file_size_bytes', type: 'bigint' })
  fileSizeBytes: number;

  @Column({ name: 'width_pixels', nullable: true })
  widthPixels: number;

  @Column({ name: 'height_pixels', nullable: true })
  heightPixels: number;

  @Column({ name: 'page_number', nullable: true })
  pageNumber: number;

  @Column({ name: 'is_cover', default: false })
  isCover: boolean;

  @Column({ name: 'quality_level', default: 100 })
  qualityLevel: number;

  @Column({
    type: 'enum',
    enum: FileStatus,
    default: FileStatus.UPLOADED,
  })
  status: FileStatus;

  @Column({ name: 'error_message', type: 'text', nullable: true })
  errorMessage: string;

  @Column({ name: 'processing_attempts', default: 0 })
  processingAttempts: number;

  @CreateDateColumn({ name: 'created_at' })
  createdAt: Date;

  @UpdateDateColumn({ name: 'updated_at' })
  updatedAt: Date;

  @Column({ name: 'uploaded_at' })
  uploadedAt: Date;

  @Column({ name: 'processed_at', nullable: true })
  processedAt: Date;

  // Relations
  @ManyToOne(() => Chapter, (chapter) => chapter.files, { onDelete: 'CASCADE' })
  @JoinColumn({ name: 'chapter_id' })
  chapter: Chapter;
}
