# 🚀 GUÍA: FASE 1 - CrearOrdenReparacion COMPLETA

**Estado Anterior**: ✅ Fase 0 (Autenticación) Completada  
**Objetivo Fase 1**: Implementar y mejorar el módulo Taller - Crear Orden de Reparación

---

## Descripción de Fase 1

La Fase 1 se enfoca en mejorar y completar la funcionalidad principal del módulo Taller: crear órdenes de reparación con:
- Búsqueda y selección de presupuesto
- Asignación de técnico responsable
- Estimación de horas de trabajo
- Validación exhaustiva de datos
- Transaccionalidad garantizada

---

## Estructura Esperada

```
Taller Module (Fase 1):
├── CrearOrdenReparacion.razor (MEJORADA)
│   ├── Formulario con Presupuesto lookup
│   ├── Selección de técnico
│   ├── Estimación de horas
│   └── Validación client-side completa
│
├── ListarOrdenesReparacion.razor (NUEVO)
│   ├── Tabla de órdenes activas
│   ├── Filtros por estado, técnico, fecha
│   └── Búsqueda avanzada
│
├── EditarOrdenReparacion.razor (NUEVO)
│   ├── Modificar orden existente
│   ├── Cambiar técnico asignado
│   └── Actualizar estado
│
└── TallerService.cs (MEJORADO)
    ├── Aplicar patrón de ComprasService
    ├── Validación exhaustiva
    ├── Transacciones (Unit of Work)
    ├── Logging detallado
    └── Manejo de errores robusto
```

---

## Pasos de Implementación

### Paso 1: Analizar ComprasService como referencia

**Ubicación**: `/Services/ComprasService.cs`

Este servicio es el modelo a seguir. Contiene:
- Validación de entrada (no nula, rangos, uniqueness)
- Transacciones explícitas (DbContext.Database.BeginTransactionAsync)
- Logging con ILogger (éxito, errores, detalles)
- Retorno consistente de tuplas (bool Exitoso, string? Mensaje)
- Métodos async para todas las operaciones

**Métodos clave a observar:**
- `CrearOrdenCompraAsync` - Lógica de creación con validación
- `ObtenerOrdenCompraConDetallesAsync` - Carga con includes
- `ActualizarEstadoOrdenAsync` - Cambios de estado

### Paso 2: Revisar TallerService actual

**Ubicación**: `/Services/TallerService.cs`

**Analizar:**
- Métodos existentes
- Validaciones actuales
- Nivel de logging
- Manejo de transacciones

**Identificar gaps:**
- ¿Dónde faltan validaciones?
- ¿Hay uso de transacciones explícitas?
- ¿Se loguean operaciones críticas?

### Paso 3: Mejorar TallerService

Aplicar el patrón de ComprasService:

```csharp
public async Task<(bool Exitoso, string? Mensaje, Guid? Id)> 
    CrearOrdenReparacionAsync(
        Guid presupuestoId,
        Guid tecnicoId,
        decimal horasEstimadas,
        string notas)
{
    try
    {
        // Validación de entrada
        if (presupuestoId == Guid.Empty)
            return (false, "El presupuesto es requerido", null);
        
        if (tecnicoId == Guid.Empty)
            return (false, "El técnico es requerido", null);
        
        if (horasEstimadas <= 0)
            return (false, "Las horas deben ser > 0", null);

        // Buscar presupuesto
        var presupuesto = await _context.Presupuestos
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.Id == presupuestoId && p.Activo);
        
        if (presupuesto == null)
            return (false, "Presupuesto no encontrado o inactivo", null);

        // Buscar técnico
        var tecnico = await _context.Tecnicos
            .FirstOrDefaultAsync(t => t.Id == tecnicoId && t.Activo);
        
        if (tecnico == null)
            return (false, "Técnico no encontrado o inactivo", null);

        // Crear orden dentro de transacción
        using var transaction = await _context.Database
            .BeginTransactionAsync();
        
        try
        {
            var orden = new OrdenReparacion
            {
                Id = Guid.NewGuid(),
                PresupuestoId = presupuestoId,
                TecnicoId = tecnicoId,
                HorasEstimadas = horasEstimadas,
                Notas = notas,
                Estado = EstadoOrdenReparacion.Pendiente,
                FechaCreacion = DateTime.UtcNow,
                Activo = true
            };

            _context.OrdenesReparacion.Add(orden);
            await _context.SaveChangesAsync();

            // Actualizar presupuesto a "En Taller"
            presupuesto.Estado = EstadoPresupuesto.EnTaller;
            presupuesto.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation(
                $"Orden reparación creada exitosamente. " +
                $"ID: {orden.Id}, Presupuesto: {presupuestoId}, " +
                $"Técnico: {tecnicoId}");

            return (true, null, orden.Id);
        }
        catch (Exception transEx)
        {
            await transaction.RollbackAsync();
            _logger.LogError(
                $"Error en transacción. Rollback ejecutado: {transEx.Message}");
            throw;
        }
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error al crear orden reparación: {ex.Message}");
        return (false, ex.Message, null);
    }
}
```

### Paso 4: Mejorar CrearOrdenReparacion.razor

**Ubicación**: `/Pages/Taller/CrearOrdenReparacion.razor`

**Características a implementar:**
- Búsqueda de presupuestos por cliente/ID
- Dropdown de técnicos disponibles
- Campos para estimación de horas
- Validación client-side con EditForm + DataAnnotationsValidator
- BusyService overlay durante procesamiento
- Confirmación antes de crear

