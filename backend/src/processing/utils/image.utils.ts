import { fromBuffer } from 'file-type';
import sharp, { Sharp } from 'sharp';

export interface ImageInfo {
  width: number;
  height: number;
  format: string;
}

export const inspectImage = async (
  buffer: Buffer,
): Promise<ImageInfo | null> => {
  try {
    const metadata = await sharp(buffer).metadata();
    if (!metadata.width || !metadata.height || !metadata.format) {
      return null;
    }

    return {
      width: metadata.width,
      height: metadata.height,
      format: metadata.format,
    };
  } catch {
    return null;
  }
};

export const ensureImageFormat = async (
  buffer: Buffer,
  allowedFormats: string[],
): Promise<boolean> => {
  const detectedType = await fromBuffer(buffer);
  if (!detectedType) {
    return false;
  }
  return allowedFormats.includes(detectedType.ext.toLowerCase());
};

export const createThumbnail = async (
  buffer: Buffer,
  size: number,
  format: 'jpeg' | 'png' | 'webp' = 'webp',
): Promise<Buffer> => {
  const transformer: Sharp = sharp(buffer).resize(size, size, {
    fit: 'cover',
    position: 'centre',
  });

  switch (format) {
    case 'jpeg':
      return transformer.jpeg({ quality: 85 }).toBuffer();
    case 'png':
      return transformer.png().toBuffer();
    default:
      return transformer.webp({ quality: 85 }).toBuffer();
  }
};
