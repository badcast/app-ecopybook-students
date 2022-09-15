using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ElectronicLib;
using System.Security.Cryptography;

namespace Copybook
{
    public interface IDocument
    {
        void ReleaseDoc(Editor editor);
        DocumentRange GetDocumentRange();

    }
    public struct DocumentRange : IDisposable
    {
        bool _disposed;

        private int point;

        public int Point { get { if (_disposed) throw new ObjectDisposedException("DocumentRange"); return point; } }

        public DocumentRange(int point)
        {
            _disposed = false;
            this.point = point;
        }

        public string GetName()
        {
            if (_disposed)
                throw new NullReferenceException();
            return null;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("DocumentRange");
            }
            _disposed = true;
            point = -1;
        }
    }
    public class Document : IDocument
    {
        DocumentRange range;

        public string Name { get; set; }
        public byte[] Data { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsDataEmpty { get { return Data == null || Data.Length == 0; } }
        public Document(DocumentRange range)
        {
            this.range = range;
        }

        void IDocument.ReleaseDoc(Editor editor)
        {
            range.Dispose();
        }

        DocumentRange IDocument.GetDocumentRange()
        {
            return range;
        }
    }
    public class DocumentHeader
    {
        private string headerName, title1, title2;
        private List<Document> _documents;

        public string HeaderName { get => headerName; }
        public string Title1 { get => title1; }
        public string Title2 { get => title2; }

        public DocumentHeader(string headerName, string title, string title2)
        {
            this.headerName = headerName;
            this.title1 = title;
            this.title2 = title2;
            this._documents = new List<Document>();
        }

        private bool TryGetDocument(string DocumentName, out Document document)
        {
            document = null;
            foreach (var item in _documents)
            {
                if (item.Name == DocumentName)
                {
                    document = item;
                    return true;
                }
            }

            return false;
        }
        public bool HasDocument(string DocumentName)
        {
            return TryGetDocument(DocumentName, out Document doc);
        }
        public Document GetDocument(string DocumentName)
        {
            if (TryGetDocument(DocumentName, out Document doc))
            {
                return doc;
            }

            return null;
        }

        public int IndexOf(Document document)
        {
            return _documents.IndexOf(document);
        }

        public Document GetDocumentAt(int index)
        {
            return _documents[index];
        }
        public int GetCount()
        {
            return _documents.Count;
        }
        public Document AddDocument(string DocumentName)
        {
            if (HasDocument(DocumentName))
            {
                throw new Exception("Data exists");
            }

            Document doc = new Document(new DocumentRange(0));
            doc.Name = DocumentName;
            doc.DateTime = DateTime.Now;
            doc.Data = null;

            _documents.Add(doc);

            return doc;
        }
        public bool DeleteDocument(string DocumentName)
        {
            for (int i = 0; i < GetCount(); i++)
            {
                var item = _documents[i];
                if (item.Name == DocumentName)
                {
                    _documents.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
        public bool SaveDocument(string Document)
        {
            return false;
        }
    }
    public class Editor
    {
        const string FILENAME = "Docs.dat";
        public static byte[] _keyStream = { 24, 13, 233, 121, 10, 10, 20, 30, 40, 50, 60, 70, 3, 6, 10, 13, 10, 20, 50, 60, 70, 80, 90, 10, 20, 33, 25, 6, 7, 8, 9, 10 };
        public static byte[] _iv = { 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255 };
        private string FileName { get { return DataBase.AppDir + "\\" + FILENAME; } }
        private string BackupFolder { get { return DataBase.AppDir + "\\Backup"; } }
        private string dbUserName;
        List<DocumentHeader> documentHeaders;
        private bool isSupportedData;
        public string UserName { get => dbUserName; }
        /// <summary>
        /// Определяет, поддерживается ли загруженная база данных 
        /// </summary>
        public bool IsSupportedData { get => isSupportedData; }
        public Editor()
        {
            documentHeaders = new List<DocumentHeader>();
            XmlDocument doc = new XmlDocument();
            doc.Load(DataBase.AppDir + "\\lections.xml");
            XmlElement root = doc["headers"];

            if (root != null)
            {
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    XmlElement c = (XmlElement)root.ChildNodes[i];

                    documentHeaders.Add(new DocumentHeader(c.GetAttribute("name"), c.GetAttribute("title"), c.GetAttribute("title2")));

                }

            }

            _current = this;
        }

        public DocumentHeader GetHeader(string HeaderId)
        {
            foreach (var item in documentHeaders)
            {
                if (item.HeaderName == HeaderId)
                {
                    return item;
                }
            }
            return null;
        }

        public DocumentHeader[] GetHeaders()
        {
            return documentHeaders.ToArray();
        }

        public void Load()
        {
            isSupportedData = true;
            if (!File.Exists(FileName))
                return;

            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(FileName)))
                {
                    //Чтение MD5 16 байтовый массив для надежности данных
                    byte[] _md5DataTest = br.ReadBytes(16);
                    long savePos = br.BaseStream.Position;

                    //Сравнение MD5 данных
                    MD5 m = MD5.Create();

                    byte[] data = new byte[br.BaseStream.Length-16];
                    byte[] md5Result = m.ComputeHash(data);

                    bool resultMd5Test = true;
                    //Проверка MD5
                    for (int i = 0; i < 16 && resultMd5Test; i++)
                    {
                        byte a = _md5DataTest[i];
                        byte b = md5Result[i];
                        resultMd5Test = a == b;
                    }
                    m.Dispose();

                    data = null;
                    _md5DataTest = null;
                    md5Result = null;

                    //Проверка условий
                    if(!resultMd5Test)
                    {
                        throw new IOException("Данные были повреждены. Проверено MD5");
                    }

                    //Если файл прошел проверку, то может продолжить свою работу
                    br.BaseStream.Position = savePos;

                    //Чтение имени 
                    int len = br.ReadInt32();
                    byte[] usrNameData = new byte[len];
                    br.Read(usrNameData, 0, len);
                    //Алгоритм расшифровки
                    using (MemoryStream ms = new MemoryStream(usrNameData))
                    {
                        using (Rijndael r = Rijndael.Create())
                        {
                            using (ICryptoTransform crpt = r.CreateDecryptor(_keyStream, _iv))
                            {
                                using (CryptoStream CS = new CryptoStream(ms, crpt, CryptoStreamMode.Read))
                                {
                                    List<byte> dec = new List<byte>();
                                    int bRead = 0;
                                    do
                                    {
                                        bRead = CS.ReadByte();
                                        if (bRead != -1)
                                            dec.Add((byte)bRead);
                                    } while (bRead != -1);
                                    dbUserName = Encoding.UTF8.GetString(dec.ToArray());
                                }
                            }
                        }
                    }

                    //->>read
                    int count = br.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        string _hName = br.ReadString();
                        DocumentHeader docH = GetHeader(_hName);
                        if (docH == null)
                        {
                            docH = new DocumentHeader(_hName, "Восстановлено", "Пусто");
                            documentHeaders.Add(docH);
                        }
                        int cnt = br.ReadInt32();
                        for (int j = 0; j < cnt; j++)
                        {
                            string dcName = br.ReadString();
                            DateTime dcDate = new DateTime(br.ReadInt64());
                            int dtLength = br.ReadInt32();
                            byte[] b = br.ReadBytes(dtLength);

                            var doc = docH.AddDocument(dcName);
                            doc.DateTime = dcDate;
                            doc.Data = b;
                        }

                    }

                }
            }
            catch
            {
                isSupportedData = false;
            }
        }
        public void Save(string UserName)
        {
            //Если условия удовлетворяет истине, то происходит резервировния данных
            if (File.Exists(FileName))
            {
                string backupFile = "";
                Directory.CreateDirectory(BackupFolder);
                int iterate = 0;
                do
                {
                    backupFile = BackupFolder + "\\" + Path.GetFileName(FileName) + ".backup" + iterate;
                    iterate++;
                } while (File.Exists(backupFile));
                File.Move(FileName, backupFile);
            }

            try
            {

                using (BinaryWriter bw = new BinaryWriter(File.Create(FileName)))
                {
                    //Занимаеи 16 байт для MD5
                    bw.Write(new byte[16]);


                    byte[] dt = Encoding.UTF8.GetBytes(dbUserName = UserName);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (Rijndael r = Rijndael.Create())
                        {
                            using (ICryptoTransform crpt = r.CreateEncryptor(_keyStream, _iv))
                            {
                                using (CryptoStream cr = new CryptoStream(ms, crpt, CryptoStreamMode.Write))
                                {
                                    cr.Write(dt, 0, dt.Length);
                                }
                            }
                        }
                        dt = ms.ToArray();
                        bw.Write(dt.Length);
                        bw.Write(dt, 0, dt.Length);
                    }

                    //---->
                    bw.Write(documentHeaders.Count);
                    for (int i = 0; i < documentHeaders.Count; i++)
                    {
                        var h = documentHeaders[i];
                        bw.Write(h.HeaderName);
                        bw.Write(h.GetCount());
                        for (int d = 0; d < h.GetCount(); d++)
                        {
                            var doc = h.GetDocumentAt(d);
                            bw.Write(doc.Name);
                            bw.Write(doc.DateTime.Ticks);
                            int length = doc.Data != null ? doc.Data.Length : 0;
                            bw.Write(length);
                            for (int j = 0; j < length; j++)
                            {
                                bw.Write(doc.Data[j]);
                            }
                        }
                    }


                    //MD5 для проверки надежности 
                    MD5 md = MD5.Create();

                    byte[] data = new byte[bw.BaseStream.Length-16];
                    bw.BaseStream.Read(data, 16, data.Length-16);

                    byte[] md5Wr = md.ComputeHash(data);
                    bw.BaseStream.Seek(0, SeekOrigin.Begin);
                    bw.BaseStream.Write(md5Wr, 0, md5Wr.Length);

                    md.Dispose();

                    md5Wr = null;
                    data = null;
                }

                GC.Collect();

            }
            catch
            {

            }
        }

        private static Editor _current;
        public static Editor Current { get; }

        public static DocumentHeader[] GetDocumentHeaders()
        {
            return _current.GetHeaders();
        }
    }
}
