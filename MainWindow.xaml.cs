using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ElectronicLib;
using System.IO;

namespace Copybook
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string haltFile = "halt.exe";
        public Editor editor;

        public enum Windows
        {
            Main,
            SelectObject,
            DocumentEditorPage,
            DocumentPage,
        }
        private IDictionary<Windows, IWindow> windows;
        private Windows _currentWindow = Windows.Main;
        private bool isWindowShowed;
        private int soundIndex = 0;
        private int[] songs;
        private string _hltCode = "_______apphalt";
        private string curDir { get => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); }
        private string haltFileUSER { get => curDir + "\\" + haltFile; }

        public bool IsHaltApplication { get; set; }
        public bool IsRequiredSecurity { get => editor != null && editor.UserName == UserName; }
        public string UserName { get => Properties.Resources.UserName; }
        public Windows CurrentWindow { get => _currentWindow; }

        public MainWindow()
        {
            current = this;

            ReadHalt();


            InitializeComponent();
        }

        private void PlaySongNext()
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            reload:
            ExecuteHalt();
            IsHaltApplication = false;
            DataBase.DataBaseInitialize();
            DesignManager des = new DesignManager("design.xml");

            this.editor = new Editor();
            this.editor.Load();

            if (!this.editor.IsSupportedData && this.editor.UserName != UserName)
            {
                MessageBox.Show("Программа обнаружела не совместимость данных. Нынешная версия не совпадает с инным.", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                IsHaltApplication = true;
                WriteHalt();
                goto reload;
            }
            windows = new Dictionary<Windows, IWindow>() {
                {  Windows.Main, new PageFirst() },
                { Windows.SelectObject, new SelectObject() },
                { Windows.DocumentEditorPage, new DocumentEditorPage()},
                { Windows.DocumentPage, new DocumentPage()}
            };

            foreach (var w in windows.Values)
            {
                w.Created();
            }


            //   songs = new int[5];
            void soundInit(int index, string pName)
            {
                var p = DataBase.GetFile(DataBase.DataBaseDirFullPath + "/music/" + pName);
            }

            /*

            soundInit(0, "music01.mp3");
            soundInit(1, "music02.mp3");
            soundInit(2, "music03.mp3");
            soundInit(3, "music04.mp3");
            soundInit(4, "music05.mp3");
            */


            DesignManager.RefreshStyle(des);
            //Audio.AudioPlayer A = new Audio.AudioPlayer();
            //var b = new Audio.Wave.WasapiOut();
            //Audio.Wave.SampleProviders.SampleChannel samp = new Audio.Wave.SampleProviders.SampleChannel(new Audio.Wave.AudioFileReader(@"D:\Development\VisualDevelopment\Windows\Copybook\Copybook\bin\x86\Debug\db\music\music05.mp3"), true);
            //b.Play();
            ShowWindow(Windows.Main);

            System.Windows.Interop.HwndSource hwnd = PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource;
            hwnd.AddHook(WndProc);
        }


        private void appClose_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
            
        }

        private void appFull_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState != WindowState.Maximized ? WindowState.Maximized : WindowState.Normal;
        }

        private void appHide_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        public IWindow ShowWindow(Windows window)
        {
            if (isWindowShowed && window == _currentWindow)
                return null;

            isWindowShowed = true;
            PageContent.Children.Clear();
            IWindow lastWindow = windows[_currentWindow];
            lastWindow.Closed();
            IWindow w = windows[window];
            _currentWindow = window;
            PageContent.Children.Add(w.GetElement());
            w.Showed();

            return w;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            //Если требование не подтверждены, то 
            if(!IsRequiredSecurity)
            {

            }
            WriteHalt();
            editor.Save(UserName);
            Environment.Exit(0);
        }

        public IWindow GetWindow(Windows window)
        {
            return windows[window];
        }

        private void ReadHalt()
        {
            var reg = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("CopybookTarget");
            void dispose()
            {
                reg.Dispose();
            }
            object obj = reg.GetValue(_hltCode);
            if(obj != null && (int)obj == 1)
            {
                IsHaltApplication = true;
                dispose();
                return;
            }
            if (!File.Exists(haltFileUSER))
                IsHaltApplication = true;
            else
            {
                FileStream fs = File.OpenRead(haltFile);
                IsHaltApplication = fs.ReadByte() != 0;
                fs.Dispose();
            }
            reg.SetValue(_hltCode, IsHaltApplication, Microsoft.Win32.RegistryValueKind.DWord);
            dispose();
        }

        private void WriteHalt()
        {
            var f = File.Create(haltFileUSER);
            try
            {
                byte[] b = BitConverter.GetBytes(IsHaltApplication);
                f.Write(b, 0, b.Length);
            }
            finally
            {
                f.Dispose();
            }
        }

        private void ExecuteHalt()
        {
            if (IsHaltApplication)
            {
                MessageBox.Show("Вы не соблюдали правила сохранения и ответственность на защиту данных.\n\nДоп. информация:\nВнимание. Вы попытались взломать программу. От имени разработчика, я вынужден блокировать приложение.\n\nКод ошибки -1", "Нарушение безопасности", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(-1);
                return;
            }
        }

        static MainWindow current;
        public static MainWindow Current { get => current; }
        public static event Action<int> MessageHandled;
        public static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            MessageHandled?.Invoke(msg);

            return IntPtr.Zero;
        }

    }
}
