using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Copybook;

namespace ElectronicLib
{
    public interface IWindow : IStyle
    {
        void Created();
        void Showed();
        void Closed();
        UIElement GetElement();
    }

    public interface IWindowBaseWithParam : IWindow
    {
        void Showed(SpecialitiesType specialitiesType);
    }


    public interface IStyle
    {
        void SetStyle(DesignManager design);
    }
}
