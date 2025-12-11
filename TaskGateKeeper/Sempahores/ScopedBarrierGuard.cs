namespace TaskGateKeeper.Sempahores;

/// <summary>
/// DI-scope friendly barrier guard, capable of being disposed, and thus pretty useful in ASP.NET DI-scopes
/// </summary>
/// <remarks>
/// <para>Best use case is to create from a DI-container within a DI-scope that itself is disposed when finished.</para>
/// <para>The ASP.NET pipeline automatically disposes of scopes, so lifecycle within ASP.NET pipeline is automatically handled.</para>
/// </remarks>
/// <typeparam name="TBarrier">any barrier type used here should be registered as a singleton in the DI-container</typeparam>
public sealed class ScopedBarrierGuard<TBarrier>(
    TBarrier barrier
    ) : IDisposable
    where TBarrier : SemaphoreBarrier
{
    // _Entered indicates this barrier has been passed within the current DI-Scope
    private bool _Entered = false;

    private bool _Disposed;

    /// <summary>
    /// Try to enter the barriered section if not already passed it for this DI-scope.
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
    /// Try to leave the barrier within the DI-scope.
    /// </summary>
    public void TryLeave()
    {
        if (_Entered)
        {
            _Entered = false;
            barrier.Leave();
        }
    }

    /// <summary>
    /// "Casual" test to see if entereable
    /// </summary>
    /// <remarks>
    /// <para>If already entered in this DI-scope, it is strictly true.</para>
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
