FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /src
#EXPOSE 8099
#EXPOSE 443


# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0 as runtime

# Uncomment the line below if running with HTTPS
#ENV ASPNETCORE_URLS=https://+:443
#ENV ASPNETCORE_URLS=http://+:80

WORKDIR /app
COPY --from=build /src/out .

#COPY ["servercert.pfx", "/https/servercert.pfx"]
#COPY ["cacert.crt", "/usr/local/share/ca-certificates/cacert.crt"]
RUN update-ca-certificates

ENTRYPOINT ["dotnet", "GrpcGreeter.dll"]