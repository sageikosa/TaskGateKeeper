using System;
using System.Collections.Generic;
using System.Text;
using TaskGateKeeper.Sempahores;

namespace TaskSampler.IndexedCriticalSections;

/// <summary>
/// Inventory zone critical sections to be indexed by composite <see cref="InventoryZoneKey"/>
/// </summary>
public class InventoryZoneCriticalSection : SemaphoreBarrier, IIndexableSemaphoreBarrier<InventoryZoneKey>
{
}
