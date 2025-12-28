using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.InputModels.ProductManagement;
using FastFood.OrderHub.Application.OutputModels.ProductManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.ProductManagement;
using FastFood.OrderHub.Application.Responses.ProductManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using FastFood.OrderHub.Domain.Entities.OrderManagement;

namespace FastFood.OrderHub.Application.UseCases.ProductManagement;

/// <summary>
/// UseCase para criar produto
/// </summary>
public class CreateProductUseCase
{
    private readonly IProductDataSource _productDataSource;
    private readonly CreateProductPresenter _presenter;

    public CreateProductUseCase(
        IProductDataSource productDataSource,
        CreateProductPresenter presenter)
    {
        _productDataSource = productDataSource;
        _presenter = presenter;
    }

    public async Task<CreateProductResponse> ExecuteAsync(CreateProductInputModel input)
    {
        // Validações de negócio
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ArgumentException("Nome do produto não pode ser vazio.", nameof(input.Name));

        if (input.Price <= 0)
            throw new ArgumentException("Preço do produto deve ser maior que zero.", nameof(input.Price));

        // Criar entidade de domínio Product
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = input.Name,
            Category = (EnumProductCategory)input.Category,
            Price = input.Price,
            Description = input.Description,
            Image = !string.IsNullOrWhiteSpace(input.ImageUrl) ? new ImageProduct { Url = input.ImageUrl } : null,
            Ingredients = input.BaseIngredients.Select(bi => new ProductBaseIngredient
            {
                Id = Guid.NewGuid(),
                Name = bi.Name,
                Price = bi.Price,
                ProductId = Guid.Empty // Será preenchido após criar o produto
            }).ToList()
        };

        // Atualizar ProductId nos ingredientes
        foreach (var ingredient in product.Ingredients)
        {
            ingredient.ProductId = product.Id;
        }

        // Validar produto usando método de domínio
        if (!product.IsValid())
            throw new InvalidOperationException("Produto inválido.");

        // Converter entidade de domínio para DTO
        var productDto = ConvertToDto(product);

        // Salvar no DataSource
        await _productDataSource.AddAsync(productDto);

        // Criar OutputModel
        var output = new CreateProductOutputModel
        {
            ProductId = product.Id,
            Name = product.Name,
            Category = (int)product.Category,
            Price = product.Price,
            Description = product.Description,
            ImageUrl = product.Image?.Url,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            BaseIngredients = product.Ingredients.Select(bi => new ProductBaseIngredientOutputModel
            {
                Id = bi.Id,
                Name = bi.Name,
                Price = bi.Price
            }).ToList()
        };

        return _presenter.Present(output);
    }

    private ProductDto ConvertToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Category = (int)product.Category,
            Price = product.Price,
            Description = product.Description,
            ImageUrl = product.Image?.Url,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            BaseIngredients = product.Ingredients.Select(bi => new ProductBaseIngredientDto
            {
                Id = bi.Id,
                Name = bi.Name,
                Price = bi.Price,
                ProductId = bi.ProductId
            }).ToList()
        };
    }
}
