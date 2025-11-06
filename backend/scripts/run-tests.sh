#!/bin/bash

# Script para ejecutar todas las pruebas unitarias del proyecto Mangalith
# Autor: Sistema de pruebas automatizado
# Fecha: $(date)

set -e  # Salir si cualquier comando falla

echo "🧪 Iniciando ejecución de pruebas unitarias de Mangalith..."
echo "=================================================="

# Navegar al directorio del backend
cd "$(dirname "$0")/.."

# Verificar que estamos en el directorio correcto
if [ ! -f "Mangalith.sln" ]; then
    echo "❌ Error: No se encontró Mangalith.sln. Asegúrate de estar en el directorio backend."
    exit 1
fi

# Verificar que el proyecto de pruebas existe
if [ ! -f "Mangalith.Tests/Mangalith.Tests.csproj" ]; then
    echo "❌ Error: No se encontró el proyecto de pruebas Mangalith.Tests."
    exit 1
fi

echo "📁 Directorio de trabajo: $(pwd)"
echo "🔍 Verificando dependencias..."

# Restaurar paquetes NuGet
echo "📦 Restaurando paquetes NuGet..."
dotnet restore

# Compilar la solución
echo "🔨 Compilando la solución..."
dotnet build --no-restore --configuration Release

# Ejecutar pruebas con diferentes niveles de detalle
echo ""
echo "🚀 Ejecutando pruebas unitarias..."
echo "=================================="

# Ejecutar pruebas con reporte detallado
dotnet test Mangalith.Tests/Mangalith.Tests.csproj \
    --no-build \
    --configuration Release \
    --verbosity minimal \
    --logger "console;verbosity=detailed" \
    --collect:"XPlat Code Coverage" \
    --results-directory ./TestResults

# Verificar el resultado
if [ $? -eq 0 ]; then
    echo ""
    echo "✅ ¡Todas las pruebas pasaron exitosamente!"
    echo "📊 Resultados guardados en ./TestResults"
    
    # Mostrar resumen de archivos de prueba
    echo ""
    echo "📋 Resumen de archivos de prueba:"
    echo "================================="
    find Mangalith.Tests -name "*.cs" -not -path "*/bin/*" -not -path "*/obj/*" | sort
    
    # Contar pruebas por categoría
    echo ""
    echo "📈 Estadísticas de pruebas:"
    echo "=========================="
    echo "🏗️  Entidades de dominio: $(find Mangalith.Tests/Domain -name "*Tests.cs" | wc -l) archivos"
    echo "⚙️  Servicios de aplicación: $(find Mangalith.Tests/Application/Services -name "*Tests.cs" | wc -l) archivos"
    echo "🔍 Validadores: $(find Mangalith.Tests/Application/Validators -name "*Tests.cs" | wc -l) archivos"
    echo "🌐 Controladores API: $(find Mangalith.Tests/Api -name "*Tests.cs" | wc -l) archivos"
    echo "🔗 Pruebas de integración: $(find Mangalith.Tests/Integration -name "*Tests.cs" | wc -l) archivos"
    
else
    echo ""
    echo "❌ Algunas pruebas fallaron. Revisa los detalles arriba."
    echo "💡 Consejos para solucionar problemas:"
    echo "   - Verifica que todas las dependencias estén instaladas"
    echo "   - Asegúrate de que no hay errores de compilación"
    echo "   - Revisa los mensajes de error específicos de las pruebas"
    exit 1
fi

echo ""
echo "🎉 Ejecución de pruebas completada."
echo "📝 Para más detalles, revisa los archivos en ./TestResults"