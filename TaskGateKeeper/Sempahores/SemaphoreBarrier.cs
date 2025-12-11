namespace TaskGateKeeper.Sempahores;

/// <summary>
/// Provides a synchronization barrier that allows only one DI-scope to enter at a time, 
/// using a semaphore-based mechanism.
/// </summary>
/// <remarks>
/// <para>
/// This abstract class offers basic enter and leave operations for mutual exclusion scenarios. It can be
/// used to coordinate access to a shared resource among multiple DI-scopes.
/// </para>
/// <para>
/// Derived classes can be used as generic type parameters to control access to either: a singleton resource, using
/// <see cref="ScopedBarrierGuard{TBarrier}"></see>;
/// or from amongst a set of indexed resources using <see cref="IndexedBarrierGuards{TKey, TBarrier}"/>
/// </para>
/// </remarks>
public abstract class SemaphoreBarrier
{
    /// <summary>
    /// Underlying semaphore used to enforce the barrier.
    /// </summary>
    private readonly SemaphoreSlim _Semaphore = new(1, 1);

    /// <summary>
    /// Reports whether the barrier is currently in use (i.e., if a DI-scope has entered and not yet left).
    /// </summary>
    public bool InUse()
        => _Semaphore.CurrentCount == 0;

    /// <summary>
    /// Attempts to enter the barrier, waiting up to the specified number of milliseconds.
    /// </summary>
    /// <param name="waitMS"></param>
    public bool Enter(int waitMS)
        => _Semaphore.Wait(waitMS);

    /// <summary>
    /// Leaves the barrier, releasing the semaphore.
    /// </summary>
    public void Leave()
    {
        _ = _Semaphore.Release();
    }
}
