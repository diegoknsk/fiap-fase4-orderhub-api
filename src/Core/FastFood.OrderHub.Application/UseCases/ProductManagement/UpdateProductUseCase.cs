using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.InputModels.ProductManagement;
using FastFood.OrderHub.Application.OutputModels.ProductManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.ProductManagement;
using FastFood.OrderHub.Application.Responses.ProductManagement;

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

    public async Task<UpdateProductResponse?> ExecuteAsync(UpdateProductInputModel input)
    {
        // Buscar produto existente
        var existingProduct = await _productDataSource.GetByIdAsync(input.ProductId);
        if (existingProduct == null)
            return null;

        // Validações de negócio
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ArgumentException("Nome do produto não pode ser vazio.", nameof(input.Name));

        if (input.Price <= 0)
            throw new ArgumentException("Preço do produto deve ser maior que zero.", nameof(input.Price));

        // Atualizar DTO
        existingProduct.Name = input.Name;
        existingProduct.Category = input.Category;
        existingProduct.Price = input.Price;
        existingProduct.Description = input.Description;
        existingProduct.ImageUrl = input.ImageUrl;

        // Atualizar BaseIngredients
        // Remover ingredientes que não estão mais na lista
        var existingIngredientIds = input.BaseIngredients
            .Where(bi => bi.Id.HasValue)
            .Select(bi => bi.Id!.Value)
            .ToList();

        existingProduct.BaseIngredients = existingProduct.BaseIngredients
            .Where(bi => existingIngredientIds.Contains(bi.Id))
            .ToList();

        // Atualizar ou adicionar ingredientes
        foreach (var ingredientInput in input.BaseIngredients)
        {
            if (ingredientInput.Id.HasValue)
            {
                // Atualizar ingrediente existente
                var existingIngredient = existingProduct.BaseIngredients
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
                existingProduct.BaseIngredients.Add(new ProductBaseIngredientDto
                {
                    Id = Guid.NewGuid(),
                    Name = ingredientInput.Name,
                    Price = ingredientInput.Price,
                    ProductId = existingProduct.Id
                });
            }
        }

        // Salvar no DataSource
        await _productDataSource.UpdateAsync(existingProduct);

        // Criar OutputModel
        var output = new UpdateProductOutputModel
        {
            ProductId = existingProduct.Id,
            Name = existingProduct.Name,
            Category = existingProduct.Category,
            Price = existingProduct.Price,
            Description = existingProduct.Description,
            ImageUrl = existingProduct.ImageUrl,
            IsActive = existingProduct.IsActive,
            CreatedAt = existingProduct.CreatedAt,
            BaseIngredients = existingProduct.BaseIngredients.Select(bi => new ProductBaseIngredientOutputModel
            {
                Id = bi.Id,
                Name = bi.Name,
                Price = bi.Price
            }).ToList()
        };

        return _presenter.Present(output);
    }
}

