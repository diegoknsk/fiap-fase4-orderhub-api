using Amazon.DynamoDBv2;
using FastFood.OrderHub.Api.Auth;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Infra.Persistence.Configurations;
using FastFood.OrderHub.Infra.Persistence.DataSources;
using FastFood.OrderHub.Infra.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Configure JWT options
//builder.Services.Configure<JwtOptions>("Admin", builder.Configuration.GetSection("JwtAdmin"));
builder.Services.Configure<JwtOptions>("Customer", builder.Configuration.GetSection("JwtCustomer"));

// Helper function to build token validation parameters
static TokenValidationParameters BuildParams(IConfiguration cfg, string section)
{
    var issuer = cfg[$"{section}:Issuer"];
    var audience = cfg[$"{section}:Audience"];
    var secret = cfg[$"{section}:SecretKey"];

    return new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!)),
        ClockSkew = TimeSpan.FromSeconds(30),

        // *** Critical point to recognize "role" as role ***
        RoleClaimType = "role",
        NameClaimType = JwtRegisteredClaimNames.Sub
    };
}

// Configure authentication with JWT schemes
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer("CustomerBearer", o =>
    {
        o.TokenValidationParameters = BuildParams(builder.Configuration, "JwtCustomer");
    })
    .AddCognitoJwtBearer(builder.Configuration);

// Configure authorization policies
builder.Services.AddAuthorization(opts =>
{
    // Pol�tica para Admin (Cognito)
    opts.AddPolicy("Admin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "aws.cognito.signin.user.admin");
    });

    opts.AddPolicy("Customer", p => p.RequireAuthenticatedUser());
    opts.AddPolicy("CustomerWithScope", p => p.RequireAssertion(ctx =>
        ctx.User.HasClaim("role", "customer") && ctx.User.HasClaim("scope", "customer")));
});


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
