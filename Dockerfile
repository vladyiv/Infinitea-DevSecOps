FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY Infinitea.Api/Infinitea.Api.csproj Infinitea.Api/

RUN dotnet restore Infinitea.Api/Infinitea.Api.csproj

COPY Infinitea.Api/ Infinitea.Api/

RUN dotnet publish Infinitea.Api/Infinitea.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Infinitea.Api.dll"]
