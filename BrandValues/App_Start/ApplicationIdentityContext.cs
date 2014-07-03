using System.Collections.Specialized;
using System.Configuration;

namespace BrandValues
{
	using System;
	using AspNet.Identity.MongoDB;
	using MongoDB.Driver;

	public class ApplicationIdentityContext : IdentityContext, IDisposable
	{
		public ApplicationIdentityContext(MongoCollection users, MongoCollection roles) : base(users, roles)
		{
		}

		public static ApplicationIdentityContext Create()
		{
			// todo add settings where appropriate to switch server & database in your own application
            //var client = new MongoClient("mongodb://localhost:27017");
            //var database = client.GetServer().GetDatabase("mydb");

            NameValueCollection appConfig = ConfigurationManager.AppSettings;
            string mongoConnectionString = appConfig["PARAM1"];
            string mongoDatabaseName = appConfig["PARAM2"];

            var settings = MongoClientSettings.FromUrl(new MongoUrl(mongoConnectionString));
            settings.WriteConcern.Journal = true;
            var client = new MongoClient(settings);
            var database = client.GetServer().GetDatabase(mongoDatabaseName);

			var users = database.GetCollection<IdentityUser>("users");
			var roles = database.GetCollection<IdentityRole>("roles");
			return new ApplicationIdentityContext(users, roles);
		}

		public void Dispose()
		{
		}
	}
}