namespace TaskGateKeeper.Sempahores;

public sealed class IndexedBarriers<TKey, TBarrier> : IDisposable
    where TKey : struct, IEquatable<TKey>
    where TBarrier : SemaphoreBarrier, new()
{
    private readonly HashSet<TKey> _Entered = [];
    private readonly IIndexedTaskBarrierDispenser<TKey, TBarrier> _Barriers;
    private bool _Disposed;

    public IndexedBarriers(
        IIndexedTaskBarrierDispenser<TKey, TBarrier> barriers
    )
    {
        _Barriers = barriers;

        // prevents dispenser from clearing out barriers while active
        _Barriers.StartingUse();
    }

    public bool TryEnter(TKey key, int waitMilli)
    {
        if (!_Entered.Contains(key))
        {
            var _blocker = _Barriers.GetOrAdd(key, 
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
            _Barriers.TryGetValue(key)?.Leave();
        }
    }

    public bool IsEnterable(TKey key)
    {
        if (!_Entered.Contains(key))
        {
            var _blocker = _Barriers.GetOrAdd(key, 
                _ndx =>            
                {
                    return new TBarrier();
                });
            return !(_blocker?.InUse() ?? true);
        }
        return true;
    }

    private void Dispose(bool disposing)
    {
        // safeguard against multiple calls
        if (!_Disposed)
        {
            if (disposing)
            {
                // clean up anything entered
                foreach (var _entered in _Entered)
                {
                    _Barriers.TryGetValue(_entered)?.Leave();
                }

                // allows the dispenser to clean up unused barriers
                _Barriers.FinishedUse();

                // forget what has been entered
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
