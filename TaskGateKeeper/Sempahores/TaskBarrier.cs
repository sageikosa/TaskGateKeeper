namespace TaskGateKeeper.Sempahores;

/// <summary>
/// Task friendly barrier, capable of being disposed, and thus pretty good in ASP.NET DI Scopes
/// </summary>
public sealed class TaskBarrier<TBarrier>(
    TBarrier barrier
    ) : IDisposable
    where TBarrier : SemaphoreBarrier
{
    private bool _Entered = false;
    private bool _Disposed;

    public bool TryEnter(int waitMilli)
    {
        if (!_Entered)
        {
            _Entered = barrier.Enter(waitMilli);
        }
        return _Entered;
    }

    public void TryLeave()
    {
        if (_Entered)
        {
            _Entered = false;
            barrier.Leave();
        }
    }

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
