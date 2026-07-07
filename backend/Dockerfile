# Multi-stage build: SDK image compiles and publishes, runtime image stays lean
# and runs as the non-root 'app' user.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Restore layers cache as long as project files are unchanged.
COPY Directory.Build.props Directory.Packages.props ./
COPY src/Meridian.Domain/Meridian.Domain.csproj src/Meridian.Domain/
COPY src/Meridian.Application/Meridian.Application.csproj src/Meridian.Application/
COPY src/Meridian.Infrastructure/Meridian.Infrastructure.csproj src/Meridian.Infrastructure/
COPY src/Meridian.Api/Meridian.Api.csproj src/Meridian.Api/
RUN dotnet restore src/Meridian.Api/Meridian.Api.csproj

COPY src/ src/
RUN dotnet publish src/Meridian.Api/Meridian.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "Meridian.Api.dll"]
