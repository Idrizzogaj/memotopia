import {
    HttpStatus,
    InternalExceptionAbstract,
} from '~common/exceptions/internal.exception.abstract';

export class CriticalException extends InternalExceptionAbstract {
    constructor(detail?: string, code?: string) {
        super({
            detail,
            code: code || 'critical-error',
            httpStatus: HttpStatus.INTERNAL_SERVER_ERROR,
            title: 'Critical error',
        });
    }
}
