import { Injectable, NestMiddleware } from '@nestjs/common';
import { Request, Response, NextFunction } from 'express';
import rateLimit from 'express-rate-limit';
import type { AppConfig } from '../../config/configuration';

@Injectable()
export class RateLimitMiddleware implements NestMiddleware {
  private limiter: ReturnType<typeof rateLimit>;

  constructor(private config: AppConfig) {
    this.limiter = rateLimit({
      windowMs: config.rateLimit.windowMs,
      max: config.rateLimit.maxRequests,
      message: {
        statusCode: 429,
        error: 'Too Many Requests',
        message: `Too many requests from this IP, please try again after ${config.rateLimit.windowMs / 1000 / 60} minutes.`,
      },
      standardHeaders: true,
      legacyHeaders: false,
      // Skip rate limiting for health checks or other internal routes
      skip: (req: Request) => {
        return req.path === '/health' || req.path.startsWith('/api/docs');
      },
    });
  }

  use(req: Request, res: Response, next: NextFunction): void {
    this.limiter(req, res, next);
  }
}
