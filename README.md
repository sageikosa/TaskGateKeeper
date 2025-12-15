TaskGateKeeper
==============
Critical sectioning for (dependency-injected) async and Task based .NET code.

[CS1996](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/lock-semantics) 
warns about code using _**await**_ inside of lock() statement sections, as _**await**_ yields the thread back to the runtime,
with no guarantee the same thread will be used to continue once the awaited task completed.  Any other thread wouldn't be able 
to release the lock(), and the very next attempt to lock the same target would hang (i.e., wait forever for a monitor that would 
never be released).

```csharp
public class BadClass
{
    private readonly static Lock _Locker = new();

    private static async Task DoStuffAsync()
    {
        // normally there'd be something "async" and more meaty than this
        // but this demonstrates the pattern that is a problem
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

I stumbled upon this potential problem (in the time of .NET 3.1) before CS1996 became a normal warning (in the time of .NET 5), and built 
a task/async-friendly critical-sectioning mini-framework around SemaphoreSlim for use in dependency-injection scenarios such as ASP.NET.

Dependency Scopes and IDisposable
---------------------------------
The Microsoft.Extensions.DependencyInjection namespace and NuGet package allows IServiceProvider to create an instance of IServiceScope
using .CreateScope().  IServiceScope is derived from IDisposable, and will dispose all IDisposable services resolved through it when it
is disposed itself.

In ASP.NET, every pipeline call from "outside" the runtime will be processed in an IServiceScope setup for the call, and disposed when the call
is complete.  This automatic disposal allows one to define interfaces and classes with IDisposable that get disposed when the scope itself ends 
(that is, when the call finishes).  This behavior is used to avoid orphaning the critical-section gatekeeping sempahores which I'll explain below.

> When creating an IServiceScope instance directly, it is important to manage the lifetime of the IServiceScope and call IDispose or reference it 
within a "using" block.

Critical-Sectioning with Semaphores
-----------------------------------
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