import { Metadata } from "next";
import { generateMetadata as generateSEOMetadata } from "@/lib/seo";
import { generateLibraryPageSEO } from "@/lib/seo-utils";

export const metadata: Metadata = generateSEOMetadata(generateLibraryPageSEO());

export default function LibraryPage() {
  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-6">Biblioteca de Manga</h1>
      <p className="text-muted-foreground mb-8">
        Explora nuestra extensa biblioteca de manga digital. Encuentra tus series favoritas y descubre nuevos títulos.
      </p>
      
      {/* TODO: Implement library grid and filters */}
      <div className="text-center py-16">
        <p className="text-lg text-muted-foreground">
          La biblioteca estará disponible en la siguiente fase de desarrollo.
        </p>
      </div>
    </div>
  );
}