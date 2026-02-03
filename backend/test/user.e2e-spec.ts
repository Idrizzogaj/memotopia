import { INestApplication } from '@nestjs/common';
import { Test, TestingModule } from '@nestjs/testing';
import * as request from 'supertest';

import { AppModule } from '~app.module';
import { UserProvider } from '~common/constants/enums/user-provider.enum';
import { User } from '~common/db/entities/user.entity';
import { UserRepository } from '~modules/user/user.repository';

import { generateJwt } from './utils/auth';

describe('Users Request Controller (e2e)', () => {
    let app: INestApplication;
    let user1: User;
    let user2: User;
    let user3: User;
    let user4: User;
    let user5: User;
    let user6: User;
    let userToken1: string;
    let userRepository: UserRepository;

    beforeAll(async () => {
        const moduleFixture: TestingModule = await Test.createTestingModule({
            imports: [AppModule],
        }).compile();

        app = moduleFixture.createNestApplication();
        await app.init();

        userRepository = await app.resolve(UserRepository);

        [user1, user2, user3, user4, user5, user6] = await userRepository.save([
            {
                provider: UserProvider.MEMOTOPIA,
                username: 'username1',
                email: 'email-1@gmail.com',
                password: 'Password123',
            },
            {
                provider: UserProvider.MEMOTOPIA,
                username: 'username2',
                email: 'email-2@gmail.com',
                password: 'Password123',
            },
            {
                provider: UserProvider.MEMOTOPIA,
                username: 'username3',
                email: 'email-3@gmail.com',
                password: 'Password123',
            },
            {
                provider: UserProvider.MEMOTOPIA,
                username: 'username4',
                email: 'email-4@gmail.com',
                password: 'Password123',
            },
            {
                provider: UserProvider.MEMOTOPIA,
                username: 'username5',
                email: 'email-5@gmail.com',
                password: 'Password123',
            },
            {
                provider: UserProvider.MEMOTOPIA,
                username: 'username6',
                email: 'email-6@gmail.com',
                password: 'Password123',
            },
        ]);

        userToken1 = `Bearer ${generateJwt({
            userId: user1.id,
            username: user1.username,
        })}`;
    });

    describe('GET /users/:id', () => {
        it('should return 200', done => {
            request(app.getHttpServer())
                .get(`/users/${user1.id}`)
                .set('Authorization', userToken1)
                .expect(200)
                .end(done);
        });

        it('should return 400 if an invalid id type is provided', done => {
            request(app.getHttpServer())
                .get('/users/aaa55')
                .set('Authorization', userToken1)
                .expect(400)
                .end(done);
        });

        it('should return 404 if the provided id does not exist', done => {
            request(app.getHttpServer())
                .get('/users/123')
                .set('Authorization', userToken1)
                .expect(404)
                .end(done);
        });
    });

    afterAll(async () => {
        await userRepository.delete({});
        await app.close();
    });
});
