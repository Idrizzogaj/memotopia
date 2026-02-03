import {
    HttpStatus,
    InternalExceptionAbstract,
} from '~common/exceptions/internal.exception.abstract';

export class ForbiddenException extends InternalExceptionAbstract {
    constructor(detail?: string, code?: string) {
        super({
            detail,
            code: code || 'forbidden',
            httpStatus: HttpStatus.FORBIDDEN,
            title: 'Forbidden',
        });
    }
}
