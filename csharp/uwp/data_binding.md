# Data Binding

Data Binding is an _incredibly_ useful tool for helping simplify seperation of concerns, and there is a huge amount of functionality available for XAML applications in both UWP and WPF applications.  But if you're just getting started, it can be a very complicated affair.

## `{x:Bind}` vs `{Binding}`

For the most part x:Bind and Binding behave the same, but be implementation is a little different.  Binding's are evaluated at runtime, whereas x:Bind is evaluated at compile time.

MSDN has a very thorough overview of [the differences between `{x:Bind}` and `{Binding}`](https://msdn.microsoft.com/windows/uwp/data-binding/data-binding-in-depth#xbind-and-binding-feature-comparison), but the most important one (in my opinion) is the default value for `Mode`.  
* `{x:Bind}` defaults to `OneTime`.  Bindings will be evaluated _once_ when the page is loaded.
* `{Binding}` defaults to `OneWay`.  Bindings are evaluated and updated every time the source property changes.

