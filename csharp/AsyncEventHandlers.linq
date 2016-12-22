<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
    Test t = new Test();
    t.MyAsyncEvent += async(s, e) => { await Task.Delay(1000); "One Second".Dump(); };
    t.MyAsyncEvent += async(s, e) => { await Task.Delay(2000); "Two Second".Dump(); };
    
    "Starting".Dump();
    await t.OnMyAsyncEvent();
    "Done".Dump();
}

public delegate Task AsyncEventHandler(object sender, EventArgs e);

public delegate Task<T> AsyncEventHandler<T>(object sender, EventArgs e);

public class Test
{
    private AsyncEvent myAsyncEvent = new AsyncEvent();
    public event AsyncEventHandler MyAsyncEvent
    {
        add { myAsyncEvent.Add(value); }
        remove { myAsyncEvent.Remove(value); }
    }
    
    public Task OnMyAsyncEvent()
    {
        return myAsyncEvent.Invoke(this, null);
    }
}

public class AsyncEvent
{
    private List<AsyncEventHandler> eventTaskGenerators;
    
    public void Add(AsyncEventHandler handler)
    {
        if (eventTaskGenerators == null)
        {
            eventTaskGenerators = new List<AsyncEventHandler>();
        }
        
        eventTaskGenerators.Add(handler);
    }
    
    public void Remove(AsyncEventHandler handler)
    {
        this.eventTaskGenerators.Remove(handler);
    }
    
    public Task Invoke(object sender, EventArgs e)
    {
        return Task.WhenAll(eventTaskGenerators.Select(g => g(this, null)));
    }
}