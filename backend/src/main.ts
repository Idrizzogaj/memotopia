import { join } from 'path';

import { NestExpressApplication } from '@nestjs/platform-express';
import { NestFactory } from '@nestjs/core';
import { ValidationPipe } from '@nestjs/common';
import { Severity } from '@sentry/node';
import { DocumentBuilder, SwaggerModule } from '@nestjs/swagger';

import { APP_CONFIG, AppConfig } from '~app.config';
import { usage } from '~utils/usage';
import { LoggerService } from '~modules/logger/logger.service';
import { logEvent } from '~modules/logger/logger.utils';
import { LoggingInterceptor } from '~modules/logger/logging.interceptor';
import { HttpExceptionFilter } from '~common/exceptions/exception.filter';

import { AppModule } from './app.module';

async function bootstrap() {
    const app = await NestFactory.create<NestExpressApplication>(AppModule, {
        logger: new LoggerService(),
    });

    const appConfig = app.get<AppConfig>(APP_CONFIG);

    app.enableCors({
        maxAge: 3600,
        credentials: true,
        origin: appConfig.server.cors_url
            ? appConfig.server.cors_url
            : (origin, callback) => {
                  callback(null, true);
              },
    });

    // TODO: evaluate if we will use morgan across the board,
    // TODO: move this segment into the logger module
    if (appConfig.environment === 'local') {
        // if in local environment, log requests to console using morgan
        let morgan: any;
        try {
            // eslint-disable-next-line @typescript-eslint/no-var-requires
            morgan = require('morgan');
        } catch (e) {
            // eslint-disable-next-line no-console
            console.info('Dev dependencies not installed - morgan logger require failed.', e);
        } finally {
            // if morgan was require'd, inject it into express
            if (morgan) {
                app.use(morgan('tiny'));
            }
        }
    }

    // Use static assets
    app.useStaticAssets(join(__dirname, '.', 'public'));

    // log requests
    app.useGlobalInterceptors(new LoggingInterceptor());

    // send exceptions as formatted  error messages
    app.useGlobalFilters(new HttpExceptionFilter());

    // TODO: Replace with pipe that uses validation exceptions
    app.useGlobalPipes(new ValidationPipe());

    // log memory usage
    setInterval(
        () => logEvent('Memory Usage', usage(), { severity: Severity.Info }),
        1000 * 60 * 15,
    );

    const options = new DocumentBuilder()
        .setTitle('Mamotopia Backend API')
        .addBearerAuth({ type: 'http', scheme: 'bearer', bearerFormat: 'JWT' }, 'access-token')
        .build();

    if (process.env.NODE_ENV != 'production') {
        const document = SwaggerModule.createDocument(app, options);
        SwaggerModule.setup('api', app, document);
    }

    const host = '0.0.0.0';
    await app.listen(appConfig.server.port, host);
    logEvent(
        `Application started @ ${host}:${appConfig.server.port}`,
        {},
        { severity: Severity.Info },
    );
}

bootstrap();
