import { parallel, series } from 'gulp';
import * as path from 'path';
import * as fs from 'fs';
import { createNpmTask } from './gulp-tools';

const tasks: string[] = [];

const copyPackageJsons = (subfolder: string) => parallel(
	() => fs.promises.copyFile(path.resolve(subfolder, 'package.base.json'), path.resolve(subfolder, 'package.json')),
	() => fs.promises.copyFile(path.resolve(subfolder, 'package-lock.base.json'), path.resolve(subfolder, 'package-lock.json'))
);

export const installPackages = (subFolder: string) => {
	const resolvedPath = path.resolve(subFolder);
	createNpmTask(
		'i_controls',
		{ path: resolvedPath, testable: true },
		subFolder,
		['i', '../dist/packages/client-controls.tgz'],
		tasks);
	createNpmTask(
		'i_tools',
		{ path: resolvedPath, testable: true },
		subFolder,
		['i', '../dist/packages/transformers.tgz', '../dist/packages/build-tools.tgz', '--save-dev'],
		tasks);

	return series(
		copyPackageJsons(subFolder),
		`i_controls ${subFolder}`,
		`i_tools ${subFolder}`
	);
};

export const installPackagesDebug = (subFolder: string) => {
	const resolvedPath = path.resolve(subFolder);
	createNpmTask(
		'i_controls_d',
		{ path: resolvedPath, testable: true },
		subFolder,
		['i', '../../../../../../ClientSideApps/dist/client-controls', '--install-links'],
		tasks);
	createNpmTask(
		'i_tools_d',
		{ path: resolvedPath, testable: true },
		subFolder,
		[
			'i',
			'../../../../../../ClientSideApps/dist/build-tools',
			'../../../../../../ClientSideApps/dist/transformers',
			'--save-dev',
			'--install-links'],
		tasks);

	return series(
		copyPackageJsons(subFolder),
		`i_controls_d ${subFolder}`,
		`i_tools_d ${subFolder}`
	);
};
