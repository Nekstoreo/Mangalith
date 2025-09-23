import * as Joi from 'joi';

export const validationSchema = Joi.object({
  // Configuración del servidor
  PORT: Joi.number().default(3000),
  NODE_ENV: Joi.string()
    .valid('development', 'production', 'test')
    .default('development'),

  // Configuración de JWT
  JWT_SECRET: Joi.string().required(),
  JWT_EXPIRES_IN: Joi.string().default('1h'),

  // Configuración de CORS
  CORS_ORIGIN: Joi.string().default('http://localhost:3000'),

  // Configuración de rate limiting
  RATE_LIMIT_WINDOW_MS: Joi.number().default(900000), // 15 minutos
  RATE_LIMIT_MAX_REQUESTS: Joi.number().default(100),

  // Configuración de logging
  LOG_LEVEL: Joi.string()
    .valid('error', 'warn', 'info', 'debug')
    .default('info'),

  // Configuración de HTTPS
  HTTPS_ENABLED: Joi.boolean().default(false),
  HTTPS_KEY_PATH: Joi.string().allow('').optional(),
  HTTPS_CERT_PATH: Joi.string().allow('').optional(),

  // Configuración de base de datos (para futuras fases)
  DATABASE_URL: Joi.string().allow('').optional(),

  // Configuración de uploads (para futuras fases)
  UPLOAD_DESTINATION: Joi.string().allow('').optional(),
  MAX_FILE_SIZE: Joi.number().optional(),
});
