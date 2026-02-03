module.exports = {
    parser: '@typescript-eslint/parser',
    parserOptions: {
        project: 'tsconfig.json',
        sourceType: 'module',
    },
    plugins: ['@typescript-eslint/eslint-plugin', 'import'],
    extends: [
        'plugin:@typescript-eslint/eslint-recommended',
        'plugin:@typescript-eslint/recommended',
        'prettier',
        'prettier/@typescript-eslint',
        'plugin:import/typescript',
    ],
    root: true,
    env: {
        node: true,
        jest: true,
    },
    rules: {
        '@typescript-eslint/interface-name-prefix': 'off',
        '@typescript-eslint/explicit-function-return-type': 'off',
        '@typescript-eslint/no-explicit-any': 'off',
        '@typescript-eslint/no-inferrable-types': 'off',
        '@typescript-eslint/camelcase': 'off',
        'no-console': 1,
        'import/order': [
            'error',
            {
                alphabetize: {
                    order: 'ignore',
                },
                'newlines-between': 'always',
                groups: ['builtin', 'external', 'internal', 'parent', 'sibling', 'index'],
                pathGroups: [
                    {
                        pattern: '~**/**',
                        group: 'internal',
                    },
                    {
                        pattern: '~**',
                        group: 'internal',
                    },
                ],
            },
        ],
        quotes: ['warn', 'single'],
        '@typescript-eslint/ban-ts-ignore': 0,
        '@typescript-eslint/no-use-before-define': 0,
        '@typescript-eslint/no-empty-interface': 0,

        'no-unused-vars': 'off',
        '@typescript-eslint/no-unused-vars': [
            'warn',
            {
                vars: 'all',
                args: 'after-used',
                ignoreRestSiblings: false,
            },
        ],
    },
    overrides: [
        {
            files: ['*.(unit|spec|e2e).ts'],
            rules: {
                'no-console': 0,
            },
        },
    ],
};
