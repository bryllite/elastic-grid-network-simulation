using System;
using System.Linq;
using Bryllite.Core.Key;
using Xunit;

namespace Bryllite.Util.Payloads.Tests
{
    public class PayloadTest
    {
        public class Person
        {
            public int id;
            public string name;
            public bool female;
            public byte age;
            public decimal height;
            public double weight;
            public string[] hobby;
            public DateTime birth;
            public byte[] key;
            public object context = null;

            public Person()
            {
            }
        }

        [Fact]
        public void ShouldBeSerializable()
        {
            string name = RndProvider.GetString(16);
            int age = RndProvider.Next(1, 100);
            byte[] rndBytes = RndProvider.GetNonZeroBytes(128);

            // build
            Payload payload = new Payload.Builder()
                .Value("name", name)
                .Value("age", age)
                .Value("rndBytes", rndBytes)
                .Build();

            // serialize & deserialize
            Payload received = Payload.Parse(payload.ToBytes());

            // assert
            Assert.Equal(name, received.Value<string>("name"));
            Assert.Equal(age, received.Value<int>("age"));
            Assert.True(rndBytes.SequenceEqual(received.Value<byte[]>("rndBytes")));
            Assert.Equal(payload.Hash.ToHex(), received.Hash.ToHex());

            Assert.Equal(payload.Hash, received.Hash);
        }

        [Fact]
        public void ShouldBeSerializableObject()
        {
            Person jade = new Person()
            {
                id = 100,
                name = "JADE KIM",
                female = false,
                age = 43,
                height = 174.5m,
                weight = 77.01,
                hobby = new string[]{ "Basketball", "Movie", "Music" },
                birth = new DateTime( 1977, 3, 19 ),
                key = PrivateKey.CreateKey().Bytes
            };

            // serialize & deserialize
            Person person = Payload.Parse(
                new Payload.Builder(jade)
                .Build()
                .ToBytes()
            ).ToObject<Person>();

            // assert
            Assert.Equal(jade.id, person.id);
            Assert.Equal(jade.name, person.name);
            Assert.Equal(jade.female, person.female);
            Assert.Equal(jade.age, person.age);
            Assert.Equal(jade.height, person.height);
            Assert.Equal(jade.weight, person.weight);
            Assert.Equal(jade.birth, person.birth);
            Assert.Equal(jade.key.ToHex(), person.key.ToHex());
            Assert.Null(person.context);

            for (int i = 0; i < jade.hobby.Length; i++)
                Assert.Equal(jade.hobby[i], person.hobby[i]);

            // serialize & deserialize
            person = Payload.Parse(
                new Payload.Builder()
                .Value("person", jade )
                .Build()
                .ToBytes()
            ).Value<Person>("person");

            // assert
            Assert.Equal(jade.id, person.id);
            Assert.Equal(jade.name, person.name);
            Assert.Equal(jade.female, person.female);
            Assert.Equal(jade.age, person.age);
            Assert.Equal(jade.height, person.height);
            Assert.Equal(jade.weight, person.weight);
            Assert.Equal(jade.birth, person.birth);
            Assert.Equal(jade.key.ToHex(), person.key.ToHex());
            Assert.Null(person.context);

            for (int i = 0; i < jade.hobby.Length; i++)
                Assert.Equal(jade.hobby[i], person.hobby[i]);
        }

        [Fact]
        public void ShouldBeAbleToOverlap()
        {
            Payload header = new Payload.Builder()
                .Value("ver", 0x01)
                .Build();

            Payload body = new Payload.Builder()
                .Value("message", "Hello, Bryllite!")
                .Value("rndBytes", RndProvider.GetNonZeroBytes(128))
                .Build();

            Payload message = new Payload.Builder()
                .Value("header", header)
                .Value("body", body)
                .Build();

            Payload received = Payload.Parse(message.ToBytes());

            Payload receivedHeader = received.Value<Payload>("header");
            Payload receivedBody = received.Value<Payload>("body");

            Assert.Equal(message.Hash, received.Hash);
            Assert.Equal(header.Hash, receivedHeader.Hash);
            Assert.Equal(body.Hash, receivedBody.Hash);

            Assert.Equal(header.Value<int>("ver"), receivedHeader.Value<int>("ver"));
            Assert.Equal(body.Value<string>("message"), receivedBody.Value<string>("message"));
            Assert.Equal(body.Value<byte[]>("rndBytes"), receivedBody.Value<byte[]>("rndBytes"));

        }
    }
}
