import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { PassportModule } from '@nestjs/passport';

import { AuthModule } from '~modules/auth/auth.module';
import { UserRepository } from '~modules/user/user.repository';
import { UserModule } from '~modules/user/user.module';

import { FavoritesService } from './favorites.service';
import { FavoritesController } from './favorites.controller';
import { FavoritesRepository } from './favorites.repository';

@Module({
    imports: [
        TypeOrmModule.forFeature([FavoritesRepository, UserRepository]),
        PassportModule.register({
            defaultStrategy: 'jwt',
        }),
        AuthModule,
        UserModule,
    ],
    providers: [FavoritesService],
    controllers: [FavoritesController],
})
export class FavoritesModule {}
