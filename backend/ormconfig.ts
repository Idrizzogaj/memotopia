import * as dotenv from 'dotenv';

const configPath = `.env.${process.env.STAGE || 'local'}`;
dotenv.config({ path: configPath });

// eslint-disable-next-line no-console
console.info(`Running on config: ${configPath}`);

const env = process.env;

const settings: Record<string, unknown> = {
    type: 'postgres',
    synchronize: false,
    logging: env.DB_LOGGING,
    logger: 'simple-console',
    database: env.POSTGRES_DB,
    host: env.POSTGRES_HOST,
    port: parseInt(env.POSTGRES_PORT, 10),
    username: env.POSTGRES_USER,
    password: env.POSTGRES_PASSWORD,
    entities: [__dirname + '/**/*.entity{.ts,.js}'],
    migrations: [__dirname + '/src/common/db/migrations/*{.js,.ts}'],
    cli: {
        entitiesDir: 'src/common/db/entities',
        migrationsDir: 'src/common/db/migrations',
    },
};

export = settings;
