import * as jwt from 'jsonwebtoken';
import { JwtPayloadDto } from '~modules/auth/dto/jwt-payload.dto';

export const generateJwt = (payload: JwtPayloadDto): string => {
    return jwt.sign(payload, process.env.JWT_SECRET, {
        expiresIn: process.env.JWT_EXPIRES_IN,
    });
};
