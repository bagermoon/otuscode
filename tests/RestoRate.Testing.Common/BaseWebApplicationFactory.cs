using DotNet.Testcontainers.Containers;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace RestoRate.Testing.Common;

public abstract class BaseWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
{
    protected virtual IReadOnlyList<IContainer> Containers => Array.Empty<IContainer>();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        Task.WaitAll(Containers.Select(container => container.StartAsync()));
        var host = CreateHostAsync(builder).GetAwaiter().GetResult();
        return host;
    }
    protected virtual Task<IHost> CreateHostAsync(IHostBuilder builder)
        => Task.FromResult(base.CreateHost(builder));
    protected virtual Task DisposeCoreAsync() => Task.CompletedTask;

    public virtual ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public override async ValueTask DisposeAsync()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    {
        await DisposeCoreAsync();
        await Task.WhenAll(Containers.Select(container => container.DisposeAsync().AsTask()));
        await base.DisposeAsync();
    }
}
