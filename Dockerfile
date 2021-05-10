FROM mcr.microsoft.com/dotnet/sdk:5.0.102-ca-patch-buster-slim-amd64 AS build-env
WORKDIR /app
EXPOSE 443  
EXPOSE 80 

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "GrpcGreeter.dll"]