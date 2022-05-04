FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["TelegramGroupFileDownloader/TelegramGroupFileDownloader.csproj", "TelegramGroupFileDownloader/"]
RUN dotnet restore "TelegramGroupFileDownloader/TelegramGroupFileDownloader.csproj"
COPY . .
WORKDIR "/src/TelegramGroupFileDownloader"
RUN dotnet build "TelegramGroupFileDownloader.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelegramGroupFileDownloader.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramGroupFileDownloader.dll"]
