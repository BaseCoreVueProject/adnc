﻿namespace Adnc.Demo.Ord.Domain.Aggregates.OrderAggregate;

public record OrderItemProduct : ValueObject
{
    public long Id { get; }

    public string Name { get; } 

    public decimal Price { get; }

    private OrderItemProduct()
    {
        Name = string.Empty;
    }

    public OrderItemProduct(long id, string name, decimal price)
    {
        this.Id = Checker.GTZero(id, nameof(id));
        this.Name = Checker.NotNullOrEmpty(name, nameof(name));
        this.Price = Checker.GTZero(price, nameof(price));
    }
}