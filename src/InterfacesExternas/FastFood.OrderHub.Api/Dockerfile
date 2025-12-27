# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto (.csproj) primeiro para otimizar cache
COPY ["src/Core/FastFood.OrderHub.Domain/FastFood.OrderHub.Domain.csproj", "src/Core/FastFood.OrderHub.Domain/"]
COPY ["src/Core/FastFood.OrderHub.Application/FastFood.OrderHub.Application.csproj", "src/Core/FastFood.OrderHub.Application/"]
COPY ["src/Core/FastFood.OrderHub.CrossCutting/FastFood.OrderHub.CrossCutting.csproj", "src/Core/FastFood.OrderHub.CrossCutting/"]
COPY ["src/Infra/FastFood.OrderHub.Infra/FastFood.OrderHub.Infra.csproj", "src/Infra/FastFood.OrderHub.Infra/"]
COPY ["src/Infra/FastFood.OrderHub.Infra.Persistence/FastFood.OrderHub.Infra.Persistence.csproj", "src/Infra/FastFood.OrderHub.Infra.Persistence/"]
COPY ["src/InterfacesExternas/FastFood.OrderHub.Api/FastFood.OrderHub.Api.csproj", "src/InterfacesExternas/FastFood.OrderHub.Api/"]

# Restaurar dependências
RUN dotnet restore "src/InterfacesExternas/FastFood.OrderHub.Api/FastFood.OrderHub.Api.csproj"

# Copiar todo o código fonte
COPY . .

# Publicar aplicação com otimizações
WORKDIR "/src/src/InterfacesExternas/FastFood.OrderHub.Api"
RUN dotnet publish "FastFood.OrderHub.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:CopyOutputSymbolsToPublishDirectory=false \
    /p:CopyOutputXmlDocumentationToPublishDirectory=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar artefatos publicados
COPY --from=build /app/publish .

# Segurança: Rodar como não-root
RUN chown -R 1001:1001 /app
USER 1001:1001

# Configurar variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Expor porta 80
EXPOSE 80

# Entrypoint
ENTRYPOINT ["dotnet", "FastFood.OrderHub.Api.dll"]

