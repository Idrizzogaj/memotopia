/**
 * Global Config
 * - module configs should be placed inside their modules
 */
import { registerAs } from '@nestjs/config';
import { plainToClass, Type, Transform } from 'class-transformer';
import { IsNumber, IsString, ValidateNested, IsOptional, IsInt, Allow } from 'class-validator';
import { InternalServerErrorException } from '@nestjs/common';

import { validateFieldsSyncOrThrow } from '~common/validation';

import version from './common/config/version';

class ServerConfig {
    /**
     * git tag
     */
    @IsString()
    version: string;

    /**
     * hash of git commit
     */
    @IsString()
    release: string;

    /**
     * Isotime when the build was made
     */
    @IsString()
    buildTime: string;

    /**
     * Port on which to serve
     */
    @IsNumber()
    port: number;

    /**
     * Isotime when the build was made
     */
    @IsString()
    cors_url: string;
}

class PostgresConfig {
    @IsString()
    @IsOptional()
    user: string;

    @IsString()
    @IsOptional()
    password: string;

    @IsString()
    @IsOptional()
    database: string;

    @IsString()
    @IsOptional()
    host: string;

    @IsOptional()
    @IsInt()
    @Transform(v => parseInt(v, 10), { toClassOnly: true })
    port: number;

    @Allow()
    ssl: any;
}

class JwtConfig {
    @IsString()
    secret: string;

    @IsString()
    expiresIn: string;

    @IsString()
    refreshTokenExpiresIn: string;
}

export class AppConfig {
    @IsString()
    environment: string;

    @ValidateNested()
    @Type(() => ServerConfig)
    server: ServerConfig;

    @ValidateNested()
    @Type(() => PostgresConfig)
    db: PostgresConfig;

    @ValidateNested()
    @Type(() => JwtConfig)
    jwt: JwtConfig;
}

export const appConfigFactory = registerAs('app', () => {
    const env = process.env;
    const apiVersion = env.API_VERSION ? env.API_VERSION : 'unknown';

    const config = plainToClass(AppConfig, {
        environment: env.API_ENVIRONMENT,
        server: {
            port: env.PORT ? parseInt(env.PORT, 10) : undefined,
            version: apiVersion,
            release: version.release,
            buildTime: version.buildTime,
            cors_url: env.CORS_URL,
        },
        db: {
            user: env.POSTGRES_USER,
            password: env.POSTGRES_PASSWORD,
            database: env.POSTGRES_DB,
            host: env.POSTGRES_HOST,
            port: parseInt(env.POSTGRES_PORT, 10),
            ssl: env.POSTGRES_SSL,
        },
        jwt: {
            secret: env.JWT_SECRET,
            expiresIn: env.JWT_EXPIRES_IN,
            refreshTokenExpiresIn: env.JWT_REFRESH_TOKEN_EXPIRES_IN,
        },
    } as AppConfig);

    validateFieldsSyncOrThrow(config, { whitelist: true, forbidNonWhitelisted: true });

    if (config.environment === 'production') {
        // extra production checks for safety
        if (!config.server.cors_url) {
            throw new InternalServerErrorException('CORS Url must be set in production');
        }
    }
    return config;
});

export const APP_CONFIG = appConfigFactory.KEY;
