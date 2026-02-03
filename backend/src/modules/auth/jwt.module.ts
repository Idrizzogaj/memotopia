import { DynamicModule } from '@nestjs/common';
import { JwtModule } from '@nestjs/jwt';

import { AppConfig, APP_CONFIG } from '~app.config';

export function jwtModuleFactory(): DynamicModule {
    return JwtModule.registerAsync({
        useFactory: (appConfig: AppConfig) => ({
            secretOrPrivateKey: appConfig.jwt.secret,
            signOptions: {
                expiresIn: appConfig.jwt.expiresIn,
            },
        }),
        inject: [APP_CONFIG],
    });
}
