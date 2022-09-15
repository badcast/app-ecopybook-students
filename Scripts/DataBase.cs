#define PACKRELOAD_

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Farbox.Packer;
using System.IO;

namespace ElectronicLib
{
    public static class DataBase
    {
        private const string DATA_BASE_DIR = "db";
        /// <summary>
        /// Полный путь к папке программы
        /// </summary>
        public static string AppDir { get { return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); } }
        /// <summary>
        /// Полный путь к папке Базы данных
        /// </summary>
        public static string DataBaseDirFullPath { get { return AppDir + "/" + DataBaseDir; } }
        /// <summary>
        /// Абсолютный путь к папке базы данных
        /// </summary>
        public static string DataBaseDir { get { return DATA_BASE_DIR; } }
        /// <summary>
        /// Каталочный класс объекта
        /// </summary>
        public static Packer Packer { get; private set; }
        public static void DataBaseInitialize()
        {
            if (Packer != null)
                return;

            string rtDir = AppDir;


            Packer = new Packer();
            string file = rtDir + "/data.db";
#if PACKRELOAD
            Packer.PackInfo.Compact(DataBaseDirFullPath);
            Packer.PackSave(file);
#else
            Packer.PackLoad(file);
#endif

        }

        public static PackFile GetFile(string path)
        {
            string isDir = DataBaseDirFullPath.ToLower();
            if (!path.ToLower().StartsWith(isDir))
                throw new FileNotFoundException("File not found " + path);

            path = path.Remove(0, isDir.Length);

            string appDir = AppDir;
            return Packer.PackInfo.GetFileFromPath(path); 
        }
    }
}
