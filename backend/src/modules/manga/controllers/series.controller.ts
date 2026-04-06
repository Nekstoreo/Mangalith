import { Controller, Get, Post, Patch, Delete, Body, Param, Query, ParseIntPipe } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiParam } from '@nestjs/swagger';
import { SeriesService } from '../services/series.service';
import { CreateSeriesDto, UpdateSeriesDto, SeriesQueryDto } from '../dto/series.dto';

@ApiTags('Series')
@Controller('series')
export class SeriesController {
  constructor(private seriesService: SeriesService) {}

  @Get()
  @ApiOperation({ summary: 'Get all series with pagination' })
  async findAll(@Query() query: SeriesQueryDto) {
    return this.seriesService.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get series by ID' })
  @ApiParam({ name: 'id', type: 'number' })
  async findOne(@Param('id', ParseIntPipe) id: number) {
    return this.seriesService.findOne(id);
  }

  @Post()
  @ApiOperation({ summary: 'Create new series' })
  async create(@Body() dto: CreateSeriesDto) {
    return this.seriesService.create(dto);
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update series' })
  @ApiParam({ name: 'id', type: 'number' })
  async update(
    @Param('id', ParseIntPipe) id: number,
    @Body() dto: UpdateSeriesDto,
  ) {
    return this.seriesService.update(id, dto);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete series' })
  @ApiParam({ name: 'id', type: 'number' })
  async remove(@Param('id', ParseIntPipe) id: number) {
    return this.seriesService.remove(id);
  }
}
