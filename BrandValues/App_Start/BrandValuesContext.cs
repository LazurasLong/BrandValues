using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using BrandValues.Entries;
using BrandValues.Models;
using MongoDB.Driver;

namespace BrandValues.App_Start
{
    public class BrandValuesContext
    {
        public MongoDatabase Database;

        public BrandValuesContext()
        {
            NameValueCollection appConfig = ConfigurationManager.AppSettings;
            string mongoConnectionString = appConfig["PARAM1"];
            
            #if(DEBUG)
            string mongoDatabaseName = appConfig["PARAM3"];
            #endif

            #if(!DEBUG)
            string mongoDatabaseName = appConfig["PARAM4"];
            #endif

            var settings = MongoClientSettings.FromUrl(new MongoUrl(mongoConnectionString));
            settings.WriteConcern.Journal = true;
            var client = new MongoClient(settings);
            Database = client.GetServer().GetDatabase(mongoDatabaseName);
        }

        public MongoCollection<Entry> Entries
        {
            get { return Database.GetCollection<Entry>("entries"); }
        }

        public MongoCollection<SiteVersion> SiteVersions
        {
            get { return Database.GetCollection<SiteVersion>("siteversions"); }
        }

        public MongoCollection<Poll> Polls
        {
            get { return Database.GetCollection<Poll>("polls"); }
        }

        public MongoCollection<ShortlistedEntry> ShortlistedEntries
        {
            get { return Database.GetCollection<ShortlistedEntry>("shortlists"); }
        }

    }
}