import type { InitialOptionsTsJest } from 'ts-jest';

export const baseConfig: InitialOptionsTsJest = {
	globals: {
		'ts-jest': {
			tsconfig: "tsconfig.jest.json"
		},
	},
	moduleFileExtensions: [
		"ts",
		"js",
		"json"
	],
	modulePaths: [
		"<rootDir>/src",
		"node_modules"
	],
	moduleDirectories: ["node_modules"],
	transform: {
		"^.+\\.(css|less|sass|scss|styl|jpg|jpeg|png|gif|eot|otf|webp|svg|ttf|woff|woff2|mp4|webm|wav|mp3|m4a|aac|oga)$": "jest-transform-stub",
		"^.+\\.ts$": "ts-jest",
		"^.+\\.js$": "babel-jest"
	},
	transformIgnorePatterns: [
		"/node_modules/(?!client-controls)"
	],
	testRegex: "\\.spec\\.(ts|js)$",
	preset: "ts-jest/presets/js-with-ts",
	setupFiles: [
		"<rootDir>/test/jest-setup.ts"
	],
	testTimeout: 100000,
	testEnvironment: "node",
	testRunner: 'jest-circus',
	collectCoverage: true,
	collectCoverageFrom: [
		"src/**/*.{js,ts}",
		"!**/*.spec.{js,ts}",
		"!**/node_modules/**",
		"!**/test/**"
	],
	coverageDirectory: "<rootDir>/test/coverage-jest",
	coverageReporters: [
		"json",
		"lcov",
		"text",
		"html"
	]
};
