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

      $('#wizardform').lastSelector();

    $('#chkAll').click(function () {
        var checkBoxes = $("input[name=values]");
        checkBoxes.prop("checked", !checkBoxes.prop("checked"));
    });

    $("#upload").click(function () {
        $("#step1").hide();
        $("#loading").fadeIn();
        }
    );

    if (uploaded) {
        $('#wizardform').find('.progress-bar').css({ width: 100 + '%' });
        $("#step1").hide();
    }
    
  });
}(window.jQuery);