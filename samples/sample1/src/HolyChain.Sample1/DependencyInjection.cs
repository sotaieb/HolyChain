using HolyChain.Sample1.UseCases.CreateOrder.Abstractions;
using HolyChain.Sample1.UseCases.CreateOrder.Activities.CreateOrder;
using HolyChain.Sample1.UseCases.CreateOrder.Services;
using Microsoft.Extensions.DependencyInjection;
using St.HolyChain.Core.Extensions;
using St.HolyChain.Core.Models;

namespace HolyChain.Sample1;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateOrderService, CreateOrderService>();
        services.AddScoped<IGetOrderService, GetOrderService>();

        services.ConfigureHolyChain(config =>
        {
            config.UseHandler<UseCases.CreateOrder.Activities.GetOrder.GetAccountActivity>();
            config.UseHandler<UseCases.CreateOrder.Activities.GetOrder.GetOrderActivity>(x => x.DependsOn  = [nameof(GetAccountActivity)]);
        });

        services.ConfigureHolyChain(config =>
        {
            config.UseHandler<ValidateOrderActivity>(x =>
                {
                    x.OrderId = 1;
                    x.GroupId = 0;
                    x.IsSync = true;
                })
                .UseHandler<GetAccountActivity>(x =>
                {
                    x.OrderId = 2;
                    x.GroupId = 10;
                })
                .UseHandler<GetProductsActivity>(x =>
                {
                    x.OrderId = 3;
                    x.GroupId = 10;
                })
                .UseHandler<CreateProviderOrderActivity>(x =>
                {
                    x.OrderId = 4;
                    x.GroupId = 20;
                    x.Type = ActivityType.Write;
                })
                .UseHandler<RegisterOrderActivity>(x => {
                    x.OrderId = 5;
                    x.GroupId = 30;
                    x.Type = ActivityType.Write;
                })
                .UseHandler<PublishOrderActivity>(x =>
                {
                    x.OrderId = 6;
                    x.GroupId = 40;
                    x.DependsOn = [nameof(CreateProviderOrderActivity), nameof(RegisterOrderActivity)];
                })
                .UseHandler<PublishStatusChangedActivity>(x =>
                {
                    x.OrderId = 7;
                    x.GroupId = 40;
                    x.DependsOn = [nameof(CreateProviderOrderActivity), nameof(RegisterOrderActivity)];
                });
        });

        return services;
    }
}