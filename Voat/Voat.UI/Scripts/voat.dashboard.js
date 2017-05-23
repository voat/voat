/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

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