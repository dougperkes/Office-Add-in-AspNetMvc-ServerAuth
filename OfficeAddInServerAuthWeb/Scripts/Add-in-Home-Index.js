/// <reference path="App.js" />
/// <reference path="~/Scripts/jquery-2.2.0.intellisense.js" />
/// <reference path="~/Scripts/_officeintellisense.js" />
(function () {
    "use strict";

    // The initialize function must be run each time a new page is loaded
    Office.initialize = function (reason) {
        var parentId = null, popupId = null, popupWindow = null;
        $(document).ready(function () {
            app.initialize();

            //setup the signalR hub
            var hub = $.connection.socketHub;
            var redirectTo = "/message";
            hub.client.sendMessage = function (message) {
                //first message must be the popupId
                if (popupId == null) {
                    popupId = message;
                } else {
                    var result = message;
                    if (result === "success") {
                        //we now have a valid auth token in the database
                        hub.server.sendMessage(popupId, "close");
                        //redirect to the message controller. 
                        window.location = redirectTo;
                    } else {
                        //we were unsuccessful in getting an auth token
                        hub.server.sendMessage(popupId, "close");
                        //show a message to the user
                        app.showNotification("User authentication", "Unable to successfully authenticate. Status is " + result);
                    }
                }
            }

            $.connection.hub.start().done(function () {
                //get the parentId from the hub
                parentId = $.connection.hub.id;
                popupId = null;

                //enable the login buttons
                $(".popupButton").prop("disabled", false);

                //need to include the parentId in the state key so we can ensure we only send messages to the correct parent
                var authState = { stateKey: stateKey, signalRHubId: parentId };
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


        });

        function showLoginPopup(url) {
            $("#connectContainer").hide();
            $("#waitContainer").show();
            popupWindow = window.open(url, "AuthPopup", "width=660,height=500,centerscreen=1"); //,menubar=0,toolbar=0,location=0,personalbar=0,status=0,titlebar=0,dialog=1')
        }
    };
})();