# 📝 Comandos Útiles - AtelierPro ERP

## 🚀 Inicio Rápido

```bash
# Opción 1: Usando el script
./start.sh

# Opción 2: Manual
cd AtelierPro
dotnet run
```

---

## 🔨 Compilación

```bash
# Compilación en modo Debug
dotnet build

# Compilación en modo Release
dotnet build --configuration Release

# Compilación con información detallada
dotnet build --verbosity detailed
```

---

## ✅ Testing

```bash
# Ejecutar todos los tests
cd AtelierPro.Tests
dotnet test

# Ejecutar tests con detalle
dotnet test --logger "console;verbosity=detailed"

# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar un test específico
dotnet test --filter "FullyQualifiedName~ReglaServiceTests"
```

---

## 🗄️ Base de Datos

```bash
# Eliminar base de datos existente
rm AtelierPro/atelierpro.db

# Crear nueva base de datos (automático al ejecutar)
dotnet run

# Ver estructura de la base de datos (SQLite)
sqlite3 AtelierPro/atelierpro.db ".schema"

# Ver datos de una tabla
sqlite3 AtelierPro/atelierpro.db "SELECT * FROM Presupuestos;"
```

---

## 🔧 Desarrollo

```bash
# Restaurar paquetes NuGet
dotnet restore

# Limpiar compilaciones anteriores
dotnet clean

# Ver información del proyecto
dotnet list package

# Agregar un paquete
dotnet add package NombreDelPaquete

# Watch mode (recompilación automática)
dotnet watch run
```

---

## 🧪 Debugging

```bash
# Ejecutar en modo debug con logs detallados
DOTNET_ENVIRONMENT=Development dotnet run --verbosity normal

# Ver conexiones a DB
export LOGGING__LOGLEVEL__MICROSOFT.ENTITYFRAMEWORKCORE=Debug
dotnet run
```

---

## 📊 Análisis de Código

```bash
# Análisis de código estático
dotnet format

# Verificar estilo
dotnet format --verify-no-changes

# Mostrar warnings como errores
dotnet build /p:TreatWarningsAsErrors=true
```

---

## 🌐 API Testing (cURL)

```bash
# Obtener todos los clientes
curl -k https://localhost:7071/api/clientes

# Obtener cliente por ID
curl -k https://localhost:7071/api/clientes/{guid}

# Crear nuevo cliente
curl -k -X POST https://localhost:7071/api/clientes \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Test Cliente",
    "historial": "Nuevo",
    "preferencias": "Email",
    "nps": 85,
    "tasaRetencion": 0.9
  }'

# Obtener todos los presupuestos
curl -k https://localhost:7071/api/presupuestos

# Obtener presupuestos por estado (0=Borrador, 1=Aprobado, 2=Cerrado, 3=Facturado)
curl -k https://localhost:7071/api/presupuestos/estado/1
```

---

## 📦 Publicación

```bash
# Publicar para Linux
dotnet publish -c Release -r linux-x64 --self-contained

# Publicar para Windows
dotnet publish -c Release -r win-x64 --self-contained

# Publicar para macOS
dotnet publish -c Release -r osx-x64 --self-contained

# Crear paquete portable
dotnet publish -c Release
```

---

## 🐳 Docker (Futuro)

```bash
# Construir imagen
docker build -t atelierpro:latest .

# Ejecutar contenedor
docker run -p 8080:80 atelierpro:latest

# Docker Compose
docker-compose up -d
```

---

## 🔍 Inspección de Assemblies

```bash
# Ver metadata del assembly
dotnet AtelierPro/bin/Debug/net6.0/AtelierPro.dll

# Listar dependencias
dotnet list reference

# Ver información del runtime
dotnet --info
```

---

## 📈 Performance Profiling

```bash
# Ejecutar con profiling de memoria
dotnet run --configuration Release

# Ver uso de memoria (Linux)
dotnet-counters monitor --process-id $(pgrep -f AtelierPro)

# Diagnóstico de performance
dotnet-trace collect --process-id $(pgrep -f AtelierPro)
```

---

## 🧹 Mantenimiento

```bash
# Limpiar todos los builds y paquetes
dotnet clean
rm -rf */bin */obj
dotnet restore

# Actualizar paquetes NuGet
dotnet list package --outdated
dotnet add package NombreDelPaquete --version X.Y.Z

# Verificar seguridad de paquetes
dotnet list package --vulnerable
```

---

## 📝 Logs y Debugging

```bash
# Ver logs en tiempo real
tail -f AtelierPro/logs/app.log

# Filtrar logs de EF Core
dotnet run | grep "EntityFrameworkCore"

# Ejecutar con debug habilitado
dotnet run --launch-profile "AtelierPro (Debug)"
```

---

## 🔐 Variables de Entorno

```bash
# Establecer entorno de desarrollo
export ASPNETCORE_ENVIRONMENT=Development

# Establecer entorno de producción
export ASPNETCORE_ENVIRONMENT=Production

# Puerto personalizado
export ASPNETCORE_URLS="https://localhost:5001;http://localhost:5000"
```

---

## 📚 Recursos Útiles

- Documentación .NET: https://docs.microsoft.com/dotnet
- EF Core: https://docs.microsoft.com/ef/core
- Blazor: https://docs.microsoft.com/aspnet/core/blazor
- xUnit: https://xunit.net/docs/getting-started/netcore/cmdline

---

**Última actualización**: Diciembre 2025
