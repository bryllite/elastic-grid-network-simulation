using BrylliteLib.Hash;
using BrylliteLib.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrylliteLib.Net
{
    public class Payload
    {
        // JSON Object
        private JObject _jObject;

        public string[] Keys
        {
            get
            {
                try
                {
                    lock (_jObject)
                    {
                        return _jObject.ToObject<Dictionary<string, object>>().Keys.ToArray();
                    }
                }
                catch (Exception)
                {
                    return new string[0];
                }
            }
        }

        public byte[] Hash
        {
            get
            {
                return HashUtil.Hash256(ToByteArray());
            }
        }

        public Payload()
        {
            _jObject = new JObject();
        }

        protected Payload(Payload payload) : this(payload.ToJObject())
        {
        }

        protected Payload(JObject jObject)
        {
            _jObject = new JObject(jObject);
        }

        protected Payload(string json)
        {
            try
            {
                _jObject = JObject.Parse(json);
            }
            catch (Exception e)
            {
                throw new Exception("Payload deserialize failed!", e);
            }
        }

        protected Payload(byte[] bytes)
        {
            _jObject = BsonConverter.FromBytes<JObject>(bytes);
        }

        protected JObject ToJObject()
        {
            return new JObject(_jObject);
        }

        protected void Set(string propertyName, JToken value)
        {
            try
            {
                lock (_jObject)
                {
                    _jObject[propertyName] = value;
                }
            }
            catch (Exception e)
            {
                throw new ArgumentOutOfRangeException($"Set() failed! propertyName={propertyName}", e);
            }
        }

        public T Get<T>(string propertyName)
        {
            if (typeof(T) == typeof(Payload))
            {
                object p = new Payload(_jObject.GetValue(propertyName).ToObject<JObject>());
                return (T)p;
            }

            return _jObject.GetValue(propertyName).ToObject<T>();
        }

        public byte[] ToByteArray()
        {
            return BsonConverter.ToBytes(_jObject);
        }

        public bool ContainsKey(string keyName)
        {
            lock (_jObject)
            {
                return _jObject.ContainsKey(keyName);
            }
        }

        public static Payload Parse(byte[] bytes)
        {
            return new Payload(bytes);
        }

        public string ToJSON()
        {
            lock (_jObject)
            {
                return _jObject.ToString();
            }
        }

        public static Payload Parse(string json)
        {
            return new Payload(json);
        }

        public void Set(string propertyName, Payload value)
        {
            lock (_jObject)
            {
                _jObject[propertyName] = value.ToJObject();
            }
        }

        public void Set(string name, bool value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, byte value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, char value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, sbyte value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, byte[] value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, short value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, ushort value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, int value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, uint value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, long value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, ulong value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, float value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, double value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, decimal value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, string value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, string[] value)
        {
            Set(name, new JArray(value));
        }

        public void Set(string name, object[] value)
        {
            Set(name, new JArray(value));
        }

        public void Set(string name, DateTime value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, DateTimeOffset value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, TimeSpan value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, Guid value)
        {
            Set(name, new JValue(value));
        }

        public void Set(string name, Uri value)
        {
            Set(name, new JValue(value));
        }
    }
}
