FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["UsersService/UsersService.csproj", "UsersService/"]
WORKDIR /src/UsersService
RUN dotnet restore "UsersService.csproj"

COPY UsersService/ .
RUN dotnet publish "UsersService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UsersService.dll"]
