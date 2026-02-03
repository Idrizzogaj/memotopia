import * as fs from 'fs';
import * as path from 'path';

import { ConfigModule } from '@nestjs/config';
import { DynamicModule } from '@nestjs/common';

import { appConfigFactory } from '../app.config';

/**
 * Configs are separated into .config.ts files
 *  but all base off the main ConfigModule that can choose
 *  the base file those configs load from
 *
 * This future-proofs any other config method we might need
 *  https://docs.nestjs.com/techniques/configuration#getting-started
 */
export function configModuleFactory(): DynamicModule {
    const envFilePath = `.env.${process.env.API_ENVIRONMENT}`;
    const envFileExits = fs.existsSync(path.resolve(__dirname, '..', '..', envFilePath));

    return ConfigModule.forRoot({
        load: [appConfigFactory],
        envFilePath: envFileExits ? envFilePath : undefined,
        isGlobal: true,
    });
}
