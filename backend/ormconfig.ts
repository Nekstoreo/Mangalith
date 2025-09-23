import { DataSource } from 'typeorm';
import { ConfigService } from '@nestjs/config';
import { config } from 'dotenv';
import { User, Manga, Chapter, File } from '@/entities/index';

// Cargar variables de entorno
config({ path: ['.env.local', '.env'] });

const configService = new ConfigService();

const AppDataSource = new DataSource({
  type: 'postgres',
  url: configService.get('DATABASE_URL') || 'postgresql://mangalith_user:changeme123@localhost:5432/mangalith_dev',
  entities: ['src/**/*.entity{.ts,.js}'],
  migrations: ['src/migrations/*{.ts,.js}'],
  migrationsTableName: 'migrations',
  synchronize: false, // Usar migraciones en producción
  logging: configService.get('NODE_ENV') === 'development',
  logger: 'advanced-console',
  maxQueryExecutionTime: 1000, // Log queries que toman más de 1s
  poolSize: 10, // Pool de conexiones optimizado para alta concurrencia
  extra: {
    // Configuración adicional para PostgreSQL
    ssl: configService.get('NODE_ENV') === 'production' ? { rejectUnauthorized: false } : false,
    max: 20, // Máximo de conexiones en el pool
    min: 2,  // Mínimo de conexiones en el pool
    acquireTimeoutMillis: 60000, // Timeout para adquirir conexión
    idleTimeoutMillis: 300000,   // Timeout para conexiones idle
  },
});

export default AppDataSource;
