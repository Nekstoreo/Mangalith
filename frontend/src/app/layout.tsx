import type { Metadata, Viewport } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { generateMetadata as generateSEOMetadata, generateStructuredData } from "@/lib/seo";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = generateSEOMetadata({
  title: "Mangalith - Lector de Manga de Código Abierto",
  description: "Tu biblioteca personal de manga digital. Organiza, lee y administra tu colección de manga de forma sencilla y elegante. Plataforma de código abierto para lectores y grupos de scanlation.",
  keywords: [
    "manga", "lector manga", "biblioteca digital", "manga online", 
    "scanlation", "CBZ", "CBR", "manga reader", "código abierto",
    "biblioteca personal", "manga digital", "lector web"
  ],
  type: "website"
});

export const viewport: Viewport = {
  width: 'device-width',
  initialScale: 1,
  maximumScale: 5,
  userScalable: true,
  themeColor: [
    { media: '(prefers-color-scheme: light)', color: '#ffffff' },
    { media: '(prefers-color-scheme: dark)', color: '#0a0a0a' }
  ],
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const structuredData = generateStructuredData({
    title: "Mangalith - Lector de Manga de Código Abierto",
    description: "Tu biblioteca personal de manga digital. Organiza, lee y administra tu colección de manga de forma sencilla y elegante.",
    url: "/"
  });

  return (
    <html lang="es">
      <head>
        <script
          type="application/ld+json"
          dangerouslySetInnerHTML={{ __html: JSON.stringify(structuredData) }}
        />
      </head>
      <body
        className={`${geistSans.variable} ${geistMono.variable} antialiased`}
      >
        {children}
      </body>
    </html>
  );
}
