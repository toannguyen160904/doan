# ---------- Stage 1: Build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy dự án và SharedModels
COPY ../SharedModels/ SharedModels/
COPY . doanapi/

# Restore & Publish
WORKDIR /src/doanapi
RUN dotnet restore doanapi.csproj
RUN dotnet publish doanapi.csproj -c Release -o /app/out /p:UseAppHost=false

# ---------- Stage 2: Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENV PORT=8080
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
EXPOSE 8080
ENTRYPOINT ["dotnet", "doanapi.dll"]