**Modelo de formulario:**
```csharp
public class CrearOrdenModelo
{
    [Required(ErrorMessage = "Selecciona un presupuesto")]
    public Guid? PresupuestoId { get; set; }

    [Required(ErrorMessage = "Selecciona un técnico")]
    public Guid? TecnicoId { get; set; }

    [Required]
    [Range(0.5, 500, 
        ErrorMessage = "Horas entre 0.5 y 500")]
    public decimal? HorasEstimadas { get; set; }

    [StringLength(500)]
    public string? Notas { get; set; }
}
```

### Paso 5: Crear ListarOrdenesReparacion.razor

**Ubicación**: `/Pages/Taller/ListarOrdenesReparacion.razor`

**Funcionalidades:**
- Tabla de órdenes con paginación
- Columnas: ID, Cliente, Presupuesto, Técnico, Estado, Horas, Acción
- Filtros: Estado, Técnico, Rango de fechas
- Búsqueda por cliente/ID
- Botones de editar y ver detalles
- Indicadores de estado (Pendiente, En Progreso, Completada)

### Paso 6: Crear EditarOrdenReparacion.razor

**Ubicación**: `/Pages/Taller/EditarOrdenReparacion.razor`

**Funcionalidades:**
- Cargar orden existente por ID
- Permitir cambios autorizados (técnico, horas, notas)
- No permitir cambio de presupuesto (validación)
- Botón para marcar como "Completada"
- Historial de cambios

---

## Guía de Directorios

```
AtelierPro/
├── Services/
│   ├── TallerService.cs (MEJORADO con ComprasService pattern)
│   ├── ComprasService.cs (REFERENCIA - no modificar)
│   └── ...
│
├── Pages/
│   ├── Taller/
│   │   ├── CrearOrdenReparacion.razor (MEJORADA)
│   │   ├── ListarOrdenesReparacion.razor (NUEVA)
│   │   ├── EditarOrdenReparacion.razor (NUEVA)
│   │   └── ...
│   └── ...
│
├── Models/
│   ├── OrdenReparacion.cs (VERIFICAR)
│   ├── Tecnico.cs (VERIFICAR)
│   └── EstadoOrdenReparacion.cs (ENUM)
│
└── Data/
    ├── AtelierProDbContext.cs (VERIFICAR DbSet)
    └── Migrations/ (VERIFICAR)
```

---

## Requisitos Previos

✅ Completados:
- Fase 0 (Autenticación) ✓
- AuthService registrado en Program.cs ✓
- [Authorize] en TallerController ✓
- Páginas de Taller protegidas con [Authorize(Roles = "Admin,Taller")] ✓

❓ Verificar:
- ¿Existe modelo OrdenReparacion en Models/?
- ¿Existe DbSet<OrdenReparacion> en AtelierProDbContext?
- ¿Está migraciones actualizada?
- ¿Existe modelo Tecnico?
- ¿Se puede buscar Presupuesto por ID?

---

## Enfoque de Implementación

### Orden recomendado:
1. **TallerService**: Copiar patrón de ComprasService, implementar CrearOrdenReparacionAsync
2. **CrearOrdenReparacion.razor**: Mejorar formulario con Presupuesto lookup
3. **ListarOrdenesReparacion.razor**: Crear lista con filtros
4. **EditarOrdenReparacion.razor**: CRUD completo
5. **Testing**: Validar flujo completo login → crear orden → listar → editar

### Apego a directrices:
- ✅ Clean Architecture: TallerService orquesta lógica
- ✅ SOLID: Métodos específicos, una responsabilidad cada uno
- ✅ Seguridad: Validación exhaustiva, transacciones
- ✅ Async/Await: Todos los métodos async
- ✅ Logging: Registrar operaciones críticas
- ✅ Errores: Tuplas (bool, string?, Guid?)

---

## Estimaciones

| Componente | Tiempo | Complejidad |
|-----------|--------|------------|
| TallerService mejorado | 1-2h | Media |
| CrearOrdenReparacion.razor | 1-1.5h | Media |
| ListarOrdenesReparacion.razor | 1.5-2h | Alta |
| EditarOrdenReparacion.razor | 1-1.5h | Media |
| Testing completo | 1h | Baja |
| **TOTAL** | **5.5-8h** | **Media-Alta** |

---

## Notas Importantes

1. **No romper lo existente**: Solo agregar/mejorar, no eliminar
2. **Compilación**: Debe compilar al 100% (0 errores)
3. **Transacciones**: Uso de `BeginTransactionAsync` + `CommitAsync` + `RollbackAsync`
4. **Logging**: Registrar decisiones y errores
5. **Validación**: Nunca confiar en datos del cliente
6. **Protección**: [Authorize(Roles = "Admin,Taller")]

---

## Comandos Útiles

```bash
# Compilar
cd /home/n3thun73r/AtelierPro/AtelierPro
dotnet build

# Ejecutar tests (si existen)
dotnet test

# Ver migraciones
dotnet ef migrations list

# Crear nueva migración (si cambias modelos)
dotnet ef migrations add NombreMigracion
dotnet ef database update
```

---

## Dónde Ir para Ayuda

1. **ComprasService** - Patrón de referencia para TallerService
2. **logica_del_erp.instructions.md** - Directrices de arquitectura
3. **FASE_0_AUTENTICACION_COMPLETADA.md** - Cómo se implementó Auth

---

**Próxima revisión**: Una vez que todas las nuevas funcionalidades en Fase 1 estén compilando sin errores.

¡Listo para empezar! 🚀
