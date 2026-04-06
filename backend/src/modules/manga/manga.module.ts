import { Module } from '@nestjs/common';
import { SeriesController } from './controllers/series.controller';
import { ChaptersController } from './controllers/chapters.controller';
import { SeriesService } from './services/series.service';
import { ChaptersService } from './services/chapters.service';

@Module({
  controllers: [SeriesController, ChaptersController],
  providers: [SeriesService, ChaptersService],
  exports: [SeriesService, ChaptersService],
})
export class MangaModule {}
