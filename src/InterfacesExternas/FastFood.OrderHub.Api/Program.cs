using Amazon.DynamoDBv2;
using FastFood.OrderHub.Infra.Auth;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Infra.Persistence.Configurations;
using FastFood.OrderHub.Infra.Persistence.DataSources;
using FastFood.OrderHub.Infra.Persistence.Repositories;
using FastFood.OrderHub.Infra.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

// Configurar JWT Security Token Handler
JwtAuthenticationConfig.ConfigureJwtSecurityTokenHandler();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adicionar IHttpContextAccessor para RequestContext
builder.Services.AddHttpContextAccessor();

// Registrar RequestContext
builder.Services.AddScoped<IRequestContext, RequestContext>();

// Configure JWT options
builder.Services.Configure<JwtOptions>("Customer", builder.Configuration.GetSection("JwtCustomer"));

// Configure authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddCustomerJwtBearer(builder.Configuration)
    .AddCognitoJwtBearer(builder.Configuration);

// Configure authorization policies
builder.Services.AddAuthorizationPolicies();

// Configurar DynamoDB
// O AddEnvironmentVariables() do WebApplication.CreateBuilder já converte DynamoDb__AccessKey para DynamoDb:AccessKey automaticamente
var dynamoDbConfig = builder.Configuration.GetSection("DynamoDb").Get<DynamoDbConfiguration>();
        
if (dynamoDbConfig == null || string.IsNullOrWhiteSpace(dynamoDbConfig.AccessKey) || string.IsNullOrWhiteSpace(dynamoDbConfig.SecretKey))
{
    throw new InvalidOperationException(
        "Configuração do DynamoDB não encontrada ou incompleta. " +
        "Verifique se as variáveis de ambiente DynamoDb__AccessKey e DynamoDb__SecretKey estão configuradas.");
}

builder.Services.AddSingleton<IAmazonDynamoDB>(dynamoDbConfig.CreateDynamoDbClient());

// Registrar Repositories
builder.Services.AddScoped<ProductDynamoDbRepository>();
builder.Services.AddScoped<OrderDynamoDbRepository>();

// Registrar DataSources
builder.Services.AddScoped<IProductDataSource, ProductDynamoDbDataSource>();
builder.Services.AddScoped<IOrderDataSource, OrderDynamoDbDataSource>();

// Registrar Presenters (OrderManagement)
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.OrderManagement.GetOrderByIdPresenter>();
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.OrderManagement.StartOrderPresenter>();
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.OrderManagement.AddProductToOrderPresenter>();
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.OrderManagement.UpdateProductInOrderPresenter>();
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.OrderManagement.RemoveProductFromOrderPresenter>();
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.OrderManagement.ConfirmOrderSelectionPresenter>();
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.OrderManagement.GetPagedOrdersPresenter>();

// Registrar Presenters (ProductManagement)
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.ProductManagement.GetProductByIdPresenter>();
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.ProductManagement.GetProductsPagedPresenter>();
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.ProductManagement.CreateProductPresenter>();
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.ProductManagement.UpdateProductPresenter>();
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.ProductManagement.DeleteProductPresenter>();

// Registrar UseCases (OrderManagement)
builder.Services.AddScoped<FastFood.OrderHub.Application.UseCases.OrderManagement.GetOrderByIdUseCase>();
builder.Services.AddScoped<FastFood.OrderHub.Application.UseCases.OrderManagement.StartOrderUseCase>();
builder.Services.AddScoped<FastFood.OrderHub.Application.UseCases.OrderManagement.AddProductToOrderUseCase>();
builder.Services.AddScoped<FastFood.OrderHub.Application.UseCases.OrderManagement.UpdateProductInOrderUseCase>();
builder.Services.AddScoped<FastFood.OrderHub.Application.UseCases.OrderManagement.RemoveProductFromOrderUseCase>();
builder.Services.AddScoped<FastFood.OrderHub.Application.UseCases.OrderManagement.ConfirmOrderSelectionUseCase>();
builder.Services.AddScoped<FastFood.OrderHub.Application.UseCases.OrderManagement.GetPagedOrdersUseCase>();

// Registrar UseCases (ProductManagement)
builder.Services.AddScoped<FastFood.OrderHub.Application.UseCases.ProductManagement.GetProductByIdUseCase>();
builder.Services.AddScoped<FastFood.OrderHub.Application.UseCases.ProductManagement.GetProductsPagedUseCase>();
builder.Services.AddScoped<FastFood.OrderHub.Application.UseCases.ProductManagement.CreateProductUseCase>();
builder.Services.AddScoped<FastFood.OrderHub.Application.UseCases.ProductManagement.UpdateProductUseCase>();
builder.Services.AddScoped<FastFood.OrderHub.Application.UseCases.ProductManagement.DeleteProductUseCase>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
