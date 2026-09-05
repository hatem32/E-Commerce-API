using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Orders;
using E_Commerce.Domain.Entities.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.API.Controllers
{
    public class OrdersController : ApiBaseController
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<OrderToReturnDto>> CreateOrder(OrderDto orderDto, CancellationToken cancellationToken)
        => ToActionResult(await _orderService.CreateOrderAsync(orderDto, GetEmailFromToken(), cancellationToken));

        [AllowAnonymous]
        [HttpGet("deliveryMethods")]
        public async Task<ActionResult<IReadOnlyList<DeliveryMethodDto>>> GetDeliveryMethods(CancellationToken cancellationToken)
            => ToActionResult(await _orderService.GetAllDeliveryMethodsAsync(cancellationToken));

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetAllOrders(CancellationToken cancellationToken)
            => ToActionResult(await _orderService.GetAllOrdersAsync(GetEmailFromToken(), cancellationToken));

        [Authorize]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderToReturnDto>> GetOrderById(Guid id, CancellationToken cancellationToken)
            => ToActionResult(await _orderService.GetOrderByIdAndEmailAsync(id, GetEmailFromToken(), cancellationToken));

        // ==================== Admin ====================

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        [ProducesResponseType(typeof(IReadOnlyList<OrderToReturnDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetAllOrdersForAdmin(CancellationToken cancellationToken)
            => ToActionResult(await _orderService.GetAllOrdersForAdminAsync(cancellationToken));

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}/status")]
        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderToReturnDto>> UpdateOrderStatus(Guid id, [FromBody] string status, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsedStatus))
                return BadRequest($"'{status}' is not a valid order status.");

            return ToActionResult(await _orderService.UpdateOrderStatusAsync(id, parsedStatus, cancellationToken));
        }
    }
}