import * as path from 'node:path';

export const isImageFile = (
  filename: string,
  allowedExtensions: string[],
): boolean => {
  const ext = path.extname(filename).toLowerCase().replace('.', '');
  return allowedExtensions.includes(ext);
};

export const normalizeEntryName = (entryName: string): string => {
  return entryName.replace(/\\/g, '/').replace(/^\/+/, '');
};

export const getBaseName = (entryName: string): string => {
  const normalized = normalizeEntryName(entryName);
  const basename = normalized.split('/').pop();
  return basename ?? normalized;
};

export const isLikelyCover = (entryName: string): boolean => {
  const normalized = getBaseName(entryName).toLowerCase();
  return /cover|portada|capa|001|000/.test(normalized);
};
