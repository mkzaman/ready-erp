FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__Default="Data Source=/data/readyerp.db"
EXPOSE 8080
RUN mkdir -p /data && chown "$APP_UID:$APP_UID" /data
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