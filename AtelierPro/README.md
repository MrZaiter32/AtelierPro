# 🌟 AtelierPro - ERP para Gestión de Taller y Siniestros

## 📋 Resumen Ejecutivo

AtelierPro es un sistema ERP moderno desarrollado en **Blazor Server (.NET 6)** que integra la valoración técnica de siniestros con todas las funciones operativas, logísticas, financieras y de servicio al cliente de un taller automotriz profesional.

---

## 🏗️ Arquitectura del Sistema

### Stack Tecnológico

- **Framework**: ASP.NET Core 6.0 (Blazor Server)
- **ORM**: Entity Framework Core 6.0
- **Base de Datos**: SQLite (desarrollo) / SQL Server/PostgreSQL (producción)
- **Testing**: xUnit
- **UI**: Bootstrap 5 + Razor Components

### Estructura del Proyecto

```
AtelierPro/
├── Controllers/          # API REST Controllers
│   ├── ClientesController.cs
│   └── PresupuestosController.cs
├── Data/                 # Capa de Acceso a Datos
│   ├── AtelierProDbContext.cs
│   └── DbSeeder.cs
├── Models/               # Modelos de Dominio
│   └── DomainModels.cs
├── Pages/                # Páginas Razor/Blazor
│   ├── Presupuestos.razor
│   ├── CRM/
│   │   └── ListaClientes.razor
│   └── ErpDashboard.razor
├── Services/             # Lógica de Negocio
│   ├── PresupuestoService.cs
│   ├── PresupuestoRepository.cs
│   ├── ReglaService.cs
│   ├── WorkflowService.cs
│   ├── ClienteService.cs
│   └── ClienteRepository.cs
└── Shared/               # Componentes Compartidos
    ├── MainLayout.razor
    └── NavMenu.razor

AtelierPro.Tests/        # Proyecto de Tests
├── PresupuestoServiceTests.cs
├── ReglaServiceTests.cs
└── WorkflowServiceTests.cs
```

---

## 🚀 Inicio Rápido

### Prerequisitos

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) o superior
- Editor de código (VS Code, Visual Studio, Rider)
- SQLite (incluido en EF Core)

### Instalación y Ejecución

```bash
# 1. Clonar el repositorio (o navegar al directorio)
cd /path/to/AtelierPro

# 2. Restaurar dependencias
dotnet restore

# 3. Compilar el proyecto
dotnet build

# 4. Ejecutar la aplicación
dotnet run --project AtelierPro/AtelierPro.csproj

# La app estará disponible en:
# - HTTPS: https://localhost:7071
# - HTTP: http://localhost:5197
```

### Ejecutar Tests

```bash
cd AtelierPro.Tests
dotnet test
```

---

## 📡 API Endpoints

### Clientes API

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/clientes` | Obtener todos los clientes |
| GET | `/api/clientes/{id}` | Obtener cliente por ID |
| POST | `/api/clientes` | Crear nuevo cliente |
| PUT | `/api/clientes/{id}` | Actualizar cliente |
| DELETE | `/api/clientes/{id}` | Eliminar cliente |
| GET | `/api/clientes/estadisticas` | Obtener NPS y retención |

### Presupuestos API

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/presupuestos` | Obtener todos los presupuestos |
| GET | `/api/presupuestos/{id}` | Obtener presupuesto por ID |
| GET | `/api/presupuestos/estado/{estado}` | Filtrar por estado |
| POST | `/api/presupuestos` | Crear nuevo presupuesto |
| PUT | `/api/presupuestos/{id}` | Actualizar presupuesto |
| DELETE | `/api/presupuestos/{id}` | Eliminar presupuesto |
| POST | `/api/presupuestos/{id}/cambiar-estado` | Cambiar estado del presupuesto |

### Ejemplo de Uso (cURL)

```bash
# Obtener todos los clientes
curl -X GET https://localhost:7071/api/clientes

# Crear un cliente
curl -X POST https://localhost:7071/api/clientes \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Juan Pérez",
    "historial": "Cliente nuevo",
    "preferencias": "Email",
    "nps": 85,
    "tasaRetencion": 0.9
  }'
```

---

## 🗂️ Módulos Implementados

### ✅ Core: Presupuestos y Siniestros

- **Modelos**: `Vehiculo`, `ItemPresupuesto`, `Tarifa`, `Presupuesto`
- **Servicios**:
  - `PresupuestoService`: Cálculo de totales con IVA correcto (formato decimal 0.16 = 16%)
  - `ReglaService`: Aplicación automática de depreciación y complementos
  - `WorkflowService`: Gestión de transiciones de estado (Borrador → Aprobado → Cerrado → Facturado)
- **Endpoints API**: CRUD completo
- **UI**: Página de listado con acciones básicas

### ✅ CRM y Experiencia del Cliente

- **Modelos**: `Cliente`, `Interaccion`
- **Servicios**: `ClienteService`, `ClienteRepository`
- **Features**:
  - Historial 360° del cliente
  - Cálculo de NPS y tasa de retención
  - Registro de interacciones
- **Endpoints API**: CRUD completo
- **UI**: Lista de clientes con detalles

### ✅ Persistencia y Base de Datos

- **DbContext**: `AtelierProDbContext` con configuración completa
- **Seeder**: `DbSeeder` con datos de ejemplo
- **Migraciones**: Usando `EnsureCreated()` para desarrollo rápido
- **Repositorios**: Patrón repositorio para Presupuestos y Clientes

