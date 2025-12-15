namespace TaskGateKeeper.Sempahores;

/// <summary>
/// Service provider scope friendly critical section, capable of being disposed, and thus useful in ASP.NET service provider scopes
/// </summary>
/// <remarks>
/// <para>Best use case is to create within a service provider scope that itself is disposed when finished.</para>
/// <para>The ASP.NET pipeline automatically disposes of scopes, so lifecycle within ASP.NET pipeline is automatically handled.</para>
/// </remarks>
/// <typeparam name="TBarrier">any barrier type used here should be registered as a singleton in the dependency container</typeparam>
public sealed class CriticalSection<TBarrier>(
    TBarrier barrier
    ) : IDisposable
    where TBarrier : SemaphoreBarrier
{
    // _Entered indicates this barrier has been passed within the current service provider scope
    private bool _Entered = false;

    private bool _Disposed;

    /// <summary>
    /// Try to enter the barriered section if not already passed it for this service provider scope.
    /// </summary>
    /// <param name="waitMilli">amount of time to wait in milliseconds if the barrier needs to be entered</param>
    /// <returns>true if progressed past the barrier</returns>
    public bool TryEnter(int waitMilli)
    {
        if (!_Entered)
        {
            _Entered = barrier.Enter(waitMilli);
        }
        return _Entered;
    }

    /// <summary>
    /// Explicitly exit an indexed barrier.
    /// </summary>
    /// <remarks>
    /// <para>This method is useful for leaving barriered sections before the service provider scope is complete.</para>
    /// <para>Usually used when flow processing has determined that the barriered section is no longer needed, 
    /// but the processing flow might continue looking for additional barriered resources.</para>
    /// <para>Work might be abandonned when additional barriered sections are needed, but are unable to be entered.</para>
    /// </remarks>
    public void TryLeave()
    {
        if (_Entered)
        {
            _Entered = false;
            barrier.Leave();
        }
    }

    /// <summary>
    /// "Casual" test to see if enterable
    /// </summary>
    /// <remarks>
    /// <para>If already entered in this service provider scope, it is strictly true.</para>
    /// <para>If the barrier is currently not in use, it is true, but still capable of being denied.</para>
    /// </remarks>
    /// <returns>true if enterable</returns>
    public bool IsEnterable()
        => _Entered || !barrier.InUse();

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                if (_Entered)
                {
                    barrier.Leave();
                    _Entered = false;
                }
            }

            _Disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code
        // Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
