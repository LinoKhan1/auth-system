# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the .csproj from the server folder
COPY ["server/server.csproj", "./"]
RUN dotnet restore "server.csproj"

# Copy all other source files from the server folder
COPY server/. ./
RUN dotnet build "server.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
RUN dotnet publish "server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080
COPY --from=publish /app/publish ./
ENTRYPOINT ["dotnet", "server.dll"]