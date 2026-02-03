import { Injectable, OnModuleInit } from '@nestjs/common';
import { ExtraErrorData, RewriteFrames, Transaction } from '@sentry/integrations';
import * as Sentry from '@sentry/node';

@Injectable()
export class SentryModule implements OnModuleInit {
    // eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
    onModuleInit() {
        Sentry.init({
            debug: false,
            dsn: process.env.SENTRY_DSN,
            release: process.env.API_ENVIRONMENT || 'unknown',
            environment: process.env.API_ENVIRONMENT,
            integrations: [
                new ExtraErrorData(),
                new RewriteFrames({
                    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
                    // @ts-ignore
                    root: globalThis.__rootdir__,
                }),
                new Transaction(),
            ],
        });
        // eslint-disable-next-line no-console
        console.log('The module has been initialized.');
    }
}
