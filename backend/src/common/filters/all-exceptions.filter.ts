import {
  ExceptionFilter,
  Catch,
  ArgumentsHost,
  HttpException,
  HttpStatus,
  Logger,
} from '@nestjs/common';
import { Request, Response } from 'express';

export interface ErrorResponse {
  statusCode: number;
  message: string | string[];
  error: string;
  timestamp: string;
  path: string;
  method: string;
}

interface ExceptionResponseObject {
  message?: string | string[];
  error?: string;
}

@Catch()
export class AllExceptionsFilter implements ExceptionFilter {
  private readonly logger = new Logger(AllExceptionsFilter.name);

  catch(exception: unknown, host: ArgumentsHost): void {
    const ctx = host.switchToHttp();
    const response = ctx.getResponse<Response>();
    const request = ctx.getRequest<Request>();

    let status: HttpStatus;
    let message: string | string[];
    let error: string;

    if (exception instanceof HttpException) {
      status = exception.getStatus();
      const exceptionResponse = exception.getResponse();

      if (typeof exceptionResponse === 'string') {
        message = exceptionResponse;
        error = exception.constructor.name;
      } else if (
        typeof exceptionResponse === 'object' &&
        exceptionResponse !== null
      ) {
        const responseObj = exceptionResponse as ExceptionResponseObject;
        message = responseObj.message || exception.message;
        error = responseObj.error || exception.constructor.name;
      } else {
        message = exception.message;
        error = exception.constructor.name;
      }
    } else if (exception instanceof Error) {
      status = HttpStatus.INTERNAL_SERVER_ERROR;
      message = 'Internal server error';
      error = 'InternalServerError';

      // Log the actual error for debugging
      this.logger.error(
        `Unhandled exception: ${exception.message}`,
        exception.stack,
        'AllExceptionsFilter',
      );
    } else {
      status = HttpStatus.INTERNAL_SERVER_ERROR;
      message = 'Unknown error occurred';
      error = 'UnknownError';

      this.logger.error(
        `Unknown exception: ${JSON.stringify(exception)}`,
        undefined,
        'AllExceptionsFilter',
      );
    }

    const errorResponse: ErrorResponse = {
      statusCode: status,
      message,
      error,
      timestamp: new Date().toISOString(),
      path: request.url,
      method: request.method,
    };

    // Log error details for debugging (except in production for security)
    if (status >= HttpStatus.INTERNAL_SERVER_ERROR) {
      this.logger.error(
        `HTTP ${status} Error: ${JSON.stringify(errorResponse)}`,
        undefined,
        'AllExceptionsFilter',
      );
    }

    response.status(status).json(errorResponse);
  }
}
