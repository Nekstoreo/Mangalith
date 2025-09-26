export interface ExtractedArchiveEntry {
  entryPath: string;
  entryName: string;
  size: number;
  isDirectory: boolean;
  lastModified?: Date;
  getData(): Promise<Buffer>;
}

export interface ExtractedImageEntry extends ExtractedArchiveEntry {
  width?: number;
  height?: number;
  format?: string;
}

export interface ArchiveExtractionSummary {
  totalEntries: number;
  imageEntries: ExtractedImageEntry[];
  skippedEntries: ExtractedArchiveEntry[];
  coverEntry?: ExtractedImageEntry;
}
