import { IsInt, Min } from 'class-validator';
import { ApiProperty } from '@nestjs/swagger';

export class UpdateProgressDto {
  @ApiProperty()
  @IsInt()
  chapterId: number;

  @ApiProperty()
  @IsInt()
  @Min(1)
  pageNumber: number;
}