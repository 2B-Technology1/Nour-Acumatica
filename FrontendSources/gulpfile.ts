import { parallel, dest, src, series } from 'gulp';
import { clean, createNpmTask, createTask, IBuildTarget } from './gulp-tools';
import { installPackages, installPackagesDebug } from './install-packages';

interface ITargetDictionary { [key: string]: IBuildTarget }

const targets: ITargetDictionary = {
	outlookPlugin: { path: "./outlook-plugin", testable: true },
	screens: { path: "./screen", testable: true },
};

const buildTasksNames: string[] = [];
const buildDevTasksNames: string[] = [];
const getTaskNames: string[] = [];
const clearTaskNames: string[] = [];
const testTaskNames: string[] = [];
const errors: Array<{ taskName: string; error: string }> = [];
const logErrors = (taskName: string, newErrors: string[]) => {
	errors.push(...newErrors.map(error => ({ taskName, error })));
};

Object.keys(targets).forEach((tKey) => {
	const target = targets[tKey];
	createNpmTask(tKey, target, 'build', ['run', 'build'], buildTasksNames, logErrors);
	createNpmTask(tKey, target, 'build-dev', ['run', 'build-dev'], buildDevTasksNames, logErrors);

	createNpmTask(tKey, target, 'install', ['ci'], getTaskNames);
	createNpmTask(tKey, target, 'test', ['run', 'test'], testTaskNames, logErrors, true);
	createTask(tKey, target, 'clear', clearTaskNames, async () =>
		clean(`${target.path}/node_modules`));
});

const printErrors = (done: (error?: { stack: string }) => void) => {
	if (errors.length === 0) {
		console.log('Task finished without errors');
		done();
		return;
	}
	errors.forEach(e => console.log(`task: ${e.taskName}\nerror: ${e.error}`));
	done({ stack: `\u001b[1m\u001b[31mTask finished with error${errors.length > 1 ? 's' : ''}\u001b[39m\u001b[22m` });
};

export const build = (done: () => void) => {
	const buildFunc = series(
		parallel(...buildTasksNames),
		parallel(...clearTaskNames),
	);

	buildFunc(() => printErrors(done));
};

export const buildDev = (done: () => void) => {
	const buildFunc = parallel(buildDevTasksNames);

	buildFunc(() => printErrors(done));
};
export const getmodulesDebug = series(
	installPackagesDebug('screen'),
	installPackagesDebug('outlook-plugin'),
	parallel(...getTaskNames)
);
export const getmodules = series(
	parallel(
		installPackages('screen'),
		installPackages('outlook-plugin')
	),
	parallel(...getTaskNames)
);
export const getTools = parallel(
	installPackages('screen'),
	installPackages('outlook-plugin')
);
export const clearmodules = parallel(...clearTaskNames);
export const test = (done: () => void) => {
	const testFunc = parallel(...testTaskNames);
	testFunc(() => printErrors(done));
};
