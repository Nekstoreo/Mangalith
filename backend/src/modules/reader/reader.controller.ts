import { Controller, Get, Post, Body, Param, ParseIntPipe, UseGuards, Req } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiParam, ApiBearerAuth } from '@nestjs/swagger';
import { ReaderService } from './reader.service';
import { UpdateProgressDto } from './dto/reader.dto';
import { JwtAuthGuard } from '@/common/guards/jwt-auth.guard';
import { Request } from 'express';

@ApiTags('Reader')
@ApiBearerAuth()
@UseGuards(JwtAuthGuard)
@Controller('reader')
export class ReaderController {
  constructor(private readerService: ReaderService) {}

  @Get('progress')
  @ApiOperation({ summary: 'Get all reading progress for current user' })
  async getProgress(@Req() req: Request) {
    const userId = (req as any).user.userId;
    return this.readerService.getUserProgress(userId);
  }

  @Get('progress/series/:seriesId')
  @ApiOperation({ summary: 'Get progress for a specific series' })
  @ApiParam({ name: 'seriesId', type: 'number' })
  async getSeriesProgress(
    @Req() req: Request,
    @Param('seriesId', ParseIntPipe) seriesId: number,
  ) {
    const userId = (req as any).user.userId;
    return this.readerService.getSeriesProgress(userId, seriesId);
  }

  @Post('progress')
  @ApiOperation({ summary: 'Update reading progress' })
  async updateProgress(@Req() req: Request, @Body() dto: UpdateProgressDto) {
    const userId = (req as any).user.userId;
    return this.readerService.updateProgress(userId, dto);
  }
}