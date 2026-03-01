# =========================
# Stage 0: Build SDK & Restore
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["server/server.csproj", "./"]
RUN dotnet restore "server.csproj"

# Copy full source including migrations
COPY server/. ./

# Build & publish (includes all folders in the assembly)
RUN dotnet publish "server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =========================
# Stage 1: Runtime (SDK for EF CLI)
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS final
WORKDIR /app

# Install EF CLI globally
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

EXPOSE 8080

# Copy published app (includes the compiled Migrations assembly)
COPY --from=build /app/publish ./

# Optional: explicitly copy migrations folder in case EF needs it at runtime
COPY server/Infrastructure/Data/Migrations ./Infrastructure/Data/Migrations

ENTRYPOINT ["dotnet", "server.dll"]