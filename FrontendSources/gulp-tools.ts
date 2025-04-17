import { dest, task, TaskFunction } from 'gulp';
import gulpif from 'gulp-if';
import { createProject } from 'gulp-typescript';
import * as tsc from 'typescript';
import { init as initSourceMaps, write as writeSourceMaps } from 'gulp-sourcemaps';
import * as path from 'path';
import * as fs from 'fs';
import del from 'del';
import { spawn } from 'child_process';

export const clean = (dirPath: string) => del([`${dirPath}/**`], { force: true });

export const buildTs = (outDir: string, prod: boolean, tsOptions?: any) => {
	const tsProject = createProject('tsconfig.json', { typescript: tsc, ...tsOptions });

	return tsProject
		.src()
		.pipe(gulpif(!prod, initSourceMaps({})))
		.pipe(tsProject())
		.pipe(gulpif(!prod, writeSourceMaps('.')))
		.pipe(dest(outDir));
};

export function createDirsIfNotExist(dir: string) {
	const dirIsResolved = dir.includes('\\');
	const separator = dirIsResolved ? '\\' : '/';
	const pathParts = dir.split(separator);
	let currentDir = '';
	for (const pathPart of pathParts) {
		if (currentDir.length > 0) {
			currentDir += separator;
		}
		currentDir += pathPart;

		if (!pathPart.includes(':') && !fs.existsSync(currentDir)) {
			fs.mkdirSync(currentDir);
		}
	}
};

export async function createDirsIfNotExistAsync(dir: string) {
	const dirIsResolved = dir.includes('\\');
	const separator = dirIsResolved ? '\\' : '/';
	const pathParts = dir.split(separator);
	let currentDir = '';
	for (const pathPart of pathParts) {
		if (currentDir.length > 0) {
			currentDir += separator;
		}
		currentDir += pathPart;

		if (!pathPart.includes(':') && !fs.existsSync(currentDir)) {
			await fs.promises.mkdir(currentDir);
		}
	}
};

export const copyPackageJson = async (
	packageJson: any,
	outDir: string,
	ignoredKeys?: string[],
	beforeSerialize?: (packageJson: any) => void
) => {
	const allIgnoredKeys = ['scripts', 'lint-staged', 'default'];
	if (ignoredKeys) {
		allIgnoredKeys.push(...ignoredKeys);
	}
	const newPackage: any = {};

	for (const key in packageJson) {
		if (!allIgnoredKeys.includes(key)) {
			newPackage[key] = packageJson[key];
		}
	}

	if (beforeSerialize) {
		beforeSerialize(newPackage);
	}

	const json = JSON.stringify(newPackage);

	createDirsIfNotExist(outDir);
	return fs.promises.writeFile(path.resolve(outDir, 'package.json'), json);
};

const errorRegex = /ERROR|ERR!/;
const logErrorFromStream = (data: string, logError?: (error: string) => void) => {
	if (!data || !logError) {
		return;
	}
	const lines = data.split('\n');
	lines.filter(s =>
		errorRegex.test(s)).forEach(logError);
};


const processStream = (data: any, logError?: (error: string) => void) => {
	const s = String(data);
	logErrorFromStream(s, logError);
	console.log(s);
};

export const spawnTask = (
	command: string, args: string[], cwd: string, done: () => void,
	taskName?: string,
	logErrors?: (taskName: string, errors: string[]) => void
) => {
	const cmd = spawn(command, args, { cwd });
	const errors: string[] = [];
	const logErrorLocally = (error: string) => {
		errors.push(error);
	};
	cmd.stdout.on('data', (data: any) => processStream(data, logErrorLocally));
	cmd.stderr.on('data', (data: any) => processStream(data, logErrorLocally));
	cmd.on('exit', (code) => {
		if (code && logErrors && taskName) {
			logErrors(taskName, errors.length > 0 ? errors : [`Unknown build error, exit code ${code}`]);
		}
		done();
	});
};

export const packResult = (
	outDir: string,
	packagesDir: string,
	packageJson: { name: string; version: string },
	done: () => void) => {

	spawnTask('npm.cmd', ['pack'], outDir, () => {
		const fullPath = path.resolve(outDir, `${packageJson.name}-${packageJson.version}.tgz`);
		createDirsIfNotExist(packagesDir);
		fs.copyFile(fullPath, `${packagesDir}\\${packageJson.name}.tgz`, () => {
			if (fs.existsSync(fullPath)) {
				fs.unlink(fullPath, () => {
					done();
				});
			} else {
				done();
			}
		});
	});
};

export interface IBuildTarget {
	path: string;
	testable: boolean;
	customPackageInstaller?: boolean;
}

const currentDir = __dirname;

export const createTask = (
	targetName: string,
	target: IBuildTarget,
	taskSuffix: string,
	taskNames: string[],
	fn: TaskFunction,
	onlyTestable: boolean = false
) => {
	if (onlyTestable && !target.testable) {
		return;
	}

	const taskName = `${targetName} ${taskSuffix}`;
	taskNames.push(taskName);
	task(taskName, fn);
};

export const createNpmTask = (
	targetName: string,
	target: IBuildTarget,
	taskSuffix: string,
	npmArgs: string[],
	taskNames: string[],
	logErrors?: (taskName: string, newErrors: string[]) => void,
	onlyTestable: boolean = false
) => {
	createTask(
		targetName,
		target,
		taskSuffix,
		taskNames,
		(done: () => void) =>
			spawnTask('npm.cmd', npmArgs, path.resolve(currentDir, target.path), done,
				`${targetName} ${taskSuffix}`, logErrors),
		onlyTestable);
};

export const replaceInFile = async (
	filePath: string,
	searchValue: string,
	replaceValue: string
) => {
	const content = await fs.promises.readFile(filePath, {encoding: 'utf-8'});
	return fs.promises.writeFile(filePath, content.replace(new RegExp(searchValue, 'g'), replaceValue));
};
