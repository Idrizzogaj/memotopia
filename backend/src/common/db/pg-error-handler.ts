import { ConflictException, InternalServerException } from '~common/exceptions';

import { PgError } from './pg-error.interface';
import * as pg from './pg-error-codes';

/**
 * Throws on PG error, throwing appropriate HTTP exception for different,
 * specific Postgres error codes, by default it just forwards the error
 */
export function throwOnDbError(e: PgError): void {
    /** TODO: Log errers to sentry */
    switch (e.code) {
        case pg.PG_UNIQUE_VIOLATION:
            throw new ConflictException(e.detail, 'db_conflict');
            break;
        default:
            throw new InternalServerException();
    }
}
