import { INestApplication } from '@nestjs/common';
import * as request from 'supertest';

import { createTestingApp } from '~common/test/bootstrap';
import { apiHeaders } from '~common/test/mock';

describe('Public E2E', () => {
    let app: INestApplication;

    beforeAll(async () => {
        app = await createTestingApp();
    });

    test('Root path should load successfully', async () => {
        const response = await request(app.getHttpServer())
            .get('/')
            .set(apiHeaders())
            .expect(200);

        expect(response.body.uptime).toBeDefined();
    });

    afterAll(async () => {
        await app.close();
    });
});
