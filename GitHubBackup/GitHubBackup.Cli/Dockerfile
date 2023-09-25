FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["GitHubBackup.Cli/GitHubBackup.Cli.csproj", "GitHubBackup.Cli/"]
RUN dotnet restore "GitHubBackup.Cli/GitHubBackup.Cli.csproj"
COPY . .
WORKDIR "/src/GitHubBackup.Cli"
RUN dotnet build "GitHubBackup.Cli.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GitHubBackup.Cli.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GitHubBackup.Cli.dll"]
