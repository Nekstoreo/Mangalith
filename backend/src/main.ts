import { NestFactory } from '@nestjs/core';
import { ValidationPipe } from '@nestjs/common';
import { AppModule } from '@/app.module';
import { WinstonLoggerService } from '@/common/logger.service';
import { AllExceptionsFilter } from '@/common/filters';
import { SecurityMiddleware, RateLimitMiddleware } from '@/common/middleware';
import configuration from '@/config/configuration';

async function bootstrap() {
  // Create config first
  const config = configuration();

  // Set custom logger
  const logger = new WinstonLoggerService(config);

  const app = await NestFactory.create(AppModule, {
    logger,
  });

  // Global prefix for API routes
  app.setGlobalPrefix('api');

  // Global exception filter
  app.useGlobalFilters(new AllExceptionsFilter());

  // Global validation pipe
  app.useGlobalPipes(
    new ValidationPipe({
      whitelist: true,
      forbidNonWhitelisted: true,
      transform: true,
      disableErrorMessages: config.nodeEnv === 'production',
    }),
  );

  // Security middleware
  app.use(new SecurityMiddleware().use.bind(new SecurityMiddleware()));

  // Rate limiting middleware
  const rateLimitMiddleware = new RateLimitMiddleware(config);
  app.use(rateLimitMiddleware.use.bind(rateLimitMiddleware));

  // CORS configuration
  app.enableCors({
    origin: config.cors.origin,
    credentials: true,
    methods: ['GET', 'POST', 'PUT', 'DELETE', 'PATCH', 'OPTIONS'],
    allowedHeaders: ['Content-Type', 'Authorization', 'X-Requested-With'],
  });

  // Body parsing middleware (automatically included by NestJS)
  // JSON parsing is enabled by default

  // HTTPS configuration (if enabled)
  if (config.https.enabled) {
    // HTTPS setup would go here - for now it's disabled
    logger.warn('HTTPS is enabled but not configured yet', 'Bootstrap');
  }

  await app.listen(config.port);
  logger.log(
    `Application is running on: http://localhost:${config.port}/api`,
    'Bootstrap',
  );
}

void bootstrap();
