# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY SharedModels/SharedModels.csproj SharedModels/
COPY doanapi/doanapi.csproj doanapi/
RUN dotnet restore doanapi/doanapi.csproj

COPY SharedModels/ SharedModels/
COPY doanapi/ doanapi/
WORKDIR /src/doanapi
RUN dotnet publish doanapi.csproj -c Release -o /app/out /p:UseAppHost=false

# ---------- Runtime (bookworm-slim) ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "doanapi.dll"]
