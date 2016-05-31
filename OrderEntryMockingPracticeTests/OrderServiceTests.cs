using System;
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

        [SetUp]
        public void Init()
        {
            _orderService = new OrderService();   
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
            var productOne = new Product()
            {
                Sku = "123456"
            };
            var productTwo = productOne;
            var order = new Order();

            Product[] products = {productOne, productTwo};

            foreach (var product in products)
            {
                var orderItem = new OrderItem()
                {
                    Product = productOne,
                    Quantity = 2
                };

                order.OrderItems.Add(orderItem);
            }

            //Act and Assert
            Assert.Throws<InvalidOperationException>(() => _orderService.PlaceOrder(order));
        }
    }
}
