FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
WORKDIR /app
EXPOSE 80

# Copy all .csproj files and restore as distinct layers. Use of the same COPY command
# for every dockerfile in the projet to take advantage of docker caching.
COPY Carsties.sln Carsties.sln
COPY src/AuctionService/AuctionService.csproj src/AuctionService/
COPY src/SearchService/SearchService.csproj src/SearchService/
COPY src/GatewayService/GatewayService.csproj src/GatewayService/
COPY src/IdentityService/IdentityService.csproj src/IdentityService/
COPY src/Contracts/Contracts.csproj src/Contracts/

# Restore package deps
RUN dotnet restore Carsties.sln

# Copy the app folders over
COPY src/IdentityService src/IdentityService
WORKDIR /app/src/IdentityService
RUN dotnet publish -c Release -o /app/src/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/src/out .
ENTRYPOINT [ "dotnet", "IdentityService.dll" ]