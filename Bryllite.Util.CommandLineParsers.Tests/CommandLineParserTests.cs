using System;
using Bryllite.Util.CommandLine;
using Xunit;

namespace Bryllite.Util.CommandLineParsers.Tests
{
    public class CommandLineParserTests
    {
        [Theory]
        [InlineData("-string=string", "-int=100", "-true=true", "-false=false", "-define", "decimal=3.141592")]
        public void ShouldBeParsable(params string[] args)
        {
            CommandLineParser cmd = new CommandLineParser(args);

            Assert.Equal("string", cmd.Value<string>("string"));
            Assert.Equal("none", cmd.Value("none", "none"));
            Assert.Equal(100, cmd.Value<int>("int"));
            Assert.Equal(100, cmd.Value<long>("int"));
            Assert.Equal(100, cmd.Value<byte>("int"));
            Assert.True(cmd.Value<bool>("true"));
            Assert.False(cmd.Value<bool>("false"));
            Assert.True(cmd.Value<bool>("define"));
            Assert.False(cmd.Value<bool>("undefined"));
            Assert.Equal(3.141592m, cmd.Value<decimal>("decimal"));
            Assert.Equal(3.141592, cmd.Value<double>("decimal"));
            //            Assert.Equal(3.141592, cmd.Value<float>("decimal"));

            Assert.True(cmd.Exists("define"));
            Assert.False(cmd.Exists("not-defined"));

            cmd.SetValue("defined", true);
            Assert.True(cmd.Value<bool>("defined"));

            cmd.SetValue("string", "new string");
            Assert.Equal("new string", cmd.Value<string>("string"));
        }

    }
}
