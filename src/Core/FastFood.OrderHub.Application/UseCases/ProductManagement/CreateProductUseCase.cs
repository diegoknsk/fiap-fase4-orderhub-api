using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.InputModels.ProductManagement;
using FastFood.OrderHub.Application.OutputModels.ProductManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.ProductManagement;
using FastFood.OrderHub.Application.Responses.ProductManagement;

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

        // Criar DTO
        var productDto = new ProductDto
        {
            Id = Guid.NewGuid(),
            Name = input.Name,
            Category = input.Category,
            Price = input.Price,
            Description = input.Description,
            ImageUrl = input.ImageUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            BaseIngredients = input.BaseIngredients.Select(bi => new ProductBaseIngredientDto
            {
                Id = Guid.NewGuid(),
                Name = bi.Name,
                Price = bi.Price,
                ProductId = Guid.Empty // Será preenchido após criar o produto
            }).ToList()
        };

        // Atualizar ProductId nos ingredientes
        foreach (var ingredient in productDto.BaseIngredients)
        {
            ingredient.ProductId = productDto.Id;
        }

        // Salvar no DataSource
        await _productDataSource.AddAsync(productDto);

        // Criar OutputModel
        var output = new CreateProductOutputModel
        {
            ProductId = productDto.Id,
            Name = productDto.Name,
            Category = productDto.Category,
            Price = productDto.Price,
            Description = productDto.Description,
            ImageUrl = productDto.ImageUrl,
            IsActive = productDto.IsActive,
            CreatedAt = productDto.CreatedAt,
            BaseIngredients = productDto.BaseIngredients.Select(bi => new ProductBaseIngredientOutputModel
            {
                Id = bi.Id,
                Name = bi.Name,
                Price = bi.Price
            }).ToList()
        };

        return _presenter.Present(output);
    }
}

