# SimpleXarts

SimpleXarts is a live update Xamarin.Forms charting library, designed to be used with MVVM.

## Getting started

### Install

Not yet on NuGet, because the structure of the charts is still subject to change,
to support a wider range of platforms.

Its not recommended to be used in its current state.

### Setup project

#### 1) Create the data to bind to a chart.
```csharp
public ObservableCollection<[Figure](Source/ChartBase/Figure.cs)> Data { get; set; } = new ObservableCollection<Figure>()
{
    new Figure(20)
    {
        Describtion = "Fruit",
        Color = System.Drawing.Color.FromArgb(240, 125, 100)
    },
    new Figure(5)
    {
        Describtion = "Fish",
        Color = System.Drawing.Color.FromArgb(100, 188, 194)
    },
    new Figure(12)
    {
        Describtion = "Sweets",
        Color = Xamarin.Forms.Color.FromRgb(242, 194, 84)
    },
    new Figure(20)
    {
        Describtion = "Vegetable",
        Color = Xamarin.Forms.Color.FromRgb(142, 215, 131)
    }
};
```

#### 2) Bind the data to a chart.
```xaml
<SimpleXarts:DonutChart Figures="{Binding Data} />
```

![gallery](Documentation/Gallery/DonutChartExample1.png)




#### 3) Update your data
```csharp
//change the value of an existing Figure
Data[0].Value = 30;

//add a new Figure to the chart
Data.Add(
    new Figure(20)
    {
        Describtion = "Spices",
        Color = Color.FromRgb(66, 72, 86)
    }
);

//remove a figure from the chart
Data.RemoveAt(1);
```