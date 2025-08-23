# ---------- Stage 1: Build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Chỉ copy đúng các project cần cho web app 'doan'
# (chú ý đúng chữ hoa/thường)
COPY SharedModels/ SharedModels/
COPY doan/ doan/

# Restore & publish đúng file csproj (tránh .sln)
RUN dotnet restore doan/doan.csproj
RUN dotnet publish doan/doan.csproj -c Release -o /app/out

# ---------- Stage 2: Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Railway cấp PORT động
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
ENTRYPOINT ["dotnet", "doan.dll"]
