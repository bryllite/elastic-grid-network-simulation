using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Util.CommandLine
{
    public interface ICommandLineBuilder
    {
        int Count { get; }

        T Value<T>(string key) where T : IConvertible;

        T Value<T>(string key, T defaultValue) where T : IConvertible;

        void SetValue<T>(string key, T value) where T : IConvertible;

        void Remove(string key);

        bool Exists(string key);

        string ToString();

        string[] ToStringArray();

        string[] ToStringArray(bool includeEscape);
    }
}
