# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore dependencies
COPY ["LiveChat.sln", "./"]
COPY ["LiveChat/LiveChat.csproj", "LiveChat/"]
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet build "LiveChat.sln" -c Release -o /app/build

# 2. Publish Stage
FROM build AS publish
RUN dotnet publish "LiveChat.sln" -c Release -o /app/publish /p:UseAppHost=false

# 3. Final Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Railway uses the PORT env var, .NET needs to listen to it
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "LiveChat.dll"]
