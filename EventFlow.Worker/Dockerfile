# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy solution and project files
COPY EventFlow.slnx ./

COPY EventFlow.Worker/EventFlow.Worker.csproj EventFlow.Worker/
COPY EventFlow.Application/EventFlow.Application.csproj EventFlow.Application/
COPY EventFlow.Domain/EventFlow.Domain.csproj EventFlow.Domain/
COPY EventFlow.Infrastructure/EventFlow.Infrastructure.csproj EventFlow.Infrastructure/

# Restore dependencies
RUN dotnet restore EventFlow.Worker/EventFlow.Worker.csproj

# Copy source code
COPY EventFlow.Worker/ EventFlow.Worker/
COPY EventFlow.Application/ EventFlow.Application/
COPY EventFlow.Domain/ EventFlow.Domain/
COPY EventFlow.Infrastructure/ EventFlow.Infrastructure/

# Build
RUN dotnet build EventFlow.Worker/EventFlow.Worker.csproj \
    -c Release \
    --no-restore

# Publish
RUN dotnet publish EventFlow.Worker/EventFlow.Worker.csproj \
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

ENTRYPOINT ["dotnet", "EventFlow.Worker.dll"]