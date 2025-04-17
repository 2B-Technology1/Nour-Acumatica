export const fetchMock = jest.fn().mockImplementation(() => Promise.resolve());
export const configureMock = jest.fn();

// tslint:disable-next-line: variable-name
export const HttpClient = jest.fn().mockImplementation(() => ({
	fetch: fetchMock,
	configure: configureMock,
}));
