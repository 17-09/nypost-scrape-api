FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY "./JobScrape/JobScrape.csproj" "JobScrape/"
RUN dotnet restore -s https://api.nuget.org/v3/index.json "JobScrape/JobScrape.csproj"
COPY . .
WORKDIR /src/JobScrape
RUN dotnet build "JobScrape.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "JobScrape.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "JobScrape.dll"]
