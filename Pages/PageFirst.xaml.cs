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
    public partial class PageFirst : UserControl, IWindow
    {
        AsyncOperation async;
        public bool IsPlayAnimation { get => MainWindow.Current.CurrentWindow == MainWindow.Windows.Main; }

        public PageFirst()
        {
            InitializeComponent();
        }

        System.Collections.IEnumerator anim()
        {
            DateTime curTime = DateTime.Now;
            double ttEveryTime = 0.8;
            this.usrName.Content = "Добро пожаловать!";//MainWindow.Current.UserName;
            int tp = -1;
            bool isTpEnded = true;
            bool isHalt = false;
            while(tp != 2)
            {
                if (!IsPlayAnimation)
                    yield break;

                DateTime startTime = DateTime.Now;

                if (isTpEnded)
                {
                    if(isHalt )
                    {
                        yield break;
                    }
                    isTpEnded = false;
                    curTime = DateTime.Now.AddSeconds(ttEveryTime);
                    tp++;
                    if (tp == 2)
                    {
                        this.usrName.Content = MainWindow.Current.UserName;
                        isHalt = true;
                        tp = 0;
                    }

                }

                double percent = 1- (curTime - startTime).TotalSeconds / ttEveryTime;

                bool isEnded = percent >= 1;

                switch (tp)
                {
                    //Показывает изначальный текст и скрывает его
                    case 0:

                        if(isEnded)
                        {
                            isTpEnded = true;
                            break;
                        }

                        usrName.Opacity = percent;

                        break;
                    case 1:
                        if (isEnded)
                        {
                            isTpEnded = true;
                            break;
                        }

                        usrName.Opacity = 1-percent;
                        break;
                }

                yield return 0;
            }

        }

        private void PlayAuthorAnimation()
        {
            if (!IsPlayAnimation)
                return;

            if (async != null && async.IsAsync)
                return;

            usrName.Opacity = 0;

            async = AsyncOperation.CreateAsync(anim());
            async.Start();


        }


        public void Closed()
        {
        }

        public void Created()
        {
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
            PlayAuthorAnimation();
        }

        private void TripleButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.ShowWindow(MainWindow.Windows.SelectObject);
        }
    }
}
