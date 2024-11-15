FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LocalStatusChecker/LocalStatusChecker.csproj", "LocalStatusChecker/"]
RUN dotnet restore "LocalStatusChecker/LocalStatusChecker.csproj"
COPY . .
WORKDIR "/src/LocalStatusChecker"
RUN dotnet build "LocalStatusChecker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LocalStatusChecker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LocalStatusChecker.dll"]
