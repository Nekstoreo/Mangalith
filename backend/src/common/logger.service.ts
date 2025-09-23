import { Injectable, LoggerService } from '@nestjs/common';
import * as winston from 'winston';
import type { AppConfig } from '../config/configuration';

@Injectable()
export class WinstonLoggerService implements LoggerService {
  private logger: winston.Logger;
  private config: AppConfig;

  constructor(config: AppConfig) {
    this.config = config;
    this.initializeLogger();
  }

  private initializeLogger() {
    const isProduction = this.config.nodeEnv === 'production';
    const logLevel = this.config.logging.level;

    const transports: winston.transport[] = [
      // Console transport for all environments
      new winston.transports.Console({
        level: logLevel,
        format: winston.format.combine(
          winston.format.timestamp(),
          winston.format.errors({ stack: true }),
          winston.format.colorize(),
          winston.format.printf(
            (
              info: winston.Logform.TransformableInfo & {
                context?: unknown;
                stack?: unknown;
              },
            ) => {
              const formatValue = (value: unknown): string => {
                if (value == null) {
                  return '';
                }
                if (value instanceof Error) {
                  return value.stack ?? value.message;
                }
                if (typeof value === 'string') {
                  return value;
                }
                if (typeof value === 'number' || typeof value === 'boolean') {
                  return value.toString();
                }

                if (typeof value === 'object') {
                  try {
                    return JSON.stringify(value);
                  } catch {
                    return '[unserializable]';
                  }
                }

                return '';
              };

              const { timestamp, level, message, context, stack } = info;

              const formattedTimestamp = formatValue(timestamp);
              const formattedContext = formatValue(context);
              const formattedStack = formatValue(stack);
              const formattedMessage =
                formatValue(message) || '(empty message)';

              const prefix = formattedTimestamp ? `${formattedTimestamp} ` : '';
              const ctx = formattedContext ? `[${formattedContext}] ` : '';
              const stackTrace = formattedStack ? `\n${formattedStack}` : '';

              return `${prefix}${level}: ${ctx}${formattedMessage}${stackTrace}`;
            },
          ),
        ),
      }),
    ];

    // File transport for production
    if (isProduction) {
      transports.push(
        new winston.transports.File({
          filename: 'logs/error.log',
          level: 'error',
          format: winston.format.combine(
            winston.format.timestamp(),
            winston.format.errors({ stack: true }),
            winston.format.json(),
          ),
        }),
        new winston.transports.File({
          filename: 'logs/combined.log',
          format: winston.format.combine(
            winston.format.timestamp(),
            winston.format.json(),
          ),
        }),
      );
    }

    this.logger = winston.createLogger({
      level: logLevel,
      transports,
      exitOnError: false,
    });
  }

  log(message: string, context?: string) {
    this.logger.info(message, { context });
  }

  error(message: string, trace?: string, context?: string) {
    this.logger.error(message, { context, stack: trace });
  }

  warn(message: string, context?: string) {
    this.logger.warn(message, { context });
  }

  debug(message: string, context?: string) {
    this.logger.debug(message, { context });
  }

  verbose(message: string, context?: string) {
    this.logger.verbose(message, { context });
  }
}
