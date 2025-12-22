using TaskGateKeeper.Sempahores;

namespace TaskSampler.SingletonCriticalSections;

/// <summary>
/// Register as a singleton and use with <see cref="CriticalSection{TBarrier}/>
/// </summary>
public class SingletonBarrier : SemaphoreBarrier
{
}
