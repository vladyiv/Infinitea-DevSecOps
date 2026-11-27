FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["GatewayService/GatewayService.csproj", "GatewayService/"]
WORKDIR /src/GatewayService
RUN dotnet restore "GatewayService.csproj"

COPY GatewayService/ .
RUN dotnet publish "GatewayService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GatewayService.dll"]
