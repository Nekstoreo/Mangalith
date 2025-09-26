import AdmZip from 'adm-zip';
import { ExtractedArchiveEntry } from '../types/archive-entry.type';
import * as path from 'node:path';
import { promises as fs } from 'node:fs';
import * as Unrar from 'unrar-js';

export enum ArchiveType {
  ZIP = 'zip',
  CBZ = 'cbz',
  CBR = 'cbr',
  RAR = 'rar',
}

export const detectArchiveType = (filename: string): ArchiveType => {
  const ext = path.extname(filename).toLowerCase().replace('.', '');
  switch (ext) {
    case 'cbz':
    case 'zip':
      return ArchiveType.ZIP;
    case 'cbr':
    case 'rar':
      return ArchiveType.RAR;
    default:
      throw new Error(`Formato de archivo no soportado: ${ext}`);
  }
};

const readZipEntries = (buffer: Buffer): ExtractedArchiveEntry[] => {
  const zip = new AdmZip(buffer);
  return zip
    .getEntries()
    .filter((entry) => !entry.isDirectory)
    .map<ExtractedArchiveEntry>((entry) => ({
      entryPath: entry.entryName,
      entryName: path.basename(entry.entryName),
      size: entry.header.size,
      isDirectory: entry.isDirectory,
      lastModified: entry.header?.time,
      getData: () => Promise.resolve(entry.getData()),
    }));
};

const readRarEntries = async (
  buffer: Buffer,
  tempDir: string,
): Promise<ExtractedArchiveEntry[]> => {
  const rarFilePath = path.join(tempDir, `rar-${Date.now()}-${Math.random()}`);
  await fs.writeFile(rarFilePath, buffer);

  try {
    const extractor = await Unrar.createExtractorFromFile({
      filename: rarFilePath,
    });
    const listResult = extractor.getFileList().fileHeaders;

    return listResult
      .filter((header: { flags: number }) => (header.flags & 0x2000) === 0)
      .map<ExtractedArchiveEntry>((header: any) => ({
        entryPath: header.name,
        entryName: path.basename(header.name),
        size: header.unpackedSize,
        isDirectory: header.flags & 0x1000,
        lastModified: header.time ? new Date(header.time * 1000) : undefined,
        getData: async () => {
          const extracted = extractor.extractFiles([header.name]);
          const [file] = extracted.files;
          if (!file) {
            throw new Error(`No se pudo extraer el archivo ${header.name}`);
          }
          return Buffer.from(file.extraction); // Uint8Array -> Buffer
        },
      }));
  } finally {
    await fs.unlink(rarFilePath);
  }
};

export const extractArchiveEntries = async (
  buffer: Buffer,
  filename: string,
  tempDir: string,
): Promise<ExtractedArchiveEntry[]> => {
  const archiveType = detectArchiveType(filename);
  if (archiveType === ArchiveType.ZIP) {
    return readZipEntries(buffer);
  }

  return readRarEntries(buffer, tempDir);
};
