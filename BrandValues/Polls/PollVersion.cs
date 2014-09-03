using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrandValues.Models
{
    public class PollVersion
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Version { get; set; }

        public PollVersion()
        {
        }

    }
}