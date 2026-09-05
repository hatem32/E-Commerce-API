using E_Commerce.Application.Common;
using E_Commerce.Application.DTOs.Orders;
using E_Commerce.Domain.Entities.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Contracts
{
    public interface IOrderService
    {
        Task<Result<OrderToReturnDto>> CreateOrderAsync(OrderDto orderDto, string email, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<DeliveryMethodDto>>> GetAllDeliveryMethodsAsync(CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<OrderToReturnDto>>> GetAllOrdersAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<OrderToReturnDto>> GetOrderByIdAndEmailAsync(Guid id, string email, CancellationToken cancellationToken = default);

        // Admin
        Task<Result<IReadOnlyList<OrderToReturnDto>>> GetAllOrdersForAdminAsync(CancellationToken cancellationToken = default);
        Task<Result<OrderToReturnDto>> UpdateOrderStatusAsync(Guid id, OrderStatus status, CancellationToken cancellationToken = default);
    }
}