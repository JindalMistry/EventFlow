# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy solution and project files first
COPY EventFlow.slnx ./

COPY EventFlow.API/EventFlow.API.csproj EventFlow.API/
COPY EventFlow.Application/EventFlow.Application.csproj EventFlow.Application/
COPY EventFlow.Domain/EventFlow.Domain.csproj EventFlow.Domain/
COPY EventFlow.Infrastructure/EventFlow.Infrastructure.csproj EventFlow.Infrastructure/

# Restore dependencies
RUN dotnet restore EventFlow.API/EventFlow.API.csproj

# Copy the remaining source code
COPY EventFlow.API/ EventFlow.API/
COPY EventFlow.Application/ EventFlow.Application/
COPY EventFlow.Domain/ EventFlow.Domain/
COPY EventFlow.Infrastructure/ EventFlow.Infrastructure/

# Build
RUN dotnet build EventFlow.API/EventFlow.API.csproj \
    -c Release \
    --no-restore

# Publish
RUN dotnet publish EventFlow.API/EventFlow.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "EventFlow.API.dll"]