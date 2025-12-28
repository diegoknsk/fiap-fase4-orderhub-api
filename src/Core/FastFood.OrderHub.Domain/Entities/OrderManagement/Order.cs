using FastFood.OrderHub.Domain.Common.Enums;

namespace FastFood.OrderHub.Domain.Entities.OrderManagement;

/// <summary>
/// Entidade de domínio: Pedido
/// </summary>
public class Order
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public ICollection<OrderedProduct> OrderedProducts { get; set; } = new List<OrderedProduct>();
    public EnumPaymentStatus PaymentStatus { get; set; }
    public EnumOrderStatus OrderStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CustomerId { get; set; }
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Adiciona um produto ao pedido e recalcula o TotalPrice
    /// </summary>
    public void AddProduct(OrderedProduct orderedProduct)
    {
        OrderedProducts.Add(orderedProduct);
        orderedProduct.OrderId = Id;
        CalculateTotalPrice();
    }

    /// <summary>
    /// Remove um produto do pedido e recalcula o TotalPrice
    /// </summary>
    public void RemoveProduct(Guid orderedProductId)
    {
        var product = OrderedProducts.FirstOrDefault(op => op.Id == orderedProductId);
        if (product != null)
        {
            OrderedProducts.Remove(product);
            CalculateTotalPrice();
        }
    }

    /// <summary>
    /// Calcula o TotalPrice somando FinalPrice de todos os OrderedProducts
    /// </summary>
    public decimal CalculateTotalPrice()
    {
        TotalPrice = OrderedProducts.Sum(op => op.FinalPrice);
        return TotalPrice;
    }

    /// <summary>
    /// Finaliza a seleção do pedido (altera status para AwaitingPayment)
    /// </summary>
    public void FinalizeOrderSelection()
    {
        OrderStatus = EnumOrderStatus.AwaitingPayment;
    }

    /// <summary>
    /// Atualiza o status do pedido
    /// </summary>
    public void UpdateStatus(EnumOrderStatus status)
    {
        OrderStatus = status;
    }

    /// <summary>
    /// Envia o pedido para a cozinha
    /// </summary>
    public void SendToKitchen()
    {
        OrderStatus = EnumOrderStatus.InPreparation;
    }

    /// <summary>
    /// Define o pedido como em preparação
    /// </summary>
    public void SetInPreparation()
    {
        OrderStatus = EnumOrderStatus.InPreparation;
    }

    /// <summary>
    /// Define o pedido como pronto para retirada
    /// </summary>
    public void SetReadyForPickup()
    {
        OrderStatus = EnumOrderStatus.ReadyForPickup;
    }
}

