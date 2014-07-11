$(document).ready(function(){

    //alert(signedUrl);

    //JWPlayer
    jwplayer("mediaplayer").setup({
        playlist: [{
            image: "https://s3-eu-west-1.amazonaws.com/valuescompetition-transcoder-thumbnails/test1-00001.png",
            sources: [{
                //file: "rtmpe://s3mng6a8yx4xfl.cloudfront.net/cfx/st/mp4:test1.mp4"
                file: signedUrl
                //file: "rtmp://sbw4t54bzxsgi.cloudfront.net/cfx/st/mp4:test1.mp4?Expires=1405698683&Signature=MjpXNy8B6FPAVafgFS0OQiFoz-4saqXu7fA-KXd6HK8R7QQ~jrx2MBZEb4dYtzJg5g6~NAcUReGsP0o9ERkzO9KkGEFhAAmjDgspOD6dVaHCOhBGN1zRteFZOsnwYi~UYYpW6AeBOGzFwqakTiBHfbnpcs28T-z8d5DhPRjLyo-3Q~tMfBhmVyd15hL7sCsfTXHLDZnaPGTpRvWkHeeSzSgOscsOVYdny66kXH15vnFhpVwKlb9afzOz37sbvbc9hC0dsgZrFL56t3r4mrRNA9~KwgLipUroSJ6btUxKY1To8PaVh60yeGnp1fIQEZxTrFMiV9WXGztWk9DTeaJLFg__&Key-Pair-Id=APKAJWFKSJRPHR2V45EA"
            }]
        }],
        width: 640,
        height: 360
    });


});