import { INestApplication } from '@nestjs/common';

import { AppModule } from '~app.module';
import { createTestingModule } from '~common/test/mock';
import { LoggingInterceptor } from '~modules/logger/logging.interceptor';

export async function createTestingApp(): Promise<INestApplication> {
    try {
        const app = (await createTestingModule([AppModule])).createNestApplication();

        app.useGlobalInterceptors(new LoggingInterceptor());

        await app.init();

        return app;
    } catch (error) {
        // eslint-disable-next-line no-console
        console.error(error);
        throw error;
    }
}
