using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bryllite.Core.Hash;
using Newtonsoft.Json.Linq;

namespace Bryllite.Util.Payloads
{
    public class Payload : IPayload
    {
        protected JObject payloads;

        public byte[] Hash => HashProvider.Hash256(ToBytes());

        public string[] Keys => payloads.ToObject<Dictionary<string, object>>().Keys.ToArray();

        protected Payload()
        {
            payloads = new JObject();
        }

        protected Payload(JObject other)
        {
            payloads = new JObject(other);
        }

        protected Payload(object obj)
        {
            payloads = JObject.FromObject(obj);
        }

        protected Payload(Payload other) : this(other.payloads)
        {
        }

        protected Payload(string json)
        {
            payloads = JObject.Parse(json);
        }

        protected Payload(byte[] bytes)
        {
            payloads = BsonConverter.FromBytes<JObject>(bytes);
        }

        public static implicit operator JObject(Payload payloads)
        {
            return payloads.payloads;
        }

        public byte[] ToBytes()
        {
            return BsonConverter.ToBytes(payloads);
        }

        public bool ContainsKey(string key)
        {
            return payloads.ContainsKey(key);
        }

        public override string ToString()
        {
            return payloads.ToString();
        }

        public static Payload Parse(byte[] bytes, int offset)
        {
            return new Payload(offset > 0 ? bytes.Skip(offset).ToArray() : bytes);
        }

        public static Payload Parse(byte[] bytes)
        {
            return new Payload(bytes);
        }

        /// <summary>
        /// JSON 내부는 unordered name/value pairs set 이기 때문에,
        /// Parse(string) 으로 읽어온 Payload와 Parse(byte[])로 읽어온 payload의 Hash값은 다르다
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Payload Parse(string json)
        {
            return new Payload(json);
        }

        public static Payload FromObject(JObject obj)
        {
            return new Payload(obj);
        }

        public static Payload FromObject(object obj)
        {
            return FromObject(JObject.FromObject(obj));
        }

        public T ToObject<T>()
        {
            return payloads.ToObject<T>();
        }

        public T Value<T>(string name)
        {
            return Value(name, default(T));
        }

        public T Value<T>(string name, T defaultValue)
        {
            try
            {
                if (typeof(T) == typeof(Payload))
                {
                    object p = new Payload(payloads[name].ToObject<JObject>());
                    return (T)p;
                }

                return payloads[name].ToObject<T>();
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public void Set(string name, JValue value)
        {
            payloads[name] = value;
        }

        public void Set(string name, JToken value)
        {
            payloads[name] = value;
        }

        public void Set(string name, JArray value)
        {
            payloads[name] = value;
        }

        public void Set(string name, JObject value)
        {
            payloads[name] = value;
        }

        public void Set(string name, object value)
        {
            Set(name, JObject.FromObject(value));
        }

        public void Set(string name, string[] values)
        {
            Set(name, new JArray(values));
        }

        public void Set(string name, object[] values)
        {
            JArray array = new JArray();
            foreach (var value in values)
                array.Add(JObject.FromObject(value));

            Set(name, array);
        }

        public class Builder
        {
            private Payload payload;

            public Builder()
            {
                payload = new Payload();
            }

            public Builder(Payload other)
            {
                payload = new Payload(other);
            }

            public Builder(JObject other)
            {
                payload = new Payload(other);
            }

            public Builder(string json)
            {
                payload = Parse(json);
            }

            public Builder(object obj)
            {
                payload = new Payload(obj);
            }

            public Builder Value(string name, JObject value)
            {
                payload.Set(name, value);
                return this;
            }

            public Builder Value(string name, JToken value)
            {
                payload.Set(name, value);
                return this;
            }

            public Builder Value(string name, JArray value)
            {
                payload.Set(name, value);
                return this;
            }

            public Builder Value(string name, JValue value)
            {
                payload.Set(name, value);
                return this;
            }

            public Builder Value(string name, object value)
            {
                return Value(name, JObject.FromObject(value));
            }


            public Builder Value(string name, string[] values)
            {
                return Value(name, new JArray(values));
            }

            public Builder Value(string name, object[] values)
            {
                JArray array = new JArray();

                foreach (var value in values)
                    array.Add(JObject.FromObject(value));

                return Value(name, array);
            }

            public Payload Build()
            {
                return payload;
            }
        }
    }
}
