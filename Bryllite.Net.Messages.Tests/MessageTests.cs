using System;
using Bryllite.Core.Key;
using Bryllite.Util;
using Bryllite.Util.Payloads;
using Xunit;

namespace Bryllite.Net.Messages.Tests
{
    public class MessageTests
    {
        [Theory]
        [InlineData(10000)]
        public void ShouldBeSerializable( int repeats )
        {
            for (int i = 0; i < repeats; i++)
            {
                PrivateKey key = PrivateKey.CreateKey();
                byte[] rndBytes = RndProvider.GetNonZeroBytes(1024);

                Payload body = new Payload.Builder()
                    .Value("rndBytes", rndBytes)
                    .Build();

                Message message = new Message.Builder()
                    .WithBody(body)
                    .Build(key);

                // serialize & deserialize
                Message received = Message.Parse(message.ToBytes());

                Assert.Equal(message.Hash, received.Hash);
                Assert.Equal(rndBytes, message.Value<byte[]>("rndBytes"));
                Assert.Equal(rndBytes, received.Value<byte[]>("rndBytes"));
                Assert.Equal(message.Value<byte[]>("rndBytes"), received.Value<byte[]>("rndBytes"));
                Assert.Equal(message.Headers.Hash, received.Headers.Hash);
                Assert.Equal(message.Body.Hash, received.Body.Hash);
            }
        }

        [Theory]
        [InlineData(10000)]
        public void ShouldBeModifiable(int repeats)
        {
            for (int i = 0; i < repeats; i++)
            {
                PrivateKey key = PrivateKey.CreateKey();
                byte[] rndBytes = RndProvider.GetNonZeroBytes(1024);

                Payload body = new Payload.Builder()
                    .Value("rndBytes", rndBytes)
                    .Build();

                Message message = new Message.Builder()
                    .WithBody(body)
                    .Build(key);

                // serialize & deserialize
                Message received = Message.Parse(message.ToBytes());
                Assert.True(received.Verify(key.Address));

                // verify ok if header changed
                received.Headers.Set("timestamp", DateTime.Now);
                Assert.True(received.Verify(key.Address));

                // verify failed if body changed
                received.Body.Set("id", RndProvider.Next(100));
                Assert.False(received.Verify(key.Address));
            }
        }

        [Theory]
        [InlineData( 10000 )]
        public void ShouldBeVerifiable( int repeats )
        {
            for (int i = 0; i < repeats; i++)
            {
                PrivateKey key = PrivateKey.CreateKey();
                PrivateKey wrongKey = key.CKD(key.Bytes);
                byte[] rndBytes = RndProvider.GetNonZeroBytes(1024);

                Payload body = new Payload.Builder()
                    .Value("rndBytes", rndBytes)
                    .Build();

                Message message = new Message.Builder()
                    .WithBody(body)
                    .Build(key);

                // serialize & deserialize
                Message received = Message.Parse(message.ToBytes());

                Assert.True(received.Verify(key.Address));
                Assert.False(received.Verify(wrongKey.Address));

                // verify failed if body has changed
                received.Body.Set("rndBytes", RndProvider.GetNonZeroBytes(128));
                Assert.False(received.Verify(key.Address));
            }
        }

    }
}
