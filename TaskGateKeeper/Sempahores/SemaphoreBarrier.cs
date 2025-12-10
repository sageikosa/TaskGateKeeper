namespace TaskGateKeeper.Sempahores;

/// <summary>
/// Provides a synchronization barrier that allows only one task to enter at a time, 
/// using a semaphore-based mechanism.
/// </summary>
/// <remarks>This abstract class offers basic enter and leave operations for mutual exclusion scenarios. It can be
/// used to coordinate access to a shared resource among multiple tasks. 
/// Derived classes are strongly-typed references that can be used for specific barrier implementations.
/// </remarks>
public abstract class SemaphoreBarrier
{
    private readonly SemaphoreSlim _Semaphore = new(1, 1);

    public bool InUse()
        => _Semaphore.CurrentCount == 0;

    public bool Enter(int waitMS)
        => _Semaphore.Wait(waitMS);

    public void Leave()
    {
        _ = _Semaphore.Release();
    }
}
