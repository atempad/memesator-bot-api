FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
EXPOSE 80

RUN apt-get update && apt-get install -y \
    ffmpeg unzip xvfb libxi6 libgconf-2-4 jq libjq1 libonig5 libxkbcommon0 libxss1 \
    libglib2.0-0 libnss3 libfontconfig1 libatk-bridge2.0-0 libatspi2.0-0 libgtk-3-0 \
    libpango-1.0-0 libgdk-pixbuf2.0-0 libxcomposite1 libxcursor1 libxdamage1 libxtst6 \
    libappindicator3-1 libasound2 libatk1.0-0 libc6 libcairo2 libcups2 libxfixes3 \
    libdbus-1-3 libexpat1 libgcc1 libnspr4 libgbm1 libpangocairo-1.0-0 libstdc++6 \
    libx11-6 libx11-xcb1 libxcb1 libxext6 libxrandr2 libxrender1 gconf-service \
    ca-certificates fonts-liberation libappindicator1 lsb-release xdg-utils curl wget && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* && \
    if [ "$(uname -m)" = "x86_64" ]; then \
      LATEST_CHROME_RELEASE=$(curl -s https://googlechromelabs.github.io/chrome-for-testing/last-known-good-versions-with-downloads.json | jq '.channels.Stable') && \
      LATEST_CHROME_URL=$(echo "$LATEST_CHROME_RELEASE" | jq -r '.downloads.chrome[] | select(.platform == "linux64") | .url') && \
      wget -N "$LATEST_CHROME_URL" -P /tmp/ && \
      unzip /tmp/chrome-linux64.zip -d /opt/ && \
      mv /opt/chrome-linux64 /opt/chrome && \
      chmod +x /opt/chrome && \
      rm /tmp/chrome-linux64.zip && \
      export PUPPETEER_EXECUTABLE_PATH=/opt/chrome/chrome ; \
    elif [ "$(uname -m)" = "aarch64" ]; then \
      apt-get update && apt-get install -y chromium-browser && \
      export PUPPETEER_EXECUTABLE_PATH=/usr/bin/chromium-browser ; \
    fi

ENV AppSettings__FFmpegPath=/usr/bin/ffmpeg
ENV PUPPETEER_EXECUTABLE_PATH=${PUPPETEER_EXECUTABLE_PATH}

USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["memesator-bot-api.csproj", "./"]
RUN dotnet restore "memesator-bot-api.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "memesator-bot-api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "memesator-bot-api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "memesator-bot-api.dll"]
