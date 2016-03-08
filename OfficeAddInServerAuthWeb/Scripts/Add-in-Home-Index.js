/// <reference path="App.js" />
/// <reference path="~/Scripts/jquery-2.2.0.intellisense.js" />
/// <reference path="~/Scripts/_officeintellisense.js" />
(function () {
    "use strict";
    var _dlg;

    // The initialize function must be run each time a new page is loaded
    Office.initialize = function (reason) {
        $(document).ready(function () {
            app.initialize();

            //enable the login buttons
            $(".popupButton").prop("disabled", false);

            //need to include the parentId in the state key so we can ensure we only send messages to the correct parent
            var authState = { stateKey: stateKey };
            //the stateKey variable must be set on the parent page
            $("#loginO365PopupButton").click(function () {
                var url = "/azureadauth/login?authState=" + encodeURIComponent(JSON.stringify(authState));
                showLoginPopup(url);
            });
            $("#loginAAD2PopupButton").click(function () {
                var url = "/azuread2auth/login?authState=" + encodeURIComponent(JSON.stringify(authState));
                showLoginPopup(url);
            });
            $("#loginGooglePopupButton").click(function () {
                var url = "/googleauth/login?authState=" + encodeURIComponent(JSON.stringify(authState));
                showLoginPopup(url);
            });
            $("#loginFacebookPopupButton").click(function () {
                var url = "/facebookauth/login?authState=" + encodeURIComponent(JSON.stringify(authState));
                redirectTo = "/message/facebook";
                showLoginPopup(url);
            });
            $("#loginDropBoxPopupButton").click(function () {
                var url = "/dropboxauth/login?authState=" + encodeURIComponent(JSON.stringify(authState));
                redirectTo = "/message/dropbox";
                showLoginPopup(url);
            });
        });
    };

    function processMessage(arg) {
        console.log("Message received in processMessage: " + JSON.stringify(arg));
        if (arg.message === "success") {
            //we now have a valid auth token in the database
            _dlg.close();
            window.location = redirectTo;
        } else {
            //something went wrong with authentication
            _dlg.close();
            app.showNotification("User authentication", "Unable to successfully authenticate. Status is " + result);
        }
    }

    function showLoginPopup(url) {
        $("#connectContainer").hide();
        $("#waitContainer").show();
        var fullUrl = location.protocol + '//' + location.hostname + (location.port ? ':' + location.port : '') +
            url;
        Office.context.ui.displayDialogAsync(fullUrl,
            { height: 40, width: 40, requireHTTPS: true },
            function (result) {
                if (!_dlg) {
                    console.log("dialog has initialized. wiring up events");
                    _dlg = result.value;
                    _dlg.addEventHandler(Microsoft.Office.WebExtension.EventType.DialogMessageReceived, processMessage);
                }
            });

    }


})();