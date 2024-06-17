using HolyChain.Sample1.UseCases.CreateOrder.Abstractions;
using HolyChain.Sample1.UseCases.CreateOrder.Activities.CreateOrder;
using HolyChain.Sample1.UseCases.CreateOrder.Activities.GetOrder;
using Microsoft.Extensions.Options;
using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Models;

namespace HolyChain.Sample1.UseCases.CreateOrder.Services;

public class GetOrderService : IGetOrderService
{
    private readonly IPipelineFactory _pipelineFactory;


    public GetOrderService(IPipelineFactory pipelineFactory)
    {
        _pipelineFactory = pipelineFactory;
    }

    public async Task<GetOrderContext> RunAsync(GetOrderRequest request, CancellationToken cancellationToken)
    {

        var pipeline = _pipelineFactory.GetPipeline<GetOrderRequest, GetOrderContext>();

        var context = await pipeline.RunAsync(request, configureContext: x =>
        {
            x.DataCollection.Set("ThrowException", request.ThrowException);
        }, configureOptions: options =>
        {
            options.Tags = [];
            options.Id = request.UserId;
            options.RunMode = RunMode.Parallel;
            options.EnableRetry = true;
        }, cancellationToken);


        return context.Data;
    }
}
