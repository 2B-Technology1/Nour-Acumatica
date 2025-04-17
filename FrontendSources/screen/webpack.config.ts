// tslint:disable: object-literal-sort-keys
import * as path from 'path';
import * as webpack from 'webpack';
import * as webpackDevServer from 'webpack-dev-server';
import * as HtmlWebpackPlugin from 'html-webpack-plugin';
import * as CopyWebpackPlugin from 'copy-webpack-plugin';
import * as MiniCssExtractPlugin from 'mini-css-extract-plugin';
import * as TerserPlugin from 'terser-webpack-plugin';
import * as nodeExternals from 'webpack-node-externals';
import * as fs from 'fs';
import { AureliaPlugin, ModuleDependenciesPlugin } from 'aurelia-webpack-plugin';
import { BundleAnalyzerPlugin } from 'webpack-bundle-analyzer';
import { CleanWebpackPlugin } from 'clean-webpack-plugin';
import {
	IBuildOptions,
	IEnvironment,
	FilesWatcherPlugin,
	getScreensWebpackInfo,
	IScreensWebpackInfo,
	RunLinterPlugin,
	ScreenConfigPlugin,
	AureliaCacheWorkaroundPlugin,
	ScreenInfoDeployPlugin
} from 'build-tools';
import * as currentPackage from './package.json';
import * as tsconfig from './tsconfig.json';

// tslint:disable-next-line: variable-name
const DuplicatePackageCheckerPlugin = require('duplicate-package-checker-webpack-plugin');
// tslint:disable-next-line: variable-name
const CircularDependencyPlugin = require('circular-dependency-plugin');

const outDir = path.resolve(__dirname, "../../Scripts/Screens");
const port = 8080;
const host = '127.0.0.1';
const open = false;

const srcDir = path.resolve(__dirname, 'src');
const scriptDir = '';
const controlsDir = "node_modules/client-controls";

const screenInfoTargetPath = path.resolve(__dirname, '../../App_Data/TSScreenInfo');
const lockFileName = 'lock.json';

const getBaseConfig = (env: IEnvironment & IBuildOptions, screenInfos: IScreensWebpackInfo): webpack.Configuration => ({
	resolve: {
		extensions: ['.ts', '.js'],
		modules: [srcDir, path.resolve(__dirname, 'node_modules'), 'node_modules'],
		alias: {
			'client-controls': path.resolve(__dirname, controlsDir)
		}
	},
	entry: screenInfos.entry,
	mode: env.production ? 'production' : 'development',
	output: {
		path: outDir,
		filename: `${scriptDir}[name].[chunkhash].bundle.js`,
		sourceMapFilename: '[file].map[query]',
		chunkFilename: `${scriptDir}[name].[chunkhash].chunk.js`
	},
	performance: { hints: false },
	stats: env.production ? "errors-only" : "minimal",
	devServer: {
		hot: !env.noreload && env.watch,
		liveReload: !env.noreload && env.watch,
		devMiddleware: {
			writeToDisk: true,
		},
		historyApiFallback: true,
		static: {
			directory: outDir,
			watch: env.watch && {
				ignored: '**/node_modules',
				poll: 1000
			} || undefined
		},
		port: env.port || port,
		open,
		host: env.host || host,
		client: {
			webSocketURL: { hostname: env.host || host, pathname: undefined, port: env.port || port },
			reconnect: env.watch
		},
	},
	devtool: env.production ? false : 'source-map',
	resolveLoader: {
		alias: {
		  'merge-html': path.resolve(__dirname, 'node_modules/build-tools', 'html-merge-loader.js'),
		  'wg-loader': path.resolve(__dirname, 'node_modules/build-tools', 'html-wrapper-generation.js'),
		  'localization-loader': path.resolve(__dirname, 'node_modules/build-tools', 'html-localization-extractor.js')
		}
	  }
});

