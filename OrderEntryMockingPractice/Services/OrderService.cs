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
            
        }
    }
}
