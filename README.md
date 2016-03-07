# Office Add-in Server Authentication Sample for ASP.net MVC

A goal of many Microsoft Office add-ins is to improve user productivity. You can get closer to achieving this goal with the help of third-party services. Most of today's services implement the OAuth 2.0 specification to allow other applications into the user data.

![](http://i.imgur.com/fdwI6Ym.png)

Keep in mind that Office add-ins run on a variety of platforms and devices, which presents a great opportunity for your add-in. You must be aware, however, of the following considerations when you try to make OAuth flows work across a combination of platforms and technologies.

## Design considerations

Office applications display add-ins within an iframe on browser platforms. Many authentication services refuse to present sign-in pages to an iframe to minimize the risk that a malicious page could interfere. This limits displaying any sign-in page in the main add-in window.

**Solution:** Start the OAuth flow from a pop-up window

After the OAuth flow is done, you might want to pass the tokens to your main add-in page and use them to make API calls to the 3rd party service. 
Some browsers, most notably Internet Explorer, navigate within security boundaries (e.g. Zones). If pages are in different security zones, it’s not possible to pass tokens from the pop-up page to the main page.

**Solution:** Store the tokens in a database and make the API calls from your server instead. Enable server-side communication via sockets to send authentication results to the main page. There are many libraries that make it easy to communicate between the server and the browser. This sample uses [SignalR](http://www.asp.net/signalr) to notify the main add-in page that the OAuth flow is done.

Because of the security zones mentioned previously, we can't ensure that the pop-up and the main add-in page share the same session identifier in your add-in. If this is the case, the add-in server doesn't have a way to determine what main add-in page it needs to notify.

**Solution:** Use the main page session ID to identify the browser session. If you have to open a pop-up, send the session identifier as part of the path or query string.

Your add-in must reliably identify the browser session that started the OAuth flow. When the flow returns to the configured redirect URI, your add-in needs to decide which browser session owns the tokens. If your add-in pages are in different security zones, the add-in won't be able to assign the tokens to the right browser session.

**Solution:** Use the state parameter in the OAuth flow to identify the session that owns the tokens. Further discussion about this technique can be found in [Encoding claims in the OAuth 2 state parameter using a JWT](https://tools.ietf.org/html/draft-bradley-oauth-jwt-encoded-state-04). Note that the article is a work in progress. 

As an additional security measure, this sample deletes tokens from the database within two minutes of requesting them. You should implement token storage policies according to your application requirements.

## Sample Implementation Notes

In this sample I have chosen to implement manual OAuth2 flows for each of the authentication providers. This allows the sample to maintain consistency in the implementation while being able to account for the variations in each of the OAuth2 implementations for each of the providers. 

## Prerequisites

To use the Office add-in Server Authentication sample, you need the following:

* Visual Studio 2015 with Office Developer Tools
* Office 365 Developer Subscription
* Valid Google, Facebook, Dropbox, and Microsoft Account (optional)

**Important note:** You must register the application with at least one of the following providers to be able to use this sample. 

### Register Application with Azure AD

Register a web application in [Azure Management portal](https://manage.windowsazure.com) with the following configuration:

Parameter | Value
--------- | --------
Name | Add-In ServerAuth sample
Type | Web application and/or web API
Sign-on URL | https://localhost:44301/AzureADAuth/Authorize
App ID URI | https://[Your Azure AD Tenant name]/AddInServerAuth 

Once you register your application, take note of the *client ID* and *client secret* values.

In the permissions section, add the Graph API and select the Send mail as a user permission.

### Register Application with Azure AD v2 App Model

Register an application in the [Microsoft app registration portal](https://apps.dev.microsoft.com) with the following configuration:

Parameter | Value
----------|--------
Name | AddInServerAuth
Redirect URI | https://localhost:44301/AzureAD2Auth/Authorize

Generate a new password and take note of the *client ID* and *Password/Public Key*.

### Register Application with Google

Register an application in the [Google Developers Console](https://console.developers.google.com) with the following configuration:

Parameter | Value
--------- | --------
Project name | ServerAuth sample (optional)
Credentials | OAuth client ID
Application type | Web application
Authorized redirect URIs | https://localhost:44301/GoogleAuth/Authorize

Once you register your application, take note of the *client ID* and *client secret* values.

### Register Application with Facebook

Register an application in the [facebook for developers portal](https://developers.facebook.com) with the following configuration:

Parameter | Value
--------- | --------
Project name | Office Add-in ServerAuth sample (optional)
Application type | Web application
Site Url | https://localhost:44301
Valid OAuth redirect URIs | https://localhost:44301/FacebookAuth/Authorize
Client OAuth Login | Yes
Web OAuth Login | Yes

Once you register your application, take note of the *App ID* and *App Secret* values.

### Register Application with Dropbox

Register an application in the [Dropbox developer portal](https://www.dropbox.com/developers) with the following configuration:

Parameter | Value
--------- | -------
Project name | Office Add-in ServerAuth sample
OAuth 2 redirect URI | https://localhost:44301/DropBoxAuth/Authorize

## Configure and run the web app

Open the project in Visual Studio and open the OfficeAddInServerAuthWeb\Web.config file. Update the Client ID and Client Secret values for each of the authenication provider entries in the file.

```xml
    <add key="AAD:ClientID" value="" />
    <add key="AAD:ClientSecret" value="" />
    <add key="Google:ClientID" value="" />
    <add key="Google:ClientSecret" value="" />
    <add key="Facebook:ClientID" value=""/>
    <add key="Facebook:ClientSecret" value=""/>
    <add key="DropBox:ClientID" value=""/>
    <add key="DropBox:ClientSecret" value=""/>
    <add key="AAD2:ClientID" value="" />
    <add key="AAD2:ClientSecret" value="" />
```

### Run the web app

In Visual Studio, press F5 to start debugging the add-in. Excel desktop will launch and the add-in will automatically load. Click on any of the authentication provider images to login.

![](http://i.imgur.com/fdwI6Ym.png)