# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj trước để cache restore
COPY SharedModels/SharedModels.csproj SharedModels/
COPY doanapi/doanapi.csproj doanapi/

# Restore theo project API (tự kéo ProjectReference)
RUN dotnet restore doanapi/doanapi.csproj

# Copy toàn bộ source
COPY SharedModels/ SharedModels/
COPY doanapi/ doanapi/

# Publish
WORKDIR /src/doanapi
RUN dotnet publish doanapi.csproj -c Release -o /app/out /p:UseAppHost=false

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "doanapi.dll"]
