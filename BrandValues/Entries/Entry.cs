using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BrandValues.Entries
{
    public class Entry
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        //Individual or team
        public string Type { get; set; }

        //Video, text, image
        public string Format { get; set; }

        public string ThumbnailUrl { get; set; }

        public string Url { get; set; }

        public List<string> Values { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<string> Likes = new List<string>();

        public int Hits { get; set; }

        public List<string> Comments = new List<string>();

        public string UserName { get; set; }

        public string UserArea { get; set; }

        public Entry()
        {
        }

        public Entry(PostEntry postEntry)
        {
            Name = postEntry.Name;
            Description = postEntry.Description;
            Format = postEntry.Format;
            Type = postEntry.Type;
            Values = postEntry.Values;
        }

        public void Edit(Edit edit)
        {
            Description = edit.NewDescription;
        }
        
    }
}