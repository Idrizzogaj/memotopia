import {
    HttpStatus,
    InternalExceptionAbstract,
} from '~common/exceptions/internal.exception.abstract';

export class BadRequestException extends InternalExceptionAbstract {
    constructor(detail?: string, code?: string) {
        super({
            detail,
            code: code || 'bad-request',
            httpStatus: HttpStatus.BAD_REQUEST,
            title: 'Bad request',
        });
    }
}
