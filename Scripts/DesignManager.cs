using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ElectronicLib
{
    public class TripleButton
    {
        public Size Size;
        public BitmapSource[] triples;
        public BitmapSource Normal { get { return triples[0]; } }
        public BitmapSource Hover { get { return triples[1]; } }
        public BitmapSource Pressed { get { return triples[2]; } }

    }

    public class DesignManager
    {
        private static List<IStyle> styles;
        public static void AddStyle(IStyle style)
        {
            if (styles == null)
                styles = new List<IStyle>();
            styles.Add(style);
        }


        public static void RefreshStyle(DesignManager design)
        {
            if (styles == null)
                return;

            foreach (var item in styles)
            {
                item.SetStyle(design);
            }
        }

        public static DesignManager current { get; set; }
        Dictionary<string, BitmapSource> dictionary;
        Dictionary<BitmapSource, ImageBrush> bitmapCache;
        Dictionary<string, BitmapImage> bitmapStream;
        Dictionary<string, TripleButton> tripleButtons;

        public string Name { get; set; }
        public string Author { get; set; }
        public Color borderColor { get; set; }

        public DesignManager(string designFile)
        {
            current = this;
            dictionary = new Dictionary<string, BitmapSource>();
            bitmapCache = new Dictionary<BitmapSource, ImageBrush>();
            LoadXmlData(designFile);
        }

        private void LoadXmlData(string designFile)
        {
            Int32Rect getRect(string rectValue)
            {
                rectValue = rectValue.Trim();
                string[] vls = rectValue.Split(',');

                return new Int32Rect(
                            int.Parse(vls[0]),
                            int.Parse(vls[1]),
                            int.Parse(vls[2]),
                            int.Parse(vls[3]));
            }
            BitmapImage getBitmap(string absolutePath)
            {
                if (bitmapStream == null)
                    bitmapStream = new Dictionary<string, BitmapImage>();


                //string p = Path.GetTempFileName();

                //File.WriteAllBytes(p, (f.stream as MemoryStream).ToArray());

                BitmapImage img = null;
                absolutePath = absolutePath.ToLower();
                if (!bitmapStream.ContainsKey(absolutePath))
                {
                    string streamPath = DataBase.DataBaseDirFullPath + "\\" + absolutePath;
                    var f = DataBase.GetFile(streamPath);

                    img = new BitmapImage();

                    img.BeginInit();
                    img.StreamSource = f.stream;
                    img.EndInit();

                    bitmapStream.Add(absolutePath, img);
                }
                else
                {
                    img = bitmapStream[absolutePath];
                }
                return img; /*new BitmapImage(new Uri("file://" + p));*/
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(designFile);

            XmlElement root = doc["design"];

            XmlElement source = root["source"];
            string path = Path.GetDirectoryName(designFile);
            BitmapImage map = getBitmap(source.GetAttribute("src"));
            map.CacheOption = BitmapCacheOption.OnLoad;
            XmlElement info = root["info"];
            this.Name = info.GetAttribute("name");
            this.Author = info.GetAttribute("author");

            XmlElement rects = root["rects"];
            if (rects != null)
                for (int i = 0; i < rects.ChildNodes.Count; i++)
                {
                    XmlNode node = rects.ChildNodes[i];

                    if (node is XmlComment)
                        continue;

                    XmlElement r_data = (XmlElement)node;
                    string item_id = r_data.Name;

                    Int32Rect item_rect = Int32Rect.Empty;

                    BitmapImage dest = map;
                    bool mapLoaded = false;
                    if (r_data.HasAttribute("src"))
                    {
                        dest = getBitmap(r_data.GetAttribute("src"));
                        mapLoaded = true;
                    }

                    if (r_data.HasAttribute("rect"))
                    {
                        //Преобразование
                        item_rect = getRect(r_data.GetAttribute("rect"));
                    }
                    else
                    {
                        item_rect = new Int32Rect(0, 0, dest.PixelWidth, dest.PixelHeight);
                    }


                    CroppedBitmap cropped = new CroppedBitmap(dest, item_rect);
                    BitmapSource bmp = cropped;

                    dictionary.Add(item_id, bmp);

                    if (mapLoaded)
                    {
                        //dest.Relo
                    }
                }

            XmlElement tripElem = root["TripleButtons"];
            if (tripElem != null)
            {
                tripleButtons = new Dictionary<string, TripleButton>();
                for (int i = 0; i < tripElem.ChildNodes.Count; i++)
                {
                    XmlElement tt = (XmlElement) tripElem.ChildNodes[i];

                    string key = tt.GetAttribute("key");
                    Int32Rect startRect = getRect(tt.GetAttribute("rect"));
                    string direction = tt.GetAttribute("direction").ToLower();
                    //Default
                    if(string.IsNullOrEmpty(direction))
                        direction = "Right";

                    int x = 0;
                    int y = 0;

                    if(direction == "Right".ToLower())
                    {
                        x = 1;
                    }
                    else if(direction == "Left".ToLower())
                    {
                        x = -1;
                    }
                    else if(direction == "Up".ToLower())
                    {
                        y = -1;
                    }
                    else if(direction == "Down".ToLower())
                    {
                        y = 1;
                    }

                    BitmapSource[] tbmp = new BitmapSource[3];
                    var tb = new TripleButton() { Size = new Size(startRect.Width, startRect.Height), triples = tbmp };
                    int w = (int)tb.Size.Width;
                    int h = (int)tb.Size.Height;
                    for (int f = 0; f < 3; f++)
                    {
                        tbmp[f] = new CroppedBitmap(map, new Int32Rect(startRect.X + (x * f * w), startRect.Y + (y * f * h), w, h));
                    }

                    tripleButtons.Add(key, tb);
                }
            }
            XmlElement form = root["form"];
            string[] f_rgb = form.GetAttribute("borderColorRGB").Trim().Split(',');
            this.borderColor = Color.FromArgb(255, byte.Parse(f_rgb[0]), byte.Parse(f_rgb[1]), byte.Parse(f_rgb[2]));

            //  map.Dispose();
        }

        public bool HasId(string id)
        {
            return dictionary.ContainsKey(id);
        }

        public ImageBrush GetImageBrushFromId(string id)
        {
            BitmapSource src = GetBitmapSourceFromId(id);
            if (src == null)
                return null;

            ImageBrush _brsh;
            if (bitmapCache.TryGetValue(src, out _brsh))
            {
                return _brsh;
            }

            bitmapCache.Add(src, _brsh = new ImageBrush(src));

            return (ImageBrush)_brsh;

        }
        public TripleButton GetTripleButton(string id)
        {
            if(tripleButtons.TryGetValue(id, out TripleButton t))
            {
                return t;
            }

            return null;
        }

        public void RefreshStyle()
        {
            RefreshStyle(this);
        }

        public BitmapSource GetBitmapSourceFromId(string id)
        {
            if (!HasId(id))
                return null;
            return dictionary[id];
        }
    }
}
