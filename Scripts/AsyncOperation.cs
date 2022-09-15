using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace ElectronicLib
{
    public class AsyncOperation : IDisposable
    {
        public class Retrace
        {
            public bool IsBreak { get; set; }
        }

        private static int _ids;
        public static AsyncOperation CreateAsync(IEnumerator enumerator)
        {
            var s = new AsyncOperation(enumerator);
            return s;
        }

        public static AsyncOperation CreateAsync(Action<Retrace> enter)
        {
            IEnumerator rator()
            {
                Retrace trace = new Retrace();
                trace.IsBreak = false;
                while (!trace.IsBreak)
                {
                    enter(trace);
                    yield return 0;
                }
            }
            return CreateAsync(rator());

        }

        private IEnumerator rator;
        private bool _isAsync;
        private int __id;
        public Dispatcher Dispatcher { get; }
        public IEnumerator Enumerator { get { if (IsDisposed) throw new ObjectDisposedException("AsyncOperation"); return rator; } }
        /// <summary>
        /// Состояние асинхронности
        /// </summary>
        public bool IsAsync { get { if (IsDisposed) throw new ObjectDisposedException("AsyncOperation"); return _isAsync; } }
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int id { get { if (IsDisposed) throw new ObjectDisposedException("AsyncOperation"); return __id; } }
        public bool IsDisposed { get; private set; }
        public AsyncOperation(IEnumerator enumerator)
        {
            if ((this.rator = enumerator) == null)
            {
                throw new ArgumentNullException();
            }
            this.Dispatcher = Dispatcher.CurrentDispatcher;
            this.__id = _ids++;
        }

        private void _AsyncHandle()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("AsyncOperation");

            _isAsync = true;
            void begin()
            {
                Dispatcher.BeginInvoke((Action)_async, DispatcherPriority.Background);
            }
            void _async()
            {
                if (!IsDisposed && IsAsync)
                {
                    _isAsync = this.Enumerator.MoveNext();

                    if (IsDisposed)
                        return;

                    if (IsAsync)
                        begin();
                }

            };

            begin();
        }

        public void Start()
        {
            if (IsAsync)
                throw new InvalidOperationException("Процесс уже выполняется. Вызовите метод Stop()");
            _AsyncHandle();
        }

        public void Stop()
        {
            _isAsync = false;
        }


        public void Dispose()
        {
            if (IsDisposed)
                return;

            Stop();
            __id = -1;
            rator = null;
            IsDisposed = true;

        }
    }
}
