import { ArgumentsHost, Catch, ExceptionFilter, HttpException } from '@nestjs/common';
import { Response } from 'express';

import { InternalExceptionAbstract } from './internal.exception.abstract';

@Catch(HttpException)
export class HttpExceptionFilter implements ExceptionFilter {
    catch(exception: InternalExceptionAbstract, host: ArgumentsHost): void {
        const status = exception.getStatus();
        let statusResponse = exception?.toStatusResponse?.();
        const response = host.switchToHttp().getResponse<Response>();

        // TODO: Logging or whatever else might be needed

        // TODO: Replace when validation pipe uses our validation exception type
        if (!statusResponse) {
            /** If the validation pipe caught the exception */
            const exceptionResponse = <any>(exception as HttpException).getResponse();
            statusResponse = {
                httpStatus: String(status),
                detail: exceptionResponse?.message.toString(),
                code: exception.code || undefined,
                title: exception.message || undefined,
            };
        }

        response.status(status).json(statusResponse);
    }
}
