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
    /// Логика взаимодействия для DocumentEditorPage.xaml
    /// </summary>
    public partial class DocumentEditorPage : UserControl, IWindow
    {
        private AddDocumentWindow docEdit;
        private DocumentPage docPage;
        private DocumentHeader[] docHeaders;
        public int DocumentHeaderIndex { get; set; }
        public DocumentHeader SelectedDocumentHeader { get { return docHeaders[DocumentHeaderIndex]; } }
        public DocumentEditorPage()
        {
            InitializeComponent();
        }

        public void Closed()
        {
        }

        public void Created()
        {
            docHeaders = Editor.GetDocumentHeaders();
            docGridView.OnSelectDocument += SelectDoc;
        }

        public UIElement GetElement()
        {
            return this;
        }

        public void SetStyle(DesignManager design)
        {
        }

        public void Showed()
        {
            title1.Content = SelectedDocumentHeader.Title1;
            title2.Content = SelectedDocumentHeader.Title2;

            RefreshListDoc();
        }

        private void RefreshListDoc()
        {
            docGridView.Clear();

            int length = SelectedDocumentHeader.GetCount();
            for (int i = 0; i < length; i++)
            {
                Document doc = SelectedDocumentHeader.GetDocumentAt(i);

                docGridView.AddItem(doc, DocumentHeaderIndex);
            }
        }

        private void SelectDoc(int index, int cmd)
        {
            var doc = SelectedDocumentHeader.GetDocumentAt(index);

            if (docPage == null)
            {
                docPage = (DocumentPage)MainWindow.Current.GetWindow(MainWindow.Windows.DocumentPage);
            }


            if (cmd == 2)
            {
                if(MessageBox.Show($"Вы уверены, что хотите удалить {doc.Name}", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    SelectedDocumentHeader.DeleteDocument(doc.Name);
                    RefreshListDoc();
                }

                return;
            }

            docPage.document = doc;
            docPage.isReadonly = cmd == 0;
            MainWindow.Current.ShowWindow(MainWindow.Windows.DocumentPage);
        }

        private void TripleButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.ShowWindow(MainWindow.Windows.SelectObject);
        }

        private void _addDocument_Click(object sender, RoutedEventArgs e)
        {
            if (docEdit == null)
            {
                docEdit = new AddDocumentWindow();
            }

            if (docEdit.ShowWindow(SelectedDocumentHeader))
            {
                string t = docEdit.Text;
                SelectedDocumentHeader.AddDocument(t);
                RefreshListDoc();
            }
        }
    }
}
