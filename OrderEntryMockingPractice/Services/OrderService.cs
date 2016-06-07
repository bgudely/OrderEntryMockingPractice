using System;
using System.Collections.Generic;
using System.Linq;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderFulfillmentService _orderFulfillmentService;
        private readonly ITaxRateService _taxRateService;
        private readonly ICustomerRepository _customerRepository;

        public OrderService(IProductRepository productRepository, IOrderFulfillmentService orderFulfillmentService, 
                            ITaxRateService taxRateService, ICustomerRepository customerRepository)
        {
            _productRepository = productRepository;
            _orderFulfillmentService = orderFulfillmentService;
            _taxRateService = taxRateService;
            _customerRepository = customerRepository;
        }

        public OrderSummary PlaceOrder(Order order)
        {
            ValidateOrder(order);

            var orderConfirmation = _orderFulfillmentService.Fulfill(order);

            var customer = _customerRepository.Get(order.CustomerId.Value);

            var orderSummary = new OrderSummary()
            {
                OrderNumber = orderConfirmation.OrderNumber,
                OrderId = orderConfirmation.OrderId,
                Taxes = _taxRateService.GetTaxEntries(customer.PostalCode, customer.Country),
                NetTotal = CalculateNetTotal(order)
            };

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

            return skuList.Count == numberOfItemsInOrder;
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

        private static decimal CalculateNetTotal(Order order)
        {
            return order.OrderItems.Sum(orderItem => orderItem.Product.Price * orderItem.Quantity);
        }
    }
}
