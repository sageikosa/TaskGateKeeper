using System.Collections.Concurrent;
using TaskGateKeeper.Sempahores;

namespace TaskGateKeeper;

public class TaskBarrierDictionary<TKey, TBarrier>
    : ITaskBarrierDictionary<TKey, TBarrier>
    where TKey : struct, IEquatable<TKey>
    where TBarrier : SemaphoreBarrier, new()
{
    private readonly ConcurrentDictionary<TKey, TBarrier> _Dictionary = new();

    public TBarrier GetOrAdd(TKey key, Func<TKey, TBarrier> addValueFactory)
        => _Dictionary.GetOrAdd(key, addValueFactory);

    public TBarrier? TryGetValue(TKey key)
        => _Dictionary.TryGetValue(key, out TBarrier? _value) ? _value : default;

    public TBarrier AddOrUpdate(TKey key, TBarrier addValue, Func<TKey, TBarrier, TBarrier> updateValueFactory)
        => _Dictionary.AddOrUpdate(key, addValue, updateValueFactory);

    public bool TryRemove(TKey key)
        => _Dictionary.TryRemove(key, out _);

    public void Clear()
        => _Dictionary.Clear();

    public bool HasKey(TKey key)
        => _Dictionary.ContainsKey(key);
}
