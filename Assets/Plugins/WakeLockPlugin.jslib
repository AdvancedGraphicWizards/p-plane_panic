mergeInto(LibraryManager.library, {

    RequestWakeLock: function () {
        if ('wakeLock' in navigator) {
            navigator.wakeLock.request('screen').then(function (wakeLock) {
                wakeLock.addEventListener('release', function () {
                    console.log('Wake Lock was released');
                });
                console.log('Wake Lock is active');
            }).catch(function (err) {
                console.error(err.name + ': ' + err.message);
            });
        }

        else {
            console.log('Wake Lock API not supported');
        }
    },
});
