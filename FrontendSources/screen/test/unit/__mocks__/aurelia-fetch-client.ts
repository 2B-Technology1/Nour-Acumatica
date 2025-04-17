// tslint:disable-next-line: variable-name
export const HttpClient = jest.fn().mockImplementation(() => ({
	fetch: jest.fn().mockImplementation(() => Promise.resolve())
}));
