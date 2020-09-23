mergeInto(LibraryManager.library, {
  IsMobileBrowser: function () {
    return (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent));
  },
  outScreen: function () {
    return (canvas.clientWidth, canvas.clientHeight);
  }
});
