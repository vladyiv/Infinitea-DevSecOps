FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["OrdersService/OrdersService.csproj", "OrdersService/"]
WORKDIR /src/OrdersService
RUN dotnet restore "OrdersService.csproj"

COPY OrdersService/ .
RUN dotnet publish "OrdersService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrdersService.dll"]
