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
        private OrderConfirmation _orderConfirmation;

        private IProductRepository _productRepository;
        private IOrderFulfillmentService _orderFulfillmentService;
        private ITaxRateService _taxRateService;

        [SetUp]
        public void Init()
        {
            _productRepository = Substitute.For<IProductRepository>();
            _orderFulfillmentService = Substitute.For<IOrderFulfillmentService>();
            _taxRateService = Substitute.For<ITaxRateService>();

            _orderConfirmation = new OrderConfirmation() { CustomerId = 42, OrderId = 12, OrderNumber = "OneTwoThree" };

            _orderService = new OrderService(_productRepository, _orderFulfillmentService, _taxRateService);
        }

        [Test]
        public void ValidOrder_Returns_OrderSummary()
        {
            //Arrange
            var product = new Product()
            {
                Name = "Test Product",
                Description = "A test Product",
                Price = 10,
                ProductId = 1,
                Sku = "TestSKU"
            };
             var order = GenerateOneProductOrder(product);
            _productRepository.IsInStock(product.Sku).Returns(true);
            _orderFulfillmentService.Fulfill(order).Returns(_orderConfirmation);


            //Act
            var placedOrder = _orderService.PlaceOrder(order);

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
            var product = new Product(){ Sku = "OutOfStock" };
            var order = GenerateOneProductOrder(product);
            _productRepository.IsInStock(product.Sku).Returns(false);

            //Act and Assert
            Assert.Throws<InvalidOperationException>(() => _orderService.PlaceOrder(order));
        }

        [Test]
        public void DuplicateSkuAndOutOfStock_Returns_AllValidationReasons()
        {
            //Arrange
            var order = GenerateDuplicateSkuOrder();
            var product = new Product(){ Sku = "OutOfStock" };
            var orderItem = new OrderItem() { Product = product, Quantity = 1 };
            order.OrderItems.Add(orderItem);

            _productRepository.IsInStock(product.Sku).Returns(false);

            //Act and Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _orderService.PlaceOrder(order));
            Assert.That(ex.Message, Is.EqualTo("SKUs are not unique, A product is out of stock"));
        }

        [Test]
        public void ValidOrder_SubmitsOrder_ToOrderFullfillmentService()
        {
            //Arrange
            var product = new Product() { Sku = "OutOfStock" };
            var order = GenerateOneProductOrder(product);
            _productRepository.IsInStock(product.Sku).Returns(true);
            _orderFulfillmentService.Fulfill(order).Returns(_orderConfirmation);

            //Act
            _orderService.PlaceOrder(order);

            //Assert
            _orderFulfillmentService.Received().Fulfill(order);
        }

        [Test]
        public void OrderSummary_Contains_OrderFulfillmentConfirmationNumber()
        {
            //Arrange
            var product = new Product() { Sku = "SomeProduct" };
            var order = GenerateOneProductOrder(product);
            _productRepository.IsInStock(product.Sku).Returns(true);
            _orderFulfillmentService.Fulfill(order).Returns(_orderConfirmation);

            //Act
            var orderSummary = _orderService.PlaceOrder(order);

            //Assert
            Assert.IsInstanceOf<string>(orderSummary.OrderNumber);
        }

        [Test]
        public void OrderSummary_Contains_OrderIDFromFulfillmentService()
        {
            //Arrange
            var product = new Product() { Sku = "Something" };
            var order = GenerateOneProductOrder(product);
            _productRepository.IsInStock(product.Sku).Returns(true);
            _orderFulfillmentService.Fulfill(order).Returns(_orderConfirmation);

            //Act
            var orderSummary = _orderService.PlaceOrder(order);

            //Assert
            Assert.That(orderSummary.OrderId, Is.EqualTo(_orderConfirmation.OrderId));
        }

        [Test]
        public void OrderSummary_Contains_TaxesForOrder()
        {
            //Arrange
            var product = new Product() {Sku = "Something"};
            var order = GenerateOneProductOrder(product);
            _productRepository.IsInStock(product.Sku).Returns(true);
            _orderFulfillmentService.Fulfill(order).Returns(_orderConfirmation);

            //Act
            var orderSummary = _orderService.PlaceOrder(order);

            //Assert
            Assert.IsInstanceOf<IEnumerable<TaxEntry>>(orderSummary.Taxes);
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
