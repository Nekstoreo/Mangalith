import { File } from '@/file/entities/file.entity';

export interface MangaMetadata {
  title?: string;
  chapter?: number;
  volume?: number;
  language?: string;
  scanlator?: string;
  series?: string;
  authors?: string[];
  tags?: string[];
  source?: string;
}

export interface PageMetadata {
  index: number;
  filename: string;
  width: number;
  height: number;
  format: string;
  isCover: boolean;
}

export interface ProcessingContext {
  file: File;
  tempDir: string;
  cacheDir: string;
  thumbnailsDir: string;
  allowedFormats: string[];
  thumbnailSizes: number[];
}

export interface ProcessingResult {
  metadata: MangaMetadata;
  pages: PageMetadata[];
  cover?: PageMetadata;
  thumbnailPaths: Record<number, string>;
}


