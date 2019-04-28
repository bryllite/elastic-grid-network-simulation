using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Bryllite.Util.Payloads
{
    public interface IPayload
    {
        byte[] Hash { get; }

        string[] Keys { get; }

        bool ContainsKey(string name);

        byte[] ToBytes();

        string ToString();

        T ToObject<T>();

        T Value<T>(string name);

        T Value<T>(string name, T defaultValue);

        void Set(string name, JObject value);

        void Set(string name, JToken value);

        void Set(string name, JValue value);

        void Set(string name, JArray value);

        void Set(string name, object value);

        void Set(string name, string[] values);

        void Set(string name, object[] values);
    }
}
