import { MigrationInterface, QueryRunner } from 'typeorm';

export class InitialMigration1758664961843 implements MigrationInterface {
  name = 'InitialMigration1758664961843';

  public async up(queryRunner: QueryRunner): Promise<void> {
    await queryRunner.query(
      `CREATE TYPE "public"."files_file_type_enum" AS ENUM('image', 'compressed', 'thumbnail')`,
    );
    await queryRunner.query(
      `CREATE TYPE "public"."files_format_enum" AS ENUM('jpg', 'jpeg', 'png', 'webp', 'cbz', 'cbr', 'zip', 'rar')`,
    );
    await queryRunner.query(
      `CREATE TYPE "public"."files_status_enum" AS ENUM('uploaded', 'processed', 'error', 'deleted')`,
    );
    await queryRunner.query(
      `CREATE TABLE "files" ("id" uuid NOT NULL DEFAULT uuid_generate_v4(), "filename" character varying(255) NOT NULL, "original_filename" character varying(255) NOT NULL, "path" character varying(500) NOT NULL, "public_url" character varying(500), "file_type" "public"."files_file_type_enum" NOT NULL, "format" "public"."files_format_enum" NOT NULL, "mime_type" character varying(100) NOT NULL, "file_size_bytes" bigint NOT NULL, "width_pixels" integer, "height_pixels" integer, "page_number" integer, "is_cover" boolean NOT NULL DEFAULT false, "quality_level" integer NOT NULL DEFAULT '100', "status" "public"."files_status_enum" NOT NULL DEFAULT 'uploaded', "error_message" text, "processing_attempts" integer NOT NULL DEFAULT '0', "created_at" TIMESTAMP NOT NULL DEFAULT now(), "updated_at" TIMESTAMP NOT NULL DEFAULT now(), "uploaded_at" TIMESTAMP NOT NULL, "processed_at" TIMESTAMP, "chapter_id" uuid, CONSTRAINT "PK_6c16b9093a142e0e7613b04a3d9" PRIMARY KEY ("id"))`,
    );
    await queryRunner.query(
      `CREATE INDEX "IDX_866212928289ec1112edd04ebf" ON "files" ("status") `,
    );
    await queryRunner.query(
      `CREATE INDEX "IDX_01a0503a262971ce7d2e625905" ON "files" ("chapter_id", "file_type") `,
    );
    await queryRunner.query(
      `CREATE TYPE "public"."chapters_status_enum" AS ENUM('uploaded', 'processing', 'ready', 'error')`,
    );
    await queryRunner.query(
      `CREATE TABLE "chapters" ("id" uuid NOT NULL DEFAULT uuid_generate_v4(), "chapter_number" numeric(5,1) NOT NULL, "volume_number" numeric(5,1), "title" character varying(255), "description" text, "status" "public"."chapters_status_enum" NOT NULL DEFAULT 'uploaded', "page_count" integer NOT NULL DEFAULT '0', "file_size_bytes" bigint NOT NULL DEFAULT '0', "original_file_name" character varying(255), "processing_started_at" TIMESTAMP, "processing_completed_at" TIMESTAMP, "error_message" text, "view_count" integer NOT NULL DEFAULT '0', "read_count" integer NOT NULL DEFAULT '0', "is_public" boolean NOT NULL DEFAULT true, "created_at" TIMESTAMP NOT NULL DEFAULT now(), "updated_at" TIMESTAMP NOT NULL DEFAULT now(), "published_at" TIMESTAMP, "manga_id" uuid, CONSTRAINT "PK_a2bbdbb4bdc786fe0cb0fcfc4a0" PRIMARY KEY ("id"))`,
    );
    await queryRunner.query(
      `CREATE INDEX "IDX_83d128d85611b9f84b7c40460f" ON "chapters" ("status") `,
    );
    await queryRunner.query(
      `CREATE INDEX "IDX_0c93ca5689c3c99aa2faf0b877" ON "chapters" ("manga_id", "chapter_number") `,
    );
    await queryRunner.query(
      `CREATE TYPE "public"."mangas_status_enum" AS ENUM('ongoing', 'completed', 'cancelled', 'hiatus')`,
    );
    await queryRunner.query(
      `CREATE TYPE "public"."mangas_type_enum" AS ENUM('manga', 'manhwa', 'manhua', 'comic', 'novel')`,
    );
    await queryRunner.query(
      `CREATE TABLE "mangas" ("id" uuid NOT NULL DEFAULT uuid_generate_v4(), "title" character varying(255) NOT NULL, "original_title" character varying(255), "description" text, "status" "public"."mangas_status_enum" NOT NULL DEFAULT 'ongoing', "type" "public"."mangas_type_enum" NOT NULL DEFAULT 'manga', "cover_image_url" character varying(500), "banner_image_url" character varying(500), "author" character varying(100), "artist" character varying(100), "year_published" integer, "total_chapters" integer NOT NULL DEFAULT '0', "total_volumes" integer NOT NULL DEFAULT '0', "genres" text, "tags" text, "rating" numeric(3,2) NOT NULL DEFAULT '0', "rating_count" integer NOT NULL DEFAULT '0', "view_count" integer NOT NULL DEFAULT '0', "is_public" boolean NOT NULL DEFAULT true, "is_featured" boolean NOT NULL DEFAULT false, "created_at" TIMESTAMP NOT NULL DEFAULT now(), "updated_at" TIMESTAMP NOT NULL DEFAULT now(), "user_id" uuid, CONSTRAINT "PK_caf32b0b7ecd79d3bbc459b989b" PRIMARY KEY ("id"))`,
    );
    await queryRunner.query(
      `CREATE INDEX "IDX_b6b7cdf1c0cfdca97060f667df" ON "mangas" ("user_id") `,
    );
    await queryRunner.query(
      `CREATE INDEX "IDX_cb0eb74c9bf86360b31d17cfda" ON "mangas" ("title", "status") `,
    );
    await queryRunner.query(
      `CREATE TYPE "public"."users_role_enum" AS ENUM('user', 'admin')`,
    );
    await queryRunner.query(
      `CREATE TYPE "public"."users_status_enum" AS ENUM('active', 'inactive', 'banned')`,
    );
    await queryRunner.query(
      `CREATE TABLE "users" ("id" uuid NOT NULL DEFAULT uuid_generate_v4(), "email" character varying(100) NOT NULL, "username" character varying(100) NOT NULL, "password" character varying(255) NOT NULL, "role" "public"."users_role_enum" NOT NULL DEFAULT 'user', "status" "public"."users_status_enum" NOT NULL DEFAULT 'active', "first_name" character varying(50), "last_name" character varying(50), "display_name" character varying(100), "avatar_url" character varying(500), "bio" text, "email_verified" boolean NOT NULL DEFAULT false, "last_login_at" TIMESTAMP, "created_at" TIMESTAMP NOT NULL DEFAULT now(), "updated_at" TIMESTAMP NOT NULL DEFAULT now(), CONSTRAINT "UQ_97672ac88f789774dd47f7c8be3" UNIQUE ("email"), CONSTRAINT "PK_a3ffb1c0c8416b9fc6f907b7433" PRIMARY KEY ("id"))`,
    );
    await queryRunner.query(
      `ALTER TABLE "files" ADD CONSTRAINT "FK_d7e0ec2e843e28b38ebbe43544a" FOREIGN KEY ("chapter_id") REFERENCES "chapters"("id") ON DELETE CASCADE ON UPDATE NO ACTION`,
    );
    await queryRunner.query(
      `ALTER TABLE "chapters" ADD CONSTRAINT "FK_021f7529e5e2d1b73f19ab3f43e" FOREIGN KEY ("manga_id") REFERENCES "mangas"("id") ON DELETE CASCADE ON UPDATE NO ACTION`,
    );
    await queryRunner.query(
      `ALTER TABLE "mangas" ADD CONSTRAINT "FK_b6b7cdf1c0cfdca97060f667df3" FOREIGN KEY ("user_id") REFERENCES "users"("id") ON DELETE CASCADE ON UPDATE NO ACTION`,
    );
  }

