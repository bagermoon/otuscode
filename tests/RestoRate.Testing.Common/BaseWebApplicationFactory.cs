using DotNet.Testcontainers.Containers;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RestoRate.Testing.Common;

public abstract class BaseWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
{
    protected virtual IReadOnlyList<IContainer> Containers => Array.Empty<IContainer>();

    protected virtual Task OnInitializeAsync() => Task.CompletedTask;
    protected virtual Task DisposeCoreAsync() => Task.CompletedTask;

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(Containers.Select(container => container.StartAsync()));
        await OnInitializeAsync();
    }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public override async ValueTask DisposeAsync()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    {
        await DisposeCoreAsync();
        await Task.WhenAll(Containers.Select(container => container.DisposeAsync().AsTask()));
        await base.DisposeAsync();
    }
}
