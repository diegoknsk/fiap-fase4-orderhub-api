using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.Exceptions;
using FastFood.OrderHub.Application.InputModels.ProductManagement;
using FastFood.OrderHub.Application.OutputModels.ProductManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.ProductManagement;
using FastFood.OrderHub.Application.Responses.ProductManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using FastFood.OrderHub.Domain.Entities.OrderManagement;

namespace FastFood.OrderHub.Application.UseCases.ProductManagement;

/// <summary>
/// UseCase para atualizar produto
/// </summary>
public class UpdateProductUseCase
{
    private readonly IProductDataSource _productDataSource;
    private readonly UpdateProductPresenter _presenter;

    public UpdateProductUseCase(
        IProductDataSource productDataSource,
        UpdateProductPresenter presenter)
    {
        _productDataSource = productDataSource;
        _presenter = presenter;
    }

    public async Task<UpdateProductResponse> ExecuteAsync(UpdateProductInputModel input)
    {
        // Buscar produto existente
        var existingProductDto = await _productDataSource.GetByIdAsync(input.ProductId);
        if (existingProductDto == null)
            throw new BusinessException("Produto não encontrado.");

        // Validações de negócio
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new BusinessException("Nome do produto não pode ser vazio.");

        if (input.Price <= 0)
            throw new BusinessException("Preço do produto deve ser maior que zero.");

        // Converter ProductDto para entidade de domínio Product
        var product = ConvertToDomainEntity(existingProductDto);

        // Atualizar propriedades da entidade de domínio
        product.Name = input.Name;
        product.Category = (EnumProductCategory)input.Category;
        product.Price = input.Price;
        product.Description = input.Description;
        product.Image = !string.IsNullOrWhiteSpace(input.ImageUrl) ? new ImageProduct { Url = input.ImageUrl } : null;

        // Atualizar BaseIngredients
        // Remover ingredientes que não estão mais na lista
        var existingIngredientIds = input.BaseIngredients
            .Where(bi => bi.Id.HasValue)
            .Select(bi => bi.Id!.Value)
            .ToList();

        product.Ingredients = product.Ingredients
            .Where(bi => existingIngredientIds.Contains(bi.Id))
            .ToList();

        // Atualizar ou adicionar ingredientes
        foreach (var ingredientInput in input.BaseIngredients)
        {
            if (ingredientInput.Id.HasValue)
            {
                // Atualizar ingrediente existente
                var existingIngredient = product.Ingredients
                    .FirstOrDefault(bi => bi.Id == ingredientInput.Id.Value);
                
                if (existingIngredient != null)
                {
                    existingIngredient.Name = ingredientInput.Name;
                    existingIngredient.Price = ingredientInput.Price;
                }
            }
            else
            {
                // Adicionar novo ingrediente
                product.Ingredients.Add(new ProductBaseIngredient
                {
                    Id = Guid.NewGuid(),
                    Name = ingredientInput.Name,
                    Price = ingredientInput.Price,
                    ProductId = product.Id
                });
            }
        }

        // Validar produto usando método de domínio
        if (!product.IsValid())
            throw new BusinessException("Produto inválido.");

        // Converter entidade de domínio de volta para DTO
        var productDto = ConvertToDto(product, existingProductDto.IsActive, existingProductDto.CreatedAt);

        // Salvar no DataSource
        await _productDataSource.UpdateAsync(productDto);

        var output = AdaptToOutputModel(product, productDto.IsActive, productDto.CreatedAt);
        return _presenter.Present(output);
    }

    private Product ConvertToDomainEntity(ProductDto dto)
    {
        return new Product
        {
            Id = dto.Id,
            Name = dto.Name,
            Category = (EnumProductCategory)dto.Category,
            Price = dto.Price,
            Description = dto.Description,
            Image = !string.IsNullOrWhiteSpace(dto.ImageUrl) ? new ImageProduct { Url = dto.ImageUrl } : null,
            Ingredients = dto.BaseIngredients.Select(bi => new ProductBaseIngredient
            {
                Id = bi.Id,
                Name = bi.Name,
                Price = bi.Price,
                ProductId = bi.ProductId
            }).ToList()
        };
    }

    private ProductDto ConvertToDto(Product product, bool isActive, DateTime createdAt)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Category = (int)product.Category,
            Price = product.Price,
            Description = product.Description,
            ImageUrl = product.Image?.Url,
            IsActive = isActive,
            CreatedAt = createdAt,
            BaseIngredients = product.Ingredients.Select(bi => new ProductBaseIngredientDto
            {
                Id = bi.Id,
                Name = bi.Name,
                Price = bi.Price,
                ProductId = bi.ProductId
            }).ToList()
        };
    }

    private UpdateProductOutputModel AdaptToOutputModel(Product product, bool isActive, DateTime createdAt)
    {
        return new UpdateProductOutputModel
        {
            ProductId = product.Id,
            Name = product.Name,
            Category = (int)product.Category,
            Price = product.Price,
            Description = product.Description,
            ImageUrl = product.Image?.Url,
            IsActive = isActive,
            CreatedAt = createdAt,
            BaseIngredients = product.Ingredients.Select(bi => new ProductBaseIngredientOutputModel
            {
                Id = bi.Id,
                Name = bi.Name,
                Price = bi.Price
            }).ToList()
        };
    }
}
