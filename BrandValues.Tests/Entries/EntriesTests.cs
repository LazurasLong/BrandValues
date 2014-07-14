using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrandValues.Entries;
using MongoDB.Bson;
using NUnit.Framework;

namespace BrandValues.Tests.Entries
{
    [TestFixture]
    class EntriesTests : AssertionHelper
    {
        [Test]
        public void ToDocument_EntryWithPrice_PriceAsDouble()
        {
            var entry = new Entry();
            entry.Price = 1;

            var document = entry.ToBsonDocument();

            Expect(document["Price"].BsonType, Is.EqualTo(BsonType.Double));
        }

        [Test]
        public void ToDocument_EntryWithAnId_IdIsAnObjectId()
        {
            var entry = new Entry();
            entry.Id = ObjectId.GenerateNewId().ToString();

            var document = entry.ToBsonDocument();

            Expect(document["_id"].BsonType, Is.EqualTo(BsonType.ObjectId));

        }
    }
}