  public async down(queryRunner: QueryRunner): Promise<void> {
    await queryRunner.query(
      `ALTER TABLE "mangas" DROP CONSTRAINT "FK_b6b7cdf1c0cfdca97060f667df3"`,
    );
    await queryRunner.query(
      `ALTER TABLE "chapters" DROP CONSTRAINT "FK_021f7529e5e2d1b73f19ab3f43e"`,
    );
    await queryRunner.query(
      `ALTER TABLE "files" DROP CONSTRAINT "FK_d7e0ec2e843e28b38ebbe43544a"`,
    );
    await queryRunner.query(`DROP TABLE "users"`);
    await queryRunner.query(`DROP TYPE "public"."users_status_enum"`);
    await queryRunner.query(`DROP TYPE "public"."users_role_enum"`);
    await queryRunner.query(
      `DROP INDEX "public"."IDX_cb0eb74c9bf86360b31d17cfda"`,
    );
    await queryRunner.query(
      `DROP INDEX "public"."IDX_b6b7cdf1c0cfdca97060f667df"`,
    );
    await queryRunner.query(`DROP TABLE "mangas"`);
    await queryRunner.query(`DROP TYPE "public"."mangas_type_enum"`);
    await queryRunner.query(`DROP TYPE "public"."mangas_status_enum"`);
    await queryRunner.query(
      `DROP INDEX "public"."IDX_0c93ca5689c3c99aa2faf0b877"`,
    );
    await queryRunner.query(
      `DROP INDEX "public"."IDX_83d128d85611b9f84b7c40460f"`,
    );
    await queryRunner.query(`DROP TABLE "chapters"`);
    await queryRunner.query(`DROP TYPE "public"."chapters_status_enum"`);
    await queryRunner.query(
      `DROP INDEX "public"."IDX_01a0503a262971ce7d2e625905"`,
    );
    await queryRunner.query(
      `DROP INDEX "public"."IDX_866212928289ec1112edd04ebf"`,
    );
    await queryRunner.query(`DROP TABLE "files"`);
    await queryRunner.query(`DROP TYPE "public"."files_status_enum"`);
    await queryRunner.query(`DROP TYPE "public"."files_format_enum"`);
    await queryRunner.query(`DROP TYPE "public"."files_file_type_enum"`);
  }
}
