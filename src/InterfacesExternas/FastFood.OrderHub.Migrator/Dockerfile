# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto (.csproj) primeiro para otimizar cache
COPY ["src/Core/FastFood.OrderHub.Domain/FastFood.OrderHub.Domain.csproj", "src/Core/FastFood.OrderHub.Domain/"]
COPY ["src/Core/FastFood.OrderHub.Application/FastFood.OrderHub.Application.csproj", "src/Core/FastFood.OrderHub.Application/"]
COPY ["src/Core/FastFood.OrderHub.CrossCutting/FastFood.OrderHub.CrossCutting.csproj", "src/Core/FastFood.OrderHub.CrossCutting/"]
COPY ["src/Infra/FastFood.OrderHub.Infra/FastFood.OrderHub.Infra.csproj", "src/Infra/FastFood.OrderHub.Infra/"]
COPY ["src/Infra/FastFood.OrderHub.Infra.Persistence/FastFood.OrderHub.Infra.Persistence.csproj", "src/Infra/FastFood.OrderHub.Infra.Persistence/"]
COPY ["src/InterfacesExternas/FastFood.OrderHub.Migrator/FastFood.OrderHub.Migrator.csproj", "src/InterfacesExternas/FastFood.OrderHub.Migrator/"]

# Restaurar dependências
RUN dotnet restore "src/InterfacesExternas/FastFood.OrderHub.Migrator/FastFood.OrderHub.Migrator.csproj"

# Copiar todo o código fonte
COPY . .

# Preparar migrações: criar pasta temporária e copiar migrações se existirem
RUN mkdir -p /migrations && \
    if [ -d "src/Infra/FastFood.OrderHub.Infra.Persistence/Migrations" ] && [ "$(ls -A src/Infra/FastFood.OrderHub.Infra.Persistence/Migrations 2>/dev/null)" ]; then \
        cp -r src/Infra/FastFood.OrderHub.Infra.Persistence/Migrations/* /migrations/; \
    else \
        touch /migrations/.keep; \
    fi

# Publicar aplicação com otimizações
WORKDIR "/src/src/InterfacesExternas/FastFood.OrderHub.Migrator"
RUN dotnet publish "FastFood.OrderHub.Migrator.csproj" \
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

# Copiar appsettings.json
COPY --from=build /src/src/InterfacesExternas/FastFood.OrderHub.Migrator/appsettings.json .

# Copiar migrações (sempre terá pelo menos o arquivo .keep)
COPY --from=build /migrations ./Migrations

# Segurança: Rodar como não-root
RUN chown -R 1001:1001 /app
USER 1001:1001

# Configurar variáveis de ambiente
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_ENVIRONMENT=Production

# Entrypoint
ENTRYPOINT ["dotnet", "FastFood.OrderHub.Migrator.dll"]

