import { MongoClient } from 'mongodb';

const MONGODB_URI = process.env.MONGODB_TEST_URI || 'mongodb://admin:testpassword@localhost:27018';
const DB_NAME = 'storyjudge_test';

async function globalSetup() {
  console.log('🔧 Global setup: Initializing test database...');

  let client: MongoClient | null = null;

  try {
    client = new MongoClient(MONGODB_URI);
    await client.connect();

    const db = client.db(DB_NAME);

    // Clear all collections
    const collections = await db.listCollections().toArray();
    for (const collection of collections) {
      await db.dropCollection(collection.name);
    }

    // Create indexes for better test performance
    await db.collection('users').createIndex({ email: 1 }, { unique: true });
    await db.collection('stories').createIndex({ authorId: 1 });
    await db.collection('stories').createIndex({ visibility: 1, status: 1 });
    await db.collection('stories').createIndex({ shareToken: 1 }, { sparse: true });
    await db.collection('reviews').createIndex({ storyId: 1 });
    await db.collection('reviews').createIndex({ reviewerId: 1 });

    console.log('✅ Test database initialized successfully');
  } catch (error) {
    console.warn('⚠️  Could not connect to test MongoDB. Tests may use development database.');
    console.warn('   Start test MongoDB with: docker-compose -f docker-compose.test.yml up -d');
  } finally {
    if (client) {
      await client.close();
    }
  }
}

export default globalSetup;
