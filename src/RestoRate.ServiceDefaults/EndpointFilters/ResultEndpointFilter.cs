using Microsoft.AspNetCore.Http;

namespace RestoRate.ServiceDefaults.EndpointFilters;
public sealed class ResultEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var response = await next(context);

        if (response is not null && response is Ardalis.Result.IResult ardalisResult)
        {
            return ardalisResult.ToMinimalApiResult();
        }

        return response;
    }
}
