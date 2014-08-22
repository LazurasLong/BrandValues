+function ($) {

  $(function(){

      //set username and contact email
      $('#ContactEmail').val(username);

      //validate form 



    $('#chkAll').click(function () {
        var checkBoxes = $("input[name=values]");
        checkBoxes.prop("checked", !checkBoxes.prop("checked"));
    });

    $("#upload").click(function (event) {

        $("#uploadform").hide();

        $("#loading").fadeIn();

        var words = [
    "Loading humorous message ... Please Wait..",
    'We are testing your patience..',
    'Time is an illusion. Loading time doubly so...',
    'Go ahead, hold your breath..',
    'Enjoy the elevator music..',
    "At least you're not on hold...",
    'Reticulating Spines',
    'Searching for Answer to Life, the Universe, and Everything'
        ], i = 0;

        setInterval(function () {
            $('#waiting').fadeOut(function () {
                $(this).html(words[i = (i + 1) % words.length]).fadeIn();
            });
            // 2 seconds
        }, 10000);

        }
    );

    //if (uploaded) {
    //    $("#uploadform").hide();
    //}


    var type = $('#Type').val();
    if (type == "team") {
        $("#teamname-txt").fadeIn();
    }

    $('#Type').on('change', function () {
        if (this.value == "team") {
            $("#teamname-txt").fadeIn();
        } else {
            $("#teamname-txt").hide();
        }
    });

  });
}(window.jQuery);