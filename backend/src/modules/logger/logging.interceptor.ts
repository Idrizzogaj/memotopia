import { CallHandler, ExecutionContext, Injectable, NestInterceptor } from '@nestjs/common';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import { AuthedRequest } from '~common/auth';
import { makeId } from '~common/make-id';
import { printRequestLog, requestToRequestLogInterface } from '~modules/logger/logger.utils';

@Injectable()
export class LoggingInterceptor implements NestInterceptor {
    intercept(context: ExecutionContext, next: CallHandler): Observable<any> {
        const request: AuthedRequest = context.switchToHttp().getRequest();
        request.requestId = makeId();
        request.requestStart = process.hrtime();

        return next.handle().pipe(
            // this will not get called on errors, @see exceptionHandler
            tap(() => {
                printRequestLog(requestToRequestLogInterface(request));
            }),
        );
    }
}
