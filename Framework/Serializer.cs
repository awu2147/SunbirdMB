using System;
using System.Xml.Serialization;
using System.IO;

namespace SunbirdMB.Framework
{
    public static class Serializer
    {
        public static Type[] ExtraTypes { get; set; } = new Type[] { };

        public static T ReadXML<T>(XmlSerializer deserializer, string path)
        {
            TextReader reader = new StreamReader(path);
            object obj = deserializer.Deserialize(reader);
            reader.Close();
            return (T)obj;
        }

        public static void WriteXML<T>(XmlSerializer serializer, object self, string path)
        {
            StreamWriter myWriter = new StreamWriter(path);
            serializer.Serialize(myWriter, self);
            myWriter.Close();
        }

        public static XmlSerializer CreateNew(Type type)
        {
            return new XmlSerializer(type, ExtraTypes);
        }

        public static XmlSerializer CreateNew(Type type, Type[] extraTypes)
        {
            return new XmlSerializer(type, extraTypes);
        }
    }
}

