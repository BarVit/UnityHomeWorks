mergeInto(LibraryManager.library, {
  DamageLogDownload: function (namePointer, textPointer) {
    var name = UTF8ToString(namePointer);
    var text = UTF8ToString(textPointer);
    var blob = new Blob([text], { type: 'text/tab-separated-values;charset=utf-8' });
    var link = document.createElement('a');

    link.href = URL.createObjectURL(blob);
    link.download = name;
    document.body.appendChild(link);
    link.click();

    setTimeout(function () {
      URL.revokeObjectURL(link.href);
      link.remove();
    }, 1000);
  }
});
