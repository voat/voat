/*

Now these lines technically are no longer blank. ;)

*/
function closeDashboardHandler(e) {

    //StyleSheet Preview loads two full pages (yeah yeah I already know), so this selector allows all the elements with id #dashboard to be selected
    var dashboard = $("div[id=dashboard]");
    //var dashboard = $('#dashboard');

    if (dashboard !== null && !$.contains(dashboard[0], e.target)) {
        dashboard.hide();

        var body = $('body');
        body.off('click.menu');

        return false;
    }
}
function registerDashboardHandler() {

    $('.top-bar > nav > a').click(function () {

        //StyleSheet Preview loads two full pages (yeah yeah I already know), so this selector allows all the elements with id #dashboard to be selected
        var dashboard = $("div[id=dashboard]");
        //var dashboard = $('#dashboard');

        var body = $('body');

        dashboard.toggle();

        if (dashboard.is(':visible')) {
            body.on('click.menu', closeDashboardHandler);
        } else {
            body.off('click.menu');
        }

        return false;
    });
}

registerDashboardHandler();