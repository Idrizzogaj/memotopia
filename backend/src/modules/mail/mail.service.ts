import { MailerService } from '@nestjs-modules/mailer';
import { Injectable } from '@nestjs/common';

import { User } from '~common/db/entities/user.entity';

/**
 * Public HTTPS base used in links inside e-mails. Never derived from the incoming Host header,
 * which a caller could forge to point the link at another server.
 */
export function publicBaseUrl(): string {
    return (process.env.PUBLIC_BASE_URL || 'https://memotopia-eu-1.duckdns.org').replace(/\/+$/, '');
}

@Injectable()
export class MailService {
    constructor(private mailerService: MailerService) {}

    async forgotPassword(user: User, resetPasswordToken: string): Promise<void> {
        const resetPasswordUrl = `${publicBaseUrl()}/users/reset/${resetPasswordToken}`;

        await this.mailerService.sendMail({
            to: user.email,
            subject: 'Memotopia password reset',
            template: './forgot-password',
            context: {
                resetPasswordUrl,
            },
        });
    }

    async verifyEmail(user: User, verificationToken: string): Promise<void> {
        const verifyUrl = `${publicBaseUrl()}/users/verify-email/${verificationToken}`;

        await this.mailerService.sendMail({
            to: user.email,
            subject: 'Confirm your Memotopia account',
            template: './verify-email',
            context: {
                username: user.username,
                verifyUrl,
            },
        });
    }
}
