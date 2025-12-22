TaskGateKeeper
==============
TaskGateKeeper gives you an easy way to protect async .NET code from the "`await` inside a `lock()`" problem 
[CS1996](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/lock-semantics) by using 
an alternate _**task-based**_ synchronization pattern instead of a _**thread-based**_ synchronization pattern. 

TaskGateKeeper uses `SemaphoreSlim` so that any thread can "release" the lock, making it feasible to use in `async`/`await` flows.

&#x21D3;&#x21D3; Example BAD Code &#x21D3;&#x21D3;
------------------------------------------------------------------
```csharp
public class BadClass
{
    // going to use the new .NET 10 Lock class, but that won't help here anyway!
    private readonly static Lock _Locker = new();

    private static async Task DoStuffAsync()
    {
        // normally the "async" Stuff would be meatier
        // but this demonstrates the problem pattern
        await Task.CompletedTask;
    }

    public static async Task<bool> DoSomething()
    {
        lock (_Locker)
        {
            // this next line fatally warns with CS1996
            await DoStuffAsync();
        }
        return true;
    }
}
```

C# `lock` Implementation
------------------------
The `lock` keyword and code-block in C# generates a `Monitor.Enter` call and a `try/finally` block in which the monitor is released if entered.  The problem is that only the actual thread that entered the monitor can release it.  The monitor is _**owned**_ by the original calling thread.

Since `async` code may use different threads on either side of an `await` call, ownership can't be guaranteed on the release call.

The Effective (but Somewhat Unwieldy) Pattern
------------------------------
A simple alternative (and the core of TaskGateKeeper) is to define a `SemaphoreSlim` and follow the same basic approach that the C# compiler uses
within a `finally` block.

```csharp
public class SimpleSemaphoreBlocking
{
    // SemaphoreSlim is "more better" than Semaphore most of the time
    private readonly static SemaphoreSlim _Barrier = new(1,1);

    private static async Task DoStuffAsync()
    {
        // normally the "async" Stuff would be meatier
        await Task.CompletedTask;
    }

    public static async Task<bool> DoSomething()
    {
        // track that any particular task's code-path has "entered" the semaphore
        var _entered = false;
        try
        {
            // try to enter, waiting for one second
            _entered = _Barrier.Wait(1000);

            if (_entered)
            {
                // succeeded in entering
                await DoStuffAsync();

                // true means "DoSomething" task did "DoStuff"
                return true;
            }
            else
            {
                // false indicates "DoSomething" task didn't "DoStuff"
                return false;
            }
        }
        finally
        {
            // this block will always be entered
            // check that the code-path for this task "entered" the semaphore region
            if (_entered)
            {                
                // yes, so need to release it (increment the semaphore's counter)
                _Barrier.Release();
            }
        }
    }
}

```
There's a lot of patternable code in that, hence the library-esque classes.

IDisposable in ASP.NET Service Scopes
-------------------------------------
The `IDisposable` interface is "special" in C# and .NET.  There is a keyword (`using`) dedicated to simplifying proper management of classes that implement it.

In the world of .NET dependency injection, `IServiceScope` is an `IDisposable` that (when disposed) will dispose all scoped services it resolved.

ASP.NET uses a new `IServiceScope` per request when resolving dependencies, and disposes of it when the request ends.  Therefore, any scope-registered `IDisposable` resolved in an ASP.NET request, will be disposed when the request ends.

Singleton Critical Sections
---------------------------
Canonical critical sections are implemented with the `CriticalSection<>` class, and derived `SemaphoreBarrier` classes.  
Each singleton-registered derived `SemaphoreBarrier` represents one critical section in the running process.  
Unique critical sections are represented by different derived `SemaphoreBarrier` classes.

