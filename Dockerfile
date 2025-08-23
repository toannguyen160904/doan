# ---------- Stage 1: Build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project folders
COPY SharedModels/ SharedModels/
COPY doan/ doan/

# Restore + publish web app
RUN dotnet restore doan/doan.csproj
RUN dotnet publish doan/doan.csproj -c Release -o /app/out

# ---------- Stage 2: Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .

# Bind port do Railway cấp
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "doan.dll"]
