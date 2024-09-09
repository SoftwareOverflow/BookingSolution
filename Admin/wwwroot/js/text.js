window.clipboardCopy = {
    copyText: function (text) {
        return navigator.clipboard.writeText(text).then(function () {
            return true;
        })
            .catch(function (error) {
                return false;
            });

        return false;
    }
};