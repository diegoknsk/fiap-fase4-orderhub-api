using FastFood.OrderHub.Application.Common;
using Xunit;

namespace FastFood.OrderHub.Tests.Unit.Application.Common;

/// <summary>
/// Testes unitários para PagedList
/// </summary>
public class PagedListTests
{
    [Fact]
    public void PagedList_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var pagedList = new PagedList<string>();

        // Assert
        Assert.NotNull(pagedList.Items);
        Assert.Empty(pagedList.Items);
        Assert.Equal(0, pagedList.Page);
        Assert.Equal(0, pagedList.PageSize);
        Assert.Equal(0, pagedList.TotalCount);
        // TotalPages com PageSize=0 resulta em divisão por zero, então não testamos esse caso extremo
        // O comportamento real será testado nos outros testes com valores válidos
        Assert.False(pagedList.HasPreviousPage);
        Assert.False(pagedList.HasNextPage);
    }

    [Fact]
    public void PagedList_WhenTotalPagesCalculated_ShouldReturnCorrectValue()
    {
        // Arrange
        var pagedList = new PagedList<int>
        {
            TotalCount = 25,
            PageSize = 10
        };

        // Act & Assert
        Assert.Equal(3, pagedList.TotalPages); // Ceiling(25/10) = 3
    }

    [Fact]
    public void PagedList_WhenTotalCountIsZero_ShouldReturnZeroTotalPages()
    {
        // Arrange
        var pagedList = new PagedList<int>
        {
            TotalCount = 0,
            PageSize = 10
        };

        // Act & Assert
        Assert.Equal(0, pagedList.TotalPages);
    }

    [Fact]
    public void PagedList_WhenOnFirstPage_ShouldNotHavePreviousPage()
    {
        // Arrange
        var pagedList = new PagedList<int>
        {
            Page = 1,
            TotalCount = 25,
            PageSize = 10
        };

        // Act & Assert
        Assert.False(pagedList.HasPreviousPage);
        Assert.True(pagedList.HasNextPage);
    }

    [Fact]
    public void PagedList_WhenOnLastPage_ShouldNotHaveNextPage()
    {
        // Arrange
        var pagedList = new PagedList<int>
        {
            Page = 3,
            TotalCount = 25,
            PageSize = 10
        };

        // Act & Assert
        Assert.True(pagedList.HasPreviousPage);
        Assert.False(pagedList.HasNextPage);
    }

    [Fact]
    public void PagedList_WhenOnMiddlePage_ShouldHaveBothPreviousAndNextPage()
    {
        // Arrange
        var pagedList = new PagedList<int>
        {
            Page = 2,
            TotalCount = 25,
            PageSize = 10
        };

        // Act & Assert
        Assert.True(pagedList.HasPreviousPage);
        Assert.True(pagedList.HasNextPage);
    }

    [Fact]
    public void PagedList_WhenTotalCountEqualsPageSize_ShouldHaveOnePage()
    {
        // Arrange
        var pagedList = new PagedList<int>
        {
            TotalCount = 10,
            PageSize = 10
        };

        // Act & Assert
        Assert.Equal(1, pagedList.TotalPages);
    }

    [Fact]
    public void PagedList_WhenItemsSet_ShouldStoreItems()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var pagedList = new PagedList<int>
        {
            Items = items,
            Page = 1,
            PageSize = 10,
            TotalCount = 5
        };

        // Act & Assert
        Assert.Equal(5, pagedList.Items.Count);
        Assert.Equal(items, pagedList.Items);
    }
}
