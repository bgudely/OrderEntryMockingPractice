using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<TaxEntry> _taxEntryList;
        private Customer _customer;

        private IProductRepository _productRepository;
        private IOrderFulfillmentService _orderFulfillmentService;
        private ITaxRateService _taxRateService;
        private ICustomerRepository _customerRepository;

        [SetUp]
        public void Init()
        {
            _productRepository = Substitute.For<IProductRepository>();
            _orderFulfillmentService = Substitute.For<IOrderFulfillmentService>();
            _taxRateService = Substitute.For<ITaxRateService>();
            _customerRepository = Substitute.For<ICustomerRepository>();

            _orderService = new OrderService(_productRepository, _orderFulfillmentService, _taxRateService, _customerRepository);

            _orderConfirmation = new OrderConfirmation() { CustomerId = 42, OrderId = 12, OrderNumber = "OneTwoThree" };
            _taxEntryList = new List<TaxEntry>()
            {
                new TaxEntry() { Description = "Default", Rate = (decimal) 0.1 },
                new TaxEntry() { Description = "High", Rate = (decimal) 0.3 }
            };
            _customer = new Customer() { CustomerId = 2, PostalCode = "12345", Country = "USA"};

            _taxRateService.GetTaxEntries(_customer.PostalCode, _customer.Country).Returns(_taxEntryList);
            _customerRepository.Get(_customer.CustomerId.Value).Returns(_customer);
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
            var order = GenerateOrderFromProducts(new[] { product });
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
            var product = new Product() {Sku = "123456"};
            var products = new [] {product, product};
            var order = GenerateOrderFromProducts(products);

            //Act and Assert
            Assert.Throws<InvalidOperationException>(() => _orderService.PlaceOrder(order));
        }

        [Test]
        public void ProductOutOfStock_ThrowsException()
        {
            //Arrange
            var product = new Product(){ Name = "Out Of Stock Product" };
            var order = GenerateOrderFromProducts(new []{ product });

            _productRepository.IsInStock(product.Sku).Returns(false);

            //Act and Assert
            Assert.Throws<InvalidOperationException>(() => _orderService.PlaceOrder(order));
        }

        [Test]
        public void DuplicateSkuAndOutOfStock_Returns_AllValidationReasons()
        {
            //Arrange
            var product = new Product() { Sku = "123456" };
            var products = new[] { product, product };
            var order = GenerateOrderFromProducts(products);

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
            var order = GenerateOrderFromProducts(new[] { product });

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
            var order = GenerateOrderFromProducts(new[] { product });

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
            var order = GenerateOrderFromProducts(new[] { product });

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
            var order = GenerateOrderFromProducts(new[] { product });

            _productRepository.IsInStock(product.Sku).Returns(true);
            _orderFulfillmentService.Fulfill(order).Returns(_orderConfirmation);

            //Act
            var orderSummary = _orderService.PlaceOrder(order);

            //Assert
            Assert.IsInstanceOf<IEnumerable<TaxEntry>>(orderSummary.Taxes);
        }

        [Test]
        public void OrderSummary_Contains_CalculatedNetTotal()
        {
            //Arrange
            var product = new Product()
            {
                Description = "A cleaning product by Billy Mays",
                Name = "OxyClean",
                Price = 5,
                Sku = "BILLYMAYSHERE"
            };
            var order = GenerateOrderFromProducts(new[] { product });

            _productRepository.IsInStock(product.Sku).Returns(true);
            _orderFulfillmentService.Fulfill(order).Returns(_orderConfirmation);

            //Act
            var orderSummary = _orderService.PlaceOrder(order);
            var expectedNetTotal = CalculateNetTotal(order);

            //Assert
            Assert.That(orderSummary.NetTotal, Is.EqualTo(expectedNetTotal));

        }

        private Order GenerateOrderFromProducts(Product[] products)
        {
            var orderItemList = new List<OrderItem>();

            foreach (var product in products)
            {
                var orderItem = new OrderItem()
                {
                    Product = product,
                    Quantity = 3
                };

                orderItemList.Add(orderItem);
            }

            var order = new Order()
            {
                OrderItems = orderItemList,
                CustomerId = _customer.CustomerId
            };

            return order;
        }

        private static decimal CalculateNetTotal(Order order)
        {
            return order.OrderItems.Sum(orderItem => orderItem.Product.Price * orderItem.Quantity);
        }
    }
}
