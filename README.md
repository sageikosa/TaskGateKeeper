TaskGateKeeper
==============
Critical sectioning for (dependency-injected) async and Task based .NET code.

CS1996 warns about code using _**await**_ inside of lock() statement sections, as _**await**_ yields the thread back to the runtime,
with no guarantee the same thread will be used to continue once the awaited task completed.  Any other thread wouldn't be able 
to release the lock(), and the very next attempt to lock the same target would hang (i.e., wait forever for a monitor that would 
never be released).

I stumbled upon this potential problem (in the time of .NET 3.1) before CS1996 became a normal warning (in the time of .NET 5), and built 
a task/async-friendly mini-framework around SemaphoreSlim for use in dependency-injection scenarios such as ASP.NET.

Dependency Scopes and IDisposable
---------------------------------
