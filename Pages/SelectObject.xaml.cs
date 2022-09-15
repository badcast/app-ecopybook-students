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
    /// Логика взаимодействия для PageFirst.xaml
    /// </summary>
    public partial class SelectObject : UserControl, IWindow
    {
        private List<ControlObjectMain> objects = new List<ControlObjectMain>();
        private DocumentEditorPage page;

        public SelectObject()
        {
            InitializeComponent();
        }


        public void Closed()
        {
        }

        public void Created()
        {
            list.Items.Clear();

            var ele = Editor.GetDocumentHeaders();

            for (int i = 0; i < ele.Length; i++)
            {
                var f = ele[i];

                ControlObjectMain obj = new ControlObjectMain() { Title = f.Title1, Title2 = f.Title2 };
                obj.Index = i + 1;
                objects.Add(obj);

                obj.Width = 700;
                obj.MouseLeftButtonUp += (o, e) =>
                {
                    SelectPage(obj.Index - 1);
                };
                list.Items.Add(obj);
                obj.MouseDown += (o, e) =>
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        SelectPage(obj.Index - 1);
                    }

                };
            }
        }

        public void SelectPage(int index)
        {
            if (page == null)
                page = (DocumentEditorPage)MainWindow.Current.GetWindow(MainWindow.Windows.DocumentEditorPage);
            page.DocumentHeaderIndex = index;
            MainWindow.Current.ShowWindow(MainWindow.Windows.DocumentEditorPage);

        }

        public UIElement GetElement()
        {
            return this;
        }

        public void SetStyle(DesignManager design)
        {
            DesignManager.RefreshStyle(design);
        }

        public void Showed()
        {
        }

        private void TripleButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.ShowWindow(MainWindow.Windows.Main);
        }
    }
}
