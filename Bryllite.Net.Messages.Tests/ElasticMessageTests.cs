using System;
using System.Collections.Generic;
using System.Text;
using Bryllite.Core.Key;
using Bryllite.Net.Elastic;
using Bryllite.Util;
using Bryllite.Util.Payloads;
using Xunit;

namespace Bryllite.Net.Messages.Tests
{
    public class ElasticMessageTests
    {
        [Fact]
        public void ShouldBeParsableElastic3D()
        {
            PrivateKey messageKey = PrivateKey.CreateKey();

            Elastic3D to = new Elastic3D(1, 2, 3);
            ElasticLayout layout = new ElasticLayout(16, 16, 16);


            Message message = new Message.Builder()
                .Body("rndBytes", RndProvider.GetNonZeroBytes(32))
                .Build(messageKey);

        }

        [Fact]
        public void PayloadShouldBeParsable()
        {
            Elastic3D to = new Elastic3D(1, 2, 3);
            ElasticLayout layout = ElasticLayout.DefineLayout(1024);

            Payload payload = new Payload.Builder()
                .Value("to", to)
                .Value("layout", layout)
                .Build();

            Payload received = Payload.Parse(payload.ToBytes());

            Elastic3D receivedTo = received.Value<Elastic3D>("to");
            ElasticLayout receivedLayout = received.Value<ElasticLayout>("layout");

            Assert.Equal(1, receivedTo.X);
            Assert.Equal(2, receivedTo.Y);
            Assert.Equal(3, receivedTo.Z);
            Assert.Equal(to, receivedTo);

            Assert.Equal(layout.X, receivedLayout.X);
            Assert.Equal(layout.Y, receivedLayout.Y);
            Assert.Equal(layout.Z, receivedLayout.Z);
        }

        [Fact]
        public void Test1()
        {
            Payload payload = new Payload.Builder()
                .Value("exists", true)
                .Build();

            Assert.True(payload.Value<bool>("exists"));
            Assert.False(payload.Value<bool>("not exists"));
        }

        [Fact]
        public void Test2()
        {
            PrivateKey key = PrivateKey.CreateKey();

            ElasticLayout layout = ElasticLayout.DefineLayout(1024);
            Elastic3D to = new Elastic3D((byte)(1 + RndProvider.Next(layout.X)), (byte)(1 + RndProvider.Next(layout.Y)), (byte)(1 + RndProvider.Next(layout.Z)));

            Message message = new Message.Builder()
                .Body("rndBytes", RndProvider.GetNonZeroBytes(128))
                .Build(key);

            PrivateKey routerKey = PrivateKey.CreateKey();

            message.RouteTo(to.TimeToLive(), to, layout, routerKey);

            Payload routes = message.Routes();

            Assert.Equal(to.TimeToLive(), routes.Value<byte>("ttl"));
            Assert.Equal(to, routes.Value<Elastic3D>("to"));
            Assert.Equal(layout, routes.Value<ElasticLayout>("layout"));

            Address router = message.Router();
            Assert.Equal(routerKey.Address, router);
            Assert.True(message.VerifyRouter());

            message.TimeToLive(0);

            Assert.Equal(0, message.TimeToLive());

            Assert.Equal(0, new Elastic3D(0, 0, 0).TimeToLive());
            Assert.Equal(1, new Elastic3D(16, 0, 0).TimeToLive());
            Assert.Equal(2, new Elastic3D(13, 15, 0).TimeToLive());
            Assert.Equal(3, new Elastic3D(1, 2, 3).TimeToLive());
        }

        [Fact]
        public void DateTimeTest()
        {
            PrivateKey key = PrivateKey.CreateKey();

            Message ping = new Message.Builder()
                .Body("rndBytes", RndProvider.GetNonZeroBytes(32))
                .Build(key);

            Message received = Message.Parse(ping.ToBytes());

            Assert.Equal(ping.TimeStamp, received.TimeStamp);
        }
    }
}
