using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskGateKeeper.Sempahores;

namespace TaskGateKeeper;

public static class DISetup
{
    public static IServiceCollection AddScopeBarriers(this IServiceCollection services)
    {
        // generic implementation for handling combinations of indexed barriers
        services.TryAddSingleton(typeof(IIndexedBarrierDispenser<,>), typeof(IndexedBarrierDispenser<,>));

        // SemaphoreBarriers used here tracked in IIndexedBarrierDispenser<,> (a singleton)
        services.TryAddScoped(typeof(IndexedBarrierGuards<,>));

        // SemaphoreBarriers used here should be registered "global" in DI container
        services.TryAddScoped(typeof(ScopedBarrierGuard<>));

        // TODO: setup some sample SemaphoreBarriers to use with ScopedBarrierGuard<>

        // TODO: setup some sample code to demonstrate usage

        return services;
    }
}
