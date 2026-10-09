import { join } from 'path';

import { MailerModule } from '@nestjs-modules/mailer';
import { HandlebarsAdapter } from '@nestjs-modules/mailer/dist/adapters/handlebars.adapter';
import { Module } from '@nestjs/common';

import { MailService } from './mail.service';

// SMTP credentials come from the environment, never from source control.
const mailUser = process.env.MAIL_USER || '';
const mailFrom = process.env.MAIL_FROM || `"Memotopia" <${mailUser}>`;

@Module({
    imports: [
        MailerModule.forRoot({
            transport: {
                host: process.env.MAIL_HOST || 'send.one.com',
                port: parseInt(process.env.MAIL_PORT || '587', 10),
                secure: false,
                auth: {
                    user: mailUser,
                    pass: process.env.MAIL_PASS || '',
                },
            },
            defaults: {
                from: mailFrom,
            },
            template: {
                dir: join(__dirname, '..', '..', '..', '..', 'templates'),
                adapter: new HandlebarsAdapter(),
                options: {
                    strict: true,
                },
            },
        }),
    ],
    providers: [MailService],
    exports: [MailService],
})
export class MailModule {}
