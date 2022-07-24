FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY . .
WORKDIR "/src/TelegramUploader"
RUN dotnet restore "TelegramUploader.csproj"
RUN dotnet build "TelegramUploader.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelegramUploader.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramUploader.dll"]
