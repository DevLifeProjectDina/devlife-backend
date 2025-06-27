
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./
WORKDIR "/src/."
RUN dotnet build "DevLifeBackend.csproj" -c Release -o /app/build

FROM build-env AS publish
RUN dotnet publish "DevLifeBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevLifeBackend.dll"]