### CriticalSection
`CriticalSection<TBarrier>` is a sealed generic class that expects a `SemaphoreBarrier` type as its only type parameter.  
`CriticalSection<TBarrier>` implements `IDisposable` to participate in dependency scoped disposal operations.  
`CriticalSection<TBarrier>` should be registered with as a scoped dependency as demonstrated in **TaskGateKeeper\DISetup.cs**.
```csharp
    public static IServiceCollection AddScopedCriticalSections(this IServiceCollection services)
    {
        ...
        // SemaphoreBarriers used here should be registered "global" in dependency container
        services.TryAddScoped(typeof(CriticalSection<>));
        ...
    }
```

### SemaphoreBarrier
`SemaphoreBarrier` is a abstract wrapper around a SemaphoreSlim in which the max capacity is capped at 1.  
`SemaphoreBarrier` is abstract so that concrete types must be defined and used in dependency registration and injection resolution.  
**TaskSampler\SingletonCriticalSections** defines several derived barrier classes that are registered as singletons in **TaskSampler\DISetup.cs**.
```csharp
    // setup all singleton barriers
    services.AddSingleton<MainBarrier>();
    services.AddSingleton<ProcessBarrier>();
    services.AddSingleton<SingletonBarrier>();
```

### MainProcessor (Sample)
`MainProcesor` in **TaskSampler** demonstrates how to get the `CriticalSection<MainBarrier>` class injected, and how to use for critical section safety.  
`MainProcesor` is setup as a scoped service in **TaskSampler\DISetup.cs**.

Indexed Critical Sections
-------------------------
Indexed critical sections represent protection around resources indexed by keys.  

The canonical use case is ensuring that two distinct order-pickers in a warehouse are not assigned to pick the same inventory from the same container or aisle at the same time.  The provisioning process that determines and assigns inventory to an order-picker must get exclusive control of the container or aisle to assign the inventory, such that if two processes are running simultaneously and need the same inventory, they cannot race each other or overcommit the inventory.  

The "key" for the resource might be the aisle-key or the container-key, or a combination of aisle (or container) and item-type for finer granularity.
Since often multiple containers or aisles may hold pickable inventory of the same item-type, the provisioning process can "walk-over" resources it cannot get access within a wait time and look elsewhere in the inventory set; allowing the provisioning process to complete faster.

### IndexedCriticalSection
`IndexedCriticalSection<TKey, TBarrier>` is a sealed generic class that expects a `TKey` type, and a `SemaphoreBarrier` that implements `IIndexableSemaphoreBarrier<TKey>`.  
`IndexedCriticalSection<TKey, TBarrier>` implements `IDisposable` to participate in dependency scoped disposal operations.  
`IndexedCriticalSection<TKey, TBarrier>` should be registered with as a scoped dependency as demonstrated in **TaskGateKeeper\DISetup.cs**.
```csharp
    public static IServiceCollection AddScopedCriticalSections(this IServiceCollection services)
    {
        ...
         // SemaphoreBarriers used here tracked in IIndexedCriticalSectionDispenser<,> (a singleton)
        services.TryAddScoped(typeof(IndexedCriticalSection<,>));
       ...
    }
```

### IIndexableSemaphoreBarrier
`IIndexableSemaphoreBarrier<TKey>` must be implemented on derived `SemaphoreBarrier` classes to participate in Indexed Critical Sections.  
`IIndexableSemaphoreBarrier<TKey>` helps ensure intent and code-reference type-checking.  
`SemaphoreBarrier` derived classes that implement `IIndexableSemaphoreBarrier<TKey>` do _**NOT**_ need to registered in the dependency container, they will be managed by the next class, the `IIndexedCriticalSectionDispenser<,>`

### IIndexedCriticalSectionDispenser
`IIndexedCriticalSectionDispenser<TKey, TBarrier>` and it's implementation defines the service behavior for gaining access to indexed semaphore barriers.  
`IIndexedCriticalSectionDispenser<TKey, TBarrier>` is expected to only be used within the `IndexedCriticalSection<,>` class.  
`IIndexedCriticalSectionDispenser<TKey, TBarrier>` manages the singleton concurrent dictionaries used for indexed critical sections, as well as tracking if any `IndexedCriticalSection<,>` classes are in active use of the dispenser.  When nothing is using a particular `<TKey, TBarrier>` combination, the dictionary may be purged of all items.