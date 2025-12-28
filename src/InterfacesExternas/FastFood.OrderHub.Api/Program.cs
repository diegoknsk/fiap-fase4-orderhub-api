using Amazon.DynamoDBv2;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Infra.Persistence.Configurations;
using FastFood.OrderHub.Infra.Persistence.DataSources;
using FastFood.OrderHub.Infra.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar DynamoDB
var dynamoDbConfig = builder.Configuration.GetSection("DynamoDb").Get<DynamoDbConfiguration>() 
    ?? new DynamoDbConfiguration
    {
        AccessKey = builder.Configuration["DynamoDb:AccessKey"] ?? Environment.GetEnvironmentVariable("DYNAMODB__ACCESSKEY") ?? string.Empty,
        SecretKey = builder.Configuration["DynamoDb:SecretKey"] ?? Environment.GetEnvironmentVariable("DYNAMODB__SECRETKEY") ?? string.Empty,
        SessionToken = builder.Configuration["DynamoDb:SessionToken"] ?? Environment.GetEnvironmentVariable("DYNAMODB__SESSIONTOKEN"),
        Region = builder.Configuration["DynamoDb:Region"] ?? Environment.GetEnvironmentVariable("DYNAMODB__REGION") ?? "us-east-1",
        ServiceUrl = builder.Configuration["DynamoDb:ServiceUrl"] ?? Environment.GetEnvironmentVariable("DYNAMODB__SERVICEURL")
    };

builder.Services.AddSingleton<IAmazonDynamoDB>(dynamoDbConfig.CreateDynamoDbClient());

// Registrar Repositories
builder.Services.AddScoped<ProductDynamoDbRepository>();
builder.Services.AddScoped<OrderDynamoDbRepository>();

// Registrar DataSources
builder.Services.AddScoped<IProductDataSource, ProductDynamoDbDataSource>();
builder.Services.AddScoped<IOrderDataSource, OrderDynamoDbDataSource>();

// Registrar Presenters (OrderManagement)
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.OrderManagement.GetOrderByIdPresenter>();

// Registrar Presenters (ProductManagement)
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.ProductManagement.GetProductByIdPresenter>();
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.ProductManagement.GetProductsPagedPresenter>();
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.ProductManagement.CreateProductPresenter>();
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.ProductManagement.UpdateProductPresenter>();
builder.Services.AddScoped<FastFood.OrderHub.Application.Presenters.ProductManagement.DeleteProductPresenter>();

// Registrar UseCases (OrderManagement)
builder.Services.AddScoped<FastFood.OrderHub.Application.UseCases.OrderManagement.GetOrderByIdUseCase>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