const getOptimization = (): webpack.Configuration => ({
	optimization: {
		minimizer: [
			new TerserPlugin({ terserOptions: { keep_classnames: true, keep_fnames: true } })
		],
		moduleIds: 'named',
		runtimeChunk: false,
		concatenateModules: false,
		splitChunks: {
			hidePathInfo: true, // prevents the path from being used in the filename when using maxSize
			chunks: "initial",
			cacheGroups: {
				default: false,
				controls: {
					test: /[\\/]client-controls[\\/]/,
					name: 'controls',
					priority: 19,
					enforce: true,
					chunks: 'all',
				},
				vendors: { // picks up everything from node_modules as long as the sum of node modules is larger than minSize
					test: /[\\/]node_modules[\\/]/,
					name: 'vendors',
					priority: 19,
					enforce: true, // causes maxInitialRequests to be ignored, minSize still respected if specified in cacheGroup
					minSize: 30000, // use the default minSize
					chunks: 'all',
				}
			}
		},
		usedExports: false,
	}
});

const getModule = (env: IEnvironment & IBuildOptions, screenInfos: IScreensWebpackInfo): webpack.Configuration => {
	const cssRules = [{
		loader: 'css-loader',
		options: { esModule: false }
	}];

	const sassRules = [{
		loader: "sass-loader",
		options: {
			sassOptions: { includePaths: ['node_modules'] }
		}
	}];

	const htmlLoaders = new Array<any>(
		{
			loader: 'html-loader',
			options: {
				minimize: false
			}
		},
		{
			loader: 'wg-loader',
		},
		{
			loader: 'localization-loader',
		}
	);

	if (screenInfos.mergeHtml) {
		htmlLoaders.push({
			loader: 'merge-html',
			options: {
				customizations: env.customizations
			}
		});
	}

	return {
		module: {
			unsafeCache: screenInfos.singleScreen ? true : undefined,
			rules: [{
				test: /\.css$/i,
				issuer: [{ not: /\.html$/i }],
				use: env.extractCss ? [{
					loader: MiniCssExtractPlugin.loader
				}, ...cssRules
				] : ['style-loader', ...cssRules]
			}, {
				test: /\.css$/i,
				issuer: /\.html$/i,
				use: cssRules
			}, {
				test: /\.scss$/,
				use: env.extractCss ? [{
					loader: MiniCssExtractPlugin.loader
				}, ...cssRules, ...sassRules
				] : ['style-loader', ...cssRules, ...sassRules],
				issuer: /\.[tj]s$/i
			}, {
				test: /\.scss$/,
				use: [...cssRules, ...sassRules],
				issuer: /\.html?$/i
			}, {
				// TODO: I think, for optimization reasons, we should separate processing of screens html templates
				// from all other html files.
				test: /\.html$/i,
				use: htmlLoaders
			}, {
				test: /(?<!\.d)\.ts?$/,
				loader: "ts-loader",
				options: {
					compiler: 'ttypescript',
					configFile: screenInfos.singleScreen ?
						ScreenConfigPlugin.tempConfigFile :
						undefined
				}
			}, {
				test: /\.d\.ts|\.map$/,
				loader: 'ignore-loader'
			 }, {
				test: /\.(png|gif|jpg|cur)$/i, loader: 'url-loader', options: { limit: 8192 }
			}, {
				test: /\.woff2(\?v=[0-9]\.[0-9]\.[0-9])?$/i,
				loader: 'url-loader',
				options: { limit: 10000, mimetype: 'application/font-woff2' }
			}, {
				test: /\.woff(\?v=[0-9]\.[0-9]\.[0-9])?$/i,
				loader: 'url-loader',
				options: { limit: 10000, mimetype: 'application/font-woff' }
			}, {
				test: /\.(ttf|eot|svg|otf)(\?v=[0-9]\.[0-9]\.[0-9])?$/i, loader: 'file-loader'
			}, {
				test: /environment\.json$/i, use: [{
					loader: "app-settings-loader",
					options: { env: env.production ? 'production' : 'development' }
				}]
			}, {
				test: /\.js$/,
				use: ["source-map-loader"],
				enforce: "pre"
			}],
		}
	};
};

