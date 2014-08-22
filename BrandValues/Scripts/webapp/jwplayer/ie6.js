$(document).ready(function() {

    switch (format) {
        case ("video"):
            LoadMedia();
            break;
        case ("story"):
            LoadText();
            break;
        case ("poem"):
            LoadText();
            break;
        case ("lyric"):
            LoadText();
            break;
        case ("other"):
            LoadText();
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
            $('#text').append("<a href=" + entryUrl + " target='_blank' ><img src='https://d3kx2j4tswsg1z.cloudfront.net/play/click-to-open-ie.png' /></a>");
        }
    }


    function LoadImage() {
        if (!cloudfrontUrl) {
            $('#images').append("<h1 class='top-padding-10'>No image uploaded along with this entry.</h1>");
        } else {
            var entryUrl = cloudfrontUrl;
            $('#images').append("<a href=" + entryUrl + " target='_blank' ><img src=" + entryUrl + " /></a>");


            $('img').each(function () {
                $(this).load(function () {
                    var maxWidth = $(this).width(); // Max width for the image
                    var maxHeight = $(this).height();   // Max height for the image
                    $(this).css("width", "auto").css("height", "auto"); // Remove existing CSS
                    $(this).removeAttr("width").removeAttr("height"); // Remove HTML attributes
                    var width = $(this).width();    // Current image width
                    var height = $(this).height();  // Current image height

                    if (width > 600) {
                        // Check if the current width is larger than the max
                        var ratio = maxHeight / height; // get ratio for scaling image
                        var maxWidth = 576;
                        //$(this).css("height", height * ratio);   // Set new height
                        $(this).css("width", maxWidth);    // Scale width based on ratio
                        //width = width * ratio;  // Reset width to match scaled image


                    }

                });
            });

            $('#images').append("<p class='text-sm'><a href='" + entryUrl + "' target='_blank'>Click here</a> to open a larger version</p>");
        }
    }


    function LoadMedia() {


      $('#mediaplayer').append("<img src='https://d3kx2j4tswsg1z.cloudfront.net/play/network-video-warning-ie.png' />");
 


    }






});


