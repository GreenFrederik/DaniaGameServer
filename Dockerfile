FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["DaniaGameServer/DaniaGameServer.csproj", "DaniaGameServer/"]
RUN dotnet restore "DaniaGameServer/DaniaGameServer.csproj"
COPY . .
WORKDIR "/src/DaniaGameServer"
RUN dotnet build "DaniaGameServer.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "DaniaGameServer.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 7777
ENTRYPOINT ["dotnet", "DaniaGameServer.dll"]
