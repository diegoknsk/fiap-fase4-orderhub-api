using FastFood.OrderHub.Application.DTOs;
using FastFood.OrderHub.Application.InputModels.OrderManagement;
using FastFood.OrderHub.Application.OutputModels.OrderManagement;
using FastFood.OrderHub.Application.Ports;
using FastFood.OrderHub.Application.Presenters.OrderManagement;
using FastFood.OrderHub.Application.Responses.OrderManagement;
using FastFood.OrderHub.Domain.Common.Enums;
using FastFood.OrderHub.Domain.Entities.OrderManagement;

namespace FastFood.OrderHub.Application.UseCases.OrderManagement;

/// <summary>
/// UseCase para adicionar produto ao pedido
/// </summary>
public class AddProductToOrderUseCase
{
    private readonly IOrderDataSource _orderDataSource;
    private readonly IProductDataSource _productDataSource;
    private readonly AddProductToOrderPresenter _presenter;

    public AddProductToOrderUseCase(
        IOrderDataSource orderDataSource,
        IProductDataSource productDataSource,
        AddProductToOrderPresenter presenter)
    {
        _orderDataSource = orderDataSource;
        _productDataSource = productDataSource;
        _presenter = presenter;
    }

    public async Task<AddProductToOrderResponse?> ExecuteAsync(AddProductToOrderInputModel input)
    {
        // Validar quantidade
        if (input.Quantity <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(input.Quantity));

        // Buscar pedido completo com Items
        var orderDto = await _orderDataSource.GetByIdAsync(input.OrderId);
        if (orderDto == null)
            return null;

        // Buscar produto
        var productDto = await _productDataSource.GetByIdAsync(input.ProductId);
        if (productDto == null)
            throw new ArgumentException("Produto não encontrado.", nameof(input.ProductId));

        // Validar que produto está ativo
        if (!productDto.IsActive)
            throw new InvalidOperationException("Produto não está disponível.");

        // Converter OrderDto para entidade de domínio Order
        var order = ConvertToDomainEntity(orderDto);

        // Criar entidade de domínio Product para OrderedProduct
        var product = new Product
        {
            Id = productDto.Id,
            Name = productDto.Name,
            Category = (EnumProductCategory)productDto.Category,
            Price = productDto.Price,
            Description = productDto.Description,
            Ingredients = productDto.BaseIngredients.Select(bi => new ProductBaseIngredient
            {
                Id = bi.Id,
                Name = bi.Name,
                Price = bi.Price,
                ProductId = bi.ProductId
            }).ToList()
        };

        // Criar entidade de domínio OrderedProduct
        var orderedProduct = new OrderedProduct
        {
            Id = Guid.NewGuid(),
            ProductId = input.ProductId,
            Product = product,
            Quantity = input.Quantity,
            Observation = input.Observation,
            CustomIngredients = new List<OrderedProductIngredient>()
        };

        // Processar ingredientes customizados
        if (input.CustomIngredients.Any())
        {
            foreach (var customIngredient in input.CustomIngredients)
            {
                // Buscar ingrediente base do produto
                var baseIngredient = product.Ingredients
                    .FirstOrDefault(bi => bi.Id == customIngredient.ProductBaseIngredientId);

                if (baseIngredient != null)
                {
                    // Validar quantidade (0 a 10)
                    var quantity = Math.Clamp(customIngredient.Quantity, 0, 10);
                    
                    orderedProduct.CustomIngredients.Add(new OrderedProductIngredient
                    {
                        Id = Guid.NewGuid(),
                        Name = baseIngredient.Name,
                        Price = baseIngredient.Price,
                        Quantity = quantity,
                        OrderedProductId = orderedProduct.Id,
                        ProductBaseIngredientId = baseIngredient.Id
                    });
                }
            }
        }
        else
        {
            // Se não houver ingredientes customizados, usar ingredientes base do produto com quantidade padrão
            foreach (var baseIngredient in product.Ingredients)
            {
                orderedProduct.CustomIngredients.Add(new OrderedProductIngredient
                {
                    Id = Guid.NewGuid(),
                    Name = baseIngredient.Name,
                    Price = baseIngredient.Price,
                    Quantity = 1, // Quantidade padrão
                    OrderedProductId = orderedProduct.Id,
                    ProductBaseIngredientId = baseIngredient.Id
                });
            }
        }

        // Calcular FinalPrice usando método de domínio
        orderedProduct.CalculateFinalPrice();

        // Adicionar produto ao pedido usando método de domínio (recalcula TotalPrice automaticamente)
        order.AddProduct(orderedProduct);

        // Converter entidade de domínio de volta para DTO
        orderDto = ConvertToDto(order, orderDto.OrderSource);

        // Salvar Order completo atualizado
        await _orderDataSource.UpdateAsync(orderDto);

        // Criar OutputModel
        var output = new AddProductToOrderOutputModel
        {
            OrderId = order.Id,
            OrderedProductId = orderedProduct.Id,
            TotalPrice = order.TotalPrice
        };

        return _presenter.Present(output);
    }

    private Order ConvertToDomainEntity(OrderDto dto)
    {
        var order = new Order
        {
            Id = dto.Id,
            Code = dto.Code,
            CustomerId = dto.CustomerId,
            CreatedAt = dto.CreatedAt,
            OrderStatus = (EnumOrderStatus)dto.OrderStatus,
            TotalPrice = dto.TotalPrice,
            OrderedProducts = dto.Items.Select(item => new OrderedProduct
            {
                Id = item.Id,
                ProductId = item.ProductId,
                OrderId = item.OrderId,
                Quantity = item.Quantity,
                Observation = item.Observation,
                FinalPrice = item.FinalPrice,
                CustomIngredients = item.CustomIngredients.Select(ci => new OrderedProductIngredient
                {
                    Id = ci.Id,
                    Name = ci.Name,
                    Price = ci.Price,
                    Quantity = ci.Quantity,
                    OrderedProductId = ci.OrderedProductId,
                    ProductBaseIngredientId = ci.ProductBaseIngredientId
                }).ToList()
            }).ToList()
        };

        return order;
    }

    private OrderDto ConvertToDto(Order order, string? orderSource = null)
    {
        return new OrderDto
        {
            Id = order.Id,
            Code = order.Code,
            CustomerId = order.CustomerId,
            CreatedAt = order.CreatedAt,
            OrderStatus = (int)order.OrderStatus,
            TotalPrice = order.TotalPrice,
            OrderSource = orderSource,
            Items = order.OrderedProducts.Select(op => new OrderedProductDto
            {
                Id = op.Id,
                ProductId = op.ProductId,
                OrderId = op.OrderId,
                Quantity = op.Quantity,
                Observation = op.Observation,
                FinalPrice = op.FinalPrice,
                ProductName = op.Product?.Name,
                Category = op.Product != null ? (int)op.Product.Category : null,
                CustomIngredients = op.CustomIngredients.Select(ci => new OrderedProductIngredientDto
                {
                    Id = ci.Id,
                    Name = ci.Name,
                    Price = ci.Price,
                    Quantity = ci.Quantity,
                    OrderedProductId = ci.OrderedProductId,
                    ProductBaseIngredientId = ci.ProductBaseIngredientId
                }).ToList()
            }).ToList()
        };
    }
}
