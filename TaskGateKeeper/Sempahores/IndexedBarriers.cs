namespace TaskGateKeeper.Sempahores;

public class IndexedBarriers<TKey, TBarrier>(
    ITaskBarrierDictionary<TKey, TBarrier> barriers
    ) : IDisposable
    where TKey : struct, IEquatable<TKey>
    where TBarrier : SemaphoreBarrier, new()
{
    private readonly HashSet<TKey> _Entered = [];
    private bool _Disposed;

    public bool TryEnter(TKey key, int waitMilli)
    {
        if (!_Entered.Contains(key))
        {
            var _blocker = barriers.GetOrAdd(key, 
                _ndx =>
                {
                    return new TBarrier();
                });
            if (_blocker?.Enter(waitMilli) ?? false)
            {
                _Entered.Add(key);
                return true;
            }
            return false;
        }
        return true;
    }

    public void TryLeave(TKey key)
    {
        if (_Entered.Remove(key))
        {
            barriers.TryGetValue(key)?.Leave();
        }
    }

    public bool IsEnterable(TKey key)
    {
        if (!_Entered.Contains(key))
        {
            var _blocker = barriers.GetOrAdd(key, 
                _ndx =>            
                {
                    return new TBarrier();
                });
            return !(_blocker?.InUse() ?? true);
        }
        return true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                foreach (var _entered in _Entered)
                {
                    barriers.TryGetValue(_entered)?.Leave();
                }
                _Entered.Clear();
            }

            _Disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code
        // Put cleanup code in Dispose(bool disposing) method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
