# Execute Commands from a VS Extension

If you're like me and you've never created a Visual Studio extension before, it can be difficult to accomplish many of the simplest tasks.  Since macros have been removed from Visual Studio, I thought I'd see if I could create an extension to duplicate the behavior of one of the macros I used.  Part of this required executing an existing Visual Studio command.

The complete code for this extension is available here: https://github.com/veleek/CollapseEverythingElse.

## What I'm trying to accomplish

The previous few versions of Visual Studio removed support for macros, and while I generally didn't use macros, there was one thing I created a macro for that I found invaluable when spelunking through a large code base.  When working with a large file containing many methods and properties, I collapse everything to give myself a high level overview of the function and then F12 through bits and pieces to find the part I'm looking for or try and tease apart how the whole thing works.

Now after a few minutes of this, I often end up with a cluttered set of methods and code blocks expanded all throughout the file.  This can be annoying when I'm scrolling back and forth between a few different methods and I keep needing to scroll over a large block of code in between the two places.  

To solve this problem, I really wanted a Visual Studio command to **Collapse Everything Else**.  A macro makes quick work of this:

1. Get the current carat position.
2. Collapse all of the code outlining sections in the document.
3. Set the carat position to the saved value.

We don't need to figure out out to manually collapse every section.  Thankfully there's already a command for that, we just need to figure out how to execute it.

## Identifying the command to execute

