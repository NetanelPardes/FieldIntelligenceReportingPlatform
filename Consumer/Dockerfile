FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY Consumer/Consumer.csproj ./Consumer/

RUN dotnet restore ./Consumer/Consumer.csproj

COPY Consumer/ ./Consumer/

RUN dotnet publish ./Consumer/Consumer.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    -p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/runtime:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Consumer.dll"]