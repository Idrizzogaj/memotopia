import { Severity } from '@sentry/types';

/**
 * An event that happened while processing
 */
export interface EventLogInterface {
    /**
     * Human readable message of this event
     */
    message: string;

    /**
     * Machine readable equivalent of the message
     */
    code: string;

    /**
     * Severity level of the message
     *  - used by sentry
     */
    severity: Severity;

    /**
     * The Service/Controller/Module function that produced this log
     */
    resource?: string;

    /**
     * Extra data usefull for debugging
     */
    extra?: Record<string, unknown>;

    /**
     * The request that this event happened at
     */
    requestId?: string;

    /**
     * Sentry Report Id
     */
    reportId?: string;
}

/**
 * A log of a express request
 */
export interface RequestLogInterface {
    /**
     * Google user id
     */
    userId?: string;

    /**
     * IPv4 address of the remote request
     */
    remoteIp: string;

    /**
     * Request id
     *  - used in queries and in sentry for debugging
     */
    requestId: string;

    /**
     * The endpoint used
     * - grouping by resource for analytics
     */
    requestRoute: string;

    /**
     * The requested resource
     * - useful for auditing read access
     */
    requestUrl: string;

    /**
     * HTTP response code
     */
    responseCode: number;

    /**
     * Time it took to handle this request in milliseconds
     */
    responseTime?: number;
}

export interface StatusResponse {
    /**
     * Message / Human Readable Status
     */
    title: string;

    /**
     * Machine readable equivalent of 'title'
     */
    code: string;

    /**
     * Human readable explanation of this response
     */
    detail?: string;

    /**
     * 'code' translated into a HTTP status code
     */
    httpStatus?: string;

    /**
     * Sentry Report Id
     */
    reportId?: string;

    /**
     * Express request Id
     */
    requestId?: string;

    /**
     * Free-form data returned by logic
     */
    meta?: any;
}
