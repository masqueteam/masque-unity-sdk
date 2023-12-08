mergeInto(LibraryManager.library, {
    OpenURLWithCallback: function(url) {
        var dataFromIframe = "";
        var urlString = UTF8ToString(url);  // Convert pointer to string
        
        const popupWidth = 400;
        const popupHeight = 650;

        const popupLeft = (window.innerWidth / 2) - (popupWidth / 2);
        const popupTop = (window.innerHeight / 2) - (popupHeight / 2);

        var popupWindow = window.open(
        urlString,
        "popupWindow",
        `width=${popupWidth},height=${popupHeight},left=${popupLeft},top=${popupTop},menubar=no`
        );

        /*
        var pollTimer = window.setInterval(function() {
        if (popupWindow.closed !== false) { 
            window.clearInterval(pollTimer);
            
            unityInstance.SendMessage("DATA", "OnPopupClosed", dataFromIframe);
        }
        }, 100);
        */
    }
});
