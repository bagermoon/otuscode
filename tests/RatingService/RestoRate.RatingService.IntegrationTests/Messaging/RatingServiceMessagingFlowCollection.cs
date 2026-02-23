using Xunit;

namespace RestoRate.RatingService.IntegrationTests.Messaging;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class RatingServiceMessagingFlowCollection
{
    public const string Name = "RatingService messaging flow";
}
