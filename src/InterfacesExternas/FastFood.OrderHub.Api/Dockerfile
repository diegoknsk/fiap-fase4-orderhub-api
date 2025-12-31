# Etapa 1: build da aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copiar arquivos .csproj e fazer restore (otimização de cache)
COPY src/Core/FastFood.OrderHub.Domain/FastFood.OrderHub.Domain.csproj src/Core/FastFood.OrderHub.Domain/
COPY src/Core/FastFood.OrderHub.Application/FastFood.OrderHub.Application.csproj src/Core/FastFood.OrderHub.Application/
COPY src/Core/FastFood.OrderHub.CrossCutting/FastFood.OrderHub.CrossCutting.csproj src/Core/FastFood.OrderHub.CrossCutting/
COPY src/Infra/FastFood.OrderHub.Infra/FastFood.OrderHub.Infra.csproj src/Infra/FastFood.OrderHub.Infra/
COPY src/Infra/FastFood.OrderHub.Infra.Persistence/FastFood.OrderHub.Infra.Persistence.csproj src/Infra/FastFood.OrderHub.Infra.Persistence/
COPY src/InterfacesExternas/FastFood.OrderHub.Api/FastFood.OrderHub.Api.csproj src/InterfacesExternas/FastFood.OrderHub.Api/

RUN dotnet restore src/InterfacesExternas/FastFood.OrderHub.Api/FastFood.OrderHub.Api.csproj

# Copiar todo o código
COPY . .

# Publicar a aplicação
RUN dotnet publish src/InterfacesExternas/FastFood.OrderHub.Api/FastFood.OrderHub.Api.csproj -c Release -o /app/publish \
    /p:CopyOutputSymbolsToPublishDirectory=false \
    /p:CopyOutputXmlDocumentationToPublishDirectory=false

# Etapa 2: imagem de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80

EXPOSE 80

ENTRYPOINT ["dotnet", "FastFood.OrderHub.Api.dll"]

