import * as path from 'path';
import * as webpack from 'webpack';
import * as webpackDevServer from 'webpack-dev-server';
import * as HtmlWebpackPlugin from 'html-webpack-plugin';
import * as MiniCssExtractPlugin from 'mini-css-extract-plugin';
import * as TerserPlugin from 'terser-webpack-plugin';
import { AureliaPlugin, ModuleDependenciesPlugin } from 'aurelia-webpack-plugin';
import { BundleAnalyzerPlugin } from 'webpack-bundle-analyzer';
import { CleanWebpackPlugin } from 'clean-webpack-plugin';
import { IEnvironment, IBuildOptions, RunLinterPlugin } from 'build-tools';

const CopyWebpackPlugin = require('copy-webpack-plugin');
const DuplicatePackageCheckerPlugin = require('duplicate-package-checker-webpack-plugin');
const currentPackage = require('./package.json');

// primary config:
const outDir = path.resolve(__dirname, "../../Scripts/OutlookPlugin");
const srcDir = path.resolve(__dirname, 'src');
const scriptDir = 'js/';
const controlsDir = "node_modules/client-controls";
const defaultPort = 8080;
const host = '127.0.0.1';
const open = false;

const cssRules = [
	{
		loader: 'css-loader',
		options: {
			esModule: false
		}
	}
];

const sassRules = [
	{
		loader: "sass-loader",
		options: {
			sassOptions: {
				includePaths: ['node_modules']
			}
		}
	}
];

const screenRelativePath = 'screens/OU/ou201000';

const getModules = (env: IEnvironment & IBuildOptions): webpack.Configuration => {
	const modules: webpack.Configuration = {
		module: {
			rules: [
				// CSS required in JS/TS files should use the style-loader that auto-injects it into the website
				// only when the issuer is a .js/.ts file, so the loaders are not applied inside html templates
				{
					test: /\.css$/i,
					issuer: [{ not: /\.html$/i }],
					use: env.extractCss ? [{
						loader: MiniCssExtractPlugin.loader
					}, ...cssRules
					] : ['style-loader', ...cssRules]
				},
				{
					test: /\.css$/i,
					issuer: /\.html$/i,
					// CSS required in templates cannot be extracted safely
					// because Aurelia would try to require it again in runtime
					use: cssRules
				},
				{
					test: /\.scss$/,
					use: env.extractCss ? [{
						loader: MiniCssExtractPlugin.loader
					}, ...cssRules, ...sassRules
					] : ['style-loader', ...cssRules, ...sassRules],
					issuer: /\.[tj]s$/i
				},
				{
					test: /\.scss$/,
					use: [...cssRules, ...sassRules],
					issuer: /\.html?$/i
				},
				{
					test: /\.html$/i, use:
						[
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
								loader: 'merge-html',
							}
						]
				},
				{
					test: /(?<!\.d)\.ts?$/, loader: "ts-loader",
					options: {
						compiler: 'ttypescript'
					}
				}, {
					test: /\.d\.ts|\.map$/,
					loader: 'ignore-loader'
				},
				// embed small images and fonts as Data Urls and larger ones as files:
				//{ test: /\.(png|gif|jpg|cur)$/i, loader: 'url-loader', options: { limit: 8192 } },
				{ test: /\.(png|gif|jpg|cur)$/i, type: 'asset' },
				{ test: /\.woff2(\?v=[0-9]\.[0-9]\.[0-9])?$/i, loader: 'url-loader', options: { limit: 10000, mimetype: 'application/font-woff2' } },
				{ test: /\.woff(\?v=[0-9]\.[0-9]\.[0-9])?$/i, loader: 'url-loader', options: { limit: 10000, mimetype: 'application/font-woff' } },
				// load these fonts normally, as files:
				{ test: /\.(ttf|eot|svg|otf)(\?v=[0-9]\.[0-9]\.[0-9])?$/i, loader: 'file-loader' },
				{
					test: /environment\.json$/i, use: [
						{ loader: "app-settings-loader", options: { env: env.production ? 'production' : 'development' } },
					]
				},
			]
		}
	};

	if (env.tests) {
		modules.module!.rules!.push({
			test: /\.[jt]s$/i, loader: 'istanbul-instrumenter-loader',
			include: srcDir, exclude: [/\.(spec|test)\.[jt]s$/i],
			enforce: 'post', options: { esModules: true },
		} as any);
	}
	return modules;
};

