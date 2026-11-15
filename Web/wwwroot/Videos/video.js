
// file: wwwroot/Videos/video.js

window.videoHelper = {
    play: function (video) {
        if (!video) return;
        // Force muting to ensure autoplay passes browser policy
        video.muted = true;
        var p = video.play();
        if (p && p.catch) {
            p.catch(function (e) {
                console.warn('videoHelper.play failed:', e.name, e.message);
            });
        }
    },
    setMuted: function (video, muted) {
        if (!video) return;
        video.muted = muted;
    }
};