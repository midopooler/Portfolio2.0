mergeInto(LibraryManager.library, {

	openPage: function (url) {
		url = Pointer_stringify(url);
		console.log('Opening link: ' + url);
		window.open(url, '_blank');
	}
});