### ✅ Testing

- **Framework**: xUnit
- **Cobertura**:
  - ReglaService: Depreciación y complementos automáticos
  - PresupuestoService: Cálculos de IVA y totales
  - WorkflowService: Transiciones de estado válidas/inválidas
- **Resultados**: 17 tests, 100% passing

---

## 🔧 Funcionalidades Clave

### 1. Motor de Reglas de Negocio (`ReglaService`)

```csharp
// Aplicación automática de depreciación por antigüedad
// Fórmula: 10% por año, máximo 50%
var presupuesto = new Presupuesto { Vehiculo = vehiculo };
presupuesto = reglaService.AplicarReglas(presupuesto, tarifa);

// Complementos automáticos (pintura, desmontaje, alineación)
// Se agregan automáticamente según configuración de la pieza
```

### 2. Cálculo Preciso de IVA

```csharp
// TasaIva en formato decimal: 0.16 = 16%
var tarifa = new Tarifa { TasaIva = 0.16m };
presupuesto = presupuestoService.CalcularTotales(presupuesto, tarifa);

// Subtotal, IVA y Total calculados correctamente
Console.WriteLine($"Subtotal: {presupuesto.Subtotal}");
Console.WriteLine($"IVA: {presupuesto.IvaAplicado}");
Console.WriteLine($"Total: {presupuesto.TotalFinal}");
```

### 3. Workflow de Estados

```
Borrador → Aprobado → Cerrado → Facturado
```

Transiciones validadas con excepciones para cambios inválidos.

---

## 📦 Módulos Planificados (Roadmap)

### 🔜 Fase 2: Operaciones Avanzadas

- [ ] **Inventario**: Control de stock, puntos de pedido, alertas
- [ ] **Compras**: Órdenes de compra automáticas, recepción y validación
- [ ] **Taller**: Asignación de técnicos, registro de tiempos reales
- [ ] **Calidad**: Checklists obligatorios, gestión de garantías

### 🔜 Fase 3: Finanzas y Gestión

- [ ] **Facturación**: Generación automática de facturas desde presupuestos
- [ ] **Cuentas por Cobrar**: Seguimiento de pagos y vencimientos
- [ ] **Tesorería**: Control de flujo de caja
- [ ] **RR.HH.**: Nómina y gestión de personal

### 🔜 Fase 4: Integraciones

- [ ] **Core Audatex**: Integración con API de valoración de siniestros
- [ ] **Movilidad**: App móvil para captura de fotos y firmas digitales
- [ ] **BI y Reportes**: Dashboards interactivos con KPIs en tiempo real

### 🔜 Fase 5: Seguridad y Producción

- [ ] **Autenticación**: ASP.NET Core Identity o JWT
- [ ] **Autorización**: Roles (Admin, Taller, Finanzas, Cliente)
- [ ] **Auditoría**: Log de cambios y acciones
- [ ] **Migración a Producción**: SQL Server/PostgreSQL, Azure/AWS deployment

---

## 🧪 Validación y Correcciones Realizadas

### ✅ Corrección Crítica: Cálculo de IVA

**Problema identificado**: Dos métodos calculaban IVA de forma inconsistente
- `CalcularTotales`: multiplicaba por `TasaIva` directamente
- `CalcularPresupuestoFinal`: dividía `TasaIva` entre 100

**Solución**: Unificación a formato decimal (0.16 = 16%) en un solo método documentado.

### ✅ Arquitectura DI Mejorada

**Problema**: Servicios Singleton con estado mutable compartido
**Solución**: 
- Servicios de dominio cambiados a **Scoped**
- Repositorios implementados para acceso a datos
- `ErpDataService` legacy mantenido solo para compatibilidad del dashboard

### ✅ Validación de Workflow

**Problema**: Transiciones de estado sin validación
**Solución**: `WorkflowService` ahora lanza `InvalidOperationException` para transiciones inválidas

---

## 📝 Notas Técnicas

### Base de Datos

La aplicación usa **SQLite** por defecto para desarrollo rápido. La base de datos se crea automáticamente en `atelierpro.db` en el directorio raíz del proyecto al iniciar la aplicación.

Para cambiar a SQL Server o PostgreSQL, actualiza la cadena de conexión en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AtelierPro;User=sa;Password=yourpassword;"
  }
}
```

Y cambia el provider en `Program.cs`:

```csharp
builder.Services.AddDbContext<AtelierProDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Datos de Seed

Al iniciar por primera vez, la aplicación puebla la base de datos con datos de ejemplo usando `DbSeeder.cs`. Esto incluye:
- 1 Presupuesto de ejemplo con vehículo y items
- 1 Cliente con interacciones
- 2 Refacciones
- 1 Orden de compra, reparación y factura
- Tarifa base configurada

---

## 🤝 Contribuir

Este proyecto es un MVP funcional. Para contribuir:

1. Fork el repositorio
2. Crea una rama feature (`git checkout -b feature/nueva-funcionalidad`)
3. Commit tus cambios (`git commit -m 'Agregar nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

---

## 📄 Licencia

Este proyecto es privado y propiedad de AtelierPro. Todos los derechos reservados.

---

## 📞 Soporte

Para preguntas o soporte técnico, contacta al equipo de desarrollo.

**Última actualización**: Diciembre 2025
**Versión**: 1.0.0-MVP
