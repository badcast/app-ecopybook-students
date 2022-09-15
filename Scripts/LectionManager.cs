
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Farbox.Packer;

namespace ElectronicLib
{
    public interface ISpecialValue
    {
        int SpecialValue { get; set; }
    }
    public enum SpecialitiesType
    {
        DefaultCollege = -1,
        AsanbaiCollege = 0,
        AsanbaiCollegeCourses = 1
    }
    public class AllSpecialities : ISpecialValue
    {
        public int SpecialValue { get; set; }
        public string Name { get; }
        public string Description { get; }

        public Specialities[] Specialities { get; }

        public AllSpecialities(string name, string description, Specialities[] specialities)
        {
            this.Name = name;
            this.Description = description;
            this.Specialities = specialities;
        }
    }
    public class Specialities : ISpecialValue
    {
        public int SpecialValue { get; set; }

        private Course[] m_courses;

        public string Name { get; }
        public string Decription { get; }

        public int CourseLength { get => m_courses.Length; }
        public Course[] Courses => m_courses;

        public Specialities(string name, string description, Course[] courses)
        {
            this.Name = name;
            this.Decription = description;
            this.m_courses = courses;
        }
    }
    public class Course : ISpecialValue
    {
        public int SpecialValue { get; set; }
        public string Name { get; }
        public Predmets[] Predmets { get; }

        public Course(string Name, Predmets[] predmets)
        {
            this.Name = Name;
            this.Predmets = predmets;
        }
    }
    public class Predmets : ISpecialValue
    {
        public int SpecialValue { get; set; }
        public string Name { get; }

        public string Description { get; }

        public Lection[] Lection { get; }

        public Predmets(string name, string description, Lection[] lections)
        {
            this.Name = name;
            this.Description = description;
            this.Lection = lections;
        }
    }
    public class Lection
    {
        const string TP_LECTION = "LECT";
        const string TP_PRACTTICE = "PRACT";

        public enum LectionType
        {
            Lection,
            Practice
        }

        public static LectionType GetLectionType(string tp)
        {
            if (tp.Length < 1)
            {
                return LectionType.Lection;
            }

            if (char.ToUpper(tp[0]) == char.ToUpper(TP_LECTION[0]))
                return LectionType.Lection;
            else return LectionType.Practice;

        }

        public string Name { get; }

        public string Description { get; }

        public LectionType Type { get; }

        public string path { get; }

        public Lection(string name, string description, LectionType lectionType, string fileName)
        {
            this.Name = name;
            this.Description = description;
            this.Type = lectionType;
            this.path = fileName;
        }
    }
    public class LectionManager
    {
        private static AllSpecialities[] all_specialities;
        private static SpecialitiesType defaultType;

        public static Packer packer { get; private set; }

        public static void LectionInitialize()
        {

            string rtDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/";

            packer = DataBase.Packer;

            XmlDocument doc = new XmlDocument();
            doc.Load(rtDir + "Lection.xml");

            string getDir(XmlElement elem)
            {
                return elem.GetAttribute("dir");
            }


            //Lection xml
            XmlElement root = doc["Lection"];
            int spectypelength = root.ChildNodes.Count;
            all_specialities = new AllSpecialities[spectypelength];
            for (int i = 0; i < spectypelength; i++)
            {
                XmlElement spec = (XmlElement)root.ChildNodes[i];
                int specialitiesLength = spec.ChildNodes.Count;
                Specialities[] specialities = new Specialities[specialitiesLength];
                for (int p = 0; p < specialitiesLength; p++)
                {
                    XmlElement s = (XmlElement)spec.ChildNodes[p];
                    int courseLength = s.ChildNodes.Count;
                    Course[] courses = new Course[courseLength];

                    string path_s  = getDir(s);

                    for (int c = 0; c < courseLength; c++)
                    {
                        XmlElement course = (XmlElement)s.ChildNodes[c];
                        int predmetsCount = course.ChildNodes.Count;
                        Predmets[] predmets = new Predmets[predmetsCount];
                        string path_cource = getDir(course);
                        for (int j = 0; j < predmetsCount; j++)
                        {
                            XmlElement pred = (XmlElement)course.ChildNodes[j];
                            int lectionLength = pred.ChildNodes.Count;
                            Lection[] lections = new Lection[lectionLength];

                            string path_predmet = getDir(pred);

                            for (int l = 0; l < lectionLength; l++)
                            {
                                XmlElement lect = (XmlElement)pred.ChildNodes[l];

                                string lTp = lect.GetAttribute("type").Trim();

                                string filePath = string.Format(@"{0}\{1}\{2}\{3}", path_s, path_cource, path_predmet, lect.GetAttribute("filename"));

                                lections[l] = new Lection(lect.GetAttribute("name"), lect.GetAttribute("description"), Lection.GetLectionType(lTp), filePath);
                            }
                            predmets[j] = new Predmets(pred.GetAttribute("name"), pred.GetAttribute("description"), lections);
                        }
                        courses[c] = new Course(course.GetAttribute("name"), predmets);
                    }

                    specialities[p] = new Specialities(s.GetAttribute("name"), s.GetAttribute("description"), courses);
                }

                all_specialities[i] = new AllSpecialities(spec.GetAttribute("name"), spec.GetAttribute("description"), specialities);
            }
        }

        public static AllSpecialities GetSpecialities(SpecialitiesType type = SpecialitiesType.DefaultCollege)
        {
            if (type == SpecialitiesType.DefaultCollege && defaultType != SpecialitiesType.DefaultCollege)
            {
                type = defaultType;
            }

            defaultType = type;

            if (type == SpecialitiesType.DefaultCollege)
                throw new Exception("Не правльный выбор");

            return all_specialities[(int)type];
        }
        public static Specialities GetSpecialValue_Specialities(SpecialitiesType specialitiesType = SpecialitiesType.DefaultCollege)
        {
            AllSpecialities root = GetSpecialities(specialitiesType);
            return root.Specialities[root.SpecialValue];
        }
        public static Course GetSpecialValue_Cource(SpecialitiesType specialitiesType = SpecialitiesType.DefaultCollege)
        {

            Specialities spec = GetSpecialValue_Specialities(specialitiesType);
            return spec.Courses[spec.SpecialValue];
        }
        public static Predmets GetSpecialValue_Predmets(SpecialitiesType specialitiesType = SpecialitiesType.DefaultCollege)
        {
            Course crs = GetSpecialValue_Cource(specialitiesType);
            return crs.Predmets[crs.SpecialValue];
        }
        public static Lection GetSpecialValue_Lection(SpecialitiesType specialitiesType = SpecialitiesType.DefaultCollege)
        {
            Predmets p = GetSpecialValue_Predmets(specialitiesType);

            return p.Lection[p.SpecialValue];
        }

        public static Stream GetSpecialValue_Stream(SpecialitiesType specialitiesType = SpecialitiesType.DefaultCollege)
        {
            return GetLectionFromStream(GetSpecialValue_Lection(specialitiesType));
        }

        public static Stream GetLectionFromStream(Lection lection)
        {
            return packer.PackInfo.GetFileFromPath(lection.path)?.stream;
        }

        public static PackFile GetFile(Lection lection)
        {
            return packer.PackInfo.GetFileFromPath(lection.path);
        }
    }
}
