using TaskGateKeeper.Sempahores;
using TaskSampler.SingletonCriticalSections;

namespace TaskSampler.SingletonSamples;

public class MainProcessor(
    CriticalSection<MainBarrier> criticalMainProcessor
    )
{
    public async Task<string> TryToDoStuff(CancellationToken token)
    {
        // try to enter the critical section, waiting up to 1 second
        if (criticalMainProcessor.TryEnter(1000, token))
        {
            // wait for 10 seconds while in the critical section
            await Task.Delay(10000, token);
            return @"critical";
        }
        else
        {
            return @"denied";
        }
    }
}
