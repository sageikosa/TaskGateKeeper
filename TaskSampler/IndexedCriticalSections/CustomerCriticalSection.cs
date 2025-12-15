using System;
using System.Collections.Generic;
using System.Text;
using TaskGateKeeper.Sempahores;

namespace TaskSampler.IndexedCriticalSections;

/// <summary>
/// Customer critical sections to be indexed by CustomerID.
/// </summary>
public class CustomerCriticalSection : SemaphoreBarrier, IIndexableSemaphoreBarrier<CustomerID>
{
}
