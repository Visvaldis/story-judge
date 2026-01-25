db = db.getSiblingDB('storyjudge');

db.createUser({
  user: 'storyjudge',
  pwd: 'storyjudge_password',
  roles: [
    {
      role: 'readWrite',
      db: 'storyjudge'
    }
  ]
});

// Create collections
db.createCollection('users');
db.createCollection('stories');
db.createCollection('reviews');
db.createCollection('reports');

// Create indexes
db.users.createIndex({ email: 1 }, { unique: true });
db.users.createIndex({ username: 1 }, { unique: true, sparse: true });

db.stories.createIndex({ authorId: 1 });
db.stories.createIndex({ visibility: 1, publishedAt: -1 });
db.stories.createIndex({ shareToken: 1 }, { sparse: true });
db.stories.createIndex({ 'reviewStats.overallScore': -1 });
db.stories.createIndex({ tags: 1 });

db.reviews.createIndex({ storyId: 1 });
db.reviews.createIndex({ reviewerId: 1 });
db.reviews.createIndex({ storyId: 1, reviewerId: 1 }, { unique: true });

db.reports.createIndex({ status: 1 });
db.reports.createIndex({ createdAt: -1 });
