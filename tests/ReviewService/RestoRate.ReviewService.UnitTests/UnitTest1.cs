using AutoFixture;
using AutoFixture.AutoNSubstitute;

namespace RestoRate.ReviewService.UnitTests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        fixture.Create<Review>();

        Assert.True(true);
    }
}
