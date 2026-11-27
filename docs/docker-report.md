# Отчёт по практической работе с контейнером
## Этапы работы:

Создание файлов API и sln-решения

<img width="983" height="103" alt="Снимок экрана 2026-10-05 в 21 30 34" src="https://github.com/user-attachments/assets/e637e27f-6cb1-4b0e-b3ea-9aa0bf888450" />

Запуск локально командой dotnet run через порт 5000 с откликом в терминале

<img width="1542" height="295" alt="Снимок экрана 2026-10-05 в 11 27 33" src="https://github.com/user-attachments/assets/968c4d1a-314c-4d52-9e9e-012df34142aa" />

Сборка образа docker build с флажком -t, чтобы задать ему понятное имя

<img width="2494" height="911" alt="Снимок экрана 2026-10-05 в 20 36 08" src="https://github.com/user-attachments/assets/f4c79d89-90c1-42b5-925a-ae9eba00b5ba" />

Запуск Dockerfile через docker run и его отображение среди образов (команда docker ps)
- Флажок -d для запуска в фоновом режиме;
- --rm для автоматического удаления контейнера по завершении работы;
- --name для задания имени контейнера; 
- -p для указания порта 8080.

<img width="2243" height="231" alt="Снимок экрана 2026-10-08 в 17 08 23" src="https://github.com/user-attachments/assets/04478f8b-22f8-4066-bddb-7b68b7186f7a" />

Отклики в терминале (некрасиво)

<img width="1069" height="260" alt="Снимок экрана 2026-10-05 в 20 09 48" src="https://github.com/user-attachments/assets/87ceb960-5023-43d8-8c37-a3df87f8ce38" />

Отклики в браузере (красиво)

<img width="803" height="371" alt="Снимок экрана 2026-10-05 в 11 28 32" src="https://github.com/user-attachments/assets/6edbc508-71fc-4335-88fe-192075ba2c18" />
<img width="845" height="859" alt="Снимок экрана 2026-10-05 в 11 28 20" src="https://github.com/user-attachments/assets/69032f1b-6bab-4b77-a681-d250892c038e" />
