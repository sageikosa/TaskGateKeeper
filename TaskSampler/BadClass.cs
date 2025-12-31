namespace TaskSampler;

public class BadClass
{
    private readonly static Lock _Locker = new();

    private static async Task DoStuffAsync()
    {
        // normally there'd be something "async" and more meaty than this
        // but this demonstrates the pattern that is a problem
        await Task.Delay(10000);
    }

    public static async Task<bool> DoSomething()
    {
        lock (_Locker)
        {
            // this next line fatally warns with CS1996
            //await DoStuffAsync();
        }
        return true;
    }
}
