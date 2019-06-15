# SimpleXarts

SimpleXarts is a live update charting library, designed to be used with MVVM.

## Getting started

### Install

Not yet on NuGet, because the structure of the charts is still subject to change,
to support a wider range of platforms

Its not recommended to be used in its current state

### Setup project

#### 1) Create the data to bind to your chart

```csharp
public ObservableCollection<Figure> Data { get; set; } = new ObservableCollection<Figure>()
{
    new Figure(20)
    {
        Describtion = "Bread",
        Color = System.Drawing.Color.Brown
    },
    new Figure(5)
    {
        Describtion = "Meat",
        Color = System.Drawing.Color.Red
    },
    new Figure(12)
    {
        Describtion = "Fish",
        Color = System.Drawing.Color.Blue
    }
};
```

#### 2) Bind it to your chart
```xaml
<SimpleXarts:DonutChart Figures="{Binding Data} />
```

#### 3) Update your data
```csharp
Data[0].Value = 30;
```