using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;

namespace OrderEntryMockingPracticeTests
{
    [TestFixture]
    public class OrderServiceTests
    {
        private OrderService _orderService;
        private Order _order;

        private IProductRepository _productRepository;

        [SetUp]
        public void Init()
        {
            _productRepository = Substitute.For<IProductRepository>();
            _orderService = new OrderService(_productRepository);
        }

        [Test]
        public void ValidOrder_Returns_OrderSummary()
        {
            //Arrange
            _order = new Order();

            //Act
            var placedOrder = _orderService.PlaceOrder(_order);

            //Assert
            Assert.IsInstanceOf<OrderSummary>(placedOrder);
        }

        [Test]
        public void DuplicateSkus_ThrowsException()
        {
            //Arrange
            var order = GenerateDuplicateSkuOrder();

            //Act and Assert
            Assert.Throws<InvalidOperationException>(() => _orderService.PlaceOrder(order));
        }

        [Test]
        public void ProductOutOfStock_ThrowsException()
        {
            //Arrange
            var product = new Product()
            {
                Sku = "OutOfStock"
            };
            var order = GenerateOneProductOrder(product);

            //Act
            _productRepository.IsInStock(product.Sku).Returns(false);

            //Assert
            Assert.Throws<InvalidOperationException>(() => _orderService.PlaceOrder(order));
        }

        private static Order GenerateOneProductOrder(Product product)
        {
            var orderItem = new OrderItem()
            {
                Product = product,
                Quantity = 1
            };

            var orderItemsList = new List<OrderItem> { orderItem };

            var order = new Order()
            {
                OrderItems = orderItemsList
            };

            return order;
        }

        private static Order GenerateDuplicateSkuOrder()
        {
            var productOne = new Product()
            {
                Sku = "123456"
            };
            var productTwo = productOne;
            var order = new Order();

            Product[] products = { productOne, productTwo };

            foreach (var product in products)
            {
                var orderItem = new OrderItem()
                {
                    Product = product,
                    Quantity = 2
                };

                order.OrderItems.Add(orderItem);
            }

            return order;
        }
    }
}
