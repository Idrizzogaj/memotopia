import { HttpException, HttpStatus } from '@nestjs/common';
import { Severity } from '@sentry/types';
import { Response } from 'express';

import { AuthedRequest } from '~common/auth';
import { InternalExceptionAbstract } from '~common/exceptions';
import { StatusResponse } from '~modules/logger/logger.interfaces';
import {
    printEventLog,
    printRequestLog,
    requestToRequestLogInterface,
} from '~modules/logger/logger.utils';
import { sendSentryException } from '~modules/logger/sentry.utils';

/**
 * Handle exceptions
 *  - report to sentry if needed and return the reportId to the user
 *  - log to console and use json format if production
 *  - return a nice error
 */
export async function exceptionHandler(
    request: AuthedRequest,
    response: Response,
    e: unknown,
): Promise<void> {
    /**
     * Report errors to sentry
     * - todo, log only 500+ and 403 once we hit unmanageable traffic
     */
    let reportId: string;
    try {
        reportId = await sendSentryException(e, { request });
    } catch (e) {
        // log sentry error handling error
        //  bypass logEvent since it will make a loop
        printEventLog({
            message: 'Error wile logging to Sentry',
            code: 'sentry-error',
            severity: Severity.Critical,
            requestId: request.requestId,
            extra: {
                exception: {
                    stack: e.stack,
                    name: e.name,
                    message: e.message,
                },
            },
        });
    }

    let statusResponse: StatusResponse;
    if (e instanceof HttpException) {
        /**
         * Http Exceptions
         *  - happens before we reach business logic
         *  - log, but not if 401
         */
        const status = e.getStatus();
        if (status !== HttpStatus.UNAUTHORIZED) {
            printEventLog({
                code: e.name,
                severity: Severity.Info,
                message: e.message,
                requestId: request.requestId,
                reportId,
            });
        }
        if (!response.headersSent) {
            response.status(status);
        }
        statusResponse = {
            code: e.name,
            title: e.message,
            httpStatus: status.toString(),
            reportId,
            requestId: request.requestId,
        };
    } else if (e instanceof InternalExceptionAbstract) {
        /**
         * Internal logic errors
         */
        if (e.httpStatus !== HttpStatus.UNAUTHORIZED) {
            printEventLog({
                code: e.code,
                severity: e.severity,
                message: e.message,
                requestId: request.requestId,
                reportId,
                extra: {
                    detail: e.detail,
                    stringify: JSON.stringify(e),
                    meta: e.meta,
                    debug: e.debug,
                },
            });
        }
        if (!response.headersSent) {
            response.status(e.httpStatus);
        }
        statusResponse = { ...e.toStatusResponse(), reportId, requestId: request.requestId };
    } else {
        /**
         * Uncaught exceptions
         *  - something went wrong, log everything
         */
        printEventLog({
            code: 'uncaught-exception',
            severity: Severity.Error,
            message: 'General server error',
            requestId: request.requestId,
            reportId,
            extra: {
                exception: {
                    stack: (e as any)?.stack,
                    name: (e as any)?.name,
                    message: (e as any)?.message,
                },
            },
        });
        statusResponse = {
            code: 'uncaught-exception',
            title: 'General server error',
            httpStatus: '500',
            reportId,
            requestId: request.requestId,
        };
        response.status(500);
    }

    // log request since LoggingInterceptor does not handle it
    printRequestLog(requestToRequestLogInterface(request));

    if (response.headersSent) {
        response.end();
        return;
    }

    /**
     * Send error back to user
     */
    if (request.headers.accept && request.headers.accept === 'application/json') {
        response.json(statusResponse);
    } else {
        // return a nice user error
        if (statusResponse.reportId) {
            response.send(sentryErrorHTML(statusResponse));
        } else {
            response.send(defaultErrorHTML(statusResponse));
        }
    }
}

/**
 * Respond as a HTML template
 */
function sentryErrorHTML(statusResponse: StatusResponse) {
    const SENTRY_DSN = process.env.SENTRY_DSN;
    if (!SENTRY_DSN) {
        // sentry not configured
        return defaultErrorHTML(statusResponse);
    }

    const user = 'The user object is not yet implemented';

    return defaultErrorHTML(
        statusResponse,
        `
<script src="https://browser.sentry-cdn.com/5.10.2/bundle.min.js"
        integrity="sha384-ssBfXiBvlVC7bdA/VX03S88B5MwXQWdnpJRbUYFPgswlOBwETwTp6F3SMUNpo9M9"
        crossorigin="anonymous"></script>
<script>
  Sentry.init({
    dsn: "${SENTRY_DSN}"
  });
  }
  function showReportDialog() {
    Sentry.showReportDialog({
        eventId: "${statusResponse.reportId}",
        title: "Tell us what happened",
        subtitle: "Giving us more information might help us track down this bug faster.",
        user: ${user}
    });
  }
</script>
`,
        '<a>We were notified of this problem - <a onclick="showReportDialog()">click here</a> if you want to help us solve it.</p>',
    );
}

function defaultErrorHTML(
    statusResponse: StatusResponse,
    extraHead: string = '',
    extraBody: string = '',
) {
    // language=HTML
    return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>API - Error ${statusResponse.httpStatus}</title>
    ${extraHead}
</head>
<body>
<h1>Sorry, we could not load this page</h1>
<h2>Error: ${statusResponse.httpStatus}: ${statusResponse.title}</h2>
${extraBody}
</body>
</html>
  `;
}
