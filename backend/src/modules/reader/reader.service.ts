import { Injectable } from '@nestjs/common';
import { PrismaService } from '@/config/prisma/prisma.service';
import { UpdateProgressDto } from './dto/reader.dto';

@Injectable()
export class ReaderService {
  constructor(private prisma: PrismaService) {}

  async getUserProgress(userId: number) {
    return this.prisma.readingProgress.findMany({
      where: { userId },
      include: {
        chapter: {
          include: {
            series: {
              select: {
                id: true,
                title: true,
                coverImageUrl: true,
              },
            },
          },
        },
      },
      orderBy: {
        lastReadAt: 'desc',
      },
    });
  }

  async getSeriesProgress(userId: number, seriesId: number) {
    return this.prisma.readingProgress.findMany({
      where: {
        userId,
        chapter: {
          seriesId,
        },
      },
      include: {
        chapter: {
          select: {
            id: true,
            chapterNumber: true,
            title: true,
          },
        },
      },
      orderBy: {
        lastReadAt: 'desc',
      },
    });
  }

  async updateProgress(userId: number, dto: UpdateProgressDto) {
    return this.prisma.readingProgress.upsert({
      where: {
        userId_chapterId: {
          userId,
          chapterId: dto.chapterId,
        },
      },
      update: {
        pageNumber: dto.pageNumber,
        lastReadAt: new Date(),
      },
      create: {
        userId,
        chapterId: dto.chapterId,
        pageNumber: dto.pageNumber,
      },
    });
  }
}