# Отчёт по практической работе с docker-compose
## Этапы работы:

В качестве сущности я выбрала товар со свойствами id, название, описание и цена. Очевидно, что в интернет-магазине необходимо иметь доступ к каталогу товаров и взаимодействовать с ним.

В docker-compose описаны два сервиса: база данных на образе PostgreSQL и api-приложение на .NET с каталогом товаров.

Я подключила базу данных, чтобы товары в каталоге сохранялись после добавления, редактирования и удаления после перезапуска приложения. (Ну а ещё обязательное использование Postgres прописано в задании). Подключение происходит через переменную окружения, в которой прописаны порт, название базы данных и имя пользователя.

Для Postgres заданы переменные для инициализации, для api – переменные для подключения к базе данных и обозначения прослушиваемого порта.
Все переменные окружения (логин, пароль, порт) я запрятала в файле .env и прописала в docker-compose только их названия.

docker-compose создаёт изолированную сеть, в которой api и БД "общаются" напрямую.

volume db_data сохраняет таблицы бд на устройство, благодаря чему данные сохраняются после перезапуска.


Сборка и запуск docker compose
<img width="1384" height="506" alt="Снимок экрана 2026-10-18 в 15 57 21" src="https://github.com/user-attachments/assets/414549a1-cbd7-4347-af06-ef7a58718c11" />

docker compose ps
<img width="2249" height="168" alt="Снимок экрана 2026-10-18 в 16 13 59" src="https://github.com/user-attachments/assets/b5acd146-5854-4599-b724-45554242b36e" />

CRUD-запросы

Чтобы было удобно осуществлять запросы, я подключила SwaggerUI.

<img width="2490" height="4599" alt="Screenshot 2026-10-18 at 15-58-41 Swagger UI" src="https://github.com/user-attachments/assets/7428b36a-407b-4287-a0c1-dbca9f780f3f" />
<img width="2103" height="706" alt="Снимок экрана 2026-10-18 в 16 06 16" src="https://github.com/user-attachments/assets/8f4aa8ad-e78f-4641-bbd9-38e79ae97e24" />
<img width="2104" height="887" alt="Снимок экрана 2026-10-18 в 16 08 59" src="https://github.com/user-attachments/assets/b613b201-a430-4f36-80d0-6f02faa5bb23" />
<img width="2111" height="961" alt="Снимок экрана 2026-10-18 в 16 09 43" src="https://github.com/user-attachments/assets/4150b75a-846b-4d10-ac17-8d0837b1eaec" />
<img width="2106" height="559" alt="Снимок экрана 2026-10-18 в 16 10 01" src="https://github.com/user-attachments/assets/7fc80778-41a2-4016-a6a3-de69067a2c97" />
<img width="2110" height="708" alt="Снимок экрана 2026-10-18 в 16 11 47" src="https://github.com/user-attachments/assets/f11f7cc6-8cc7-45ec-b474-4a7e555dfbc8" />
<img width="2101" height="533" alt="Снимок экрана 2026-10-18 в 16 12 30" src="https://github.com/user-attachments/assets/272a6f52-78b6-4a98-8667-271d08f946b3" />

docker compose logs
<img width="2490" height="1353" alt="Снимок экрана 2026-10-18 в 16 14 52" src="https://github.com/user-attachments/assets/1bd66ff8-ba24-4344-9b76-e88a671d9d56" />

Сохранение данных после перезапуска проверяла прямо в терминале в отдельном окне с помощью curl

На скриншоте:

1-й ввод команды — проверила каталог;

2-й ввод — остановила docker-compose (поэтому команда вывела ошибку);

3-й ввод — снова запустила docker-compose и заново проверила каталог.
<img width="2180" height="242" alt="Снимок экрана 2026-10-18 в 16 36 25" src="https://github.com/user-attachments/assets/0d5682d2-7f2a-4259-bec6-d103c03f96db" />
