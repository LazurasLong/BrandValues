function get_browser() {
    var ua = navigator.userAgent, tem, M = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];
    if (/trident/i.test(M[1])) {
        tem = /\brv[ :]+(\d+)/g.exec(ua) || [];
        return 'IE ' + (tem[1] || '');
    }
    if (M[1] === 'Chrome') {
        tem = ua.match(/\bOPR\/(\d+)/)
        if (tem != null) { return 'Opera ' + tem[1]; }
    }
    M = M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, '-?'];
    if ((tem = ua.match(/version\/(\d+)/i)) != null) { M.splice(1, 1, tem[1]); }
    return M[0];
}

function get_browser_version() {
    var ua = navigator.userAgent, tem, M = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];
    if (/trident/i.test(M[1])) {
        tem = /\brv[ :]+(\d+)/g.exec(ua) || [];
        return 'IE ' + (tem[1] || '');
    }
    if (M[1] === 'Chrome') {
        tem = ua.match(/\bOPR\/(\d+)/)
        if (tem != null) { return 'Opera ' + tem[1]; }
    }
    M = M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, '-?'];
    if ((tem = ua.match(/version\/(\d+)/i)) != null) { M.splice(1, 1, tem[1]); }
    return M[1];
}


$(document).ready(function () {

    //fix width bug
    if (get_browser() == 'Firefox') {
        if (get_browser_version() < 5) {
            
            $("#play").css({ "max-width": "65%" });
        }
    }


    switch (format) {
        case ("video"):
            LoadMedia();
            break;
        case ("text"):
            LoadText();
            break;
        case ("image"):
            LoadImage();
            break;
    }

    function LoadText() {
        if (!cloudfrontUrl) {
            
            $('#text').append("<h1 class='top-padding-10'>No documents, PDF's, or other text uploaded along with this entry.</h1>");
        } else {
            var entryUrl = cloudfrontUrl;
            $('#text').append("<a href=" + entryUrl + " target='_blank' ><img src='https://d3kx2j4tswsg1z.cloudfront.net/play/click-to-open.png' class='entry-image' /></a>");
        }
    }


    function LoadImage() {
        if (!cloudfrontUrl) {
            $('#images').append("<h1 class='top-padding-10'>No image uploaded along with this entry.</h1>");
        } else {
            var entryUrl = cloudfrontUrl;
            $('#images').append("<a href=" + entryUrl + " target='_blank' ><img src=" + entryUrl + " class='entry-image' /></a>");
        }
    }


    function LoadMedia() {

        if (networkCheck == "True") {
            $('#mediaplayer').append("<img src='https://d3kx2j4tswsg1z.cloudfront.net/play/network-video-warning.png' class='entry-image' />");
        } else {
            if (!videoThumbnailUrl || !rtmpUrl || !fallbackUrl) {
                $('#mediaplayer').append("<h1>No video uploaded along with this entry.</h1>");
            } else {
                jwplayer.key = "D7QMo1Ir9C8AM7Rbowp5IFudmR8sc8K4pzXVb4PNirw=";

                //JWPlayer
                jwplayer("mediaplayer").setup({
                    playlist: [{
                        image: videoThumbnailUrl,
                        sources: [
                            {
                                file: rtmpUrl,
                                type: "rtmp"
                            },
                        //{
                        //    file: appleUrl,
                        //    type: "hls"
                        //},
                        {
                            file: fallbackUrl
                        }]
                    }],
                    //sharing: {
                    //    link: document.URL
                    //},
                    //primary: "flash",
                    width: "100%",
                    aspectratio: "16:9",
                    //autostart: true,
                    ga: { idstring: videoName }
                });

                jwplayer().onError(function () {
                    jwplayer().load({ file: "https://s3-eu-west-1.amazonaws.com/valuescompetition-degraded/video-encoding.mp4", image: "https://s3-eu-west-1.amazonaws.com/valuescompetition-degraded/video-encoding.png" });
                    jwplayer().play();
                });

            }
        }




    }




});


