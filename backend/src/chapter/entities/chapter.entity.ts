import {
  Entity,
  PrimaryGeneratedColumn,
  Column,
  CreateDateColumn,
  UpdateDateColumn,
  ManyToOne,
  OneToMany,
  JoinColumn,
  Index,
} from 'typeorm';
import { Manga } from '@/manga/entities/manga.entity';
import { File } from '@/file/entities/file.entity';

export enum ChapterStatus {
  UPLOADED = 'uploaded',
  PROCESSING = 'processing',
  READY = 'ready',
  ERROR = 'error',
}

@Entity('chapters')
@Index(['manga', 'chapterNumber']) // Índice compuesto para ordenar capítulos
@Index(['status']) // Índice para filtrado por estado
export class Chapter {
  @PrimaryGeneratedColumn('uuid')
  id: string;

  @Column({ name: 'chapter_number', type: 'decimal', precision: 5, scale: 1 })
  chapterNumber: number;

  @Column({
    name: 'volume_number',
    type: 'decimal',
    precision: 5,
    scale: 1,
    nullable: true,
  })
  volumeNumber: number;

  @Column({ length: 255, nullable: true })
  title: string;

  @Column({ type: 'text', nullable: true })
  description: string;

  @Column({
    type: 'enum',
    enum: ChapterStatus,
    default: ChapterStatus.UPLOADED,
  })
  status: ChapterStatus;

  @Column({ name: 'page_count', default: 0 })
  pageCount: number;

  @Column({ name: 'file_size_bytes', type: 'bigint', default: 0 })
  fileSizeBytes: number;

  @Column({ name: 'original_file_name', length: 255, nullable: true })
  originalFileName: string;

  @Column({ name: 'processing_started_at', nullable: true })
  processingStartedAt: Date;

  @Column({ name: 'processing_completed_at', nullable: true })
  processingCompletedAt: Date;

  @Column({ name: 'error_message', type: 'text', nullable: true })
  errorMessage: string;

  @Column({ name: 'view_count', default: 0 })
  viewCount: number;

  @Column({ name: 'read_count', default: 0 })
  readCount: number;

  @Column({ name: 'is_public', default: true })
  isPublic: boolean;

  @CreateDateColumn({ name: 'created_at' })
  createdAt: Date;

  @UpdateDateColumn({ name: 'updated_at' })
  updatedAt: Date;

  @Column({ name: 'published_at', nullable: true })
  publishedAt: Date;

  // Relations
  @ManyToOne(() => Manga, (manga) => manga.chapters, { onDelete: 'CASCADE' })
  @JoinColumn({ name: 'manga_id' })
  manga: Manga;

  @OneToMany(() => File, (file) => file.chapter, { cascade: true })
  files: File[];
}
