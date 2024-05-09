# Carsties

## Section 5 - OAuth 2.0 and OpenID Connect

### OAuth 2.0

Security standard where we give one app permission in another application.

Instead of giving a username and password, we give them a key, whose permissions we can revoke at any time.

**Terminology**

- _Resource Owner_: The user, who has the rights to certain resources in the app
- _Client_: The key recipient, who can make requests on behalf of the user/
- _Authorization Server_: Server who knows the Resource Owner and can authenticate them, and
  is a 3rd party server that the Resource Server trusts. The user already has an account here.
- _Resource Server_: API/service the client wants to use on behalf of the resource owner.
- _Redirect URI_: Where auth server redirects resource owner back to after granting access
- _Response Type_: Type of data client is expecting to receive (most common: "code" an authorization code)
- _Scope_: Granular permissions the client wants
- _Consent_: ex: "Do you want to allow Carsties to access your profile/email/etc" [Allow/Deny]
- _Client Id_: Identifies client with auth server
- _Client Secret_: Lets client securely share information privately behind the scenes
- _Authorization Code_: Short-lived code, combined with client secret, allows client to exchange for
  an access token
- _Access Token_: Key that the client will use from that point forward to communicate with the Resource Server

**Oath 2.0 Flow**

1. Resource owner (user) attempts to access protected resources in client
2. Client redirects to Authorization Server (using browser)
   - Inputs:
     - ClientId
     - RedirectUrl
     - ResponseType
     - Scopes
3. Authorization server logs you in or recognizes you are already logged in
4. Consent form (we will not implement this)
5. Auth server returns auth code to client
6. Client contacts auth server directly (not using resource owner's browser)
   - Inputs
     - Auth code
     - Client ID
     - Client Secret
7. Auth server responds with Access Token
8. Client includes Access Token on requests to Resource Server

### OpenId Connect

OAuth 2.0 provides a key, but does not provide any information about the logged in user.

OpenId Connect (OIDC) gives the client additional information about the user.

## Section 6 - Adding a Gateway Service

### Introduction

We will be using Microsoft's YARP (Yet Another Reverse Proxy) for this.

#### What is a Reverse Proxy?

Normal request:

`Client => Proxy => Internet => Destination`

A reverse proxy is on the destination side. It takes the request from the internet and determines what destination to route it to.

#### Do we Need a Reverse Proxy?

- Typical in Microservices
- Single surface area for requests - do not need to know individual service endpoints
- Client unaware of any internal services
- Security
- SSL Termination
- URL rewriting
- _Can_ be used for Load Balancing
- Caching

### Adding the Gateway Service

#### Create the Project

From the root directory:

- `dotnet new web -o src/GatewayService`
- `dotnet sln add src/GatewayService`

#### Configuring YARP

See `appSettings.Development.json`.

#### Authentication

[YARP authentication documentation](https://microsoft.github.io/reverse-proxy/articles/authn-authz.html)

We will be using OpenId Connect, and passing the authentication cookie along to the services behind the proxy.

### Adding a new Client to the Identity service configuration

- ClientSecret
- `AllowedGrantTypes = GrantTypes.CodeAndClientCredentials`
  - Client can securely talk inside our network to identity inside our network without browser involvement
  - Method would not be valid for a mobile app
- RequirePkce = false
  - Works for our app type

## Section 7: Dockerizing our Application

Up to this point in developing and testing, we needed to open multiple terminal sessions and run the various projects locally, along with
their external dependencies (Rabbit, PostGRES, Mongo) separately in docker.

This is a nuisance to need to start up all projects individually, and it would be nice to only need to locally run the components that
we are currently developing.

### Dockerizing a Project

1. Create a Dockerfile ([example](/src/AuctionService/Dockerfile))
   1. Pull an image to build the project on (SDK), name it `build`
   2. Specify working directory
   3. Expose port to be used internally in docker
   4. Copy all solution files (this will be cached, making it fast, when repeated for other projects)
   5. Restore nuget packages
   6. Copy the app-specific source files over
   7. Set working directory
   8. Run the `dotnet publish` command
   9. Pull a runtime image (do not need full SDK)
   10. Set working directory
   11. Copy from `build` publish directory to our runtime image
   12. Specify entry point for startup
2. Update [docker-compose ](/docker-compose.yml)
   1. Give the service and image name/tag for docker, e.g. `willreichl/auction-svc:latest`
   2. Add `build` property with reference to the dockerfile we created
   3. Update environment variables
      1. Specify `ASPNETCORE_ENVIRONMENT`, usually still `Development`, but may change if you need a docker-specific appSettings.json file
      2. Specify `ASPNETCORE_URLS` as `http://+:80`, to be used internally for docker
      3. For any external dependencies, like Postgres, we can now just specify the name of the service in docker and the host will be mapped,
         e.g. `ConnectionStrings__DefaultConnection=Server=postgres:5432;` instead of `ConnectionStrings__DefaultConnection=Server=localhost:5432;`
         1. Note also that `:` in our local settings becomes `__` for nested values
      4. Specify port mappings for how we would access the service externally from docker
      5. In some cases, code tweaks may be required, like an environment switch on startup
3. Build the image, e.g. `docker compose build auction-svc`
   1. Repeat this upon any changes!

## Appendix A: Testing

### Introduction

"Test Double" - alternative to testing in your real database

- "Fake" - Has a working implementation, but is not the real implementation. Ex: an in-memory DB used in place of our real DB.
- "Mock" - Objects that return a programmed response when called a certain way. No working implementation.
- "Stub" - Like a mock, no working implementation, but just returns values when called.

We will use XUnit for this.
