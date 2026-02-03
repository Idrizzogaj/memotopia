import { ArgumentsHost, Catch, HttpServer } from '@nestjs/common';
import { BaseExceptionFilter } from '@nestjs/core';

import { AuthedRequest } from '~common/auth';
import { exceptionHandler } from '~modules/logger/exception.handler';

/**
 * Catch all uncaught exceptions
 */
@Catch()
export class ExceptionFilter extends BaseExceptionFilter {
    constructor(appRef: HttpServer) {
        super(appRef);
    }
    // eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
    async catch(exception: unknown, host: ArgumentsHost) {
        const ctx = host.switchToHttp();
        const req = ctx.getRequest<AuthedRequest>();
        const res = ctx.getResponse();
        await exceptionHandler(req, res, exception);
    }
}
