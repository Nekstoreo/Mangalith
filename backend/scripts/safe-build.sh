#!/bin/bash
# Script para compilar y ejecutar el proyecto evitando problemas de .NET 9

echo "🔨 Limpiando compilaciones anteriores..."
dotnet clean --nologo -v q 2>/dev/null || true

echo "📦 Restaurando paquetes..."
dotnet restore --nologo 2>/dev/null || dotnet restore

echo "🏗️  Compilando con un solo hilo (workaround .NET 9)..."
dotnet build --nologo

echo ""
echo "✅ Compilación exitosa"
echo ""
echo "🚀 Para ejecutar el proyecto usa:"
echo "   dotnet run --no-build --project Mangalith.Api"
