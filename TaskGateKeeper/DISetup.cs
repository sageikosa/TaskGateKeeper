using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskGateKeeper.Sempahores;

namespace TaskGateKeeper;

public static class DISetup
{
    public static IServiceCollection AddTaskBarriers(this IServiceCollection services)
    {
        services.TryAddSingleton(typeof(IIndexedTaskBarrierDispenser<,>), typeof(IndexedTaskBarrierDispenser<,>));

        // TODO: make sure task-barriers in ITaskBarrierDictionary<,> get removed when no longer used

        // SemaphoreBarriers used here tracked in ITaskBarrierDictionary<,> (a singleton)
        services.TryAddScoped(typeof(IndexedBarriers<,>));

        // SemaphoreBarriers used here should be registered "global" in DI container
        services.TryAddScoped(typeof(TaskBarrier<>));

        // TODO: setup some sample SemaphoreBarriers to use with TaskBarrier<>

        // TODO: setup some sample code to demonstrate usage

        return services;
    }
}
