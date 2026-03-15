using System.Collections.Concurrent;

using MassTransit;

using RestoRate.Contracts.Restaurant;
using RestoRate.Contracts.Restaurant.Requests;

namespace RestoRate.ReviewService.IntegrationTests;

public sealed class TestRestaurantStatusResponder
{
    private readonly ConcurrentDictionary<Guid, GetRestaurantStatusResponse> _responses = new();
    private readonly ConcurrentQueue<Guid> _requestedRestaurantIds = new();

    public void SetResponse(Guid restaurantId, bool exists, RestaurantStatus status)
    {
        _responses[restaurantId] = new GetRestaurantStatusResponse(restaurantId, exists, status);
    }

    public void Reset()
    {
        _responses.Clear();

        while (_requestedRestaurantIds.TryDequeue(out _))
        {
        }
    }

    public void RecordRequest(Guid restaurantId)
        => _requestedRestaurantIds.Enqueue(restaurantId);

    public GetRestaurantStatusResponse Resolve(Guid restaurantId)
        => _responses.TryGetValue(restaurantId, out var response)
            ? response
            : new GetRestaurantStatusResponse(restaurantId, Exists: false, Status: RestaurantStatus.Unknown);

    public Guid[] RequestedRestaurantIds()
        => _requestedRestaurantIds.ToArray();
}

public sealed class GetRestaurantStatusRequestTestConsumer(
    TestRestaurantStatusResponder responder)
    : IConsumer<GetRestaurantStatusRequest>
{
    public Task Consume(ConsumeContext<GetRestaurantStatusRequest> context)
    {
        var restaurantId = context.Message.RestaurantId;
        responder.RecordRequest(restaurantId);

        return context.RespondAsync(responder.Resolve(restaurantId));
    }
}
