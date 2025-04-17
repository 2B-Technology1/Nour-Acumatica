const fs = require('fs');
const cp = require('child_process');

fs.copyFileSync('package.base.json', 'package.json');
fs.copyFileSync('package-lock.base.json', 'package-lock.json');

cp.exec('npm i ../dist/packages/client-controls.tgz', (error, stdout, stderr) => {
	if (error) {
		console.log(error);
		return;
	}
	console.log(stdout);
	console.log(stderr);
	cp.exec('npm i ../dist/packages/transformers.tgz ../dist/packages/build-tools.tgz --save-dev',
		(error, stdout, stderr) => {
			if (error) {
				console.log(error);
				return;
			}
			console.log(stdout);
			console.log(stderr);
		});
})
