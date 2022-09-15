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
    /// Логика взаимодействия для DataGridView.xaml
    /// </summary>
    public partial class DataGridView : UserControl
    {

        public Action<int, int> OnSelectDocument;

        public DataGridView()
        {
            InitializeComponent();
        }

        public void AddItem(Document documentFrom, int docHeaderIndex)
        {
            DataItem d = new DataItem();

            void show(int cmd)
            {
                SelectObject(documentFrom, Editor.GetDocumentHeaders()[docHeaderIndex].IndexOf(documentFrom), cmd);

            }

            d.clickReadonly += (o, e) =>
            {
                show(0);
            };
            d.clickEdit += (o, e) =>
            {
                show(1);
            };
            d.clickDelete += (o, e) =>
            {
                show(2);
            };

            d.SetInfo(items.Items.Count + 1, documentFrom.Name, documentFrom.DateTime.ToShortDateString(), documentFrom);



            items.Items.Add(d);
        }

        public void Clear()
        {
            items.Items.Clear();
        }

        private void items_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void SelectObject(Document dataItem, int index, int cmd)
        {
            OnSelectDocument?.Invoke(index, cmd);
        }
    }
}
