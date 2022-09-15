using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace ElectronicLib
{
    public delegate void StreamEvent(byte currentByte);
    public delegate void StreamBlockEvent(byte[] buffered);
    public delegate void StreamProgressEvent(double progress);

    /// <summary>
    /// Представляет собой класс Потока управлением, которое занимает асинхронное чтение и запись
    /// </summary>
    public class AsyncStream : Stream, IDisposable
    {
        private Stream data;

        public override bool CanRead => data.CanRead;

        public override bool CanSeek => data.CanSeek;

        public override bool CanWrite => data.CanWrite;

        public override long Length => data.Length;

        public override long Position { get => data.Position; set => data.Position = value; }

        public event StreamEvent OnStreamChanged;
        public event StreamEvent OnStreamRead;
        public event StreamBlockEvent OnBlockRead;
        public event StreamProgressEvent OnProgress;

        public AsyncStream()
        {
            data = new MemoryStream();
        }

        public AsyncStream(Stream stream)
        {
            data = stream;
        }

        public override void Flush()
        {
            data.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int res = data.Read(buffer, offset, count);
            OnBlockRead?.Invoke(buffer);
            OnProgress?.Invoke(1D * Position / Length);
            return res;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return data.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            data.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Position = offset;
            for (; offset < count; offset++)
            {
                byte b = buffer[offset];
                data.WriteByte(b);
                OnStreamChanged?.Invoke(b);
            }
        }

        public override void WriteByte(byte value)
        {
            data.WriteByte(value);
        }

        public override int ReadByte()
        {
            int b = data.ReadByte();

            if (b != -1)
                OnStreamRead?.Invoke((byte)b);

            return b;
        }

        public override void Close()
        {
            data.Close();
        }

        public new void Dispose()
        {
            data.Dispose();
        }
    }

    public static class G_G
    {

        public static bool destroy(ref string target)
        {
            object t = target;
            target = null;
            bool des = destroy(ref t);
            target = t as string;
            return des;
        }
        public static bool destroy(ref object target, GCCollectionMode mode = GCCollectionMode.Forced)
        {
            if (target == null)
                return false;

            WeakReference wef = new WeakReference(target);
            target = null;

            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.Batch;

            GC.Collect(GC.GetGeneration(wef), mode);
            System.GC.WaitForPendingFinalizers();
            if (wef.Target != null)
                GC.Collect(GC.GetGeneration(wef), mode);
            System.GC.WaitForPendingFinalizers();

            return (target = wef.Target) == null;
        }

    }
}
