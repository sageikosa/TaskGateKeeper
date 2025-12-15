namespace TaskGateKeeper.Sempahores;

/// <summary>
/// Marker interface for SemaphoreBarriers to help ensure registered correctly.
/// </summary>
/// <typeparam name="TKey">key constraint</typeparam>
public interface IIndexableSemaphoreBarrier<TKey>
    where TKey : struct, IEquatable<TKey>
{
}
