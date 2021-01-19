mergeInto(LibraryManager.library, {

	openPage: function (url) {
		console.log('Opening link: ' + url);
		window.open(url, '_blank');
	}
});