using RestaurantManagement.Models;

namespace RestaurantManagement.Helpers
{
    public static class OrderHelper
    {
        public static string GetOrderStatusName(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Ordered => "Đã gọi món",
                OrderStatus.Preparing => "Đang chuẩn bị",
                OrderStatus.Canceled => "Đã huỷ",
                OrderStatus.Completed => "Hoàn tất",
                _ => "Không rõ"
            };
        }
    }
}
