const { defaults: tsjPreset } = require('ts-jest/presets');

process.env.NODE_ENV = 'test';
process.env.API_ENVIRONMENT = process.env.API_ENVIRONMENT || 'test';

module.exports = {
    preset: 'ts-jest',

    verbose: true,
    moduleFileExtensions: ['js', 'json', 'ts'],
    rootDir: 'src',
    testRegex: '.unit.ts$',
    transform: {
        ...tsjPreset.transform,
    },
    coverageDirectory: '../coverage',
    testEnvironment: 'node',
    moduleNameMapper: {
        '~(.*)$': '<rootDir>/$1',
    },
    reporters: ['default', 'jest-junit'],
    setupFilesAfterEnv: ['../jest.setup.js'],

    globals: {
        'ts-jest': {
            isolatedModules: true,
        },
    },
};
