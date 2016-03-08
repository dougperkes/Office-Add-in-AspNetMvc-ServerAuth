Office.initialize = function (reason) {
    $(document).ready(function () {
        console.log("Sending auth complete message through dialog: " + oauthResult.authStatus);
        Office.context.ui.messageParent(oauthResult.authStatus);
    });
}