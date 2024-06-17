using HolyChain.Sample1.UseCases.CreateOrder.Abstractions;
using HolyChain.Sample1.UseCases.CreateOrder.Activities.CreateOrder;
using Microsoft.Extensions.Options;
using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Models;

namespace HolyChain.Sample1.UseCases.CreateOrder.Services;

public class CreateOrderService : ICreateOrderService
{
    private readonly IPipelineFactory _pipelineFactory;

    public CreateOrderService(IPipelineFactory pipelineFactory)
    {
        _pipelineFactory = pipelineFactory;
    }

    public async Task<CreateOrderContext> RunAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var pipeline = _pipelineFactory.GetPipeline<CreateOrderRequest, CreateOrderContext>();

        var context = await pipeline.RunAsync(request, configureContext: x =>
        {
            x.DataCollection.Set("ThrowException", request.ThrowException);
        }, configureOptions: x =>
        {
            x.Id = request.UserId;
            x.RunMode = RunMode.Parallel;
            x.EnableRetry = true;
            x.CleanCompletedEvents = true;
        }, cancellationToken);
      
        
        return context.Data;
    }
}
