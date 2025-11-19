using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Humanizer;

namespace RestoRate.Restaurant.IntegrationTests;

public class AspireAppHost() : DistributedApplicationFactory(typeof(Projects.RestoRate_AppHost)), IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await StartAsync();
}
