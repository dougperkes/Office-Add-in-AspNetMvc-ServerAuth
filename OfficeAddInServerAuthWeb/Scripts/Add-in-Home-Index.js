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

            hub.client.sendMessage = function (message) {
                //first message must be the popupId
                if (popupId == null) {
                    popupId = message;
                } else {
                    var result = message;
                    if (result == "success") {
                        //we now have a valid auth token in the database
                        hub.server.sendMessage(popupId, "close");
                        //redirect to the message controller. 
                        window.location = "/message";
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

                //need to include the parentId in the state key so we can ensure we only send messages to the correct parent
                var authState = { stateKey: stateKey, signalRHubId: parentId };
                //the stateKey variable must be set on the parent page
                $("#loginO365PopupButton").click(function () {
                    $("#connectContainer").hide();
                    $("#waitContainer").show();
                    var url = "/azureadauth/login?authState=" + encodeURIComponent(JSON.stringify(authState));
                    popupWindow = window.open(url, "AuthPopup", "width=500,height=500,centerscreen=1"); //,menubar=0,toolbar=0,location=0,personalbar=0,status=0,titlebar=0,dialog=1')
                });
            });


        });
    };
})();