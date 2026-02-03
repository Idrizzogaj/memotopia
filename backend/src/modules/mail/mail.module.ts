import { join } from 'path';

import { MailerModule } from '@nestjs-modules/mailer';
import { HandlebarsAdapter } from '@nestjs-modules/mailer/dist/adapters/handlebars.adapter';
import { Module } from '@nestjs/common';

import { MailService } from './mail.service';

@Module({
    imports: [
        MailerModule.forRoot({
            // transport: 'smtps://user@example.com:topsecret@smtp.example.com',
            // or
            transport: {
                host: 'send.one.com',
                secure: false,
                auth: {
                    user: 'rese-pw@memotopia.com', // TODO: add to .env
                    pass: 'Arber_135', // TODO: add to .env
                },
            },
            defaults: {
                from: '"No Reply" <rese-pw@memotopia.com>',
            },
            template: {
                dir: join(__dirname, '..', '..', '..', '..', 'templates'),
                adapter: new HandlebarsAdapter(), // or new PugAdapter() or new EjsAdapter()
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
