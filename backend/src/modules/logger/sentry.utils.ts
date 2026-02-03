import { Scope } from '@sentry/hub';
import * as Sentry from '@sentry/minimal';
import { Event, Severity } from '@sentry/types';
import { getClientIp } from 'request-ip';

import { AuthedRequest } from '~common/auth';
import { InternalExceptionAbstract } from '~common/exceptions';

function applyRequest(scope: Scope, request: AuthedRequest): void {
    // user details
    const user: any = {};
    if (request.userToken) {
        user.username = request.userToken.username;
        user.id = request.userToken.userId;
    }
    if (request.headers) {
        user.ip_address = getClientIp(request);
        scope.setExtra('referer', request.headers.referer);
        scope.setExtra('user-agent', request.headers['user-agent']);
    }
    scope.setUser(user);

    if (request.requestId) {
        scope.setTag('requestId', request.requestId);
    }

    scope.setTag('requestRoute', `${request.method}:${request.route?.path}`);

    scope.setExtra('requestUrl', request.url);
}

/**
 * Report event to sentry with some nice context
 */
export async function sendSentryEvent(
    event: Event,
    context: { request?: AuthedRequest; resource?: string; severity?: Severity },
): Promise<string> {
    return new Promise(resolve => {
        Sentry.withScope(async scope => {
            if (context.request) {
                applyRequest(scope, context.request);
            }
            scope.setLevel(context.severity || Severity.Error);
            if (context.resource) {
                scope.setTag('resource', context.resource);
            }

            resolve(Sentry.captureEvent(event));
        });
    });
}

/**
 * Report exception to sentry with some nice context
 */
export async function sendSentryException(
    exception: unknown,
    context: { request?: AuthedRequest },
): Promise<string> {
    return new Promise(resolve => {
        Sentry.withScope(async scope => {
            if (context.request) {
                applyRequest(scope, context.request);
            }
            scope.setLevel(Severity.Error);

            if (exception instanceof InternalExceptionAbstract) {
                scope.setExtra('meta', exception.meta);
                scope.setExtra('detail', exception.detail);
                scope.setTag('httpStatus', exception.httpStatus.toString());
                scope.setTag('code', exception.code);
            }

            resolve(Sentry.captureException(exception));
        });
    });
}
