using System;
using System.Collections.Generic;
using System.Linq;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        private readonly IProductRepository _productRepository;

        public OrderService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public OrderSummary PlaceOrder(Order order)
        {
            ValidateOrder(order);

            var orderSummary = new OrderSummary();

            return orderSummary;
        }

        public void ValidateOrder(Order order)
        {
            if (AreSkusUnique(order) == false)
            {
                throw new InvalidOperationException("SKUs are not unique");
            }

            if (AreProductsInStock(order, _productRepository) == false)
            {
                throw new InvalidOperationException("A product is out of stock");
            }
        }

        private static bool AreSkusUnique(Order order)
        {
            var numberOfItemsInOrder = order.OrderItems.Count;
            var skuList = order.OrderItems
                .Select(orderItem => orderItem.Product.Sku)
                .Distinct()
                .ToList();

            if (skuList.Count == numberOfItemsInOrder)
            {
                return true;
            }

            return false;
        }

        private static bool AreProductsInStock(Order order, IProductRepository productRepository)
        {
            foreach (var item in order.OrderItems)
            {
                if (!productRepository.IsInStock(item.Product.Sku))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
