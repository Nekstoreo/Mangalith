import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { LoggerModule } from '@/common/logger.module';
import { ArchiveProcessorService } from './services/archive-processor.service';

@Module({
  imports: [ConfigModule, LoggerModule],
  providers: [ArchiveProcessorService],
  exports: [ArchiveProcessorService],
})
export class MangaProcessingModule {}


