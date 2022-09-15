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
using System.IO;
using ElectronicLib;
namespace Copybook
{
    /// <summary>
    /// Логика взаимодействия для DocumentEditorPage.xaml
    /// </summary>
    public partial class DocumentPage : UserControl, IWindow
    {

        const string DEF_STR = "Внимание! Пока запущена это приложение, Вы не сможете использовать функцию ОС \"Буфер обмена\".";
        const string DEF_STR_1 = "Программа обнаружила контент других документов, и была упразднена! Вы не сможете использовать Буфер обмена.";
        public class RtfC : RichTextBox
        {
            protected override void OnPreviewTextInput(TextCompositionEventArgs e)
            {
                base.OnPreviewTextInput(e);
            }
            protected override void OnTextChanged(TextChangedEventArgs e)
            {
                base.OnTextChanged(e);
            }
            protected override void OnContextMenuOpening(ContextMenuEventArgs e)
            {
                base.OnContextMenuOpening(e);
            }
            protected override void OnPreviewKeyDown(KeyEventArgs e)
            {
                if(e.SystemKey == Key.LeftCtrl || e.SystemKey == Key.RightCtrl)
                {
                    return;
                }
                base.OnKeyDown(e);
            }
        }

        private SolidColorBrush sr = new SolidColorBrush();
        private bool isSaved = false;
        public Document document;
        public bool isReadonly = false;
        public DocumentPage()
        {
            InitializeComponent();
        }

        private void SetStr(Color color, string val)
        {
            (_hintBar.Foreground = sr).SetValue(SolidColorBrush.ColorProperty, color);
            _hintBar.Content = val;
        }

        public void Closed()
        {
            
        }

        public void Created()
        {
            System.Collections.IEnumerator clipboartClear()
            {
                yield break;
                while (true)
                {

                    yield return null;
                }
            }
            AsyncOperation op = AsyncOperation.CreateAsync(clipboartClear());
            op.Start();


            docEditor.TextChanged += (o, e) =>
            {
                isSaved = false;
            };


            MainWindow.MessageHandled += (msg) =>
            {
                if (msg == 0x302)
                {
                    Clipboard.Clear();
                }

                if (MainWindow.Current.CurrentWindow == MainWindow.Windows.DocumentPage )
                {
                    if (!docEditor.IsReadOnly && (Clipboard.ContainsText() || Clipboard.ContainsText(TextDataFormat.Rtf) || Clipboard.ContainsText(TextDataFormat.CommaSeparatedValue)))
                    {
                        SetStr(Color.FromRgb(255, 0, 0), DEF_STR_1);
                        Clipboard.Clear();
                    }
                }
            };

            docEditor.AllowDrop = false;
            docEditor.IsManipulationEnabled = false;
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
            if (document == null)
                return;

            docEditor.IsReadOnly = isReadonly;
            SetStr(Color.FromRgb(255, 255, 255), docEditor.IsReadOnly ? "Документ доступен только для чтения." : DEF_STR);
            FlowDocument doc = docEditor.Document;
            DocToEmpty();

            if (!document.IsDataEmpty)
            {
                Stream s = ReadStream();
                docEditor.SelectAll();
                docEditor.Selection.Load(s, DataFormats.Rtf);
                s.Dispose();
            }
            title1.Content = document.Name;

            isSaved = true;
        }

        private void DocToEmpty()
        {
            docEditor.SelectAll();
            docEditor.Selection.Text = string.Empty;
        }

        private Stream ReadStream()
        {
            MemoryStream ms = new MemoryStream();
            byte[] b = document.Data;
            ms.Write(b, 0, b.Length);
            return ms;
        }

        private void SaveDoc()
        {
            if (!isSaved && !isReadonly)
            {
                MemoryStream ms = new MemoryStream();
                docEditor.SelectAll();
                docEditor.Selection.Save(ms, DataFormats.Rtf);
                byte[] b = ms.ToArray();
                ms.Dispose();
                document.Data = b;
            }
            isSaved = true;
        }

        private void TripleButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSaved && !isReadonly)
            {
                if (MessageBox.Show("Не сохраненные данные могут быть утерены. Вернуться?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    return;
                }
            }

            MainWindow.Current.ShowWindow(MainWindow.Windows.DocumentEditorPage);
        }

        private void TextToColor(Color color)
        {
            if (isReadonly)
                return;

            docEditor.Selection.ApplyPropertyValue(RichTextBox.ForegroundProperty, new SolidColorBrush(color));
        }

        private void saveDoc_Click(object sender, RoutedEventArgs e)
        {
            SaveDoc();
        }

        private void toBlack_Click(object sender, RoutedEventArgs e)
        {
            TextToColor(Color.FromArgb(255, 0, 0, 0));
        }

        private void toRed_Click(object sender, RoutedEventArgs e)
        {
            TextToColor(Color.FromArgb(255, 230, 0, 0));
        }

        PrintDialog pt = new PrintDialog();
        private void print_Click(object sender, RoutedEventArgs e)
        {
            //Выбор принтера
            bool? res = pt.ShowDialog();
            if (res.HasValue && res.Value)
                pt.PrintDocument(((IDocumentPaginatorSource)this.docEditor.Document).DocumentPaginator, "Печать документа");
        }

        Microsoft.Win32.SaveFileDialog sf = new Microsoft.Win32.SaveFileDialog() { Filter = DataFormats.Rtf + "(*.rtf)|*.rtf" };
        private void saveDocTo_Click(object sender, RoutedEventArgs e)
        {
            bool? res = sf.ShowDialog();
            if (res.HasValue && res.Value)
            {
                try
                {
                    TextRange tt = new TextRange(docEditor.Document.ContentStart, docEditor.Document.ContentEnd);
                    using (FileStream fs = File.Create(sf.FileName))
                    {
                        tt.Save(fs, DataFormats.Rtf);
                    }
                }
                catch
                {
                    MessageBox.Show("Ошибка сохранения. Попробуйте сохранить в другом каталоге.", "IO/Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                sf.FileName = "";
            }
        }
    }
}
