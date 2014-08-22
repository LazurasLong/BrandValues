using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

        public string VideoThumbnailUrl { get; set; }

        public string Url { get; set; }

        public string Values { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<string> Likes = new List<string>();

        public List<string> Votes = new List<string>();

        public int Hits { get; set; }

        public List<PostComment> Comments { get; set; }

        public string UserName { get; set; }

        public string ContactEmail { get; set; }

        public string ContactTel { get; set; }

        public string UserFirstName { get; set; }

        public string UserSurname { get; set; }

        public string TeamName { get; set; }

        public string TeamNumber { get; set; }

        public string TeamMemberNames { get; set; }

        public string UserArea { get; set; }

        public DateTime CreatedOn { get; set; }

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
            TeamName = postEntry.TeamName;
            TeamNumber = postEntry.TeamNumber;
            TeamMemberNames = postEntry.TeamMemberNames;
            ContactEmail = postEntry.ContactEmail;
            ContactTel = postEntry.ContactTel;
        }

        public void Edit(Edit edit)
        {
            Description = edit.Description;
            Name = edit.Name;
        }
        
    }
}