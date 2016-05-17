using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading;

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
            currentId = (int)coll.Aggregate(pipeline).First()[1]; //максимальный id
        }

        public int Add(Document doc)
        {
            try
            {
                doc.Id = Interlocked.Increment(ref currentId);
                coll.InsertOne(doc);
                return doc.Id;
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

        public IList<int> AddRange(IEnumerable<Document> docs)
        {
            try
            {
                foreach(var t in docs)
                    t.Id = Interlocked.Increment(ref currentId);
                coll.InsertMany(docs);
                return docs.Select(x => x.Id).ToList();
            }
            catch (AggregateException)
            {
                return new int[] { -1 };
            }
            catch
            {
                return new int[] { -100 };
            }
        }

        public Document Get(int id)
        {
            return coll.AsQueryable().First(x => x.extId == id);
        }

        public async Task<int> AddAsync(Document doc)
        {
            try
            {
                doc.Id = Interlocked.Increment(ref currentId);
                await coll.InsertOneAsync(doc);
                return doc.Id;
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

        public async Task<IList<int>> AddRangeAsync(IEnumerable<Document> docs)
        {
            try
            {
                foreach (var t in docs)
                    t.Id = Interlocked.Increment(ref currentId);
                await coll.InsertManyAsync(docs);
                return docs.Select(x => x.Id).ToList();
            }
            catch (AggregateException)
            {
                return new int[] { -1 };
            }
            catch
            {
                return new int[] { -100 };
            }
        }
    }
}
