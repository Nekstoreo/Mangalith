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
import { User } from '@/users/entities/user.entity';
import { Chapter } from '@/chapter/entities/chapter.entity';

export enum MangaStatus {
  ONGOING = 'ongoing',
  COMPLETED = 'completed',
  CANCELLED = 'cancelled',
  HIATUS = 'hiatus',
}

export enum MangaType {
  MANGA = 'manga',
  MANHWA = 'manhwa',
  MANHUA = 'manhua',
  COMIC = 'comic',
  NOVEL = 'novel',
}

@Entity('mangas')
@Index(['title', 'status']) // Índice compuesto para búsquedas
@Index(['user']) // Índice para filtrado por usuario
export class Manga {
  @PrimaryGeneratedColumn('uuid')
  id: string;

  @Column({ length: 255 })
  title: string;

  @Column({ name: 'original_title', length: 255, nullable: true })
  originalTitle: string;

  @Column({ type: 'text', nullable: true })
  description: string;

  @Column({
    type: 'enum',
    enum: MangaStatus,
    default: MangaStatus.ONGOING,
  })
  status: MangaStatus;

  @Column({
    type: 'enum',
    enum: MangaType,
    default: MangaType.MANGA,
  })
  type: MangaType;

  @Column({ name: 'cover_image_url', length: 500, nullable: true })
  coverImageUrl: string;

  @Column({ name: 'banner_image_url', length: 500, nullable: true })
  bannerImageUrl: string;

  @Column({ length: 100, nullable: true })
  author: string;

  @Column({ length: 100, nullable: true })
  artist: string;

  @Column({ name: 'year_published', nullable: true })
  yearPublished: number;

  @Column({ name: 'total_chapters', default: 0 })
  totalChapters: number;

  @Column({ name: 'total_volumes', default: 0 })
  totalVolumes: number;

  @Column({ name: 'genres', type: 'simple-array', nullable: true })
  genres: string[];

  @Column({ name: 'tags', type: 'simple-array', nullable: true })
  tags: string[];

  @Column({
    name: 'rating',
    type: 'decimal',
    precision: 3,
    scale: 2,
    default: 0,
  })
  rating: number;

  @Column({ name: 'rating_count', default: 0 })
  ratingCount: number;

  @Column({ name: 'view_count', default: 0 })
  viewCount: number;

  @Column({ name: 'is_public', default: true })
  isPublic: boolean;

  @Column({ name: 'is_featured', default: false })
  isFeatured: boolean;

  @CreateDateColumn({ name: 'created_at' })
  createdAt: Date;

  @UpdateDateColumn({ name: 'updated_at' })
  updatedAt: Date;

  // Relations
  @ManyToOne(() => User, (user) => user.mangas, { onDelete: 'CASCADE' })
  @JoinColumn({ name: 'user_id' })
  user: User;

  @OneToMany(() => Chapter, (chapter) => chapter.manga, { cascade: true })
  chapters: Chapter[];
}
