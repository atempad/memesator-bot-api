FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
EXPOSE 80

RUN apt-get update
RUN apt-get install -y ffmpeg
RUN apt-get clean
ENV AppSettings__FFmpegPath=/usr/bin/ffmpeg

RUN apt-get install -y wget libxss1 libgconf-2-4 gnupg2
RUN wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | gpg --dearmor -o /usr/share/keyrings/google-chrome-keyring.gpg
RUN echo "deb [arch=amd64 signed-by=/usr/share/keyrings/google-chrome-keyring.gpg] https://dl.google.com/linux/chrome/deb/ stable main" | tee /etc/apt/sources.list.d/google-chrome.list > /dev/null
RUN apt-get update && apt-get install -y google-chrome-stable
RUN apt-get clean && rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*
ENV PUPPETEER_EXECUTABLE_PATH=/usr/bin/google-chrome-stable

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
