import { Injectable, Inject } from '@nestjs/common';

import { APP_CONFIG, AppConfig } from '~app.config';
import { HealthCheckDto } from '~health-check.dto';
import { secondsToDhms } from '~utils/duration';

@Injectable()
export class AppService {
    constructor(
        @Inject(APP_CONFIG)
        private appConfig: AppConfig,
    ) {}

    getHello(): string {
        return 'Hello World!';
    }

    async getHealthCheck(): Promise<HealthCheckDto> {
        const config = this.appConfig.server;
        return {
            uptime: secondsToDhms(process.uptime()),
            environment: this.appConfig.environment,
            release: config.release,
            version: config.version,
            buildTime: config.buildTime,
        };
    }
}
