// @ts-check
const eslint = require('@eslint/js');
const { defineConfig } = require('eslint/config');
const tseslint = require('typescript-eslint');
const angular = require('angular-eslint');

module.exports = defineConfig([
  {
    files: ['**/*.ts'],
    extends: [
      eslint.configs.recommended,
      tseslint.configs.recommended,
      tseslint.configs.stylistic,
      angular.configs.tsRecommended,
    ],
    processor: angular.processInlineTemplates,
    rules: {
      '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
      '@angular-eslint/directive-selector': [
        'error',
        {
          type: 'attribute',
          prefix: 'app',
          style: 'camelCase',
        },
      ],
      '@angular-eslint/component-selector': [
        'error',
        {
          type: 'element',
          prefix: 'app',
          style: 'kebab-case',
        },
      ],
    },
  },
  {
    files: ['src/app/shared/**/*.ts'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            {
              group: ['**/core/**', '**/features/**'],
              message: 'Warstwa shared nie może zależeć od core ani features.',
            },
          ],
        },
      ],
    },
  },
  {
    files: ['src/app/core/**/*.ts'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            { group: ['**/features/**'], message: 'Warstwa core nie może zależeć od features.' },
          ],
        },
      ],
    },
  },
  {
    files: ['src/app/features/dashboard/**/*.ts'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            {
              regex: '^(\.\./)+(inventory|management|storefront)/',
              message: 'Moduł dashboard nie może importować innych modułów funkcjonalnych.',
            },
          ],
        },
      ],
    },
  },
  {
    files: ['src/app/features/inventory/**/*.ts'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            {
              regex: '^(\.\./)+(dashboard|management|purchase-orders|shipments|storefront)/',
              message: 'Moduł inventory nie może importować innych modułów funkcjonalnych.',
            },
          ],
        },
      ],
    },
  },
  {
    files: ['src/app/features/management/**/*.ts'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            {
              regex: '^(\.\./)+(dashboard|inventory|purchase-orders|shipments|storefront)/',
              message: 'Moduł management nie może importować innych modułów funkcjonalnych.',
            },
          ],
        },
      ],
    },
  },
  {
    files: ['src/app/features/purchase-orders/**/*.ts'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            {
              regex: '^(\.\./)+(dashboard|inventory|management|shipments|storefront)/',
              message: 'Moduł purchase-orders nie może importować innych modułów funkcjonalnych.',
            },
          ],
        },
      ],
    },
  },
  {
    files: ['src/app/features/shipments/**/*.ts'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            {
              regex: '^(\.\./)+(dashboard|inventory|management|purchase-orders|storefront)/',
              message: 'Moduł shipments nie może importować innych modułów funkcjonalnych.',
            },
          ],
        },
      ],
    },
  },
  {
    files: ['src/app/features/storefront/**/*.ts'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            {
              regex: '^(\.\./)+(dashboard|inventory|management|purchase-orders|shipments)/',
              message: 'Moduł storefront nie może importować innych modułów funkcjonalnych.',
            },
          ],
        },
      ],
    },
  },
  {
    files: ['**/*.html'],
    extends: [angular.configs.templateRecommended, angular.configs.templateAccessibility],
    rules: {},
  },
]);
