using TaskGateKeeper.Sempahores;

namespace TaskGateKeeper;

/// <summary>
/// Defines a thread-safe collection that maps keys to semaphore barriers, providing methods to add, retrieve, update,
/// and remove barriers associated with specific keys.
/// </summary>
/// <remarks>This interface is designed for scenarios where multiple asynchronous operations need to be
/// coordinated using barriers associated with distinct keys. Implementations are expected to be thread-safe, allowing
/// concurrent access and modification. The dictionary ensures that each key is associated with a unique semaphore
/// barrier, enabling fine-grained synchronization across tasks.</remarks>
/// <typeparam name="TKey">The type of key used to identify each semaphore barrier. Must be a value type that implements <see
/// cref="IEquatable{TKey}"/>.</typeparam>
/// <typeparam name="TBarrier">The type of semaphore barrier stored in the dictionary. Must inherit from <see cref="SemaphoreBarrier"/> and have a
/// parameterless constructor.</typeparam>
public interface IIndexedTaskBarrierDispenser<TKey, TBarrier>
    where TKey: struct, IEquatable<TKey>
    where TBarrier : SemaphoreBarrier, new()
{
    /// <summary>
    /// Called by <see cref="IndexedBarriers{TKey, TBarrier}"/> to signal starting use of barriers in a DI scope."
    /// </summary>
    /// <remarks>Called in <see cref="IndexedBarriers{TKey, TBarrier}"/> ctor()</remarks>
    void StartingUse();

    /// <summary>
    /// Called by <see cref="IndexedBarriers{TKey, TBarrier}"/> to signal finished use of barriers in a DI scope."
    /// </summary>
    /// <remarks>Called in <see cref="IndexedBarriers{TKey, TBarrier}"/> Dispose</remarks>
    void FinishedUse();

    /// <summary>
    /// Gets the existing barrier associated with the specified key, or adds a new barrier created by the specified
    /// factory if none exists.
    /// </summary>
    /// <remarks>
    /// If multiple threads attempt to add a barrier for the same key concurrently, only one barrier
    /// will be created and returned for that key. Subsequent calls with the same key will return the existing
    /// barrier.
    /// </remarks>
    /// <param name="key">The key used to locate or create the barrier.</param>
    /// <param name="valueFactory">A function that provides a new barrier instance for the specified key if no barrier is currently associated with
    /// it.</param>
    /// <returns>
    /// The barrier associated with the specified key. If no barrier exists, a new one is created using the value
    /// factory and returned.
    /// </returns>
    TBarrier GetOrAdd(TKey key, Func<TKey, TBarrier> valueFactory);

    /// <summary>
    /// Attempts to retrieve the barrier associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose associated barrier is to be retrieved.</param>
    /// <returns>The barrier associated with the specified key if found; otherwise, null.</returns>
    TBarrier? TryGetValue(TKey key);
}