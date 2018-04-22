using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FAKE.MongoDB
{
    public class MongoHelper
    {
        public static BsonDocument ___Eval___(string ConnectionString, string DbName, string JavaScript
            , bool Nolock = true)
        {
            var client = new MongoClient(ConnectionString);
            var db = client.GetDatabase(DbName);
            BsonDocument bd = new BsonDocument();
            bd.Add("eval", JavaScript);
            bd.Add("nolock", Nolock);
            return db.RunCommand<BsonDocument>(new BsonDocumentCommand<BsonDocument>(bd));
        }
        public static string ___ToJson___(BsonDocument Bson)
        {
            return Bson.ToJson(new JsonWriterSettings()
            {
                OutputMode = JsonOutputMode.Strict
            });
        }
        private static IMongoCollection<BsonDocument> GetCollection(string ConnectionString, string DbName, string CollectionName)
        {
            var client = new MongoClient(ConnectionString);
            var db = client.GetDatabase(DbName);
            return db.GetCollection<BsonDocument>(CollectionName);
        }

        public static string Eval(string ConnectionString, string DbName, string JavaScript
            , bool Nolock = true)
        {
            return ___ToJson___(___Eval___(ConnectionString, DbName, JavaScript, Nolock));
        }


        public static void Export(string ConnectionString, string DbName, string CollectionName, string ExportFile,
            string[] pipeline = null)
        {
            List<BsonDocument> o = null;
            var c = GetCollection(ConnectionString, DbName, CollectionName);
            if (pipeline == null || !pipeline.Any())
            {
                o = c.Find<BsonDocument>(Builders<BsonDocument>.Filter.Empty)
                    .ToList();
            }
            else
            {
                o = c.Aggregate<BsonDocument>(PipelineDefinition<BsonDocument, BsonDocument>.Create(pipeline))
                    .ToList();
            }
            var d = new BsonDocument()
            {
                { "o", BsonArray.Create(o)}
            }.ToBson();
            File.WriteAllBytes(ExportFile, d);
        }
        public static void Import(string ConnectionString, string DbName, string CollectionName, string ImportFile,
            int CopyMode = 1)
        {
            var i = BsonSerializer.Deserialize<BsonDocument>(File.ReadAllBytes(ImportFile))["o"].AsBsonArray;
            var c = GetCollection(ConnectionString, DbName, CollectionName);
            if (CopyMode == 0)
            {
                c.InsertMany(i.Select(t => t.AsBsonDocument));
            }
            else if (CopyMode == 1)
            {
                c.DeleteMany(Builders<BsonDocument>.Filter.Empty);
                c.InsertMany(i.Select(t => t.AsBsonDocument));
            }
            else if (CopyMode == 2)
            {
                foreach (var d in i)
                {
                    c.ReplaceOne(Builders<BsonDocument>.Filter.Eq("_id", d["_id"].AsObjectId), d.AsBsonDocument, new UpdateOptions()
                    {
                        IsUpsert = true
                    });
                }
            }
        }
    }
}
