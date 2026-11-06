FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# Both the SQLite file and the uploaded product images live under /data so a
# single mounted volume keeps them in sync. If only one of them is persisted,
# Product.ImagePath rows end up pointing at files that no longer exist.
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__Default="Data Source=/data/readyerp.db" \
    Storage__UploadPath="/data/uploads"
EXPOSE 8080
# Pre-create and chown as root, before dropping privileges. Creating the
# uploads directory here also means a fresh named volume inherits the correct
# ownership, because Docker seeds a new named volume from the image directory.
RUN mkdir -p /data/uploads && chown -R "$APP_UID:$APP_UID" /data
VOLUME ["/data"]
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ReadyErp.Api.csproj", "./"]
RUN dotnet restore "ReadyErp.Api.csproj"
COPY . .
RUN dotnet publish "ReadyErp.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ReadyErp.Api.dll"]