# сборка
# беру официальный образ Microsoft с .NET SDK и называю этот этап build, дальше он нам пригодится
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# назначаю рабочую директорию
WORKDIR /src

# копирую только файл проекта, чтобы не скачивать зависимости проекта заново, если они не изменились
COPY Infinitea.Api/Infinitea.Api.csproj Infinitea.Api/

# восстанавливаю/скачиваю зависимости проекта
RUN dotnet restore Infinitea.Api/Infinitea.Api.csproj

# теперь копирую весь проект
COPY Infinitea.Api/ Infinitea.Api/

# собираю приложение
RUN dotnet publish Infinitea.Api/Infinitea.Api.csproj \ 
    -c Release \
    -o /app/publish \
    # ставлю --no-restore, потому что restore уже был выполнен выше
    --no-restore

# запуск
# беру официальный образ Microsoft с ASP.NET Runtime для запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0
# задаю рабочую директорию
WORKDIR /app
# беру готовое приложение build из самой первой строки
COPY --from=build /app/publish .
# указываю порт 8080, который используется внутри контейнера
EXPOSE 8080
# указываю, на каком порту нужно принимать запросы
ENV ASPNETCORE_URLS=http://+:8080
# пишу, что именно нужно выполнить при запуске образа
ENTRYPOINT ["dotnet", "Infinitea.Api.dll"]
