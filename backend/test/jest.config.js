const { defaults: tsjPreset } = require('ts-jest/presets');

process.env.NODE_ENV = 'test';
process.env.API_ENVIRONMENT = process.env.API_ENVIRONMENT || 'test';

module.exports = {
  moduleFileExtensions: ['js', 'json', 'ts'],
  rootDir: '..',
  testEnvironment: 'node',
  testRegex: '.e2e.ts$',
  transform: {
    ...tsjPreset.transform
  },
  moduleNameMapper: {
    '~(.*)$': '<rootDir>/src/$1',
  },
  setupFilesAfterEnv: ['./jest.setup.js'],

  globals: {
    'ts-jest': {
      isolatedModules: true
    }
  }
};
