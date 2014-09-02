using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrandValues.Models
{
    public class Poll
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Value { get; set; }

        public List<string> Votes = new List<string>();

        public Poll()
        {
        }

    }
}