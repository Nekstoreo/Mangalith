import { Injectable, NotFoundException } from '@nestjs/common';
import { PrismaService } from '@/config/prisma/prisma.service';
import { CreateSeriesDto, UpdateSeriesDto, SeriesQueryDto } from '../dto/series.dto';

@Injectable()
export class SeriesService {
  constructor(private prisma: PrismaService) {}

  async findAll(query: SeriesQueryDto) {
    const { page = 1, limit = 10, search, sortBy = 'createdAt', order = 'desc' } = query;
    const skip = (page - 1) * limit;

    const where = search ? {
      OR: [
        { title: { contains: search, mode: 'insensitive' as const } },
        { author: { contains: search, mode: 'insensitive' as const } },
      ],
    } : {};

    const [data, total] = await Promise.all([
      this.prisma.series.findMany({
        where,
        orderBy: { [sortBy]: order },
        skip,
        take: limit,
        include: {
          _count: {
            select: { chapters: true },
          },
        },
      }),
      this.prisma.series.count({ where }),
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
    const series = await this.prisma.series.findUnique({
      where: { id },
      include: {
        chapters: {
          orderBy: { chapterNumber: 'asc' },
          select: {
            id: true,
            chapterNumber: true,
            title: true,
            uploadDate: true,
            _count: {
              select: { pages: true },
            },
          },
        },
      },
    });

    if (!series) {
      throw new NotFoundException('Series not found');
    }

    return series;
  }

  async create(dto: CreateSeriesDto) {
    return this.prisma.series.create({
      data: dto,
    });
  }

  async update(id: number, dto: UpdateSeriesDto) {
    await this.findOne(id);
    
    return this.prisma.series.update({
      where: { id },
      data: dto,
    });
  }

  async remove(id: number) {
    await this.findOne(id);
    
    return this.prisma.series.delete({
      where: { id },
    });
  }
}
