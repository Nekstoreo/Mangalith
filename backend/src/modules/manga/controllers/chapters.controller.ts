import { Controller, Get, Post, Body, Param, Query, ParseIntPipe } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiParam } from '@nestjs/swagger';
import { ChaptersService } from '../services/chapters.service';
import { CreateChapterDto, ChapterQueryDto } from '../dto/chapter.dto';

@ApiTags('Chapters')
@Controller('chapters')
export class ChaptersController {
  constructor(private chaptersService: ChaptersService) {}

  @Get('series/:seriesId')
  @ApiOperation({ summary: 'Get chapters by series' })
  @ApiParam({ name: 'seriesId', type: 'number' })
  async findBySeries(
    @Param('seriesId', ParseIntPipe) seriesId: number,
    @Query() query: ChapterQueryDto,
  ) {
    return this.chaptersService.findBySeries(seriesId, query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get chapter by ID with pages' })
  @ApiParam({ name: 'id', type: 'number' })
  async findOne(@Param('id', ParseIntPipe) id: number) {
    return this.chaptersService.findOne(id);
  }

  @Post()
  @ApiOperation({ summary: 'Create new chapter' })
  async create(@Body() dto: CreateChapterDto) {
    return this.chaptersService.create(dto);
  }
}
