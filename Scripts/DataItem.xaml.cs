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
    /// Логика взаимодействия для DataItem.xaml
    /// </summary>
    public partial class DataItem : UserControl
    {
        public Document DocumentEntry { get; set; }
        public string Title { get { return titleValue.Content.ToString(); } set { titleValue.Content = value; } }
        public string DataValue { get { return dataValue.Content.ToString(); } set { dataValue.Content = value; } }
        public int Index { get { return int.Parse(nomerHeader.Content.ToString()); } set { nomerHeader.Content = value; } }

        public event Action<object, RoutedEventArgs> clickReadonly;
        public event Action<object, RoutedEventArgs> clickEdit;
        public event Action<object, RoutedEventArgs> clickDelete;

        public DataItem()
        {
            InitializeComponent();
        }

        public void SetInfo(int index, string title, string data, Document document)
        {
            Title = title;
            DataValue = data;
            Index = index;

            viewBt.Click += clickReadonly.Invoke;
            editBt.Click += clickEdit.Invoke;
            deleteBt.Click += clickDelete.Invoke;

        }
    }
}
