using SimpleXarts.ChartBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SimpleXarts
{
    internal class FigureAccessCollection : KeyedCollection<object, FigureAccess>
    {
        protected override object GetKeyForItem(FigureAccess item) => item.Source;
    }
}
