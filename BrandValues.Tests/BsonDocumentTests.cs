using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using NUnit.Framework;

namespace BrandValues.Tests
{
    class BsonDocumentTests
    {
        public BsonDocumentTests()
        {
            JsonWriterSettings.Defaults.Indent = true;
        }

        [Test]
        public void EmptyDocument()
        {
            var document = new BsonDocument();
            Console.WriteLine(document.ToJson());
        }

        [Test]
        public void AddElements()
        {
            var person = new BsonDocument
            {
                {"age", new BsonInt32(54)},
                {"IsAlive", true}
            };
            person.Add("firstName", new BsonString("bob"));

            Console.WriteLine(person);
        }

        [Test]
        public void AddingArrays()
        {
            var person = new BsonDocument();
            person.Add("address", new BsonArray(new[] {"101 Some Road", "Unit 501"}));

            Console.WriteLine(person);
        }

        [Test]
        public void EmbeddedDocument()
        {
            var person = new BsonDocument
            {
                {"contact", new BsonDocument
                {
                    {"phone", "12345"},
                    {"email", "test@test.com"}
                }}
            };

            Console.WriteLine(person);
        }

        [Test]
        public void BsonValueConversions()
        {
            var person = new BsonDocument
            {
                {"age", 54}
            };

            Console.WriteLine(person["age"].AsInt32 + 10 );
            Console.WriteLine(person["age"].ToDouble() + 10);
            Console.WriteLine(person["age"].IsString);
        }

        [Test]
        public void ToBson()
        {
            //To BSON, then deserialize

            var person = new BsonDocument
            {
                {"firstName", "bob"}
            };

            var bson = person.ToBson();

            Console.WriteLine(BitConverter.ToString(bson));

            var deserializedPerson = BsonSerializer.Deserialize<BsonDocument>(bson);
            Console.WriteLine(deserializedPerson);
        }
    }
}
