# SimpleXarts

SimpleXarts is a live update charting library, designed to be used with MVVM and not leak into the Viewmodel.

## Getting started

### Install

Not yet on NuGet, because the structure of the charts is still subject to change,
to support a wider range of platforms

Its not recommended to be used in its current state

### Setup project

#### 1°) Create the data to bind to your chart

This can be a list of any types that implement the needed Properties

```csharp
public class MyFigure
{
    public int Value {get;set;}
    public string Describtion {get;set;}
    public System.Drawing.Color Color {get;set;}
}
```

You don't need to set every property, just the ones you need.
For live updates, your class needs to implement the INotifyPropertyChange