import { MailerService } from '@nestjs-modules/mailer';
import { Injectable } from '@nestjs/common';

import { User } from '~common/db/entities/user.entity';

@Injectable()
export class MailService {
    constructor(private mailerService: MailerService) {}

    async forgotPassword(user: User, host: string, resetPasswordToken: string): Promise<void> {
        const resetPasswordUrl = `http://${host}/users/reset/${resetPasswordToken}`;

        await this.mailerService.sendMail({
            to: user.email,
            // from: '"Support Team" <support@example.com>', // override default from
            subject: 'Memotopia password reset',
            template: './forgot-password', // `.hbs` extension is appended automatically
            context: {
                resetPasswordUrl,
            },
        });
    }
}
