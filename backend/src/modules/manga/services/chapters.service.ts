import { Injectable, NotFoundException } from '@nestjs/common';
import { PrismaService } from '@/config/prisma/prisma.service';
import { CreateChapterDto, ChapterQueryDto } from '../dto/chapter.dto';

@Injectable()
export class ChaptersService {
  constructor(private prisma: PrismaService) {}

  async findBySeries(seriesId: number, query: ChapterQueryDto) {
    const { page = 1, limit = 20 } = query;
    const skip = (page - 1) * limit;

    const [data, total] = await Promise.all([
      this.prisma.chapter.findMany({
        where: { seriesId },
        orderBy: { chapterNumber: 'desc' },
        skip,
        take: limit,
        include: {
          _count: {
            select: { pages: true },
          },
        },
      }),
      this.prisma.chapter.count({ where: { seriesId } }),
    ]);

    return {
      data,
      meta: {
        total,
        page,
        limit,
        totalPages: Math.ceil(total / limit),
      },
    };
  }

  async findOne(id: number) {
    const chapter = await this.prisma.chapter.findUnique({
      where: { id },
      include: {
        series: {
          select: {
            id: true,
            title: true,
          },
        },
        pages: {
          orderBy: { pageNumber: 'asc' },
        },
      },
    });

    if (!chapter) {
      throw new NotFoundException('Chapter not found');
    }

    return chapter;
  }

  async create(dto: CreateChapterDto) {
    return this.prisma.chapter.create({
      data: dto,
      include: {
        series: {
          select: {
            id: true,
            title: true,
          },
        },
      },
    });
  }
}
