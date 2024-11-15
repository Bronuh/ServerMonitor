# Unstable Server Monitoring System Wrapped in Docker
> Why can't you just work?

A quick fix written by me to monitor my unstable server. It requires an additional, but stable, server to function.
The system consists of two parts:
- **Exposer** - the component for the unstable server, responsible for providing data to the monitor.
- **Monitor** - the component for the stable server, responsible for checking the status of the unstable server and sending notifications to Telegram.

> [!NOTE]
> Before using the solution, you need to create a bot using [@BotFather](https://t.me/botfather) and also obtain your `chat_id` from [@userinfobot](https://t.me/userinfobot).
> After creating the bot, you must start it by sending the `/start` command in a chat with it.
> Additionally, **Docker** with **Compose** is required on both systems.

## Getting Started
1. Clone the repository to a convenient location on both machines.
```bash
git clone https://github.com/Bronuh/ServerMonitor.git
```
2. Navigate to the repository directory
```bash
cd ServerMonitor/
```
3. It is recommended to prepare the *.env* file in advance
```bash
cp .env.template .env
```

## Installing and Configuring **Exposer** on the Unstable Server
Before running, it is advisable to configure the port for Exposer in the *.env* file.
To start **Exposer**, simply run the following command inside the repository directory:
```bash
./app exposer start
```
After building and starting, it will be accessible at `http://localhost:5015/online`
Currently, it does not return anything except an empty response with a status of 200 OK.

## Installing and Configuring Monitor on the Stable Server
Before starting, it is **REQUIRED** to configure the service in the *.env* file
- **SERVER_URL** - should point to any HTTP endpoint on the unstable server that returns 200 OK.
  - This is exactly what **Exposer** does.
- **PING_TARGET** - should point to the unstable server or the main network gateway of the network in which it is located.
- **CHECK_INTERVAL** - the check interval, in seconds.
- **TELEGRAM_TOKEN** - your Telegram bot token.
- **TELEGRAM_CHAT_ID** - the chat_id of the user who will receive notifications about the server and network status.

To start **Monitor**, simply run the following command inside the repository directory:
```bash
./app monitor start
```

## Working with ./app
The `./app` file makes it easier to manage services within the repository. A basic call to `./app` without arguments will display a list of available services and actions for them.
Command syntax: `./app <service> <action> [args]`
Currently available services:
- exposer
- monitor
- all - for debugging, starts both services

Actions:
- start - starts the service
- stop - stops the service
- restart - restarts the service. The --hard flag will also remove the existing service image.
- update - stops the service, removes the existing image, runs `git pull`, and starts the service
