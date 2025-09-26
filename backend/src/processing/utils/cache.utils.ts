import { promises as fs } from 'node:fs';
import * as path from 'node:path';
import { WinstonLoggerService } from '@/common/logger.service';

export interface CacheEntry<T> {
  key: string;
  value: T;
  createdAt: number;
}

export class JsonFileCache<T> {
  constructor(
    private readonly cacheDir: string,
    private readonly logger: WinstonLoggerService,
  ) {}

  private getPath(key: string): string {
    const sanitized = key.replace(/[^a-zA-Z0-9-_]/g, '_');
    return path.join(this.cacheDir, `${sanitized}.json`);
  }

  async get(key: string): Promise<T | null> {
    try {
      const filePath = this.getPath(key);
      const data = await fs.readFile(filePath, 'utf8');
      const parsed = JSON.parse(data) as CacheEntry<T>;
      return parsed.value;
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code !== 'ENOENT') {
        this.logger.warn(
          `Fallo al leer cache ${key}: ${(error as Error).message}`,
          'JsonFileCache',
        );
      }
      return null;
    }
  }

  async set(key: string, value: T): Promise<void> {
    const filePath = this.getPath(key);
    await fs.mkdir(path.dirname(filePath), { recursive: true });
    const entry: CacheEntry<T> = {
      key,
      value,
      createdAt: Date.now(),
    };
    await fs.writeFile(filePath, JSON.stringify(entry, null, 2), 'utf8');
  }
}
