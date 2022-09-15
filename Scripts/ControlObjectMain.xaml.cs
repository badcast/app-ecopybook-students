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

namespace Copybook
{
    /// <summary>
    /// Логика взаимодействия для ControlObjectMain.xaml
    /// </summary>
    public partial class ControlObjectMain : UserControl
    {
        public int Index { get => int.Parse(indexLabel.Content.ToString()); set => indexLabel.Content = value; }
        public string Title { get => textLabel.Content.ToString(); set => textLabel.Content = value; }
        public string Title2 { get => textLabel2.Content.ToString(); set => textLabel2.Content = value; }
        public ControlObjectMain()
        {
            InitializeComponent();
        }
    }
}
