using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BrandValues.Models
{
    public class SiteVersion
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Homepage { get; set; }

        public SiteVersion()
        {
        }

        public void Edit(SiteVersionViewModel edit)
        {
            Homepage = edit.Homepage;
        }
    }
}