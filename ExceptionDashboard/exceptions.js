let excElement = null;
let excItem = null;

$('.show-exception').on('click', function (ev) {
    if (excElement != null) {
        excElement.hide();
        excItem.removeClass('selected');
    }

    $(ev.target).addClass('selected');

    var excId = $(ev.target).data('exception-id');
    excElement = $('#exc-' + excId);
    excElement.show();
    excItem = $(ev.target).parents('.exception-item');
    excItem.addClass('selected');
});