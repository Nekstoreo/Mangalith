import { Metadata } from "next";
import { generateMetadata as generateSEOMetadata } from "@/lib/seo";
import { generateSearchPageSEO } from "@/lib/seo-utils";

interface SearchPageProps {
  searchParams: { q?: string }
}

export async function generateMetadata({ searchParams }: SearchPageProps): Promise<Metadata> {
  return generateSEOMetadata(generateSearchPageSEO(searchParams.q));
}

export default function SearchPage({ searchParams }: SearchPageProps) {
  const query = searchParams.q;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-6">
        {query ? `Resultados para "${query}"` : 'Buscar Manga'}
      </h1>
      
      {query ? (
        <p className="text-muted-foreground mb-8">
          Mostrando resultados de búsqueda para &quot;{query}&quot;
        </p>
      ) : (
        <p className="text-muted-foreground mb-8">
          Busca en nuestra extensa biblioteca de manga por título, autor, género y más.
        </p>
      )}
      
      {/* TODO: Implement search functionality */}
      <div className="text-center py-16">
        <p className="text-lg text-muted-foreground">
          La funcionalidad de búsqueda estará disponible en la siguiente fase de desarrollo.
        </p>
      </div>
    </div>
  );
}