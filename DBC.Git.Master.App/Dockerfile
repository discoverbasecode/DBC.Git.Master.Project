FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DBC.Git.Master.App/DBC.Git.Master.App.csproj", "DBC.Git.Master.App/"]
RUN dotnet restore "DBC.Git.Master.App/DBC.Git.Master.App.csproj"
COPY . .
WORKDIR "/src/DBC.Git.Master.App"
RUN dotnet build "./DBC.Git.Master.App.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DBC.Git.Master.App.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DBC.Git.Master.App.dll"]
