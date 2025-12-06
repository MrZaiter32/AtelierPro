#!/bin/bash

# Script de inicio rápido para AtelierPro ERP
# Este script ejecuta la aplicación y abre el navegador

echo "🚀 Iniciando AtelierPro ERP..."
echo ""

# Navegar al directorio del proyecto
cd "$(dirname "$0")/AtelierPro"

# Verificar si .NET está instalado
if ! command -v dotnet &> /dev/null; then
    echo "❌ Error: .NET SDK no está instalado"
    echo "Por favor instala .NET 6.0 SDK desde: https://dotnet.microsoft.com/download"
    exit 1
fi

# Compilar el proyecto
echo "📦 Compilando proyecto..."
dotnet build --configuration Release

if [ $? -ne 0 ]; then
    echo "❌ Error en la compilación"
    exit 1
fi

echo "✅ Compilación exitosa"
echo ""
echo "🌐 Iniciando servidor web..."
echo "   - HTTPS: https://localhost:7071"
echo "   - HTTP:  http://localhost:5197"
echo ""
echo "Presiona Ctrl+C para detener el servidor"
echo ""

# Ejecutar la aplicación
dotnet run --configuration Release
