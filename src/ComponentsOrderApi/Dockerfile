FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ComponentsOrderApi.csproj", "./"]
RUN dotnet restore "./ComponentsOrderApi.csproj"

COPY . .
RUN dotnet publish "./ComponentsOrderApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

COPY --from=build /app/publish .

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000} dotnet ComponentsOrderApi.dll"]
