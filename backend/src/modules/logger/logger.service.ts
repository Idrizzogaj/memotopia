import { LoggerService as NestLoggerService } from '@nestjs/common';
import { Severity } from '@sentry/types';

import { logEvent } from '~modules/logger/logger.utils';

/* eslint-disable no-console */

const ignoredContexts = ['InstanceLoader', 'RoutesResolver', 'RouterExplorer'];

export class LoggerService implements NestLoggerService {
    private readonly context: string;
    constructor(context?: string) {
        this.context = context;
    }

    log(message: any, context?: string) {
        // do not log module loading
        if (ignoredContexts.includes(context || this.context)) {
            return;
        }
        logEvent(message, {}, { severity: Severity.Log, resource: context || this.context });
    }
    error(message: any, trace?: string, context?: string) {
        logEvent(
            message,
            { trace },
            { severity: Severity.Error, resource: context || this.context },
        );
    }
    warn(message: any, context?: string) {
        logEvent(message, {}, { severity: Severity.Warning, resource: context || this.context });
    }
    debug?(message: any, context?: string) {
        logEvent(message, {}, { severity: Severity.Debug, resource: context || this.context });
    }
    verbose?(message: any, context?: string) {
        logEvent(message, {}, { severity: Severity.Info, resource: context || this.context });
    }
}
