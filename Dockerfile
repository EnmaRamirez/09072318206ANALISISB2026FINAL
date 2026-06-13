FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["NetGuardGT.Api/NetGuardGT.Api.csproj", "NetGuardGT.Api/"]
RUN dotnet restore "NetGuardGT.Api/NetGuardGT.Api.csproj"

COPY . .
RUN dotnet publish "NetGuardGT.Api/NetGuardGT.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["sh", "-c", "dotnet NetGuardGT.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
