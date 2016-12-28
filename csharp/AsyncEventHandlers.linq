<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
    Sample x = new Sample();
    x.LoadingAsync += async (s, e) => { await Task.Delay(2000); "Waited 2 seconds.".Dump(); };
	x.LoadingAsync += async (s, e) => { await Task.Delay(1000); "Waited 1 second.".Dump(); };
	x.Loaded += (s, e) => { "Loaded".Dump(); };
    
    "Starting".Dump();
    await x.LoadAsync();
    "Done".Dump();
}

public delegate Task AsyncEventHandler(object sender, EventArgs e);
public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e);

public class Sample
{
    private AsyncEvent loadingAsyncEvent = new AsyncEvent();
    public event AsyncEventHandler LoadingAsync
    {
        add { loadingAsyncEvent.Add(value); }
        remove { loadingAsyncEvent.Remove(value); }
    }
	
	public event EventHandler Loaded;
   
	public async Task LoadAsync()
	{
		await this.OnLoadingAsync();
		this.OnLoaded();
	}
	
    protected Task OnLoadingAsync()
	{
        return loadingAsyncEvent.InvokeAsync(this, new EventArgs());
    }
	
	protected void OnLoaded()
	{
		this.Loaded?.Invoke(this, new EventArgs());
	}
}

public class AsyncEvent
{
	private List<AsyncEventHandler> eventHandlers;

	public void Add(AsyncEventHandler handler)
	{
		if (eventHandlers == null)
		{
			eventHandlers = new List<AsyncEventHandler>();
		}

		eventHandlers.Add(handler);
	}

	public void Remove(AsyncEventHandler handler)
	{
		if (this.eventHandlers == null)
		{
			return;
		}

		this.eventHandlers.Remove(handler);
	}

	public Task InvokeAsync(object sender, EventArgs args)
	{
		if (this.eventHandlers == null)
		{
			return Task.CompletedTask;
		}
		
		return Task.WhenAll(eventHandlers.Select(g => g(sender, args)));
	}
}

public class AsyncEvent<TEventArgs>
{
	private List<AsyncEventHandler<TEventArgs>> eventHandlers;

	public void Add(AsyncEventHandler<TEventArgs> handler)
	{
		if (eventHandlers == null)
		{
			eventHandlers = new List<AsyncEventHandler<TEventArgs>>();
		}

		eventHandlers.Add(handler);
	}

	public void Remove(AsyncEventHandler<TEventArgs> handler)
	{
		if (this.eventHandlers == null)
		{
			return;
		}
		
		this.eventHandlers.Remove(handler);
	}

	public Task InvokeAsync(object sender, TEventArgs args)
	{
		if (this.eventHandlers == null)
		{
			return Task.CompletedTask;
		}
		
		return Task.WhenAll(eventHandlers.Select(g => g(sender, args)));
	}
}
