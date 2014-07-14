using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using NUnit.Framework;

namespace BrandValues.Tests
{
    class PocoTests
    {
        public PocoTests()
        {
            JsonWriterSettings.Defaults.Indent = true;
        }

        public class Person
        {
            public string FirstName { get; set; }
            public int Age { get; set; }

            public List<string> Address = new List<string>();
            public Contact Contact = new Contact();

            [BsonIgnore]
            public string IgnoreMe { get; set; }

            [BsonElement("New")]
            public string Old { get; set; }

            [BsonIgnoreIfNull]
            public string Null { get; set; }
        }

        public class Contact
        {
            public string Email { get; set; }
            public string Phone { get; set; }
        }

        [Test]
        public void Automatic()
        {
            var person = new Person
            {
                Age = 64,
                FirstName = "bob"
            };

            Console.WriteLine(person.ToJson());
        }

        [Test]
        public void SerializationAttributes()
        {
            var person = new Person();
            Console.WriteLine(person.ToJson());
        }
    }
}
