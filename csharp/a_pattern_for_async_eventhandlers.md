# A Pattern for Async EventHandlers

The [standard pattern for events in C#](https://msdn.microsoft.com/en-us/library/w369ty8x.aspx) is unfortunately not very friendly for use with asyncronous event handlers due to the fact that the built in [`EventHandler`](https://msdn.microsoft.com/en-us/library/system.eventhandler.aspx) delegate returns void, so there's no easy way to perform any asyncronous operation short of blocking the caller until you're complete.  

While there's little we can do about existing code built around this pattern, it's possible to extend it a little bit so that we can take advantage of it in our code bases and build ourselves a foundation for more complete async event support in the future using a little bit of boilerplate.

## Wishing for `async event` 

In an ideal world we could easily extend the C# compile support `async event` alongside `event` and automatically construct all of the standard event handler goop that `event` provides.  I believe that I could technically get away with writing a bunch of Roslyn stuff to do this, but there doesn't seem to be an easy way to expose that functionality to others, so in the meantime let's just stick with some canned code and a snippet or two.


## AsyncEventHandler

The first layer that we need is the async equivalent of `EventHandler`

    public delegate Task AsyncEventHandler(object sender, EventArgs args)
    public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs args)

Thankfully there's no strict restriction on the return type for event delegates so we can just return a `Task` and be done with it.

## Event Properties

