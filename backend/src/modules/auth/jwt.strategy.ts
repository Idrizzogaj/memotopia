import { Injectable } from '@nestjs/common';
import { PassportStrategy } from '@nestjs/passport';
import { ExtractJwt, Strategy } from 'passport-jwt';

import { User } from '~common/db/entities/user.entity';
import { UnauthorizedException } from '~common/exceptions';
import { AuthService } from '~modules/auth/auth.service';

import { JwtPayloadDto } from './dto/jwt-payload.dto';

@Injectable()
export class JwtStrategy extends PassportStrategy(Strategy) {
    constructor(private readonly authService: AuthService) {
        super({
            jwtFromRequest: ExtractJwt.fromAuthHeaderAsBearerToken(),
            secretOrKey: process.env.JWT_SECRET,
        });
    }

    async validate(payload: JwtPayloadDto): Promise<User> {
        const user = await this.authService.validateUser(payload);

        if (!user) throw new UnauthorizedException();

        return user;
    }
}
