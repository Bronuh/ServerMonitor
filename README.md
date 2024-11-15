# Система мониторинга нестабильных серверов, завернутая в Docker
> Почему ты не можешь просто работать?

Костыль, написанный мной для мониторинга моего нестабильного сервера. Для работает требует наличия еще одного сервера, но стабильного.
Состоит из двух частей:
- **Exposer** - часть для нестабильного сервера, отвечающая за предоставлении данных для монитора.
- **Monitor** - часть для стабильного сервера, отвечающая за проверку состояния нестабильного сервера и отправку уведомлений в Telegram.

> [!NOTE]
> Прежде чем начать пользоваться решением необходимо создать бота в [@BotFather](https://t.me/botfather) а также получить свой `chat_id` в [@userinfobot](https://t.me/userinfobot).
> После создания бота необходимо запустить его командой `/start` в беседе с ним.
> Также для работы на обеих системах требуется **Docker** с **Compose**.

## Подготовка к работе
1. необходимо склонировать репозиторий в удобное место на обеих машинах.
```bash
git clone https://github.com/Bronuh/ServerMonitor.git
```
2. перейти в директорию репозитория
```bash
cd ServerMonitor/
```
3. рекомендуется заранее подготовить *.env* файл 
```bash
cp .env.template .env
```

## Установка и настройка **Exposer** на нестабильном сервере
Перед запуском желательно сконфигурировать порт для Exposer в *.env* файле.
Для запуска **Exposer** достаточно выполнить команду внутри директории репозитория:
```bash
./app exposer start
```
После сборки и запуска он будет доступен по адресу `http://localhost:5015/online`
В данный момент он не возвращает ничего кроме пустого ответа со статусом 200 OK.

## Установка и настройка Monitor на стабильном сервере
Перед запуском **НЕОБХОДИМО** сконфигурировать сервис в *.env* файле
- **SERVER_URL** - должен указывать на любой HTTP эндпоинт на нестабильном сервере, который вернет 200 OK.
  - Именно это делает **Exposer**.
- **PING_TARGET** - должен указывать на нестабильный сервер или основной сетевой шлюз сети, в которой он находится.
- **CHECK_INTERVAL** - интервал проверок, в секундах.
- **TELEGRAM_TOKEN** - токен вашего Telegram бота.
- **TELEGRAM_CHAT_ID** - chat_id пользователя, которому будут отправляться уведомления о состоянии сервера и сети.

Для запуска **Exposer** достаточно выполнить команду внутри директории репозитория:
```bash
./app monitor start
```

## Работа с ./app
Файл `./app` позволяет легче управлять сервисами с репозитории. Обычный вызов `./app` без аргументов выведет список доступных сервисов и действий над ними.
Синтаксис команды: `./app <service> <action> [args]`
На данный момент доступны сервисы:
- exposer
- monitor
- all - для отладки, запускает оба сервиса

Действия:
- start - запускает сервис
- stop - останавливает сервис
- restart - перезапускает сервис. Ключ --hard также удалит существующий образ сервиса.
- update - остановит сервис, удалит существующий образ, выполнит `git pull` и запустит сервис