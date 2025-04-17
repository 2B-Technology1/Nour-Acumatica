import type { InitialOptionsTsJest } from 'ts-jest';
import { baseConfig } from '../jest.base.config';

const config: InitialOptionsTsJest = {
	...baseConfig,
	globals: {
		'ts-jest': {
			tsconfig: "tsconfig.jest.json"
		},
		HTML_MERGED: false
	},
	moduleNameMapper: {
		"^aurelia-binding$": "<rootDir>/node_modules/aurelia-binding",
		'^@App/(.*)$': '<rootDir>/src/$1',
		"^client-controls/(.*)$": "<rootDir>/node_modules/client-controls//$1",
	},
};

export default config;
