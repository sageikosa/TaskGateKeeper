TaskGateKeeper
==============
TaskGateKeeper gives you an easy way to protect async .NET code from the "`await` inside a `lock()`" problem 
[CS1996](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/lock-semantics) by using 
an alternate _**task-based**_ synchronization pattern instead of a _**thread-based**_ synchronization pattern. 

TaskGateKeeper uses `SemaphoreSlim` so that any thread can "release" the lock, making it feasible to use in `async`/`await` flows.

&#x21D3;&#x21D3;&#x21D3; Example BAD Code &#x21D3;&#x21D3;&#x21D3;
------------------------------------------------------------------
```csharp
public class BadClass
{
    // going to use the new .NET 10 Lock class, but that won't help here anyway!
    private readonly static Lock _Locker = new();

    private static async Task DoStuffAsync()
    {
        // normally the "async" part would be meatier
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

Simple and Lengthy Alternative
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
        // normally the "async" part would be meatier
        await Task.CompletedTask;
    }

    public static async Task<bool> DoSomething()
    {
        var _entered = false;
        try
        {
            // try to enter, waiting for one second
            _entered = _Barrier.Wait(1000);

            if (_entered)
            {
                // succeeded in entering
                await DoStuffAsync();
            }
            else
            {
                throw new InvalidOperationException(@"waited too long");
            }
        }
        finally
        {
            if (_entered)
            {
                // only call if locally entered
                _Barrier.Release();
            }
        }
        return true;
    }
}

```
That's a lot of patternable code, which is why I wrapped it in some classes.

IDisposable in ASP.NET Service Scopes
-------------------------------------
The `IDisposable` interface is "special" in C# and .NET.  There is a keyword (`using`) dedicated to simplifying proper management of classes that implement it.

In the world of .NET dependency injection, `IServiceScope` is an `IDisposable` that will dispose all scoped services it resolved when itself is disposed.

ASP.NET uses an `IServiceScope` per request when routing requests from the outside world, meaning that there is automatic disposal for scope-registered `IDisposable` classes.  

Critical-Sectioning with Semaphores
-----------------------------------
> TODO: mutex is like a check-out register for a thread;

> TODO: semaphore is like a row of cars (or single car) flagged to go through a one-lane road that needs "bi-directional" travel, and part-way in, the driver can switch

Quick overview of [Synchronization Primitives in .NET](https://learn.microsoft.com/en-us/dotnet/standard/threading/overview-of-synchronization-primitives).
While Semaphore and [SemaphoreSlim](https://learn.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim?view=net-10.0) are listed as a thread synchronization primitive, this is strictly true only for calls to Wait or WaitAsync, which will block if the semaphore's
CurrentCount is 0.  Release can be called by any thread, unlike in a Mutex, which operates on a thread ownership model rather than a limited counter 
gatekeeping model.

This means that SemaphoreSlim with maximum capacity of 1 can be used as a critical section counter, and the thread that releases the section doesn't have to be the same thread that entered the section.  Normally in serial processing code this is the same thread anyway, but in async task-based code, 
it can be another thread from the runtime's thread pool.

SemaphoreBarrier Base Class and Derived Classes
-----------------------------------------------

Scoped Critical-Section Service
-------------------------------

Indexed Critical-Section Dispenser Service
------------------------------------------

Indexed Scoped Critical-Section Service
---------------------------------------