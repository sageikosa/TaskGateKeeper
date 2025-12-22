using Microsoft.Extensions.DependencyInjection;
using TaskSampler.SingletonCriticalSections;
using TaskSampler.SingletonSamples;

namespace TaskSampler;

public static class DISetup
{
    /// <summary>
    /// Add sample critical sections used in TaskSampler project
    /// </summary>
    public static IServiceCollection AddSampleCriticalSections(this IServiceCollection services)
    {
        // setup all singleton barriers
        services.AddSingleton<MainBarrier>();
        services.AddSingleton<ProcessBarrier>();
        services.AddSingleton<SingletonBarrier>();

        // NOTE: no need to setup indexed critical sections, the standard implementation handles everything needed
        return services;
    }

    public static IServiceCollection AddSampleProcessors(this IServiceCollection services)
    {
        services.AddScoped<MainProcessor>();
        return services;
    }
}
