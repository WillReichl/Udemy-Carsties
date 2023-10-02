# Carsties

## Section 5 - OAuth 2.0 and OpenID Connect

### OAuth 2.0

Security standard where we give one app permission in another application.

Instead of giving a username and password, we give them a key, whose permissions we can revoke at any time.

**Terminology**
* _Resource Owner_: The user, who has the rights to certain resources in the app
* _Client_: The key recipient, who can make requests on behalf of the user/
* _Authorization Server_: Server who knows the Resource Owner and can authenticate them, and
	is a 3rd party server that the Resource Server trusts. The user already has an account here.
* _Resource Server_: API/service the client wants to use on behalf of the resource owner.
* _Redirect URI_: Where auth server redirects resource owner back to after granting access
* _Response Type_: Type of data client is expecting to receive (most common: "code" an authorization code)
* _Scope_: Granular permissions the client wants
* _Consent_: ex: "Do you want to allow Carsties to access your profile/email/etc" [Allow/Deny]
* _Client Id_: Identifies client with auth server
* _Client Secret_: Lets client securely share information privately behind the scenes
* _Authorization Code_: Short-lived code, combined with client secret, allows client to exchange for
	an access token
* _Access Token_: Key that the client will use from that point forward to communicate with the Resource Server

**Oath 2.0 Flow**

1. Resource owner (user) attempts to access protected resources in client
2. Client redirects to Authorization Server (using browser)
	* Inputs:
		* ClientId
		* RedirectUrl
		* ResponseType
		* Scopes
3. Authorization server logs you in or recognizes you are already logged in
4. Consent form (we will not implement this)
5. Auth server returns auth code to client
6. Client contacts auth server directly (not using resource owner's browser)
	* Inputs
		* Auth code
		* Client ID
		* Client Secret
7. Auth server responds with Access Token
8. Client includes Access Token on requests to Resource Server

### OpenId Connect

OAuth 2.0 provides a key, but does not provide any information about the logged in user.

OpenId Connect (OIDC) gives the client additional information about the user.