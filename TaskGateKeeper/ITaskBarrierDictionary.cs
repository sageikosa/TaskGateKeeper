using TaskGateKeeper.Sempahores;

namespace TaskGateKeeper;

public interface ITaskBarrierDictionary<TKey, TBarrier>
    where TKey: struct, IEquatable<TKey>
    where TBarrier : SemaphoreBarrier, new()
{
    bool HasKey(TKey key);
    TBarrier GetOrAdd(TKey key, Func<TKey, TBarrier> valueFactory);
    TBarrier? TryGetValue(TKey key);
    TBarrier AddOrUpdate(TKey key, TBarrier addValue, Func<TKey, TBarrier, TBarrier> updateValueFactory);
    bool TryRemove(TKey key);
    void Clear();
}