FROM debian:bookworm-slim AS build
WORKDIR /src

RUN apt-get update && apt-get install -y wget ca-certificates libicu-dev \
    && update-ca-certificates

RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh \
    && chmod +x dotnet-install.sh \
    && ./dotnet-install.sh --channel 8.0

ENV PATH="$PATH:/root/.dotnet"
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

WORKDIR /src/SmartTasksAPI

COPY . .


RUN dotnet restore "SmartTasksAPI.csproj"
RUN dotnet publish "SmartTasksAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM debian:bookworm-slim
WORKDIR /app

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

RUN apt-get update && apt-get install -y wget ca-certificates \
    && update-ca-certificates \
    && wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh \
    && chmod +x dotnet-install.sh \
    && ./dotnet-install.sh --channel 8.0 --runtime aspnetcore

ENV PATH="$PATH:/root/.dotnet"

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "SmartTasksAPI.dll"]