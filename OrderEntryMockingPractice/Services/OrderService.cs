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
            var messages = new List<string>();

            if (order == null)
            {
                messages.Add("Order is null");
            }

            if (SkusAreUnique(order) == false)
            {
                messages.Add("SKUs are not unique");
            }

            if (ProductsAreInStock(order, _productRepository) == false)
            {
                messages.Add("A product is out of stock");
            }

            if (messages.Any())
            {
                throw new InvalidOperationException(String.Join(", ", messages));
            }
        }

        private static bool SkusAreUnique(Order order)
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

        private static bool ProductsAreInStock(Order order, IProductRepository productRepository)
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
