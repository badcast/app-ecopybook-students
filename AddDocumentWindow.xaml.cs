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
using System.Windows.Shapes;

namespace Copybook
{
    /// <summary>
    /// Логика взаимодействия для AddDocumentWindow.xaml
    /// </summary>
    public partial class AddDocumentWindow : Window
    {
        private bool isResult = false;
        public string Text { get { return textBox.Text; } }
        public DocumentHeader header;
        public AddDocumentWindow()
        {
            InitializeComponent();
        }

        private void CloseWindow()
        {

            string msg = "";
            if (string.IsNullOrEmpty(Text))
            {
                msg = "Ошибка ввода имени документа. Пожалуйста введите коректное имя.";
            }
            else if(header.HasDocument(Text))
            {
                msg = "Документ с таким именем уже существует.";
            }

            if (!string.IsNullOrEmpty(msg))
            {
                isResult = false;
                MessageBox.Show(msg);
                return;
            }

            this.Close();
        }



        //cancel
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isResult = false;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            isResult = true;
            CloseWindow();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        public bool ShowWindow(DocumentHeader headerDoc)
        {
            header = headerDoc;
            isResult = false;
            this.ShowDialog();
            return isResult;
        }
    }
}
