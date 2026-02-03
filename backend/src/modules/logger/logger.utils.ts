import { Severity } from '@sentry/types';
import { getClientIp } from 'request-ip';

import { AuthedRequest } from '~common/auth';
import { InternalExceptionAbstract } from '~common/exceptions';
import { EventLogInterface, RequestLogInterface } from '~modules/logger/logger.interfaces';
import { sendSentryEvent, sendSentryException } from '~modules/logger/sentry.utils';

/**
 * List of environments where we use nice print
 * - CloudWatch likes plain JSON
 */
export const NICE_PRINT = ['local', 'test'];

/**
 * Log events to CloudWatch and Sentry
 * @param message human readable message
 * @param extra extra context variables usable in debugging
 * @param [context]
 * @param [context.exception] exception that prompted this log
 * @param [context.request] express request object
 * @param [context.resource] a custom resource string
 * @param [context.severity]
 */
export function logEvent(
    message: string,
    extra: Record<string, unknown>,
    context: {
        exception?: Error;
        request?: AuthedRequest;
        resource?: string;
        severity: Severity;
    },
): void {
    const eventLog: EventLogInterface = {
        message,
        code:
            (context?.exception as InternalExceptionAbstract)?.code ||
            context?.exception?.name ||
            context.severity,
        severity: context?.severity,
        resource: context?.resource,
        extra,
        requestId: (context?.request?.requestId || extra?.requestId) as string,
    };

    if (context.exception) {
        eventLog.extra.exception = {
            stack: context.exception.stack,
            name: context.exception.name,
            message: context.exception.message,
        };
    }

    // log under all conditions
    printEventLog(eventLog);

    if (
        !context?.severity ||
        [Severity.Error, Severity.Critical, Severity.Fatal, Severity.Warning].includes(
            context.severity,
        )
    ) {
        // report to sentry and wait for reportId
        const promise = context?.exception
            ? sendSentryException(context.exception, context)
            : sendSentryEvent(
                  {
                      message,
                      extra,
                  },
                  context,
              );

        promise.catch(e => {
            printEventLog({
                message: 'Error wile logging to Sentry',
                code: 'sentry-error',
                severity: Severity.Critical,
                requestId: eventLog.requestId,
                extra: {
                    exception: {
                        stack: e.stack,
                        name: e.name,
                        message: e.message,
                    },
                },
            });
            printEventLog({ ...eventLog });
        });
    }
}

export const printEventLog = !!NICE_PRINT.includes(process.env.API_ENVIRONMENT)
    ? ({ message, code, severity, resource, extra, requestId }: EventLogInterface): void => {
          const niceMessage = [
              Severity.Error,
              Severity.Critical,
              Severity.Fatal,
              Severity.Warning,
          ].includes(severity)
              ? `\x1B[31m${message}\x1B[0m`
              : `\x1B[32m${message}\x1B[0m`;

          const niceExtra =
              extra && Object.keys(extra).length > 0 ? '\n' + JSON.stringify(extra, null, 2) : '';

          // this log is formatted in a way useful for development - not logging
          // eslint-disable-next-line no-console
          console.log(
              `${new Date().toISOString()} ${resource ||
                  '-'} ${code} "${niceMessage}" ${requestId || '-'}${niceExtra}`,
          );
      }
    : // eslint-disable-next-line no-console
      (data: EventLogInterface): void => console.log(JSON.stringify(data));

export function requestToRequestLogInterface(request: AuthedRequest): RequestLogInterface {
    const end = process.hrtime(request.requestStart);

    const data: RequestLogInterface = {
        remoteIp: getClientIp(request),
        requestId: request.requestId,
        requestRoute: `${request.method}:${request.route?.path}`,
        requestUrl: `${request.method}:${request.url}`,
        responseCode: request.res?.statusCode,
    };

    if (request.requestStart) {
        data.responseTime = (end[0] * 1e9 + end[1]) / 1000000;
    }

    // logged in users
    if (request.userToken) {
        data.userId = request.userToken.userId;
    }

    return data;
}

export const printRequestLog = !!NICE_PRINT.includes(process.env.API_ENVIRONMENT)
    ? ({
          responseCode,
          requestId,
          requestUrl,
          remoteIp,
          // requestRoute, used for analytics
          responseTime,
          userId,
      }: RequestLogInterface): void => {
          const niceResponseCode =
              responseCode > 201
                  ? `\x1B[31m${responseCode || '-'}\x1B[0m`
                  : `\x1B[32m${responseCode || '-'}\x1B[0m`;

          // this log is formatted in a way useful for development - not logging
          // eslint-disable-next-line no-console
          console.log(
              `${new Date().toISOString()} ${niceResponseCode} ${requestUrl} ${remoteIp} ${userId ||
                  '-'} ${requestId || '-'} ${responseTime ? `${responseTime}ms` : '-'}`,
          );
      }
    : // eslint-disable-next-line no-console
      (data: RequestLogInterface): void => console.log(JSON.stringify(data));
