# Binding Events with Parameters

It's relatively easy to bind events to UI using [Caliburn.Micro](http://caliburnmicro.com) and the [cheat-sheet](http://caliburnmicro.com/documentation/cheat-sheet) provides a good overview of the various different ways you can bind events to UI elements. 

I started out with a simple button like this

```xml
    <Button x:Name="Evaluate" Content="Evaluate"/>
```

And it worked fine, but I needed access to the UI element for the quick hack-job that I was doing (yes, in general it's good design to avoid referring to the UI directly from the view model but for this scenario it's fine).  So I added a more complicated handler to bind the parameter that I needed.

```xml
    <Button x:Name="Evaluate" Content="Evaluate">
      <i:Interaction.Triggers>
        <i:EventTrigger EventName="Click">
          <cal:ActionMessage MethodName="Evaluate">
            <cal:Parameter Value="{Binding ElementName=GridBox}" />
          </cal:ActionMessage>
        </i:EventTrigger>
      </i:Interaction.Triggers>
    </Button>
```

Unfortunately I started getting `NullReferenceException`s all over whenever I clicked the button.  My attempts at debugging showed that I was getting a null reference on a line that I had already successfully debugged past.  I was attempting to use an `async void` event handler in this case and I assumed it was related to that issue, but no amount of tweaking and twiddling would get it to work.  

Eventually I noticed that somehow my event was getting called twice, but the second time without the parameter being set.  This gave me the push needed to realize that the XML was using BOTH the convention based event binding and the explicit event binding, but the convention based binding had no parameter and so just passed null.  Renaming the button to something other than `Evaluate` finally got rid of the issue.
