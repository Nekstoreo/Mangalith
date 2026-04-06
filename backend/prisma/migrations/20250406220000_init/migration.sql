-- CreateEnum
CREATE TYPE "user_role" AS ENUM ('ADMIN', 'MODERATOR', 'READER');

-- CreateTable Series
CREATE TABLE "series" (
    "id" BIGSERIAL NOT NULL,
    "title" VARCHAR(255) NOT NULL,
    "description" TEXT,
    "author" VARCHAR(255),
    "cover_image_url" VARCHAR(500),
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "series_pkey" PRIMARY KEY ("id")
);

-- CreateTable Chapters
CREATE TABLE "chapters" (
    "id" BIGSERIAL NOT NULL,
    "series_id" BIGINT NOT NULL,
    "chapter_number" INTEGER NOT NULL,
    "title" VARCHAR(255),
    "upload_date" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "chapters_pkey" PRIMARY KEY ("id")
);

-- CreateTable Pages
CREATE TABLE "pages" (
    "id" BIGSERIAL NOT NULL,
    "chapter_id" BIGINT NOT NULL,
    "page_number" INTEGER NOT NULL,
    "image_url" VARCHAR(500) NOT NULL,
    "image_width" INTEGER,
    "image_height" INTEGER,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "pages_pkey" PRIMARY KEY ("id")
);

-- CreateTable Users
CREATE TABLE "users" (
    "id" BIGSERIAL NOT NULL,
    "email" VARCHAR(255) NOT NULL,
    "username" VARCHAR(50) NOT NULL,
    "password_hash" TEXT NOT NULL,
    "role" "user_role" NOT NULL DEFAULT 'READER',
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "users_pkey" PRIMARY KEY ("id")
);

-- CreateTable ReadingProgress
CREATE TABLE "reading_progress" (
    "id" BIGSERIAL NOT NULL,
    "user_id" BIGINT NOT NULL,
    "chapter_id" BIGINT NOT NULL,
    "page_number" INTEGER NOT NULL,
    "last_read_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "reading_progress_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "series_title_key" ON "series"("title");

-- CreateIndex
CREATE INDEX "idx_series_title" ON "series"("title");

-- CreateIndex
CREATE UNIQUE INDEX "chapters_series_id_chapter_number_key" ON "chapters"("series_id", "chapter_number");

-- CreateIndex
CREATE INDEX "idx_chapters_series_id" ON "chapters"("series_id");

-- CreateIndex
CREATE UNIQUE INDEX "pages_chapter_id_page_number_key" ON "pages"("chapter_id", "page_number");

-- CreateIndex
CREATE INDEX "idx_pages_chapter_id" ON "pages"("chapter_id");

-- CreateIndex
CREATE UNIQUE INDEX "users_email_key" ON "users"("email");

-- CreateIndex
CREATE UNIQUE INDEX "users_username_key" ON "users"("username");

-- CreateIndex
CREATE UNIQUE INDEX "reading_progress_user_id_chapter_id_key" ON "reading_progress"("user_id", "chapter_id");

-- CreateIndex
CREATE INDEX "idx_reading_progress_user_id" ON "reading_progress"("user_id");

-- CreateIndex
CREATE INDEX "idx_reading_progress_chapter_id" ON "reading_progress"("chapter_id");

-- AddForeignKey
ALTER TABLE "chapters" ADD CONSTRAINT "chapters_series_id_fkey" FOREIGN KEY ("series_id") REFERENCES "series"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pages" ADD CONSTRAINT "pages_chapter_id_fkey" FOREIGN KEY ("chapter_id") REFERENCES "chapters"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "reading_progress" ADD CONSTRAINT "reading_progress_user_id_fkey" FOREIGN KEY ("user_id") REFERENCES "users"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "reading_progress" ADD CONSTRAINT "reading_progress_chapter_id_fkey" FOREIGN KEY ("chapter_id") REFERENCES "chapters"("id") ON DELETE CASCADE ON UPDATE CASCADE;
