export default {
  extends: ['stylelint-config-standard-scss'],
  ignoreFiles: ['dist/**/*', '.angular/**/*'],
  rules: {
    'no-empty-source': null,
    'selector-pseudo-element-no-unknown': [
      true,
      {
        ignorePseudoElements: ['ng-deep'],
      },
    ],
    'selector-class-pattern': [
      '^[a-z][a-z0-9-]*$',
      {
        ignoreSelectors: ['^mdc-'],
      },
    ],
  },
};
