+function ($) {

  $(function(){

    $('#wizardform').bootstrapWizard({
      'tabClass': 'nav nav-tabs',
      'onNext': function(tab, navigation, index) {
        var valid = false;
        $('[data-required="true"]', $( $(tab.html()).attr('href') )).each(function(){
          return (valid = $(this).parsley( 'validate' ));
        });
        return valid;
      },
      onTabClick: function(tab, navigation, index) {
        return false;
      },
      onTabShow: function(tab, navigation, index) {
        var $total = navigation.find('li').length;
        var $current = index+1;
        var $percent = ($current/$total) * 100;
        $('#wizardform').find('.progress-bar').css({width:$percent+'%'});
      }
    });

    $('#chkAll').click(function () {
        var checkBoxes = $("input[name=values]");
        checkBoxes.prop("checked", !checkBoxes.prop("checked"));
    });

    $("#upload").click(function () {
        $("#step1").hide();
        $("#step2").hide();
        $("#pager").hide();
        
        $('#wizardform').bootstrapWizard('last');

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

    if (uploaded) {
        $('#wizardform').find('.progress-bar').css({ width: 100 + '%' });
        $("#step1").hide();
        $("#step2").hide();
        $("#pager").hide();
    }


    $('#type').on('change', function () {
        if (this.value == "team") {
            $("#teamname-txt").fadeIn();
        } else {
            $("#teamname-txt").hide();
        }
    });

  });
}(window.jQuery);