using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ElectronicLib;

namespace Copybook
{
    /// <summary>
    /// Логика взаимодействия для PictureTile.xaml
    /// </summary>
    public partial class PictureTile : Image, IStyle
    {
        public TileMode TileMode { get => (TileMode)GetValue(TileBrush.TileModeProperty); set => SetValue(TileBrush.TileModeProperty, value); }
        public PictureTile()
        {
            InitializeComponent();
        }

        public void SetStyle(DesignManager design)
        {
            Source = design.GetBitmapSourceFromId(this.Tag.ToString());
            
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DesignManager.AddStyle(this);
        }
    }
}
