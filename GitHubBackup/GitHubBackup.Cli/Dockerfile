FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["GithubBackup.Cli/GithubBackup.Cli.csproj", "GithubBackup.Cli/"]
RUN dotnet restore "GithubBackup.Cli/GithubBackup.Cli.csproj"
COPY . .
WORKDIR "/src/GithubBackup.Cli"
RUN dotnet build "GithubBackup.Cli.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GithubBackup.Cli.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GithubBackup.Cli.dll"]
