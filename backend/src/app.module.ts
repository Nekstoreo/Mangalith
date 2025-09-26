import { Module } from '@nestjs/common';
import { ConfigModule, ConfigService } from '@nestjs/config';
import { TypeOrmModule } from '@nestjs/typeorm';
import { AppController } from './app.controller';
import { AppService } from './app.service';
import { LoggerModule } from '@/common/logger.module';
import { UploadsModule } from '@/uploads/uploads.module';
import { configuration, validationSchema } from '@/config';

@Module({
  imports: [
    ConfigModule.forRoot({
      load: [configuration],
      isGlobal: true,
      validationSchema,
      envFilePath: ['.env.local', '.env'],
    }),
    TypeOrmModule.forRootAsync({
      imports: [ConfigModule],
      useFactory: (configService: ConfigService) => ({
        type: 'postgres',
        url:
          configService.get('DATABASE_URL') ||
          'postgresql://mangalith_user:changeme123@localhost:5432/mangalith_dev',
        entities: ['dist/**/*.entity{.ts,.js}'],
        migrations: ['dist/migrations/*{.ts,.js}'],
        migrationsTableName: 'migrations',
        synchronize: false,
        logging: configService.get('NODE_ENV') === 'development',
        logger: 'advanced-console',
        maxQueryExecutionTime: 1000,
        poolSize: 10,
        extra: {
          ssl:
            configService.get('NODE_ENV') === 'production'
              ? { rejectUnauthorized: false }
              : false,
          max: 20,
          min: 2,
          acquireTimeoutMillis: 60000,
          idleTimeoutMillis: 300000,
        },
      }),
      inject: [ConfigService],
    }),
    LoggerModule,
    UploadsModule,
  ],
  controllers: [AppController],
  providers: [AppService],
})
export class AppModule {}
