using Microsoft.Extensions.DependencyInjection;
using TaskSampler.SingletonCriticalSections;

namespace TaskSampler;

public static class DISetup
{
    public static IServiceCollection AddSampleSemaphores(this IServiceCollection services)
    {
        // setup all singleton barriers
        services.AddSingleton<MasterCriticalSection>();
        services.AddSingleton<ProcessCriticalSection>();
        services.AddSingleton<SingletonCriticalSection>();

        // NOTE: no need to setup indexed critical sections, the standard implementation handles everything needed
        return services;
    }
}
