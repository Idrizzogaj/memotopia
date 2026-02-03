import {
    HttpStatus,
    InternalExceptionAbstract,
} from '~common/exceptions/internal.exception.abstract';

export class ConflictException extends InternalExceptionAbstract {
    constructor(detail?: string, code?: string) {
        super({
            detail,
            code: code || 'conflict',
            httpStatus: HttpStatus.CONFLICT,
            title: 'Conflict',
        });
    }
}
