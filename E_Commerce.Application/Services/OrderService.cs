using AutoMapper;
using E_Commerce.Application.Common;
using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Orders;
using E_Commerce.Application.Specifications;
using E_Commerce.Domain.Contracts;
using E_Commerce.Domain.Entities.Orders;
using E_Commerce.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Services
{
    public class OrderService(
      IMapper mapper,
      IUnitOfWork unitOfWork,
      IBasketRepository basketRepository) : IOrderService
    {
        public async Task<Result<OrderToReturnDto>> CreateOrderAsync(OrderDto orderDto, string email, CancellationToken cancellationToken = default)
        {
            var basket = await basketRepository.GetBasketAsync(orderDto.BasketId, cancellationToken);

            if (basket == null)
                return Result<OrderToReturnDto>.Fail(Error.NotFound("Basket Not Found", $"Basket With Id {orderDto.BasketId} Is Not Found"));

            if (basket.Items.Count == 0)
                return Result<OrderToReturnDto>.Fail(Error.Validation("Basket is Empty", $"Can Not Create Order With Basket With Id {orderDto.BasketId}"));



            var orderRepo = unitOfWork.GetRepository<Order, Guid>();
            var productRepo = unitOfWork.GetRepository<Product, int>();

            var existingOrder = await orderRepo.GetByIdAsync(new PaymentIntentSpec(basket.PaymentIntentId), cancellationToken);
            if (existingOrder is not null) orderRepo.Remove(existingOrder);

            var productIds = basket.Items.Select(i => i.Id).ToHashSet();
            var products = (await productRepo.GetAllAsync(new ProductsWithIdsSpecifications(productIds), cancellationToken)).ToDictionary(x => x.Id);

            var orderItems = new List<OrderItem>(basket.Items.Count);
            foreach (var item in basket.Items)
            {
                if (!products.TryGetValue(item.Id, out var product))
                    return Result<OrderToReturnDto>.Fail(Error.NotFound("Product Not Found", $"Product With Id {item.Id} Is Not Found "));

                orderItems.Add(new OrderItem
                {
                    Price = product.Price,
                    Quantity = item.Quantity,
                    Product = new ProductItemOrdered
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        PictureUrl = product.PictureUrl
                    }
                });
            }

            var orderAddress = mapper.Map<OrderAddress>(orderDto.ShipToAddress);

            var deliveryRepo = unitOfWork.GetRepository<DeliveryMethod, int>();
            var deliveryMethod = await deliveryRepo.GetByIdAsync(orderDto.DeliveryMethodId, cancellationToken);
            if (deliveryMethod == null)
                return Result<OrderToReturnDto>.Fail(Error.NotFound("Delivery Method Not Found", $"DeliveryMethod With Id {orderDto.DeliveryMethodId} Is Not Found "));

            var subTotal = orderItems.Sum(x => x.Quantity * x.Price);

            var order = new Order(email, orderItems, orderAddress, deliveryMethod, subTotal, basket.PaymentIntentId);

            orderRepo.Add(order); // local
            var result = await unitOfWork.SaveChangesAsync(cancellationToken);

            if (result <= 0)
            {
                return Result<OrderToReturnDto>.Fail(Error.Failure("Order Save Failed", "Cannot create order."));
            }

            await basketRepository.DeleteBasketAsync(orderDto.BasketId, cancellationToken);

            return mapper.Map<OrderToReturnDto>(order);
        }

        public async Task<Result<IReadOnlyList<DeliveryMethodDto>>> GetAllDeliveryMethodsAsync(CancellationToken cancellationToken = default)
        {
            var deliveryMethods = await unitOfWork.GetRepository<DeliveryMethod, int>().GetAllAsync(cancellationToken);
            return Result<IReadOnlyList<DeliveryMethodDto>>.Ok(mapper.Map<IReadOnlyList<DeliveryMethodDto>>(deliveryMethods));
        }

        public async Task<Result<IReadOnlyList<OrderToReturnDto>>> GetAllOrdersAsync(string email, CancellationToken cancellationToken = default)
        {
            var orders = await unitOfWork.GetRepository<Order, Guid>()
                .GetAllAsync(new OrderSpecifications(email), cancellationToken);
            return Result<IReadOnlyList<OrderToReturnDto>>.Ok(mapper.Map<IReadOnlyList<OrderToReturnDto>>(orders));
        }

        public async Task<Result<OrderToReturnDto>> GetOrderByIdAndEmailAsync(Guid id, string email, CancellationToken cancellationToken = default)
        {
            var order = await unitOfWork.GetRepository<Order, Guid>().GetByIdAsync(new OrderSpecifications(id, email), cancellationToken);
            if (order == null)
                return Result<OrderToReturnDto>.Fail(Error.NotFound("Order Not Found", $"Order With Id {id} Is Not Found"));
            return mapper.Map<OrderToReturnDto>(order);
        }

        // ---------------- Admin ----------------

        public async Task<Result<IReadOnlyList<OrderToReturnDto>>> GetAllOrdersForAdminAsync(CancellationToken cancellationToken = default)
        {
            var orders = await unitOfWork.GetRepository<Order, Guid>().GetAllAsync(new OrderSpecifications(), cancellationToken);
            return Result<IReadOnlyList<OrderToReturnDto>>.Ok(mapper.Map<IReadOnlyList<OrderToReturnDto>>(orders));
        }

        public async Task<Result<OrderToReturnDto>> UpdateOrderStatusAsync(Guid id, OrderStatus status, CancellationToken cancellationToken = default)
        {
            var orderRepo = unitOfWork.GetRepository<Order, Guid>();

            // Plain FindAsync-based lookup (not a specification), so the returned entity IS
            // tracked by EF - unlike specification-based reads which use AsNoTracking and
            // silently drop any change made without an explicit Update() call.
            var order = await orderRepo.GetByIdAsync(id, cancellationToken);

            if (order is null)
                return Result<OrderToReturnDto>.Fail(Error.NotFound("Order Not Found", $"Order With Id {id} Is Not Found"));

            order.Status = status;
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await GetOrderForAdminByIdAsync(id, cancellationToken);
        }

        private async Task<Result<OrderToReturnDto>> GetOrderForAdminByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var order = await unitOfWork.GetRepository<Order, Guid>().GetByIdAsync(new OrderSpecifications(id), cancellationToken);
            if (order == null)
                return Result<OrderToReturnDto>.Fail(Error.NotFound("Order Not Found", $"Order With Id {id} Is Not Found"));
            return mapper.Map<OrderToReturnDto>(order);
        }
    }
}