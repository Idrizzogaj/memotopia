import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { PassportModule } from '@nestjs/passport';

import { RestrictionsService } from './restrictions.service';
import { RestrictionsController } from './restrictions.controller';
import { RestrictionsRepository } from './restrictions.repository';

@Module({
    imports: [
        TypeOrmModule.forFeature([RestrictionsRepository]),
        PassportModule.register({
            defaultStrategy: 'jwt',
        }),
    ],
    providers: [RestrictionsService],
    controllers: [RestrictionsController],
    exports: [RestrictionsService],
})
export class RestrictionsModule {}
