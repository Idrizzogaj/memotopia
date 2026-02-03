import { Request } from 'express';

import { GoogleTokenInterface } from '~common/google/google.interfaces';

/**
 * Types the Express Request with the custom patient and backend user tokens
 */
export interface AuthedRequest extends Request {
    userToken?: GoogleTokenInterface;
    requestId: string;
    requestStart: [number, number];
}
