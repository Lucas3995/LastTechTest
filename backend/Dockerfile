# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/LastTechTest.Antecipacao.Api/LastTechTest.Antecipacao.Api.csproj", "LastTechTest.Antecipacao.Api/"]
COPY ["src/LastTechTest.Antecipacao.Application/LastTechTest.Antecipacao.Application.csproj", "LastTechTest.Antecipacao.Application/"]
COPY ["src/LastTechTest.Antecipacao.Domain/LastTechTest.Antecipacao.Domain.csproj", "LastTechTest.Antecipacao.Domain/"]
COPY ["src/LastTechTest.Antecipacao.Infrastructure/LastTechTest.Antecipacao.Infrastructure.csproj", "LastTechTest.Antecipacao.Infrastructure/"]

RUN dotnet restore "LastTechTest.Antecipacao.Api/LastTechTest.Antecipacao.Api.csproj"

COPY src/ .
WORKDIR "/src/LastTechTest.Antecipacao.Api"
RUN dotnet build "LastTechTest.Antecipacao.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "LastTechTest.Antecipacao.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LastTechTest.Antecipacao.Api.dll"]
