import { Injectable, BadRequestException, InternalServerErrorException } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { WinstonLoggerService } from '@/common/logger.service';
import { File } from '@/file/entities/file.entity';
import * as fs from 'node:fs/promises';
import * as path from 'node:path';
import { extractArchiveEntries } from '../utils/archive.utils';
import { isLikelyCover } from '../utils/file.utils';
import { ensureImageFormat, inspectImage, createThumbnail } from '../utils/image.utils';
import { JsonFileCache } from '../utils/cache.utils';
import { ArchiveExtractionSummary, ExtractedImageEntry } from '../types/archive-entry.type';
import { MangaMetadata, PageMetadata, ProcessingContext, ProcessingResult } from '../types/manga-processing.types';
import pLimit from 'p-limit';

@Injectable()
export class ArchiveProcessorService {
  private readonly cache: JsonFileCache<ProcessingResult>;

  constructor(
    private readonly configService: ConfigService,
    private readonly logger: WinstonLoggerService,
  ) {
    const cacheDir = this.configService.get<string>('processing.cacheDir');
    if (!cacheDir) {
      throw new Error('processing.cacheDir no configurado');
    }

    this.cache = new JsonFileCache(cacheDir, this.logger);
  }

  async process(file: File): Promise<ProcessingResult> {
    const cached = await this.cache.get(file.id);
    if (cached) {
      this.logger.debug(`Cache hit para archivo ${file.id}`, 'ArchiveProcessorService');
      return cached;
    }

    const tempDir = this.configService.get<string>('processing.tempDir');
    const thumbnailsDir = this.configService.get<string>('processing.thumbnailsDir');
    const allowedFormats = this.configService.get<string[]>('processing.supportedImageFormats') ?? [];
    const thumbnailSizes = this.configService.get<number[]>('processing.thumbnailSizes') ?? [];
    const concurrency = this.configService.get<number>('processing.concurrency') ?? 2;

    if (!tempDir || !thumbnailsDir || allowedFormats.length === 0 || thumbnailSizes.length === 0) {
      throw new InternalServerErrorException('Configuración de procesamiento incompleta');
    }

    const context: ProcessingContext = {
      file,
      tempDir,
      cacheDir: this.configService.get('processing.cacheDir'),
      thumbnailsDir,
      allowedFormats,
      thumbnailSizes,
    };

    await this.ensureDirectories([tempDir, thumbnailsDir]);

    const buffer = await fs.readFile(file.path);

    const summary = await this.extractEntries(buffer, file.filename, context);

    if (summary.imageEntries.length === 0) {
      throw new BadRequestException('El archivo no contiene imágenes válidas');
    }

    const pages = await this.buildPageMetadata(summary.imageEntries, context, concurrency);
    const metadata = this.inferMetadataFromFilename(file.originalFilename);
    const cover = summary.coverEntry ? pages.find((page) => page.filename === summary.coverEntry?.entryName) : pages[0];
    const thumbnails = await this.generateThumbnails(cover, summary, context);

    const result: ProcessingResult = {
      metadata,
      pages,
      cover,
      thumbnailPaths: thumbnails,
    };

    await this.cache.set(file.id, result);

    return result;
  }

  private async ensureDirectories(dirs: string[]): Promise<void> {
    await Promise.all(
      dirs.map((dir) =>
        fs.mkdir(dir, { recursive: true }).catch((error) => {
          this.logger.error(`No se pudo crear directorio ${dir}: ${(error as Error).message}`, error.stack, 'ArchiveProcessorService');
          throw new InternalServerErrorException('Error preparando directorios de procesamiento');
        }),
      ),
    );
  }

  private async extractEntries(
    buffer: Buffer,
    filename: string,
    context: ProcessingContext,
  ): Promise<ArchiveExtractionSummary> {
    const entries = await extractArchiveEntries(buffer, filename, context.tempDir);
    const allowedExtensions = context.allowedFormats;

    let coverEntry: ExtractedImageEntry | undefined;
    const imageEntries: ExtractedImageEntry[] = [];
    const skippedEntries = [];

    for (const entry of entries) {
      if (entry.isDirectory) {
        continue;
      }

      if (!(await ensureImageFormat(await entry.getData(), allowedExtensions))) {
        skippedEntries.push(entry);
        continue;
      }

      const data = await entry.getData();
      const info = await inspectImage(data);

      if (!info) {
        skippedEntries.push(entry);
        continue;
      }

      const imageEntry: ExtractedImageEntry = {
        ...entry,
        width: info.width,
        height: info.height,
        format: info.format,
        getData: async () => data,
      };

      if (!coverEntry && isLikelyCover(entry.entryName)) {
        coverEntry = imageEntry;
      }

      imageEntries.push(imageEntry);
    }

    imageEntries.sort((a, b) => a.entryName.localeCompare(b.entryName, undefined, { numeric: true }));

    if (!coverEntry) {
      coverEntry = imageEntries[0];
    }

    return {
      totalEntries: entries.length,
      imageEntries,
      skippedEntries,
      coverEntry,
    };
  }

  private inferMetadataFromFilename(filename: string): MangaMetadata {
    const name = path.parse(filename).name;
    const metadata: MangaMetadata = {};

    const chapterMatch = name.match(/chapter\s*(\d+(?:\.\d+)?)/i);
    const volumeMatch = name.match(/volume\s*(\d+(?:\.\d+)?)/i);
    const titleMatch = name.match(/^(.*?)\s*(?:chapter|vol|volume)/i);

    if (chapterMatch) {
      metadata.chapter = Number.parseFloat(chapterMatch[1]);
    }

    if (volumeMatch) {
      metadata.volume = Number.parseFloat(volumeMatch[1]);
    }

    if (titleMatch) {
      metadata.title = titleMatch[1].replace(/[_-]+/g, ' ').trim();
    } else {
      metadata.title = name.replace(/[_-]+/g, ' ').trim();
    }

    return metadata;
  }

  private async buildPageMetadata(
    entries: ExtractedImageEntry[],
    context: ProcessingContext,
    concurrency: number,
  ): Promise<PageMetadata[]> {
    const limit = pLimit(concurrency);

    const results = await Promise.all(
      entries.map((entry, index) =>
        limit(async () => {
          const data = await entry.getData();
          const info = await inspectImage(data);
          if (!info) {
            throw new BadRequestException(`Imagen inválida: ${entry.entryName}`);
          }
          return {
            index,
            filename: entry.entryName,
            width: info.width,
            height: info.height,
            format: info.format,
            isCover: false,
          } satisfies PageMetadata;
        }),
      ),
    );

    return results;
  }

  private async generateThumbnails(
    cover: PageMetadata | undefined,
    summary: ArchiveExtractionSummary,
    context: ProcessingContext,
  ): Promise<Record<number, string>> {
    if (!cover) {
      return {};
    }

    const coverEntry = summary.imageEntries.find((entry) => entry.entryName === cover.filename);

    if (!coverEntry) {
      throw new InternalServerErrorException('No se encontró la imagen de portada para thumbnails');
    }

    const data = await coverEntry.getData();

    const thumbnails: Record<number, string> = {};

    await Promise.all(
      context.thumbnailSizes.map(async (size) => {
        const thumbnailBuffer = await createThumbnail(data, size, 'webp');

        const fileName = `${context.file.id}-${size}.webp`;
        const filePath = path.join(context.thumbnailsDir, fileName);
        await fs.writeFile(filePath, thumbnailBuffer);
        thumbnails[size] = filePath;
      }),
    );

    return thumbnails;
  }
}


