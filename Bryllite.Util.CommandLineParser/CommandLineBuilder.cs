using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Bryllite.Util.CommandLine
{
    public class CommandLineBuilder : ICommandLineBuilder
    {
        protected Dictionary<string, string> KeyValuePairs = new Dictionary<string, string>();

        public int Count => KeyValuePairs.Count;


        public CommandLineBuilder()
        {
        }

        public CommandLineBuilder WithValue<T>(string key, T value) where T : IConvertible
        {
            SetValue(key, value);
            return this;
        }

        public ICommandLineBuilder Build()
        {
            return this;
        }

        public bool Exists(string key)
        {
            lock (KeyValuePairs)
                return KeyValuePairs.ContainsKey(key);
        }

        public T Value<T>(string key) where T : IConvertible
        {
            return Value(key, default(T));
        }

        public T Value<T>(string key, T defaultValue) where T : IConvertible
        {
            lock (KeyValuePairs)
            {
                if (KeyValuePairs.ContainsKey(key))
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                    if (!ReferenceEquals(converter, null) && converter.CanConvertFrom(typeof(string)))
                        return (T)converter.ConvertFrom(KeyValuePairs[key]);
                }
            }

            return defaultValue;
        }

        public void SetValue<T>(string key, T value) where T : IConvertible
        {
            lock (KeyValuePairs)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                if (!ReferenceEquals(converter, null) && converter.CanConvertTo(typeof(string)))
                    KeyValuePairs[key] = converter.ConvertToString(value);
            }
        }

        public void Remove(string key)
        {
            lock (KeyValuePairs)
                KeyValuePairs.Remove(key);
        }

        public override string ToString()
        {
            return string.Join(" ", ToStringArray());
        }

        public string[] ToStringArray()
        {
            return ToStringArray(false);
        }

        public string[] ToStringArray( bool includeEscape )
        {
            List<string> strings = new List<string>();

            lock (KeyValuePairs)
            {
                foreach (var pair in KeyValuePairs)
                {
                    if (includeEscape)
                        strings.Add($"\"{pair.Key}={pair.Value}\"");
                    else strings.Add($"{pair.Key}={pair.Value}");
                }
            }

            return strings.ToArray();
        }

    }
}
