namespace TaskGateKeeper.Sempahores;

/// <summary>
/// Implementation of indexed barrier guard management, for gate-keeping access to SemaphoreBarriers across multiple
/// DI-scopes.
/// </summary>
/// <remarks>
/// Use <see cref="TryEnter(TKey, int)"/> to attempt to gain access to a barrier.  
/// Proceed with processing only if successful.
/// </remarks>
/// <typeparam name="TKey">value type to use as a key</typeparam>
/// <typeparam name="TBarrier">barriers used are created and destroyed by this class</typeparam>
public sealed class IndexedBarrierGuards<TKey, TBarrier> : IDisposable
    where TKey : struct, IEquatable<TKey>
    where TBarrier : SemaphoreBarrier, new()
{
    /// <summary>List of all keys that have entered barriers in this scoped instance.</summary>
    private readonly HashSet<TKey> _Entered = [];

    /// <summary>Reference to the injected generic singleton dispenser</summary>
    private readonly IIndexedBarrierDispenser<TKey, TBarrier> _Barriers;

    /// <summary>Disposed status flag</summary>
    private bool _Disposed;

    /// <summary>
    /// Intended to be called from DI container.
    /// </summary>
    /// <param name="barriers">should be registered as generic singleton</param>
    public IndexedBarrierGuards(
        IIndexedBarrierDispenser<TKey, TBarrier> barriers
    )
    {
        // capture injected dispenser
        _Barriers = barriers;

        // prevents dispenser from clearing out barriers while active
        _Barriers.StartingUse();
    }

    /// <summary>
    /// Get barrier for the key in the dispenser, or add a new one.
    /// </summary>
    /// <param name="key">key to associated with the barrier</param>
    /// <returns>barrier indexed by key in dispenser</returns>
    /// <param name="key">key for the barriered section</param>
    private TBarrier GetBarrier(TKey key)
        => _Barriers.GetOrAdd(key, _ndx => new TBarrier());

    /// <summary>
    /// Try to enter an indexed barrier, waiting for a number of milliseconds if semaphore is not ready for use.
    /// </summary>
    /// <param name="key">key for the barrier in the dispenser</param>
    /// <param name="waitMilli">max wait time (if needed)</param>
    /// <remarks>
    /// <para>If unable to enter the barrier, any concurrently unsafe processing should be bypassed</para>
    /// </remarks>
    /// <returns>true if barrier entered, otherwise false</returns>
    /// <param name="key">key for the barriered section</param>
    public bool TryEnter(TKey key, int waitMilli)
    {
        if (!_Entered.Contains(key))
        {
            var _blocker = GetBarrier(key);
            if (_blocker.Enter(waitMilli))
            {
                _Entered.Add(key);
                return true;
            }
            return false;
        }
        return true;
    }

    /// <summary>
    /// Explicitly exit an indexed barrier.
    /// </summary>
    /// <remarks>
    /// <para>This method is useful for leaving barriered sections before the DI scope is complete.</para>
    /// <para>Usually used when flow processing has determined that the barriered section is no longer needed, 
    /// but the processing flow might continue looking for additional barriered resources.</para>
    /// <para>Work might be abandonned when additional barriered sections are needed, but are unable to be entered.</para>
    /// </remarks>
    /// <param name="key">key for the barriered section</param>
    public void TryLeave(TKey key)
    {
        if (_Entered.Remove(key))
        {
            _Barriers.TryGetValue(key)?.Leave();
        }
    }

    /// <summary>
    /// "Casual" test to see if enterable
    /// </summary>
    /// <remarks>
    /// <para>If already entered in this DI-scope, it is strictly true.</para>
    /// <para>If the barrier is currently not in use, it is true, but still capable of being denied.</para>
    /// </remarks>
    /// <returns>true if enterable</returns>
    /// <param name="key">key for the barriered section</param>
    public bool IsEnterable(TKey key)
    {
        if (!_Entered.Contains(key))
        {
            var _blocker = GetBarrier(key);
            return !_blocker.InUse();
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
