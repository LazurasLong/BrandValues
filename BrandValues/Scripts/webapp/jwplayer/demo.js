$(document).ready(function(){

    //alert(signedUrl);

    //JWPlayer
    jwplayer("mediaplayer").setup({
        playlist: [{
            image: "https://s3-eu-west-1.amazonaws.com/valuescompetition-transcoder-thumbnails/test1-00001.png",
            sources: [{
                //file: "rtmpe://s3mng6a8yx4xfl.cloudfront.net/cfx/st/mp4:test1.mp4"
                //file: "rtmp://sbw4t54bzxsgi.cloudfront.net/cfx/st/mp4:test1/test1.mp4" + signedUrl
                //file: "rtmp://sbw4t54bzxsgi.cloudfront.net/cfx/st/test1.mp4?Expires=1405931341&Signature=g0SjVT3auERxby2E5y6DWRnkb9l9fThlfX99V-HgGm8AtlMLvG~jn7Gc6FF0G0pQ5N6~09t3koFVBoqmdEovT0q3NtBeUUEJDrkC08AjAf8Y01GkqDE19eWnkk843d5QuwgSIKe8S50mR314suGTLyoltPL2aUPZYuCEPRPHKx-aYdzfJ~qeQQ0DKSgeyeufmYZ4EcoAK7j01A1I5XgW1EQ7wvX1WXbx7jY2ypxvdwTTe64lWAoy-Fy2vxm4rdb3oo2cyNN~nupbbl2JZ5UB1-rvbhzV6UDYdaIxdt4uqvhZceOLTILfND-9FTbjQ7VVK-KQDvFojMUTdGbCETFkxQ__&Key-Pair-Id=APKAJWFKSJRPHR2V45EA"
                //file: "rtmp://sbw4t54bzxsgi.cloudfront.net/cfx/st/mp4:test1.mp4?Policy=eyJTdGF0ZW1lbnQiOlt7IkNvbmRpdGlvbiI6eyJEYXRlTGVzc1RoYW4iOnsiQVdTOkVwb2NoVGltZSI6MTQwNTkzMTM0MX19fV19&Signature=JYtx86Z2xtUs3Rlgu0T8ZdjUl-HD3vUgf2-VYmmMrETzYEZ6sGNAwygzOIIcrXkj52d6c6b-TJOWvF-2Sk099vvpWLVuOUZ5-1xwqaT6Q6iIFgDX6cb7dKNWjJ5VWp2L3Pp3LvyyXmzoHVEOKUtkhg-qT3hVALdAldfoNQc33~e-7551atLOvCksfaGcKMK-j2L1JVZXMT9Zfb0dk~z6e5MXMP6TIzxDNLOOHH8hHyuxu7l4SkLfGTWLk97tyL8j4MW-9jK7Exr2CEYKQceQYr-Sag6FnkxXOd4yUJWOeAvIAYAEj4bhAKkybs~skSCbd5ektrQiAlcSwp3PuenKdQ__&Key-Pair-Id=APKAJWFKSJRPHR2V45EA"
                file: "rtmp://sbw4t54bzxsgi.cloudfront.net/cfx/st/mp4:test1/test1.mp4?Expires=1405935432&Signature=iuyqXi1WqU1RO65DPVFZGDPPCjTR-P6nYvvInD40-LN4G0wd0H4zuRQ4F6KREAUKv55BdNH2l2Po-SMLztOcT2JPaCHZl4ulWWlnXAFjuFMRMEXtn6XbomQeoyHnqStlXpyF8q1SWDmLy8JDTzE6o6OMft-5VxMThm5fGoddlfCB-Cgp98o3piIa7Ika8MydI8~884GSbc2yNri1Z5GMAOiK67B2DtzcC~hCfkkE2RXqWA5s8hEf42hFGDSbtU2WYToYjFCUx1n2JIjsrVtmT-6QAiXdOT~nCbn4VnsGxYV4x2F8pzyb4c8nXLpxMGroPGoLYNQEnurk1dDsFsCkxg__&Key-Pair-Id=APKAJWFKSJRPHR2V45EA"
            }]
        }],
        width: 640,
        height: 360
    });


});