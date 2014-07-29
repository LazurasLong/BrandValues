
$(function () {
    var ajaxFormSubmit = function() {
        var $form = $(this);

        var options = {
            url: $form.attr("action"),
            type: $form.attr("method"),
            data: $form.serialize()
        };

        $.ajax(options).done(function (data) {
            var $target = $($form.attr("data-brandvalues-target"));
            $target.replaceWith(data);
        });

        return false;
    };

    var submitAutocomplete = function (event, ui) {
        var $input = $(this);
        $input.val(ui.item.entryName);

        //console.log($input);
        //var $form = $input.parents("form:first");

        var $form = $('#searchSubmit');
        $form.submit();
    };

    var createAutocomplete = function() {
        var $input = $(this);

        var options = {
            minLength: 2,
            source: $input.attr("data-brandvalues-autocomplete"),
            select: submitAutocomplete
        };

        $input.autocomplete(options).data("ui-autocomplete")._renderItem = function (ul, item) {
            //var $a = $("<a></a>").text(item.label);
            //$("<span class='fr'></span>").text(item.year).prependTo($a);
            //$("<small class='db'></small>").text(item.cast).appendTo($a);
            //$("<span class='fl rg'><span style='width: " + item.rating + "%;'></span></span>").appendTo($a);
            //$("<small class='db'></small>").text(item.rating + "/100").appendTo($a);
            //return $("<li></li>").append($a).appendTo(ul);
            return $("<li>")
        .append("<a>" + item.entryName + "<br><span class='search-by'>by " + item.userFirstName + " " + item.userSurname + "</span></a>")
        .appendTo(ul);
        };
    };



    $("form[data-brandvalues-ajax='true']").submit(ajaxFormSubmit);
    $("input[data-brandvalues-autocomplete]").each(createAutocomplete);

});