export interface AppConfig {
  port: number;
  nodeEnv: string;
  jwt: {
    secret: string;
    expiresIn: string;
  };
  cors: {
    origin: string;
  };
  rateLimit: {
    windowMs: number;
    maxRequests: number;
  };
  logging: {
    level: string;
  };
  https: {
    enabled: boolean;
    keyPath?: string;
    certPath?: string;
  };
  database?: {
    url?: string;
  };
  uploads?: {
    destination?: string;
    maxFileSize?: number;
  };
  processing: {
    tempDir: string;
    cacheDir: string;
    thumbnailsDir: string;
    concurrency: number;
    thumbnailSizes: number[];
    supportedImageFormats: string[];
  };
}

export default (): AppConfig => ({
  port: parseInt(process.env.API_PORT || '3001', 10),
  nodeEnv: process.env.NODE_ENV || 'development',
  jwt: {
    secret: process.env.JWT_SECRET || 'fallback-secret-change-in-production',
    expiresIn: process.env.JWT_EXPIRES_IN || '1h',
  },
  cors: {
    origin: process.env.CORS_ORIGIN || 'http://localhost:3000',
  },
  rateLimit: {
    windowMs: parseInt(process.env.RATE_LIMIT_WINDOW_MS || '900000', 10),
    maxRequests: parseInt(process.env.RATE_LIMIT_MAX_REQUESTS || '100', 10),
  },
  logging: {
    level: (process.env.LOG_LEVEL || 'info') as
      | 'error'
      | 'warn'
      | 'info'
      | 'debug',
  },
  https: {
    enabled: process.env.HTTPS_ENABLED === 'true',
    keyPath: process.env.HTTPS_KEY_PATH,
    certPath: process.env.HTTPS_CERT_PATH,
  },
  database: {
    url: process.env.DATABASE_URL,
  },
  uploads: {
    destination: process.env.UPLOAD_DESTINATION || './storage/uploads',
    maxFileSize: parseInt(process.env.MAX_FILE_SIZE || '104857600', 10), // 100MB
  },
  processing: {
    tempDir: process.env.PROCESSING_TEMP_DIR || './storage/temp',
    cacheDir:
      process.env.PROCESSING_CACHE_DIR || './storage/cache/processing',
    thumbnailsDir:
      process.env.PROCESSING_THUMBNAILS_DIR || './storage/thumbnails',
    concurrency: parseInt(process.env.PROCESSING_CONCURRENCY || '2', 10),
    thumbnailSizes: (process.env.PROCESSING_THUMBNAIL_SIZES || '128,256,512')
      .split(',')
      .map((value) => parseInt(value.trim(), 10))
      .filter((value) => Number.isFinite(value) && value > 0),
    supportedImageFormats: (process.env.SUPPORTED_IMAGE_FORMATS ||
      'jpg,jpeg,png,webp')
      .split(',')
      .map((value) => value.trim().toLowerCase())
      .filter((value) => Boolean(value)),
  },
});
