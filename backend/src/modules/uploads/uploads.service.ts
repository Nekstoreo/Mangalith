import { Injectable } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import * as sharp from 'sharp';
import { promises as fs } from 'fs';
import * as path from 'path';
import { v4 as uuidv4 } from 'uuid';

@Injectable()
export class UploadsService {
  private readonly uploadDir: string;

  constructor(private configService: ConfigService) {
    this.uploadDir = this.configService.get('UPLOAD_DIR', 'uploads');
    this.ensureUploadDir();
  }

  private async ensureUploadDir() {
    try {
      await fs.mkdir(this.uploadDir, { recursive: true });
    } catch (error) {
      console.error('Failed to create upload directory:', error);
    }
  }

  async saveImage(file: Express.Multer.File): Promise<string> {
    const filename = `${uuidv4()}.webp`;
    const filepath = path.join(this.uploadDir, filename);

    await sharp(file.buffer)
      .webp({ quality: 85 })
      .toFile(filepath);

    return `/uploads/${filename}`;
  }

  async deleteImage(imageUrl: string): Promise<void> {
    const filename = path.basename(imageUrl);
    const filepath = path.join(this.uploadDir, filename);

    try {
      await fs.unlink(filepath);
    } catch (error) {
      console.error('Failed to delete image:', error);
    }
  }
}
