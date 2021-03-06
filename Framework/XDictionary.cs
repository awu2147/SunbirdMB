﻿using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SunbirdMB.Framework
{
    public class XDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public XDictionary() { }
        public XDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
        public XDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
        public XDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public XDictionary(int capacity) : base(capacity) { }
        public XDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey), Serializer.ExtraTypes);
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), Serializer.ExtraTypes);

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey), Serializer.ExtraTypes);
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), Serializer.ExtraTypes);

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

        #endregion
    }

}
