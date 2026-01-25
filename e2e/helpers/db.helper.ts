import { MongoClient, Db } from 'mongodb';

const MONGODB_URI = process.env.MONGODB_TEST_URI || 'mongodb://admin:testpassword@localhost:27018';
const DB_NAME = 'storyjudge_test';

let client: MongoClient | null = null;
let db: Db | null = null;

export async function getDb(): Promise<Db> {
  if (!client) {
    client = new MongoClient(MONGODB_URI);
    await client.connect();
    db = client.db(DB_NAME);
  }
  return db!;
}

export async function closeDb(): Promise<void> {
  if (client) {
    await client.close();
    client = null;
    db = null;
  }
}

export async function clearCollection(collectionName: string): Promise<void> {
  const database = await getDb();
  try {
    await database.collection(collectionName).deleteMany({});
  } catch {
    // Collection might not exist yet
  }
}

export async function clearAllCollections(): Promise<void> {
  const database = await getDb();
  const collections = await database.listCollections().toArray();
  for (const collection of collections) {
    await database.collection(collection.name).deleteMany({});
  }
}

export async function seedUser(user: {
  email: string;
  displayName: string;
  passwordHash: string;
}): Promise<string> {
  const database = await getDb();
  const result = await database.collection('users').insertOne({
    ...user,
    roleLevel: 'Junior',
    reputationScore: 0,
    createdAt: new Date(),
    updatedAt: new Date(),
  });
  return result.insertedId.toString();
}

export async function seedStory(story: {
  authorId: string;
  authorName: string;
  title: string;
  storyType: string;
  situation: string;
  task: string;
  action: string;
  result: string;
  visibility: string;
  status: string;
  shareToken?: string;
}): Promise<string> {
  const database = await getDb();
  const result = await database.collection('stories').insertOne({
    ...story,
    tags: [],
    coverage: [],
    reviewStats: {
      totalReviews: 0,
      overallScore: 0,
      categoryAverages: {},
    },
    createdAt: new Date(),
    updatedAt: new Date(),
  });
  return result.insertedId.toString();
}

export async function getStoryById(id: string): Promise<any> {
  const database = await getDb();
  const { ObjectId } = await import('mongodb');
  return database.collection('stories').findOne({ _id: new ObjectId(id) });
}

export async function getReviewsByStoryId(storyId: string): Promise<any[]> {
  const database = await getDb();
  return database.collection('reviews').find({ storyId }).toArray();
}
