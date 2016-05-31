using System;
using System.Collections.Generic;
using System.Linq;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
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
        }

        public bool AreSkusUnique(Order order)
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
    }
}
