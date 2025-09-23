import { Button } from "@/components/ui/button";
import { Card, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { BookOpen, Upload, Users, Zap } from "lucide-react";
import Link from "next/link";

export default function Home() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-background to-muted/20">
      {/* Header */}
      <header className="border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="container mx-auto px-4 py-4 flex items-center justify-between">
          <div className="flex items-center space-x-2">
            <BookOpen className="h-8 w-8 text-primary" />
            <span className="text-2xl font-bold">Mangalith</span>
          </div>
          <div className="flex items-center space-x-4">
            <Link href="/auth/login">
              <Button variant="ghost">Iniciar Sesión</Button>
            </Link>
            <Link href="/auth/register">
              <Button>Registrarse</Button>
            </Link>
          </div>
        </div>
      </header>

      {/* Hero Section */}
      <main className="container mx-auto px-4 py-16">
        <div className="text-center mb-16">
          <h1 className="text-5xl font-bold mb-6 bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
            Tu Biblioteca de Manga Digital
          </h1>
          <p className="text-xl text-muted-foreground mb-8 max-w-2xl mx-auto">
            Organiza, lee y administra tu colección de manga digital de forma sencilla y elegante.
            Sube tus archivos, crea tu biblioteca personal y disfruta de la mejor experiencia de lectura.
          </p>
          <div className="flex items-center justify-center space-x-4">
            <Link href="/auth/register">
              <Button size="lg" className="text-lg px-8">
                <Upload className="mr-2 h-5 w-5" />
                Comenzar Ahora
              </Button>
            </Link>
            <Link href="/library">
              <Button variant="outline" size="lg" className="text-lg px-8">
                <BookOpen className="mr-2 h-5 w-5" />
                Ver Biblioteca
              </Button>
            </Link>
          </div>
        </div>

        {/* Features Grid */}
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6 mb-16">
          <Card>
            <CardHeader>
              <div className="h-12 w-12 bg-primary/10 rounded-lg flex items-center justify-center mb-4">
                <Upload className="h-6 w-6 text-primary" />
              </div>
              <CardTitle>Subida Fácil</CardTitle>
              <CardDescription>
                Sube tus archivos manga en formato CBZ, CBR o ZIP de forma rápida y segura.
              </CardDescription>
            </CardHeader>
          </Card>

          <Card>
            <CardHeader>
              <div className="h-12 w-12 bg-primary/10 rounded-lg flex items-center justify-center mb-4">
                <BookOpen className="h-6 w-6 text-primary" />
              </div>
              <CardTitle>Biblioteca Personal</CardTitle>
              <CardDescription>
                Organiza tu colección con etiquetas, categorías y listas personalizadas.
              </CardDescription>
            </CardHeader>
          </Card>

          <Card>
            <CardHeader>
              <div className="h-12 w-12 bg-primary/10 rounded-lg flex items-center justify-center mb-4">
                <Zap className="h-6 w-6 text-primary" />
              </div>
              <CardTitle>Lectura Fluida</CardTitle>
              <CardDescription>
                Disfruta de una experiencia de lectura optimizada con múltiples modos de visualización.
              </CardDescription>
            </CardHeader>
          </Card>

          <Card>
            <CardHeader>
              <div className="h-12 w-12 bg-primary/10 rounded-lg flex items-center justify-center mb-4">
                <Users className="h-6 w-6 text-primary" />
              </div>
              <CardTitle>Multiplataforma</CardTitle>
              <CardDescription>
                Accede a tu biblioteca desde cualquier dispositivo con sincronización automática.
              </CardDescription>
            </CardHeader>
          </Card>

          <Card>
            <CardHeader>
              <div className="h-12 w-12 bg-primary/10 rounded-lg flex items-center justify-center mb-4">
                <BookOpen className="h-6 w-6 text-primary" />
              </div>
              <CardTitle>Progreso Inteligente</CardTitle>
              <CardDescription>
                Mantén un seguimiento automático de tu progreso de lectura en todas tus series.
              </CardDescription>
            </CardHeader>
          </Card>

          <Card>
            <CardHeader>
              <div className="h-12 w-12 bg-primary/10 rounded-lg flex items-center justify-center mb-4">
                <Upload className="h-6 w-6 text-primary" />
              </div>
              <CardTitle>Gestión Avanzada</CardTitle>
              <CardDescription>
                Herramientas poderosas para organizar y gestionar grandes colecciones de manga.
              </CardDescription>
            </CardHeader>
          </Card>
        </div>

        {/* CTA Section */}
        <div className="text-center">
          <div className="bg-muted/50 rounded-lg p-8 max-w-2xl mx-auto">
            <h2 className="text-2xl font-bold mb-4">¿Listo para comenzar?</h2>
            <p className="text-muted-foreground mb-6">
              Únete a miles de lectores que ya organizan su colección de manga con Mangalith.
              Es gratuito, de código abierto y está diseñado para la mejor experiencia de lectura.
            </p>
            <Link href="/auth/register">
              <Button size="lg" className="text-lg px-12">
                Crear Cuenta Gratuita
              </Button>
            </Link>
          </div>
        </div>
      </main>

      {/* Footer */}
      <footer className="border-t mt-16">
        <div className="container mx-auto px-4 py-8">
          <div className="text-center text-muted-foreground">
            <div className="flex items-center justify-center space-x-2 mb-4">
              <BookOpen className="h-5 w-5" />
              <span className="font-semibold">Mangalith</span>
            </div>
            <p>Lector de manga de código abierto • Hecho con ❤️ para la comunidad</p>
          </div>
        </div>
      </footer>
    </div>
  );
}
