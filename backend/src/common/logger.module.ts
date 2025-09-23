import { Module } from '@nestjs/common';
import { WinstonLoggerService } from './logger.service';
import { ConfigModule } from '@nestjs/config';
import configuration from '../config/configuration';

@Module({
  imports: [ConfigModule],
  providers: [
    {
      provide: WinstonLoggerService,
      useFactory: () => {
        const config = configuration();
        return new WinstonLoggerService(config);
      },
    },
  ],
  exports: [WinstonLoggerService],
})
export class LoggerModule {}
