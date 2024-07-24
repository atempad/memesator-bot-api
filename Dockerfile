# BASE
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

EXPOSE 80

RUN apt-get update && apt-get install -y \
    apt-utils \
    ffmpeg unzip xvfb libxi6 libgconf-2-4 jq libjq1 libonig5 libxkbcommon0 libxss1 \
    libglib2.0-0 libnss3 libfontconfig1 libatk-bridge2.0-0 libatspi2.0-0 libgtk-3-0 \
    libpango-1.0-0 libgdk-pixbuf2.0-0 libxcomposite1 libxcursor1 libxdamage1 libxtst6 \
    libappindicator3-1 libasound2 libatk1.0-0 libc6 libcairo2 libcups2 libxfixes3 \
    libdbus-1-3 libexpat1 libgcc1 libnspr4 libgbm1 libpangocairo-1.0-0 libstdc++6 \
    libx11-6 libx11-xcb1 libxcb1 libxext6 libxrandr2 libxrender1 gconf-service \
    ca-certificates fonts-liberation libappindicator1 lsb-release xdg-utils curl wget \
    chromium pciutils && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

ENV AppSettings__FFmpegPath=/usr/bin/ffmpeg
ENV PUPPETEER_EXECUTABLE_PATH=/usr/bin/chromium

USER $APP_UID
WORKDIR /app

# BUILD
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim-amd64 AS build

ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
ARG TARGETOS

RUN arch=$TARGETARCH \
    && if [ "$arch" = "amd64" ]; then arch="x64"; fi \
    && echo $TARGETOS-$arch > /tmp/rid

WORKDIR /src

COPY ["memesator-bot-api.csproj", "./"]
RUN dotnet restore "memesator-bot-api.csproj" -r $(cat /tmp/rid)
COPY . .
WORKDIR "/src/"
RUN dotnet build "memesator-bot-api.csproj" -c Release -o /app/build -r $(cat /tmp/rid) --self-contained false --no-restore

# PUBLISH
FROM build AS publish

RUN dotnet publish "memesator-bot-api.csproj" -c Release -o /app/publish -r $(cat /tmp/rid) --self-contained false --no-restore

# FINAL
FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "memesator-bot-api.dll"]
