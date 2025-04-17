const fs = require('fs');
const { exec } = require('child_process');
const path = require('path');

const deleteFolderRecursive = function (directoryPath) {
	if (fs.existsSync(directoryPath)) {
		fs.readdirSync(directoryPath).forEach((file, index) => {
			const curPath = path.join(directoryPath, file);
			if (fs.lstatSync(curPath).isDirectory()) {
				deleteFolderRecursive(curPath);
			}
			else {
				fs.unlinkSync(curPath);
			}
		});
		fs.rmdirSync(directoryPath);
	}
};

exec('gulp build', { maxBuffer: 1000000000 }, (error, stdout, stderr) => {
	if (error) {
	  console.error(`exec error: ${error}`);
	}
	else {
		deleteFolderRecursive('node_modules');
	}
	console.log(stdout);
	console.log(stderr);
	if (error) {
		throw 'Build Failed';
	}
});
