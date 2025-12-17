using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskGateKeeper.Sempahores;

namespace TaskGateKeeper;

public static class DISetup
{
    /// <summary>
    /// Add services needed for scope-friendly critical sections and indexed critical sections
    /// </summary>
    public static IServiceCollection AddScopedCriticalSections(this IServiceCollection services)
    {
        // generic implementation for handling combinations of indexed barriers
        services.TryAddSingleton(typeof(IIndexedCriticalSectionDispenser<,>), typeof(IndexedCriticalSectionDispenser<,>));

        // SemaphoreBarriers used here tracked in IIndexedCriticalSectionDispenser<,> (a singleton)
        services.TryAddScoped(typeof(IndexedCriticalSection<,>));

        // SemaphoreBarriers used here should be registered "global" in dependency container
        services.TryAddScoped(typeof(CriticalSection<>));

        // TODO: setup some sample code to demonstrate usage

        return services;
    }
}
