using System.Collections.Concurrent;
using TaskGateKeeper.Sempahores;

namespace TaskGateKeeper;

public sealed class IndexedBarrierDispenser<TKey, TBarrier>
    : IIndexedBarrierDispenser<TKey, TBarrier>
    where TKey : struct, IEquatable<TKey>
    where TBarrier : SemaphoreBarrier, new()
{
    /// <summary>
    /// Concurrent dictionary mapping keys to semaphore barriers.
    /// </summary>
    private readonly ConcurrentDictionary<TKey, TBarrier> _Dictionary = new();

    /// <summary>
    /// Lock used to protect active use count and cleanup
    /// </summary>
    private readonly Lock _UseCounterLock = new();

    /// <summary>
    /// Number of active uses of the dispenser
    /// </summary>
    private int _UseCounter = 0;

    /// <summary>
    /// Gets the semaphore barrier associated with the specified key, or adds a new one if it does not exist.
    /// </summary>
    /// <param name="key">key for the barrier</param>
    /// <param name="addValueFactory">function to generate a barrier if not found in the dictionary</param>
    /// <returns>Barrier for the key</returns>
    public TBarrier GetOrAdd(TKey key, Func<TKey, TBarrier> addValueFactory)
        => _Dictionary.GetOrAdd(key, addValueFactory);

    /// <summary>
    /// Attempts to get the semaphore barrier associated with the specified key.
    /// </summary>
    /// <param name="key">key for the barrier</param>
    /// <returns>specified barrier, or null</returns>
    public TBarrier? TryGetValue(TKey key)
        => _Dictionary.TryGetValue(key, out TBarrier? _value) ? _value : default;

    /// <summary>
    /// Called by <see cref="IndexedBarrierGuards{TKey, TBarrier}"/> to signal starting use of barriers in a DI scope."
    /// </summary>
    public void StartingUse()
    {
        // lock to prevent increment while decrement and cleanup happening
        using (_UseCounterLock.EnterScope())
        {
            // increase counter
            _UseCounter++;
        }
    }

    /// <summary>
    /// Called by <see cref="IndexedBarrierGuards{TKey, TBarrier}"/> to signal finished use of barriers in a DI scope."
    /// </summary>
    public void FinishedUse()
    {
        // lock to prevent increment while decrement and cleanup happening
        using (_UseCounterLock.EnterScope())
        {
            // decrease counter
            _UseCounter--;

            // cleanup dictionary if last active use
            // due to the lock, cannot be raced against by a new active use
            if (_UseCounter <= 0)
            {
                _Dictionary.Clear();
                _UseCounter = 0;
            }
        }
    }
}