When you declare an `event` in a class, the compiler automatically implements `add` and `remove` [event properties](https://msdn.microsoft.com/en-us/library/8843a9ch.aspx) for you.  These can be manually defined to support more complex eventing scenarios

```csharp
public event AsyncEventHandler LoadedAsync
{
    add { ... }
    remove { ... }
}
```

Within `add/remove` the `value` keyword gives us a reference to the `AsyncEventHandler` instance that was added or removed from the event the same way you get `value` in a `set` property accessor.  

Multiple event handlers can be registered with a single event, so we need to store our list of handlers somewhere.  In a regular event, all of the event handlers are combined into a single delegate using [`Delegate.Combine`](https://msdn.microsoft.com/en-us/library/b1eh4771(v=vs.110).aspx).  This works great for `void` methods, but because we need the return value (the `Task` we're going to have to use something else).  We can use whatever sort of collection we want, but let's just stick with `List`.  


```csharp
private List<AsyncEventHandler> LoadedAsyncHandlers = new List<AsyncEventHandler>();

public event AsyncEventHandler LoadedAsync
{
    add { this.LoadedAsyncEventHandlers.Add(value); }
    remove { this.LoadedAsyncEventHandlers.Remove(value); }
}
```

## Raising async events

Great, now just need to add a method to raise our event and we should be good.  The standard pattern for this is pretty simple and we can modify it to be async pretty easily.

```csharp
public async Task OnLoadedAsync()
{
    if(this.LoadedAsync != null)
    {
        await this.LoadedAsync(this, new EventArgs())
    }
}
```

Uh oh!  This is giving us an error:

    CS0079 The event 'LoadedAsync' can only appear on the left hand side of += or -=

With a regular event, the compiler gives us access direct access to a delegate field, so it makes sense that we can just invoke it since it handles making sure that each handler is combined into a single invocation point.  But when manually implementing the event properties, we can do whatever want with the target event handlers, so we'll also need to manually invoke them.  

```csharp
public async Task OnLoadedAsync()
{
    if(this.LoadedAsyncEventHandlers.Count > 0)
    {
        object sender = this;
        EventArgs args = new EventArgs();
        await Task.WhenAll(this.LoadedAsyncEventHandlers.Select(h => h(sender, args)));
    }
}
```

We can only return a single Task, so we're using a simple LINQ statement to invoke the handlers and grouping all the resulting Tasks using `Task.WhenAll`.  This is going to execute all of the event handlers simultaneously, but it's entirely possible they need to run sequentially (an exercise left up to the user).

## Putting it all together

Now let's take a quick look at how this will get used (in [LINQPad](https://linqpad.net))

```csharp
Sample x = new Sample();
x.LoadedAsync += async (s, e) => { await Task.Delay(2000); "Waited 2 seconds.".Dump(); };
x.LoadedAsync += async (s, e) => { await Task.Delay(1000); "Waited 1 second.".Dump(); };

"Starting".Dump();
await x.OnLoadedAsync();
"Done".Dump();
```

Gives us the output

```
Starting
Waited 1 second.
Waited 2 seconds.
Done
```

This shows us that everything is getting run in parallel as we expected and the use pattern is almost identical to a regular event.  

## AsyncEvent helper class

The implementation that we've got above will work perfectly well in many cases, but it's missing a bunch of safety checks that you get with regular events (e.g. dealing with race conditions during add/remove).  Once you start adding these and a few extra things, the amount of code you need for each event starts to get a bit unwieldy.  

You can help with this by pulling out a bunch of the functionality into a separate AsyncEvent class.  See my [example AsyncEvent implementation LINQPad script](./AsyncEventHandlers.linq) for a starting point.  

## Boilerplate Snippets

This snippets should make it easier for you to implement this pattern in your own code without needing to worry about getting the boilerplate wrong.  

### VSCode Snippets

* Follow the directions for [creating your own VSCode snippets](https://code.visualstudio.com/docs/customization/userdefinedsnippets#_creating-your-own-snippets) and then add the following snippets.

```json
{
    "Async Event": {
        "prefix": "asyncevent",
        "body": [
            "private AsyncEvent ${1:asyncEvent} = new AsyncEvent();",
            "public event AsyncEventHandler ${2:AsyncEvent}",
            "{",
            "    add { ${1:asyncEvent}.Add(value); }",
            "    remove { ${1:asyncEvent}.Remove(value); }",
            "}",
            "",
            "protected Task On${2:AsyncEvent}()",
            "{",
            "    return ${1:asyncEvent}.InvokeAsync(this, new EventArgs()$0);",
            "}"
        ],
        "description": "Boilerplate for an async/await friendly event using an `AsyncEvent` helper class (see Async Event Class snippet)"
    },
    "Async Event Class": {
        "prefix": "asynceventclass",
        "body":[
            "public delegate Task AsyncEventHandler(object sender, EventArgs e);",
            "",
            "public class AsyncEvent",
            "{",
            "	private List<AsyncEventHandler> eventHandlers;",
            "",
            "	public void Add(AsyncEventHandler handler)",
            "	{",
            "		if (eventHandlers == null)",
            "		{",
            "			eventHandlers = new List<AsyncEventHandler>();",
            "		}",
            "",
            "		eventHandlers.Add(handler);",
            "	}",
            "",
            "	public void Remove(AsyncEventHandler handler)",
            "	{",
            "		if (this.eventHandlers == null)",
            "		{",
            "			return;",
            "		}",
            "",
            "		this.eventHandlers.Remove(handler);",
            "	}",
            "",
            "	public Task InvokeAsync(object sender, EventArgs args)",
            "	{",
            "		if (this.eventHandlers == null || this.eventHandlers.Count == 0)",
            "		{",
            "			return Task.CompletedTask;",
            "		}",
            "		",
            "		return Task.WhenAll(eventHandlers.Select(g => g(sender, args)));",
            "	}",
            "}"
        ],
        "description": "A reusable AsyncEvent helper class to encapsulate the logic for implementing an async event."
    },
    "Async Event Full": {
        "prefix": "asynceventf",
        "body":[
            "public delegate Task ${3:AsyncEventHandler}(object sender, EventArgs e);",
            "",
            "private List<${3:AsyncEventHandler}> ${1:asyncEventHandlers} = new List<${3:AsyncEventHandler}>();",
            "",
            "public event ${3:AsyncEventHandler} ${2:AsyncEvent}",
            "{",
            "    add { this.${1:asyncEventHandlers}.Add(value); }",
            "    remove { this.${1:asyncEventHandlers}.Remove(value); }",
            "}",
            "",
            "protected virtual async Task On${2:AsyncEvent}()",
            "{",
            "    if(this.${1:asyncEventHandlers}.Count > 0)",
            "    {",
            "        object sender = this;",
            "        EventArgs args = new EventArgs()$0;",
            "        await Task.WhenAll(this.${1:asyncEventHandlers}.Select(h => h(sender, args)));",
            "    }",
            "}"
        ],
        "description": "Boilerplate for an async/await friendly event which __does not__ depend on an AsyncEvent helper class."
    } 
}
```

### Visual Studio Snippets

Visual Studio snippets are harder to write.  So they are not ready yet.