FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
# Install system dependencies, including ping
RUN apt-get update && apt-get install -y iputils-ping
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ServerMonitor/ServerMonitor.csproj", "ServerMonitor/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "ServerMonitor/ServerMonitor.csproj"
COPY . .
WORKDIR "/src/ServerMonitor"
RUN dotnet build "ServerMonitor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ServerMonitor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerMonitor.dll"]
