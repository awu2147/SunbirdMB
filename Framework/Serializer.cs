using System;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace SunbirdMB.Framework
{
    public static class Serializer
    {
        static Serializer()
        {
            var e = new XmlDeserializationEvents();
            e.OnUnknownAttribute += On_UnknownAttribute;
            e.OnUnknownElement += On_UnknownElement;
            e.OnUnknownNode += On_UnknownNode;
            e.OnUnreferencedObject += On_UnreferencedObject;
            XmlDeserializationEvents = e;
        }

        public static XmlDeserializationEvents XmlDeserializationEvents { get; set; }
        public static Type[] ExtraTypes { get; set; } = new Type[] { };

        public static T ReadXML<T>(XmlSerializer deserializer, string path)
        {
            using (TextReader stream = new StreamReader(path))
            {
                object obj = deserializer.Deserialize(stream);
                return (T)obj;
                //using (XmlReader reader = XmlReader.Create(stream))
                //{
                //    object obj = deserializer.Deserialize(stream);
                //    return (T)obj;
                //}
            }
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

        public static T CloneBySerialization<T>(T obj, XmlSerializer serializer)
        {
            WriteXML<T>(serializer, obj, @"obj\.tempcache");
            return ReadXML<T>(serializer, @"obj\.tempcache");
        }

        public static void On_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
        }

        public static void On_UnknownElement(object sender, XmlElementEventArgs e)
        {
        }

        public static void On_UnknownNode(object sender, XmlNodeEventArgs e)
        {
        }

        public static void On_UnreferencedObject(object sender, UnreferencedObjectEventArgs e)
        {
        }

    }
}

