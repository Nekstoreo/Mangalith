import { DataSource } from 'typeorm';
import * as bcrypt from 'bcrypt';
import { User, UserRole, UserStatus } from '@/users/entities/user.entity';
import { Manga, MangaStatus, MangaType } from '@/manga/entities/manga.entity';
import { Chapter, ChapterStatus } from '@/chapter/entities/chapter.entity';
import { File } from '@/file/entities/file.entity';

async function seed() {
  console.log('🌱 Starting database seeding...');

  // Crear DataSource con todas las entidades necesarias
  const AppDataSource = new DataSource({
    type: 'postgres',
    url:
      process.env.DATABASE_URL ||
      'postgresql://mangalith_user:changeme123@localhost:5432/mangalith_dev',
    entities: [User, Manga, Chapter, File],
  });

  await AppDataSource.initialize();
  console.log('📦 Database connection established');

  const entityManager = AppDataSource.manager;

  try {
    // Limpiar datos existentes usando queries SQL
    console.log('🧹 Clearing existing data...');

    // Primero eliminar archivos (por las foreign keys)
    await entityManager.deleteAll(File);
    await entityManager.deleteAll(Chapter);
    await entityManager.deleteAll(Manga);
    await entityManager.deleteAll(User);

    // Crear usuarios de prueba
    console.log('👤 Creating test users...');
    const hashedPassword = await bcrypt.hash('password123', 10);

    const adminUser = entityManager.create(User, {
      email: 'admin@mangalith.com',
      username: 'admin',
      password: hashedPassword,
      role: UserRole.ADMIN,
      status: UserStatus.ACTIVE,
      firstName: 'Admin',
      lastName: 'User',
      displayName: 'Administrator',
      emailVerified: true,
    });

    const testUser = entityManager.create(User, {
      email: 'user@mangalith.com',
      username: 'testuser',
      password: hashedPassword,
      role: UserRole.USER,
      status: UserStatus.ACTIVE,
      firstName: 'Test',
      lastName: 'User',
      displayName: 'Test User',
      emailVerified: true,
    });

    await entityManager.save([adminUser, testUser]);
    console.log('✅ Created 2 test users');

    // Crear mangas de prueba
    console.log('📚 Creating test mangas...');

    const mangasData = [
      {
        title: 'One Piece',
        originalTitle: 'ワンピース',
        description:
          'La gran aventura de Luffy y su tripulación en busca del One Piece.',
        status: MangaStatus.ONGOING,
        type: MangaType.MANGA,
        author: 'Eiichiro Oda',
        artist: 'Eiichiro Oda',
        yearPublished: 1997,
        totalChapters: 1100,
        genres: [
          'Action',
          'Adventure',
          'Comedy',
          'Drama',
          'Fantasy',
          'Shounen',
        ],
        tags: ['Pirates', 'Treasure', 'Friendship', 'Power'],
        rating: 9.2,
        ratingCount: 15420,
        viewCount: 5000000,
        isPublic: true,
        isFeatured: true,
        user: adminUser,
      },
      {
        title: 'Attack on Titan',
        originalTitle: '進撃の巨人',
        description:
          'La humanidad lucha por sobrevivir contra gigantes devoradores de hombres.',
        status: MangaStatus.COMPLETED,
        type: MangaType.MANGA,
        author: 'Hajime Isayama',
        artist: 'Hajime Isayama',
        yearPublished: 2009,
        totalChapters: 139,
        genres: ['Action', 'Drama', 'Fantasy', 'Horror', 'Mystery', 'Shounen'],
        tags: ['Titans', 'Survival', 'War', 'Military'],
        rating: 9.0,
        ratingCount: 12850,
        viewCount: 4200000,
        isPublic: true,
        isFeatured: true,
        user: adminUser,
      },
      {
        title: 'Death Note',
        originalTitle: 'デスノート',
        description:
          'Un estudiante encuentra un cuaderno que le permite matar a cualquiera escribiendo su nombre.',
        status: MangaStatus.COMPLETED,
        type: MangaType.MANGA,
        author: 'Tsugumi Ohba',
        artist: 'Takeshi Obata',
        yearPublished: 2003,
        totalChapters: 108,
        genres: [
          'Mystery',
          'Psychological',
          'Supernatural',
          'Thriller',
          'Shounen',
        ],
        tags: ['Death', 'Genius', 'Psychological', 'Supernatural'],
        rating: 8.8,
        ratingCount: 11200,
        viewCount: 3800000,
        isPublic: true,
        isFeatured: false,
        user: testUser,
      },
      {
        title: 'Demon Slayer',
        originalTitle: '鬼滅の刃',
        description:
          'Un joven lucha contra demonios para salvar a su hermana convertida en demonio.',
        status: MangaStatus.ONGOING,
        type: MangaType.MANGA,
        author: 'Koyoharu Gotouge',
        artist: 'Koyoharu Gotouge',
        yearPublished: 2016,
        totalChapters: 205,
        genres: ['Action', 'Historical', 'Shounen', 'Supernatural'],
        tags: ['Demons', 'Historical', 'Swordsmanship', 'Family'],
        rating: 8.7,
        ratingCount: 9800,
        viewCount: 3500000,
        isPublic: true,
        isFeatured: true,
        user: adminUser,
      },
      {
        title: 'My Hero Academia',
        originalTitle: '僕のヒーローアカデミア',
        description:
          'En un mundo donde la mayoría de la población tiene superpoderes, un chico sin poderes quiere ser héroe.',
        status: MangaStatus.ONGOING,
        type: MangaType.MANGA,
        author: 'Kohei Horikoshi',
        artist: 'Kohei Horikoshi',
        yearPublished: 2014,
        totalChapters: 400,
        genres: ['Action', 'Comedy', 'School', 'Shounen', 'Super Power'],
        tags: ['Heroes', 'School', 'Superpowers', 'Training'],
        rating: 8.5,
        ratingCount: 15200,
        viewCount: 4800000,
        isPublic: true,
        isFeatured: false,
        user: testUser,
      },
    ];

    const savedMangas = await entityManager.save(Manga, mangasData);
    console.log('✅ Created 5 test mangas');

    // Crear algunos capítulos de ejemplo para los primeros mangas
    console.log('📖 Creating sample chapters...');

    const chaptersData: Partial<Chapter>[] = [];

    // One Piece - primeros 10 capítulos
    const onePiece = savedMangas.find((m) => m.title === 'One Piece')!;
    for (let i = 1; i <= 10; i++) {
      chaptersData.push({
        chapterNumber: i,
        title: `Capítulo ${i}`,
        description: `Capítulo ${i} de One Piece`,
        status: ChapterStatus.READY,
        pageCount: Math.floor(Math.random() * 20) + 15, // 15-35 páginas
        fileSizeBytes: Math.floor(Math.random() * 5000000) + 1000000, // 1-6MB
        viewCount: Math.floor(Math.random() * 10000) + 1000,
        readCount: Math.floor(Math.random() * 5000) + 500,
        isPublic: true,
        publishedAt: new Date(),
        manga: onePiece,
      });
    }

    // Attack on Titan - primeros 5 capítulos
    const attackOnTitan = savedMangas.find(
      (m) => m.title === 'Attack on Titan',
    )!;
    for (let i = 1; i <= 5; i++) {
      chaptersData.push({
        chapterNumber: i,
        title: `Capítulo ${i}`,
        description: `Capítulo ${i} de Attack on Titan`,
        status: ChapterStatus.READY,
        pageCount: Math.floor(Math.random() * 15) + 20, // 20-35 páginas
        fileSizeBytes: Math.floor(Math.random() * 4000000) + 800000, // 0.8-4.8MB
        viewCount: Math.floor(Math.random() * 8000) + 800,
        readCount: Math.floor(Math.random() * 4000) + 400,
        isPublic: true,
        publishedAt: new Date(),
        manga: attackOnTitan,
      });
    }

    await entityManager.save(Chapter, chaptersData);
    console.log('✅ Created 15 sample chapters');

    console.log('🎉 Database seeding completed successfully!');
    console.log('\n📊 Summary:');
    console.log('- 2 users created');
    console.log('- 5 mangas created');
    console.log('- 15 chapters created');
    console.log('\n🔐 Test credentials:');
    console.log('Admin: admin@mangalith.com / password123');
    console.log('User: user@mangalith.com / password123');
  } catch (error) {
    console.error('❌ Error during seeding:', error);
    throw error;
  } finally {
    await AppDataSource.destroy();
  }
}

// Ejecutar seeding si se llama directamente
if (require.main === module) {
  seed()
    .then(() => {
      console.log('✅ Seeding finished');
      process.exit(0);
    })
    .catch((error) => {
      console.error('❌ Seeding failed:', error);
      process.exit(1);
    });
}

export { seed };