const getPlugins = (env: IEnvironment & IBuildOptions, screenInfos: IScreensWebpackInfo): webpack.Configuration => {
	const viewsFor = `{src,${controlsDir}}/**/!(tslib)*.{ts,js}`;
	const plugins: webpack.WebpackPluginInstance[] = [
		new AureliaPlugin({
			viewsFor,
			entry: Object.keys(screenInfos.entry)
		}),
		...screenInfos.htmlPlugins,
		new webpack.ProvidePlugin({
			process: 'process/browser',
		}),
		new AureliaCacheWorkaroundPlugin(),
		new ScreenInfoDeployPlugin({
			sourceDir: path.resolve(controlsDir, 'screenInfos'),
			targetDir: path.resolve(__dirname, screenInfoTargetPath)
		}),
		new webpack.DefinePlugin({
			HTML_MERGED: screenInfos.mergeHtml
		}),
		{
			apply: (compiler: webpack.Compiler) => {
				const lockFilePath = path.join(screenInfoTargetPath, lockFileName);
				compiler.hooks.beforeCompile.tap('Write jsons lock', () => {
					console.log(`Writing lock file ${lockFilePath}`);
					const lockJson = JSON.stringify({ pid: process.pid, ppid: process.ppid, });
					try {
						if (!fs.existsSync(screenInfoTargetPath)) {
							fs.mkdirSync(screenInfoTargetPath);
						}
						fs.writeFileSync(lockFilePath, lockJson);
					}
					catch (error) {
						console.error(error);
					}
				});
				compiler.hooks.failed.tap('Write jsons lock', () => {
					console.log(`Removing lock file ${lockFilePath}`);
					try {
						fs.unlinkSync(lockFilePath);
					}
					catch (error) {
						console.error(error);
					}
				});
				compiler.hooks.done.tap('Write jsons lock', () => {
					console.log(`Removing lock file ${lockFilePath}`);
					try {
						fs.unlinkSync(lockFilePath);
					}
					catch (error) {
						console.error(error);
					}
				});
			}
		},
	];

	if (!env.production) {
		plugins.push(new RunLinterPlugin('../../../../../'));
	}

	if (env.watch) {
		plugins.push(new FilesWatcherPlugin({
			addDeletePaths: [path.resolve('./src/screens')],
			changePaths: [
				path.resolve('./build'),
				path.resolve('./webpack.config.ts'),
				path.resolve('./tsconfig.json'),
				path.resolve('./node_modules/client-controls')
			]
		}));
	}

	if (!screenInfos.singleScreen) {
		plugins.push(new HtmlWebpackPlugin({
			// template: 'index2.ejs',
			filename: 'enhance.html',
			minify: false
		}));
		plugins.push(new CleanWebpackPlugin({ cleanStaleWebpackAssets: !!env.production }));
	}
	else {
		plugins.push(new ScreenConfigPlugin({
			...screenInfos,
			defaultFoldersToInclude: ["src/*",
				"src/extensions",
				"src/resources",
				"test"],
			tsconfig,
			baseDir: path.resolve(__dirname)
		}));
	}

	if (!env.tests) {
		if (!screenInfos.singleScreen) {
			plugins.push(new DuplicatePackageCheckerPlugin());
		}
		plugins.push(new CopyWebpackPlugin({
			patterns: [
				{ from: 'static', to: outDir, globOptions: { ignore: ['**/*.css', '**/.*'] } }
			]
		}));
	}

	if (env.analyze) {
		plugins.push(new BundleAnalyzerPlugin());
	}

	if (env.extractCss) {
		plugins.push(new MiniCssExtractPlugin({ // updated to match the naming conventions for the js files
			filename: '[name].[contenthash].bundle.css',
			chunkFilename: '[name].[contenthash].chunk.css'
		}));
	}

	if (env.analyzeDependencies) {
		plugins.push(new CircularDependencyPlugin());
	}

	return { plugins };
};

const config = (env: IEnvironment & IBuildOptions = {}): webpack.Configuration => {
	const screensInfo = getScreensWebpackInfo(env);

	return {
		...getBaseConfig(env, screensInfo),
		...getOptimization(),
		...getModule(env, screensInfo),
		...getPlugins(env, screensInfo)
	};
};

export default config;
