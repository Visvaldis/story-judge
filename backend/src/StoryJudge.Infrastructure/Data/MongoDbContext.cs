using MongoDB.Driver;
using StoryJudge.Core.Models;

namespace StoryJudge.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        CreateIndexes();
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Story> Stories => _database.GetCollection<Story>("stories");
    public IMongoCollection<Review> Reviews => _database.GetCollection<Review>("reviews");
    public IMongoCollection<Report> Reports => _database.GetCollection<Report>("reports");

    private void CreateIndexes()
    {
        // User indexes
        var userIndexes = new List<CreateIndexModel<User>>
        {
            new(Builders<User>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true }),
            new(Builders<User>.IndexKeys.Ascending(u => u.Username),
                new CreateIndexOptions<User>
                {
                    Unique = true,
                    PartialFilterExpression = Builders<User>.Filter.Type(u => u.Username, MongoDB.Bson.BsonType.String)
                })
        };
        Users.Indexes.CreateMany(userIndexes);

        // Story indexes
        var storyIndexes = new List<CreateIndexModel<Story>>
        {
            new(Builders<Story>.IndexKeys.Ascending(s => s.AuthorId)),
            new(Builders<Story>.IndexKeys
                .Ascending(s => s.Visibility)
                .Descending(s => s.PublishedAt)),
            new(Builders<Story>.IndexKeys.Ascending(s => s.ShareToken),
                new CreateIndexOptions { Sparse = true }),
            new(Builders<Story>.IndexKeys.Descending(s => s.ReviewStats.OverallScore)),
            new(Builders<Story>.IndexKeys.Ascending(s => s.Tags))
        };
        Stories.Indexes.CreateMany(storyIndexes);

        // Review indexes
        var reviewIndexes = new List<CreateIndexModel<Review>>
        {
            new(Builders<Review>.IndexKeys.Ascending(r => r.StoryId)),
            new(Builders<Review>.IndexKeys.Ascending(r => r.ReviewerId)),
            new(Builders<Review>.IndexKeys
                .Ascending(r => r.StoryId)
                .Ascending(r => r.ReviewerId),
                new CreateIndexOptions { Unique = true })
        };
        Reviews.Indexes.CreateMany(reviewIndexes);

        // Report indexes
        var reportIndexes = new List<CreateIndexModel<Report>>
        {
            new(Builders<Report>.IndexKeys.Ascending(r => r.Status)),
            new(Builders<Report>.IndexKeys.Descending(r => r.CreatedAt))
        };
        Reports.Indexes.CreateMany(reportIndexes);
    }
}
