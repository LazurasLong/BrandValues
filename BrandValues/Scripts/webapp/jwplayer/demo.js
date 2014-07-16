$(document).ready(function(){

    //alert(signedUrl);

    jwplayer.key = "D7QMo1Ir9C8AM7Rbowp5IFudmR8sc8K4pzXVb4PNirw=";

    //JWPlayer
    jwplayer("mediaplayer").setup({
        playlist: [{
            image: "https://s3-eu-west-1.amazonaws.com/valuescompetition-transcoder-thumbnails/test1-00001.png",
            sources: [{
                file: rtmpUrl
            },{
                file: appleUrl
            },{
                file: fallbackUrl
            }]
        }],
        sharing: {
            link: document.URL
        },
        primary: "flash",
        width: 640,
        height: 360,
        ga: { idstring: "Video title" }
    });


});