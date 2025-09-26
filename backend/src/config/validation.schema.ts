import * as Joi from 'joi';

export const validationSchema = Joi.object({
  // Puerto del servidor
  API_PORT: Joi.number().default(3001),

  // Ambiente
  NODE_ENV: Joi.string()
    .valid('development', 'production', 'test')
    .default('development'),

  // Configuración JWT
  JWT_SECRET: Joi.string().required(),
  JWT_EXPIRES_IN: Joi.string().default('1h'),

  // Configuración CORS
  CORS_ORIGIN: Joi.string().default('http://localhost:3000'),

  // Configuración rate limiting
  RATE_LIMIT_WINDOW_MS: Joi.number().default(900000), // 15 minutos
  RATE_LIMIT_MAX_REQUESTS: Joi.number().default(100),

  // Configuración logging
  LOG_LEVEL: Joi.string()
    .valid('error', 'warn', 'info', 'debug')
    .default('info'),

  // Configuración HTTPS
  HTTPS_ENABLED: Joi.boolean().default(false),
  HTTPS_KEY_PATH: Joi.when('HTTPS_ENABLED', {
    is: true,
    then: Joi.string().required(),
    otherwise: Joi.string().optional(),
  }),
  HTTPS_CERT_PATH: Joi.when('HTTPS_ENABLED', {
    is: true,
    then: Joi.string().required(),
    otherwise: Joi.string().optional(),
  }),

  // Configuración de base de datos (requerida)
  DATABASE_URL: Joi.string().required(),

  // Configuración de archivos
  UPLOAD_DESTINATION: Joi.string().default('./storage/uploads'),
  MAX_FILE_SIZE: Joi.number().default(104857600), // 100MB

  // Configuración de PostgreSQL desde docker-compose
  POSTGRES_DB: Joi.string().default('mangalith_dev'),
  POSTGRES_USER: Joi.string().default('mangalith_user'),
  POSTGRES_PASSWORD: Joi.string().default('changeme123'),
});
