using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading;
using MongoDB.Bson.Serialization;

namespace SearchEngineTools
{
    public class MongoStorage : IDocumentStorage
    {
        const string connectionString = "mongodb://localhost";
        MongoClient client = new MongoClient(connectionString);
        private IMongoDatabase db;
        private IMongoCollection<Document> coll;
        private int currentId;

        public MongoStorage()
        {
            db = client.GetDatabase("docs");
            coll = db.GetCollection<Document>("entities");
            var match = new BsonDocument
            {
                {
                    "$group",
                    new BsonDocument
                    {
                        {"_id", ""},
                        {"last", new BsonDocument
                            {
                                { "$max", "$_id"}
                            }
                        }
                    }
                }
            };
            PipelineDefinition<Document, BsonDocument> pipeline = new[]
            {
                match
            };
            try
            {
                currentId = (int)coll.Aggregate(pipeline).First()[1]; //максимальный id
            }
            catch (Exception ex)
            {
                currentId = 0;
            }
        }

        public int Add(Document doc)
        {
            try
            {
                doc.intId = Interlocked.Increment(ref currentId);
                coll.InsertOne(doc);
                return 1;
            }
            catch (AggregateException)
            {
                return -1;
            }
            catch
            {
                return -100;
            }
        }

        public int AddRange(IEnumerable<Document> docs)
        {
            try
            {
                foreach (var t in docs)
                    t.intId = Interlocked.Increment(ref currentId);
                coll.InsertMany(docs);
                return docs.Count();
            }
            catch (AggregateException)
            {
                return -1;
            }
            catch
            {
                return -100;
            }
        }

        public Document Get(int id)
        {
            return coll.AsQueryable().First(x => x.intId == id);
        }

        public IList<Document> GetRange(IList<int> ids)
        {
            var query = new FilterDefinitionBuilder<Document>().In("_id", ids);
            return coll.Find(query).ToList();
        }

        public IList<DocStat> GetDocStat(IList<int> ids)
        {
            var query = new FilterDefinitionBuilder<Document>().In("_id", ids);

            var tt = coll.Find(query).Project(
                Builders<Document>.Projection
                    .Include(x => x.len)
                    .Include(x => x.tlen)
                ).ToList();
            return tt.Select(x => BsonSerializer.Deserialize<DocStat>(x)).ToList();
        }

        public void ClearStorage()
        {
            db.DropCollection("entities");
        }

        public async Task<int> AddAsync(Document doc)
        {
            try
            {
                doc.intId = Interlocked.Increment(ref currentId);
                await coll.InsertOneAsync(doc);
                return doc.intId;
            }
            catch (AggregateException)
            {
                return -1;
            }
            catch
            {
                return -100;
            }
        }

        public async Task<int> AddRangeAsync(IEnumerable<Document> docs)
        {
            try
            {
                foreach (var t in docs)
                    t.intId = Interlocked.Increment(ref currentId);
                await coll.InsertManyAsync(docs);
                return docs.Count();
            }
            catch (AggregateException)
            {
                return -1;
            }
            catch
            {
                return -100;
            }
        }
    }
}
