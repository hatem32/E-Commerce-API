using AutoMapper;
using E_Commerce.Application.Common;
using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Baskets;
using E_Commerce.Application.Specifications;
using E_Commerce.Domain.Contracts;
using E_Commerce.Domain.Entities.Orders;
using E_Commerce.Domain.Entities.Products;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentGateway _paymentGateway;
        private readonly PaymentGatewaySettings _stripe;
        private readonly IMapper _mapper;

        public PaymentService(IBasketRepository basketRepository,
        IUnitOfWork unitOfWork,
        IPaymentGateway paymentGateway,
        IOptions<PaymentGatewaySettings> stripeSettings,
        IMapper mapper)
        {
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
            _paymentGateway = paymentGateway;
            _stripe = stripeSettings.Value;
            _mapper = mapper;
        }
        public async Task<Result<BasketDto>> CreateOrUpdatePaymentIntentAsync(string basketId, CancellationToken cancellationToken = default)
        {
            // Get The Basket [Validate]
            var basket = await _basketRepository.GetBasketAsync(basketId, cancellationToken);

            if (basket == null)
                return Result<BasketDto>.Fail(Error.NotFound("Basket Not Found", $"Basket With Id {basketId} Is Not Found"));

            if (basket.Items.Count == 0)
                return Result<BasketDto>.Fail(Error.Validation("Basket is Empty", $"Can Not Create Order With Basket With Id {basketId}"));


            // Get The Products [Validate the Price]
            var productRepo = _unitOfWork.GetRepository<Product, int>();
            var productIds = basket.Items.Select(i => i.Id).ToHashSet();
            var products = (await productRepo.GetAllAsync(new ProductsWithIdsSpecifications(productIds), cancellationToken)).ToDictionary(x => x.Id);

            foreach (var item in basket.Items)
            {
                if (!products.TryGetValue(item.Id, out var product))
                    return Result<BasketDto>.Fail(Error.NotFound("Product Not Found", $"Product With Id {item.Id} Is Not Found "));

                item.Price = product.Price;
            }


            // Get The Delivery Method [Validate]
            if (!basket.DeliveryMethodId.HasValue)
            {
                return Error.Validation("Delivery Method Id Is Required");
            }

            var deliveryRepo = _unitOfWork.GetRepository<DeliveryMethod, int>();
            var deliveryMethod = await deliveryRepo.GetByIdAsync(basket.DeliveryMethodId.Value, cancellationToken);
            if (deliveryMethod == null)
                return Result<BasketDto>.Fail(Error.NotFound("Delivery Method Not Found", $"DeliveryMethod With Id {basket.DeliveryMethodId} Is Not Found "));

            basket.ShippingPrice = deliveryMethod.Cost;

            // Total Amount = Subtotal + DeliveryMethod Cost
            var subtotal = basket.Items.Sum(i => i.Quantity * i.Price);
            var amount = (long)Math.Round((subtotal + deliveryMethod.Cost) * 100m);

            // Create or Update Payment Intent
            if (!string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                await _paymentGateway.UpdatePaymentIntentAsync(basket.PaymentIntentId, amount, cancellationToken);
            }
            else
            {
                var result = await _paymentGateway.CreatePaymentIntentAsync(amount, _stripe.DefaultCurrency, cancellationToken);
                basket.PaymentIntentId = result.PaymentIntentId;
                basket.ClientSecret = result.ClientSecret;
            }

            // Save The Basket {Updated}
            await _basketRepository.CreateOrUpdateBasketAsync(basket, cancellationToken: cancellationToken);
            return _mapper.Map<BasketDto>(basket);
        }

        public async Task PaymentFailed(string paymentIntentId)
        {
            var orderRepo = _unitOfWork.GetRepository<Order, Guid>();

            var order = await orderRepo.GetByIdAsync(new PaymentIntentSpec(paymentIntentId));

            if (order == null)
                return;

            order.Status = OrderStatus.PaymentFailed;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task PaymentSucceeded(string paymentIntentId)
        {
            var orderRepo = _unitOfWork.GetRepository<Order, Guid>();

            var order = await orderRepo.GetByIdAsync(new PaymentIntentSpec(paymentIntentId));

            if (order == null)
                return;
            order.Status = OrderStatus.PaymentReceived;

            await _unitOfWork.SaveChangesAsync();
        }


    }
}