const getPlugins = (env: IEnvironment & IBuildOptions): webpack.Configuration => {
	const { tests, extractCss, analyze } = env;

	const plugins: webpack.WebpackPluginInstance[] = [
		new AureliaPlugin({ viewsFor: `{src,${controlsDir}}/**/!(tslib)*.{ts,js}` }),
		new ModuleDependenciesPlugin({
			'aurelia-testing': ['./compile-spy', './view-spy']
		}),
		new HtmlWebpackPlugin({
			template: 'index.ejs',
			minify: false,
			screenViewName: `${screenRelativePath}.html`
		}),
		new CleanWebpackPlugin(),
		new webpack.ProvidePlugin({
			process: 'process/browser',
		})
	];

	if (!tests) {
		plugins.push(...[
			new DuplicatePackageCheckerPlugin(),
			new CopyWebpackPlugin({
				patterns: [
					{ from: 'static', to: outDir, globOptions: { ignore: ['**/*.css', '**/.*'] } }
				]
			})
		]);
	}

	if (!env.production) {
		plugins.push(new RunLinterPlugin('../../../../../'));
	}

	if (extractCss) {
		plugins.push(new MiniCssExtractPlugin({ // updated to match the naming conventions for the js files
			filename: env.production ? '[name].[contenthash].bundle.css' : '[name].[hash].bundle.css',
			chunkFilename: env.production ? '[name].[contenthash].chunk.css' : '[name].[hash].chunk.css'
		}));
	}

	if (analyze) {
		plugins.push(new BundleAnalyzerPlugin());
	}

	return { plugins };
};


const config = (env: IEnvironment & IBuildOptions): webpack.Configuration => ({
	//  watch:true,
	resolve: {
		extensions: ['.ts', '.js'],
		modules: [srcDir, path.resolve(__dirname, 'node_modules'), 'node_modules'],

		alias: {
			'client-controls': path.resolve(__dirname, controlsDir)
		}
	},
	entry: {
		app: [
			'aurelia-bootstrapper'
		]
	},
	mode: env.production ? 'production' : 'development',
	output: {
		path: outDir,
		filename: scriptDir + (env.production ? '[name].[chunkhash].bundle.js' : '[name].[hash].bundle.js'),
		sourceMapFilename: '[file].map[query]',
		chunkFilename: scriptDir + (env.production ? '[name].[chunkhash].chunk.js' : '[name].[hash].chunk.js')
	},
	optimization: {
		minimizer: [
			new TerserPlugin({ terserOptions: { keep_classnames: true, keep_fnames: true } })
		],
		runtimeChunk: false,  // separates the runtime chunk, required for long term cacheability
		// moduleIds is the replacement for HashedModuleIdsPlugin and NamedModulesPlugin deprecated in https://github.com/webpack/webpack/releases/tag/v4.16.0
		// changes module id's to use hashes be based on the relative path of the module, required for long term cacheability
		moduleIds: 'deterministic',
		splitChunks: {
			hidePathInfo: true, // prevents the path from being used in the filename when using maxSize
			chunks: "initial",

			cacheGroups: {
				default: false,

				// This is the HTTP/1.1 optimized cacheGroup configuration.
				vendors: { // picks up everything from node_modules as long as the sum of node modules is larger than minSize
					test: /[\\/]node_modules[\\/]/,
					name: 'vendors',
					priority: 19,
					enforce: true, // causes maxInitialRequests to be ignored, minSize still respected if specified in cacheGroup
					minSize: 30000 // use the default minSize
				},
				vendorsAsync: { // vendors async chunk, remaining asynchronously used node modules as single chunk file
					test: /[\\/]node_modules[\\/]/,
					name: 'vendors.async',
					chunks: 'async',
					priority: 9,
					reuseExistingChunk: true,
					minSize: 10000  // use smaller minSize to avoid too much potential bundle bloat due to module duplication.
				},
				controls: {
					test: /[\\/]client-controls[\\/]/,
					name: 'controls',
					priority: 19,
					enforce: true,
					chunks: 'all',
				},
				commonsAsync: { // commons async chunk, remaining asynchronously used modules as single chunk file
					name: 'commons.async',
					minChunks: 2, // Minimum number of chunks that must share a module before splitting
					chunks: 'async',
					priority: 0,
					reuseExistingChunk: true,
					minSize: 10000  // use smaller minSize to avoid too much potential bundle bloat due to module duplication.
				}
			}
		}
	},
	performance: { hints: false },
	devServer: {
		devMiddleware: {
			writeToDisk: true,
		},
		historyApiFallback: true,
		open: open,
		hot: env.hmr,
		port: env.port || defaultPort,
		host: env.host || host,
		static: {
			directory: outDir,
		},
		client: {
			webSocketURL: { hostname: env.host || host, pathname: undefined, port: env.port || defaultPort },
			reconnect: env.watch
		},
	},
	devtool: env.production ? false : 'source-map',
	resolveLoader: {
		alias: {
			'merge-html': path.resolve(__dirname, 'node_modules/build-tools', 'html-merge-loader.js'),
			'wg-loader': path.resolve(__dirname, 'node_modules/build-tools', 'html-wrapper-generation.js')
		}
	},
	...getModules(env),
	...getPlugins(env)
});

export default config;
