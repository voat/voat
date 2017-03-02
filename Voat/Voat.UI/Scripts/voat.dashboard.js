/*

Now these lines technically are no longer blank. ;)

*/
function closeDashboardHandler(e) {

    var dashboard = document.getElementById("dashboard");

    if (dashboard !== null && !$.contains(dashboard, e.target)) {
        $(dashboard).hide();

        var body = $('body');
        body.off('click.menu');

        return false;
    }
}
$('.top-bar > nav > a').click(function () {
    var dashboard = $('#dashboard');
    var body = $('body');

    dashboard.toggle();

    if (dashboard.is(':visible')) {
        body.on('click.menu', closeDashboardHandler);
    } else {
        body.off('click.menu');
    }

    return false;
});