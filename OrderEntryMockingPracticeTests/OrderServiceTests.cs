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
    }
}
