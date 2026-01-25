import { MongoClient } from 'mongodb';

const MONGODB_URI = process.env.MONGODB_TEST_URI || 'mongodb://admin:testpassword@localhost:27018';
const DB_NAME = 'storyjudge_test';

async function globalTeardown() {
  console.log('🧹 Global teardown: Cleaning up test database...');

  let client: MongoClient | null = null;

  try {
    client = new MongoClient(MONGODB_URI);
    await client.connect();

    const db = client.db(DB_NAME);

    // Drop all collections
    const collections = await db.listCollections().toArray();
    for (const collection of collections) {
      await db.dropCollection(collection.name);
    }

    console.log('✅ Test database cleaned up successfully');
  } catch (error) {
    console.warn('⚠️  Could not clean up test database:', error);
  } finally {
    if (client) {
      await client.close();
    }
  }
}

export default globalTeardown;
