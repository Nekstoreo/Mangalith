import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { UploadsController } from './uploads.controller';
import { UploadsService } from './uploads.service';
import { File } from '@/file/entities/file.entity';
import { LoggerModule } from '@/common/logger.module';
import { MangaProcessingModule } from '@/processing/manga-processing.module';

@Module({
  imports: [
    TypeOrmModule.forFeature([File]),
    LoggerModule,
    MangaProcessingModule,
  ],
  controllers: [UploadsController],
  providers: [UploadsService],
  exports: [UploadsService],
})
export class UploadsModule {}