import {
    HttpStatus,
    InternalExceptionAbstract,
} from '~common/exceptions/internal.exception.abstract';

export class InstanceExistsException extends InternalExceptionAbstract {
    constructor(detail?: string, code?: string) {
        super({
            detail,
            code: code || 'instance-exists',
            httpStatus: HttpStatus.BAD_REQUEST,
            title: 'Instance exists',
        });
    }
}
