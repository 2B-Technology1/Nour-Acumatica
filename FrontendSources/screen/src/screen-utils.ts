const screenIdLength = 8;

export const getScreenIdFromUrl = () => {
	const filePath = window.location.pathname;
	const lastSlash = filePath.lastIndexOf('/');
	const dotPosition = filePath.lastIndexOf('.');
	const screenId = lastSlash > -1 && dotPosition > lastSlash ? filePath.substring(lastSlash + 1, dotPosition) : '';
	// due to PXSiteMapNode is created with url.ToLower() (backend code) and it is critical to be in lower case there
	// it is needed to return screenId from pathname to upper case
	//TODO: It should be reworked. Why screenId gets from url?
	return screenId.length > screenIdLength ? screenId : screenId.toUpperCase();
};

export const getScreenPath = (screenId: string) => {
	//TODO: It should be reworked. Why screenId gets from url?
	if (screenId.length > screenIdLength) {
		return `systemScreens/${screenId}`;
	}

	return `screens/${screenId.substr(0, 2).toUpperCase()}/${screenId}/${screenId}`;
};

export const getScreenPathFromUrl = () => {
	const screenId = getScreenIdFromUrl();
	if (!screenId || screenId.length < 3) {
		return '';
	}
	return getScreenPath(screenId);
};
