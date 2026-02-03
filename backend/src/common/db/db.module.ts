import { DynamicModule } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';

import { AppConfig, APP_CONFIG } from '~app.config';

export function dbModuleFactory(): DynamicModule {
    return TypeOrmModule.forRootAsync({
        inject: [APP_CONFIG],
        useFactory: (appConfig: AppConfig) => ({
            type: 'postgres',
            host: appConfig.db.host,
            port: appConfig.db.port,
            username: appConfig.db.user,
            password: appConfig.db.password,
            database: appConfig.db.database,
            entities: [__dirname + '/**/*.entity{.ts,.js}'],
            synchronize: true,
        }),
    });
}
