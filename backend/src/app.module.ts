import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { AuthModule } from './modules/auth/auth.module';
import { MangaModule } from './modules/manga/manga.module';
import { ReaderModule } from './modules/reader/reader.module';
import { UploadsModule } from './modules/uploads/uploads.module';
import { PrismaModule } from './config/prisma/prisma.module';

@Module({
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
      envFilePath: ['.env', '.env.local'],
    }),
    PrismaModule,
    AuthModule,
    MangaModule,
    ReaderModule,
    UploadsModule,
  ],
})
export class AppModule {}
