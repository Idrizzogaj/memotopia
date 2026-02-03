import {
    HttpStatus,
    InternalExceptionAbstract,
} from '~common/exceptions/internal.exception.abstract';

export class GoneException extends InternalExceptionAbstract {
    constructor(detail?: string, code?: string) {
        super({
            detail,
            httpStatus: HttpStatus.GONE,
            code: code || 'gone',
            title: 'Gone',
        });
    }
}