Use the [MSDN documentation on the subject](https://msdn.microsoft.com/en-us/library/cc826040.aspx) was not quite as bad as it could have been.  It simple directs you to look at a set of `.vsct` files located in the Visual Studio SDK installation directory.  In my case they were located in `C:\Program Files (x86)\Microsoft Visual Studio 14.0\VSSDK\VisualStudioIntegration\common\inc`.  

Because these files are actually used to generate the menus, it can be a little hit or miss trying to find a given command due to the shortcut key bindings in the command text.  I'm looking for the **Collapse All** command, and I was able to find it by searching for `collapse_all` in `SharedCmdDef.vsct`.  

```xml
<Button guid="guidVSStd10" id="ECMD_OUTLN_COLLAPSE_ALL" priority="0x0000" type="Button">
  <CommandFlag>CommandWellOnly</CommandFlag>
  <CommandFlag>DynamicVisibility</CommandFlag>
  <CommandFlag>DefaultInvisible</CommandFlag>
  <CommandFlag>DefaultDisabled</CommandFlag>
  <Strings>
    <ButtonText>Collapse &amp;All Outlining</ButtonText>
  </Strings>
</Button>
```

We can use the `guid` and `id` attributes to find the appropriate GUIDs in the SDK.  
* Command set GUIDs are defined (at least in the managed interface) in the `VSConstants.CMDSETID` class.  In our case, `guidVSStd10` actually maps to `VSConstants.CMDSETID.StandardCommandSet2010_guid`.  The command id enum name maps close enough to the command set id that you should be able to figure out which one is which for any other command sets.  
* Command ids are defined as a set of enumerations in `VSConstants`.  Again, in this case `ECMD_OUTLN_COLLAPSE_ALL` maps to `VSConstants.VSStd2010CmdID.OUTLN_COLLAPSE_ALL`.  This should be enough to allow you to find any commands.

## Getting the service to execute the command

You have two options here depending on whether you want to execute the command asynchronously or synchronously.

### Asynchronously execute a command
This is actually one of the easiest methods to stumble on, since it's available right on one of the base classes `IVsUIShell` (inconsistent casing in class names is one of my pet peeves, so working with the VS SDK is super fun!).

`IVsUIShell` exposes the `PostExecCommandMethod` which accepts the command set GUID and command ID along with a set of execution options and a pointer to a set of input arguments if the command requires them.

The command set and command ID are the ones we determined above.  The executions options are wonderfully unclear, but thankfully we can use some of the slightly varied documentation for the synchronous method below to figure out a bit more.  For the most part, you can generally just ignore the last two parameters. 

```csharp
IVsUIShell shell = (IVsUIShell)serviceProvider.GetService(typeof(SVsUIShell));

Guid commandSet = VSConstants.CMDSETID.StandardCommandSet2010_guid;
uint commandId = (uint)VSConstants.VSStd2010CmdID.OUTLN_COLLAPSE_ALL;
int result = shell.PostExecCommand(commandSet, commandId, 0, null);

if(result == VSConstants.S_OK) ...
```

This begins executing the command and immediately returns control to you if everything is setup correctly.  If you need to perform any operation _after_ the command completes, take a look at executing the command synchronously.

### Synchronously Execute a Command

The [documentation for PostExecCommand](https://msdn.microsoft.com/en-us/library/microsoft.visualstudio.shell.interop.ivsuishell.postexeccommand.aspx?f=255&MSPPError=-2147217396) points you down the wrong path when you're trying to execute a command synchronously.  Use the `SUIHostCommandDispatcher` service (which returns an `IOleCommandTarget` instance) instead of `IVsUIShell`.

After that, executing the command is almost the same as with the asynchronous option, the only difference being that there's an additional out parameter at the end that returns the command result.

```csharp
IOleCommandTarget commandDispatcher = this.ServiceProvider.GetService(typeof(SUIHostCommandDispatcher)) as IOleCommandTarget;

Guid commandSet = VSConstants.CMDSETID.StandardCommandSet2010_guid;
uint commandId = (uint)VSConstants.VSStd2010CmdID.OUTLN_COLLAPSE_ALL;
var result = commandDispatcher.Exec(commandSet, commandId, 0, IntPtr.Zero, IntPtr.Zero);

if(result == VSConstants.S_OK) ...
```

#### How to not execute a command synchronously

I wrote this before testing it out because I was trying to follow the documentation, and of course that doesn't work.

Executing a command synchronously is very similar to the async path, but also just different enough to be annoying.  To start with, the synchronous method is located on a separate service from the asynchronous method; specifically the Host Command Dispatcher service.  Also, you can't query for that service using the default service provider.

The SDK Templates provide a `ServiceProvider` property for you automatically, which provides access to the explicitly implemented `System.IServiceProvider` interface through the package containing your extension.  This interface only exposes the `GetService(Type serviceType)` method.  We actually need to use the `QueryService(Guid serviceId)` method expased by a separate `IServiceProvider` interface, `Microsoft.VisualStudio.OLE.Interop.IServiceProvider`.  Thankfully this is exposed directly from the package object, so we can get the service we need by querying for the service id `VSConstants.SID_SUIHostCommandDispatcher`:

After that, executing the command is almost the same as with the asynchronous option, the only different being that there's an additional out parameter at the end that returns the command result.

```csharp
IOleCommandTarget commandDispatcherObj = this.package.QueryService(VSConstants.SID_SUIHostCommandDispatcher) as IOleCommandTarget;

Guid commandSet = VSConstants.CMDSETID.StandardCommandSet2010_guid;
uint commandId = (uint)VSConstants.VSStd2010CmdID.OUTLN_COLLAPSE_ALL;
int result = commandDispatcher.Exec(commandSet, commandId, 0, IntPtr.Zero, IntPtr.Zero);

if(result == VSConstants.S_OK) ...
```

If you're interested in fiddling around with the execution options, the documentation in this case actually provides a bit more detail about what it's looking for.  The `OLECMDEXECOPT` actually exists and has some reasonable documentation.

```csharp
// Specifies how the object should execute the command. Possible values are taken
// from the Microsoft.VisualStudio.OLE.Interop.OLECMDEXECOPT and Microsoft.VisualStudio.OLE.Interop.OLECMDID_WINDOWSTATE_FLAG
// enumerations.
```

`OLECMDID_WINDOWSTATE_FLAG` on the other hand, appears to not be exposed to the managed interface at all, but there is [some reasonable MSDN documentation on it](https://msdn.microsoft.com/en-us/library/windows/desktop/aa344051(v=vs.85).aspx). 
