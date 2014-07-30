$(document).ready(function() {

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


