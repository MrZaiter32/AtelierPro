# 🏛️ Módulos Finales del ERP AtelierPro - Arquitectura Definitiva

## 📋 Tabla de Contenidos
1. [Módulos Finales](#módulos-finales)
2. [Entidades por Módulo](#entidades-por-módulo)
3. [Relaciones Entre Módulos](#relaciones-entre-módulos)
4. [Flujos de Comunicación](#flujos-de-comunicación)
5. [API Endpoints Completa](#api-endpoints-completa)
6. [Implementación Fase a Fase](#implementación-fase-a-fase)

---

## 🎯 Módulos Finales

El ERP AtelierPro constará de **8 módulos core** (Audatex se descarta):

| # | Módulo | Estado | Responsabilidad |
|---|--------|--------|-----------------|
| 1 | **Presupuestos** | ✅ Existe | Cotización y valuación de reparaciones |
| 2 | **CRM** | ✅ Existe | Gestión de clientes e interacciones |
| 3 | **Taller** | ❌ Falta | Órdenes de reparación y asignación técnica |
| 4 | **Inventario** | ❌ Falta | Stock de refacciones y movimientos |
| 5 | **Compras** | ❌ Falta | Órdenes de compra y proveedores |
| 6 | **Finanzas** | ❌ Falta | Facturación, pagos, cuentas por cobrar |
| 7 | **Calidad** | ❌ Falta | Inspecciones, checklists, garantía |
| 8 | **Activos** | ❌ Falta | Herramientas, equipos, calibraciones |

---

## 📊 Entidades por Módulo

### 1️⃣ MÓDULO PRESUPUESTOS (CORE)
```csharp
Presupuesto
├── Id: Guid (PK)
├── Vehiculo: Vehiculo (1-a-1)
│   ├── Vin: string (PK)
│   ├── Version: string
│   ├── AntiguedadAnios: int
│   └── ValorResidual: decimal
├── Items: List<ItemPresupuesto> (1-a-N)
│   ├── Id: Guid (PK)
│   ├── Tipo: TipoItemPresupuesto (Pieza|ManoObra|Pintura)
│   ├── Codigo: string
│   ├── Descripcion: string
│   ├── TiempoAsignadoHoras: double
│   ├── PrecioUnitario: decimal
│   ├── PorcentajeAjuste: decimal
│   ├── RequierePintura: bool
│   ├── RequiereDesmontajeDoble: bool
│   └── RequiereAlineacion: bool
├── Tarifa: Tarifa (1-a-1)
│   ├── Id: Guid
│   ├── PrecioManoObraHora: decimal
│   ├── PrecioPinturaHora: decimal
│   ├── TasaIva: decimal (0.16 = 16%)
│   ├── FactorRecargo: decimal
│   └── FactorDescuento: decimal
├── ClienteId: Guid (FK) → Cliente
├── Subtotal: decimal (Calculated)
├── IvaAplicado: decimal
├── TotalFinal: decimal
└── Estado: EstadoPresupuesto (Borrador|Aprobado|Cerrado|Facturado)
```

**Servicios:**
- `PresupuestoService` - Cálculos
- `PresupuestoRepository` - CRUD
- `ReglaService` - Depreciation & auto-complements
- `WorkflowService` - State machine

**API Endpoints:**
```
✅ GET    /api/presupuestos
✅ GET    /api/presupuestos/{id}
✅ GET    /api/presupuestos/estado/{estado}
✅ POST   /api/presupuestos
✅ PUT    /api/presupuestos/{id}
✅ DELETE /api/presupuestos/{id}
✅ POST   /api/presupuestos/{id}/cambiar-estado
```

---

### 2️⃣ MÓDULO CRM (CLIENTES)
```csharp
Cliente
├── Id: Guid (PK)
├── Nombre: string
├── Email: string
├── Telefono: string
├── Direccion: string
├── Historial: string
├── Preferencias: string
├── Nps: double (0-10)
├── TasaRetencion: double (%)
└── Interacciones: List<Interaccion> (1-a-N)
    ├── Id: Guid
    ├── Fecha: DateTime
    ├── Tipo: string (Llamada|Email|Reunión)
    └── Resultado: string

Presupuesto.ClienteId → Cliente (N-a-1)
OrdenReparacion.ClienteId → Cliente (N-a-1)
Factura.ClienteId → Cliente (N-a-1)
```

**Servicios:**
- `ClienteService` - CRM operations
- `ClienteRepository` - CRUD + search

**API Endpoints:**
```
✅ GET    /api/clientes
✅ GET    /api/clientes/{id}
✅ POST   /api/clientes
✅ PUT    /api/clientes/{id}
✅ DELETE /api/clientes/{id}
✅ GET    /api/clientes/estadisticas
```

---

### 3️⃣ MÓDULO TALLER (ÓRDENES DE REPARACIÓN)
```csharp
OrdenReparacion
├── Id: Guid (PK)
├── PresupuestoId: Guid (FK) → Presupuesto (1-a-1, Estado=Aprobado)
├── ClienteId: Guid (FK) → Cliente (1-a-1)
├── TecnicoId: Guid (FK) → Tecnico (1-a-1)
├── Inicio: DateTime
├── Fin: DateTime? (null si no terminada)
├── HorasReales: double
├── Estado: string (Pendiente|EnProgreso|Completada|Cancelada)
└── Items: List<ItemOrdenReparacion> (1-a-N)
    ├── ItemPresupuestoId: Guid (FK)
    ├── Completado: bool
    ├── Observaciones: string
    └── FechaComplecion: DateTime?

Tecnico
├── Id: Guid (PK)
├── Nombre: string
├── Email: string
├── Telefono: string
├── Especialización: string
├── DisponibilidadActual: int (horas)
└── OrdenesCo signadas: List<OrdenReparacion>

Disponibilidad
├── Id: Guid (PK)
├── TecnicoId: Guid (FK)
├── Dia: DateTime
├── HorasDisponibles: double
└── HorasOcupadas: double
```

**Relaciones:**
- Presupuesto (Aprobado) → OrdenReparacion (1-a-1)
- Presupuesto.Items → ItemOrdenReparacion (N-a-N)
- Cliente → OrdenReparacion (1-a-N)
- Tecnico → OrdenReparacion (1-a-N)

**Servicios:**
- `OrdenReparacionService` - CRUD + workflow
- `TecnicoService` - Gestión técnicos
- `DisponibilidadService` - Calendario

**API Endpoints:**
```
POST   /api/ordenes-reparacion/{presupuestoId}
GET    /api/ordenes-reparacion
GET    /api/ordenes-reparacion/{id}
PUT    /api/ordenes-reparacion/{id}
POST   /api/ordenes-reparacion/{id}/finalizar
GET    /api/tecnicos
GET    /api/tecnicos/{id}/disponibilidad
```

---

### 4️⃣ MÓDULO INVENTARIO (REFACCIONES & STOCK)
```csharp
Refaccion
├── Sku: string (PK)
├── Nombre: string
├── Descripcion: string
├── StockActual: int
├── StockMinimo: int
├── CostoPromedio: decimal
├── PrecioVenta: decimal
├── ProveedorId: Guid (FK)
├── UltimaEntrada: DateTime?
└── UltimaSalida: DateTime?

MovimientoInventario
├── Id: Guid (PK)
├── Refaccion: Refaccion (FK - 1-a-N)
├── Tipo: TipoMovimiento (Entrada|Salida|Ajuste|Rechazo)
├── Cantidad: int
├── CantidadAnterior: int
├── Fecha: DateTime
├── UsuarioId: Guid
├── Referencia: string (OrdenCompraId|OrdenReparacionId|AjusteId)
└── Observaciones: string

AlertaInventario
├── Id: Guid (PK)
├── Refaccion: Refaccion (FK)
├── Tipo: TipoAlerta (StockBajo|Vencimiento|Sobrestock)
├── FechaAlerta: DateTime
├── Resuelta: bool
└── Observaciones: string
```

**Relaciones:**
- Refaccion → MovimientoInventario (1-a-N)
- ItemPresupuesto (Tipo=Pieza) ← Refaccion (cuando se cierra OR)
- Refaccion.ProveedorId → Proveedor (N-a-1)

**Servicios:**
- `InventarioService` - Gestión stock
- `MovimientoInventarioService` - Movimientos
- `AlertaInventarioService` - Alertas automáticas

**API Endpoints:**
```
GET    /api/inventario
GET    /api/inventario/{sku}
GET    /api/inventario/alertas
POST   /api/inventario/{sku}/movimiento
PUT    /api/inventario/{sku}
GET    /api/inventario/bajo-stock
GET    /api/inventario/movimientos
```

---

### 5️⃣ MÓDULO COMPRAS (ÓRDENES & PROVEEDORES)
```csharp
Proveedor
├── Id: Guid (PK)
├── Nombre: string
├── ContactoPrincipal: string
├── Email: string
├── Telefono: string
├── Dirección: string
├── CUIT: string
├── CuentaBancaria: string
├── Rating: int (1-5 estrellas)
├── CondicionesPago: string
└── OrdenesCo mpra: List<OrdenCompra>

OrdenCompra
├── Id: Guid (PK)
├── NumeroOrden: string
├── ProveedorId: Guid (FK) → Proveedor (N-a-1)
├── Fecha: DateTime
├── FechaEntregaEsperada: DateTime
├── FechaEntregaReal: DateTime?
├── Items: List<ItemOrdenCompra> (1-a-N)
│   ├── Id: Guid
│   ├── Refaccion: Refaccion (FK)
│   ├── CantidadSolicitada: int
│   ├── CantidadRecibida: int
│   ├── PrecioUnitario: decimal
│   └── Subtotal: decimal
├── MontoTotal: decimal
├── ImpuestosTotal: decimal
├── TotalConImpuestos: decimal
├── Estado: EstadoOrdenCompra (Generada|Enviada|Recibida|Rechazada|Facturada)
├── FacturaProveedor: string?
└── ObservacionesEntrega: string
```

**Relaciones:**
- StockBajo (AlertaInventario) → OrdenCompra (generación automática)
- ItemOrdenCompra.Refaccion → Refaccion (N-a-1)
- OrdenCompra → Proveedor (N-a-1)

**Servicios:**
- `ProveedorService` - Gestión proveedores
- `OrdenCompraService` - CRUD + workflow
- `ComprasAutomaticasService` - Generación desde stock bajo

**API Endpoints:**
```
GET    /api/proveedores
POST   /api/proveedores
PUT    /api/proveedores/{id}
GET    /api/ordenes-compra
POST   /api/ordenes-compra
PUT    /api/ordenes-compra/{id}
POST   /api/ordenes-compra/{id}/recibir
GET    /api/ordenes-compra/por-proveedor/{proveedorId}
```

---

### 6️⃣ MÓDULO FINANZAS (FACTURACIÓN & PAGOS)
```csharp
Factura
├── Id: Guid (PK)
├── NumeroFactura: string (unique)
├── PresupuestoId: Guid (FK) → Presupuesto (1-a-1, Estado=Cerrado)
├── ClienteId: Guid (FK) → Cliente (1-a-1)
├── Fecha: DateTime
├── FechaVencimiento: DateTime
├── Items: List<FacturaItem> (copia de ItemPresupuesto)
├── Subtotal: decimal
├── Impuestos: decimal
├── Descuentos: decimal
├── Total: decimal
├── Estado: EstadoFactura (Pendiente|Pagada|Vencida|Cancelada|NotaCredito)
├── Pagos: List<Pago> (1-a-N)
└── NotasObservaciones: string

Pago
├── Id: Guid (PK)
├── FacturaId: Guid (FK) → Factura (N-a-1)
├── Monto: decimal
├── Fecha: DateTime
├── MetodoPago: string (Efectivo|Tarjeta|Transferencia|Cheque)
├── Referencia: string (NumComprobanteTransferencia|UltimosDígitos)
└── Observaciones: string

CuentaPorCobrar
├── Id: Guid (PK)
├── ClienteId: Guid (FK) → Cliente (1-a-1)
├── MontoOriginal: decimal
├── MontoPagado: decimal
├── MontoPendiente: decimal (Calculated)
├── DíasVencida: int (Calculated)
├── ÚltimoPago: DateTime?
├── PróximoVencimiento: DateTime
└── Alertas: List<AlertaCobro>

ReporteFinanciero
├── Id: Guid (PK)
├── Periodo: string (Mes-Año)
├── IngresosTotales: decimal
├── CostosDirectos: decimal
├── GastosOperacionales: decimal
├── Utilidad: decimal
├── CuentasPorCobrar: decimal
├── CuentasPorPagar: decimal
└── GeneradoEn: DateTime
```

**Relaciones:**
- Presupuesto (Cerrado) → Factura (1-a-1)
- Factura → Pago (1-a-N)
- Cliente → CuentaPorCobrar (1-a-1)
- CuentaPorCobrar → Factura (1-a-N)

**Servicios:**
- `FacturaService` - Generación desde presupuestos
- `PagoService` - Registro de pagos
- `CuentasPorCobrarService` - Gestión CxC
- `FinanceReportService` - Reportes

**API Endpoints:**
```
GET    /api/facturas
POST   /api/facturas/{presupuestoId}
GET    /api/facturas/{id}
PUT    /api/facturas/{id}
POST   /api/pagos
GET    /api/cuentas-por-cobrar
GET    /api/cuentas-por-cobrar/vencidas
POST   /api/reportes/flujo-caja
GET    /api/reportes/ingresos/{periodo}
```

---

### 7️⃣ MÓDULO CALIDAD (INSPECCIONES & GARANTÍA)
```csharp
ChecklistControl
├── Id: Guid (PK)
├── OrdenReparacionId: Guid (FK) → OrdenReparacion (1-a-1)
├── Responsable: string
├── Fecha: DateTime
├── Items: List<ChecklistItem> (1-a-N)
│   ├── Id: Guid
│   ├── Aspecto: string
│   ├── Cumplido: bool
│   ├── Foto: string? (URL o blob)
│   └── Anotaciones: string
├── Aprobado: bool
├── ObservacionesGenerales: string
└── FirmaResponsable: string?

ReclamoGarantia
├── Id: Guid (PK)
├── PresupuestoId: Guid (FK) → Presupuesto (1-a-1)
├── ClienteId: Guid (FK) → Cliente (1-a-1)
├── FechaReclamo: DateTime
├── Motivo: string
├── Descripcion: string
├── Evidencia: string? (foto/video URL)
├── Estado: EstadoReclamo (Recibido|EnAnalisis|Aprobado|Rechazado|Resuelto)
├── Resolucion: string
├── CostoReparacion: decimal
├── FechaResolucion: DateTime?
└── ObservacionesFinales: string

PeriodicidadCalibracion (para Activos)
├── Id: Guid (PK)
├── Tipo: string (Calibrador|Micrometro|etc)
├── DíasEntreCalib: int
└── Norma: string (ISO|IRAM|etc)
```

**Relaciones:**
- OrdenReparacion → ChecklistControl (1-a-1, al finalizar)
- Presupuesto → ReclamoGarantia (1-a-N)

**Servicios:**
- `QualityControlService` - Checklists
- `WarrantyService` - Reclamos

**API Endpoints:**
```
POST   /api/ordenes-reparacion/{id}/control-calidad
GET    /api/control-calidad/{id}
POST   /api/reclamos-garantia
GET    /api/reclamos-garantia
PUT    /api/reclamos-garantia/{id}
GET    /api/reclamos-garantia/pendientes
```

---

### 8️⃣ MÓDULO ACTIVOS (HERRAMIENTAS & CALIBRACIÓN)
```csharp
Activo
├── Id: Guid (PK)
├── Nombre: string
├── Tipo: TipoActivo (Herramienta|Equipo|Instrumento)
├── Modelo: string
├── NumeroSerie: string
├── FechaAdquisicion: DateTime
├── ValorAdquisicion: decimal
├── Ubicacion: string
├── Estado: EstadoActivo (Operativo|EnMantenimiento|Defectuoso|Retirado)
├── RequiereCalibración: bool
├── PeriodicidadCalibración: int (días)
├── UltimaCalibracion: DateTime?
├── ProximaCalibración: DateTime?
└── CalibracionVencida: bool (Calculated)

RegistroCalibración
├── Id: Guid (PK)
├── ActivoId: Guid (FK) → Activo (1-a-1)
├── Fecha: DateTime
├── Responsable: string
├── TecnicoExterno: string?
├── CertificadoUrl: string?
├── ResultadoConformidad: bool (Conforme|No Conforme)
├── ObservacionesTécnicas: string
└── ProximaCalibracion: DateTime

HistorialMantenimiento
├── Id: Guid (PK)
├── ActivoId: Guid (FK) → Activo (1-a-N)
├── Tipo: TipoMantenimiento (Preventivo|Correctivo)
├── Fecha: DateTime
├── Descripcion: string
├── CostoMano: decimal
├── CostoRepuestos: decimal
└── TecnicoResponsable: string
```

**Relaciones:**
- Activo → RegistroCalibración (1-a-N)
- Activo → HistorialMantenimiento (1-a-N)

**Servicios:**
- `AssetService` - Gestión activos
- `CalibrationService` - Control de calibraciones
- `MaintenanceService` - Mantenimiento preventivo

**API Endpoints:**
```
GET    /api/activos
POST   /api/activos
PUT    /api/activos/{id}
GET    /api/activos/calibraciones-vencidas
POST   /api/activos/{id}/calibrar
GET    /api/activos/{id}/historial
POST   /api/activos/{id}/mantenimiento
```

---

## 🔄 Relaciones Entre Módulos

### Diagrama de Relaciones Principales

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│  PRESUPUESTOS (Core)                                           │
│  ├─ 1-a-1 → Vehiculo                                           │
│  ├─ 1-a-N → ItemPresupuesto                                    │
│  ├─ 1-a-1 → Tarifa                                             │
│  └─ N-a-1 → Cliente                                            │
│       │                                                         │
│       ├─ Genera → ORDEN REPARACION (Taller)                    │
│       │   ├─ 1-a-1 → Presupuesto (Aprobado)                   │
│       │   ├─ 1-a-1 → Tecnico                                   │
│       │   ├─ 1-a-N → ItemOrdenReparacion                       │
│       │   ├─ Consume → REFACCIONES (Inventario)                │
│       │   └─ Genera → CHECKLIST CONTROL (Calidad)              │
│       │                                                         │
│       └─ Genera → FACTURA (Finanzas, Estado=Cerrado)           │
│           ├─ 1-a-1 → Cliente                                   │
│           ├─ 1-a-N → Pago                                      │
│           └─ Actualiza → CUENTA POR COBRAR                     │
│                                                                 │
├─ N-a-1 → CLIENTE (CRM)                                         │
│   ├─ 1-a-N → Interaccion                                       │
│   ├─ 1-a-N → OrdenReparacion                                   │
│   ├─ 1-a-N → Factura                                           │
│   └─ 1-a-1 → CuentaPorCobrar                                   │
│                                                                 │
├─ Stock Bajo → ORDEN COMPRA (Compras)                           │
│   ├─ N-a-1 → Proveedor                                         │
│   └─ 1-a-N → ItemOrdenCompra (Refaccion)                       │
│       └─ Actualiza → INVENTARIO (Inventario)                   │
│                                                                 │
└─ Cierre Reparación → RECLAMO GARANTIA (Calidad)                │
    ├─ N-a-1 → Presupuesto                                       │
    └─ N-a-1 → Cliente                                           │
                                                                 │
ACTIVOS (Herramientas)                                            │
├─ 1-a-N → RegistroCalibración                                   │
└─ 1-a-N → HistorialMantenimiento                                │
```

---

## 📤 Flujos de Comunicación

### Flujo 1: Presupuesto → Reparación → Facturación

```
1. CLIENTE solicita presupuesto
   ↓
2. PRESUPUESTO (Borrador)
   ├─ Vehiculo registrado
   ├─ Items añadidos
   ├─ Tarifa aplicada
   └─ Reglas aplicadas
   ↓
3. PRESUPUESTO calculado
   ├─ PresupuestoService.CalcularTotales()
   ├─ ReglaService.AplicarDepreciacion()
   ├─ ReglaService.AplicarComplementos()
   └─ WorkflowService.CambiarEstado(Aprobado)
   ↓
4. CLIENTE aprueba → PRESUPUESTO (Aprobado)
   ↓
5. ORDEN REPARACION creada
   ├─ OrdenReparacionService.Crear()
   ├─ TecnicoService.BuscarDisponible()
   ├─ DisponibilidadService.ReservarHoras()
   └─ WorkflowService.CambiarEstado(Pendiente)
   ↓
6. REPARACIÓN comienza
   ├─ WorkflowService.CambiarEstado(EnProgreso)
   ├─ MovimientoInventarioService.Salida(Refaccion)
   └─ InventarioService.ActualizarStock()
   ↓
7. REPARACIÓN completada
   ├─ WorkflowService.CambiarEstado(Completada)
   ├─ QualityControlService.CrearChecklist()
   ├─ ChecklistControl aprobado
   └─ WorkflowService.PresupuestoEstado(Cerrado)
   ↓
8. FACTURA generada
   ├─ FacturaService.CrearDesdePresupuesto()
   ├─ FacturaService.GenerarNumeroFactura()
   ├─ WorkflowService.PresupuestoEstado(Facturado)
   └─ CuentasPorCobrarService.Crear()
   ↓
9. PAGO recibido
   ├─ PagoService.Registrar()
   ├─ FacturaService.MarcarPagada()
   └─ FinanceReportService.ActualizarMetricas()
```

**Servicios Involucrados:**
- PresupuestoService
- ReglaService
- WorkflowService
- OrdenReparacionService
- TecnicoService
- MovimientoInventarioService
- InventarioService
- QualityControlService
- FacturaService
- PagoService
- CuentasPorCobrarService

---

### Flujo 2: Stock Bajo → Orden Compra → Entrada Inventario

```
1. SISTEMA monitorea stock
   ↓
2. Stock por debajo del mínimo
   ├─ AlertaInventarioService.Crear(StockBajo)
   └─ ComprasAutomaticasService.GenararOrdenCompra()
   ↓
3. ORDEN COMPRA creada
   ├─ OrdenCompraService.Crear()
   ├─ ProveedorService.BuscarMejorPrecio()
   └─ WorkflowService.CambiarEstado(Generada)
   ↓
4. ORDEN enviada
   ├─ EmailService.NotificarProveedor()
   └─ WorkflowService.CambiarEstado(Enviada)
   ↓
5. MERCANCÍA recibida
   ├─ OrdenCompraService.Recibir()
   ├─ ItemOrdenCompra validada
   └─ InventarioService.ActualizarStock()
   ↓
6. MOVIMIENTO inventario registrado
   ├─ MovimientoInventarioService.Entrada()
   ├─ Refaccion.StockActual += Cantidad
   ├─ WorkflowService.OrdenCompraEstado(Recibida)
   └─ AlertaInventarioService.Resolver()
   ↓
7. FACTURA proveedor registrada
   ├─ OrdenCompraService.RegistrarFactura()
   └─ FinanceReportService.ActualizarCuentasPorPagar()
```

**Servicios Involucrados:**
- AlertaInventarioService
- ComprasAutomaticasService
- OrdenCompraService
- ProveedorService
- InventarioService
- MovimientoInventarioService
- EmailService
- FinanceReportService

---

### Flujo 3: Control de Calidad & Reclamos

```
1. ORDEN REPARACION completada
   ↓
2. CHECKLIST CONTROL creado
   ├─ QualityControlService.CrearChecklist()
   ├─ Items del checklist evaluados
   └─ Foto/evidencias capturadas
   ↓
3. CHECKLIST evaluado
   ├─ SI Aprobado → Presupuesto.Estado = Cerrado
   ├─ NO Aprobado → Retrabajo requerido
   └─ Notificación al técnico
   ↓
4. RECLAMO GARANTIA (Si aplica)
   ├─ Cliente reporta problema
   ├─ WarrantyService.CrearReclamo()
   ├─ Evidencia (foto/video) adjuntada
   └─ Estado = Recibido
   ↓
5. RECLAMO en análisis
   ├─ WarrantyService.AnalizarReclamo()
   ├─ Comparar con ChecklistControl
   └─ Estado = EnAnalisis
   ↓
6. DECISIÓN
   ├─ SI Aprobado
   │  ├─ OrdenReparacion nueva (retrabajo)
   │  ├─ FacturaService.GenerarNotaCredito()
   │  └─ Estado = Aprobado
   └─ NO Rechazado
      ├─ WarrantyService.Rechazar()
      └─ Estado = Rechazado
   ↓
7. RECLAMO resuelto
   ├─ WarrantyService.Resolver()
   ├─ EmailService.NotificarCliente()
   └─ FinanceReportService.ActualizarGarantias()
```

**Servicios Involucrados:**
- QualityControlService
- WarrantyService
- OrdenReparacionService
- FacturaService
- EmailService
- FinanceReportService

---

## 📡 API Endpoints Completa

### PRESUPUESTOS
```
GET    /api/presupuestos
GET    /api/presupuestos/{id}
GET    /api/presupuestos/estado/{estado}
POST   /api/presupuestos
PUT    /api/presupuestos/{id}
DELETE /api/presupuestos/{id}
POST   /api/presupuestos/{id}/cambiar-estado
```

### CRM - CLIENTES
```
GET    /api/clientes
GET    /api/clientes/{id}
POST   /api/clientes
PUT    /api/clientes/{id}
DELETE /api/clientes/{id}
GET    /api/clientes/estadisticas
GET    /api/clientes/{id}/historial
POST   /api/clientes/{id}/interaccion
```

### TALLER
```
POST   /api/ordenes-reparacion/{presupuestoId}
GET    /api/ordenes-reparacion
GET    /api/ordenes-reparacion/{id}
PUT    /api/ordenes-reparacion/{id}
POST   /api/ordenes-reparacion/{id}/finalizar
GET    /api/tecnicos
GET    /api/tecnicos/{id}
GET    /api/tecnicos/{id}/disponibilidad
POST   /api/tecnicos/{id}/reservar/{horas}
```

### INVENTARIO
```
GET    /api/inventario
GET    /api/inventario/{sku}
GET    /api/inventario/alertas
POST   /api/inventario/{sku}/movimiento
PUT    /api/inventario/{sku}
GET    /api/inventario/bajo-stock
GET    /api/inventario/movimientos
GET    /api/inventario/movimientos/refaccion/{sku}
```

### COMPRAS
```
GET    /api/proveedores
POST   /api/proveedores
PUT    /api/proveedores/{id}
GET    /api/ordenes-compra
POST   /api/ordenes-compra
PUT    /api/ordenes-compra/{id}
POST   /api/ordenes-compra/{id}/recibir
GET    /api/ordenes-compra/por-proveedor/{proveedorId}
```

### FINANZAS
```
GET    /api/facturas
POST   /api/facturas/{presupuestoId}
GET    /api/facturas/{id}
PUT    /api/facturas/{id}
POST   /api/pagos
GET    /api/cuentas-por-cobrar
GET    /api/cuentas-por-cobrar/vencidas
POST   /api/reportes/flujo-caja
GET    /api/reportes/ingresos/{periodo}
GET    /api/reportes/gastos/{periodo}
```

### CALIDAD
```
POST   /api/ordenes-reparacion/{id}/control-calidad
GET    /api/control-calidad/{id}
POST   /api/reclamos-garantia
GET    /api/reclamos-garantia
PUT    /api/reclamos-garantia/{id}
GET    /api/reclamos-garantia/pendientes
```

### ACTIVOS
```
GET    /api/activos
POST   /api/activos
PUT    /api/activos/{id}
GET    /api/activos/calibraciones-vencidas
POST   /api/activos/{id}/calibrar
GET    /api/activos/{id}/historial
POST   /api/activos/{id}/mantenimiento
```

---

## 🔧 Implementación Fase a Fase

### FASE 1: MVP Pro (Semanas 1-2) - 12-15 horas
**Objetivo:** Hacer el MVP producción-ready

- [x] ✅ Presupuestos básico (ya existe)
- [x] ✅ CRM básico (ya existe)
- [ ] 🚧 Autenticación & Roles
- [ ] 🚧 Formularios CRUD en UI
- [ ] 🚧 Deployment

**Salida:** APP accesible con login seguro

---

### FASE 2: ERP Core (Semanas 3-4) - 17-21 horas
**Objetivo:** Cerrar el flujo de reparación

- [ ] 🚧 Módulo TALLER (Órdenes de Reparación)
  - Crear OrdenReparacion desde Presupuesto
  - Asignar técnico automáticamente
  - Registrar progreso
  
- [ ] 🚧 Módulo INVENTARIO (Refacciones & Stock)
  - Crear/actualizar refacciones
  - Registrar movimientos
  - Alertas de stock bajo
  
- [ ] 🚧 Módulo COMPRAS (Órdenes & Proveedores)
  - Crear proveedores
  - Generar órdenes desde alertas
  - Recibir mercancía

**Salida:** Flujo completo presupuesto → reparación → facturación

---

### FASE 3: Operaciones (Semanas 5-6) - 10-14 horas
**Objetivo:** Automatización y reporting

- [ ] 🚧 Módulo FINANZAS (Facturación)
  - Generar facturas desde presupuestos
  - Registrar pagos
  - Cuentas por cobrar

- [ ] 🚧 Módulo CALIDAD (Checklists & Garantía)
  - Checklist al finalizar reparación
  - Reclamos de garantía
  - Retrabajos

- [ ] 🚧 ACTIVOS (Herramientas & Calibración)
  - Registro de activos
  - Control de calibraciones

**Salida:** Sistema de calidad y finanzas completo

---

### FASE 4: Inteligencia (Semanas 7-8) - 20-25 horas
**Objetivo:** Analytics y optimización

- [ ] 🚧 Dashboard con gráficas (Chart.js)
- [ ] 🚧 Reportes financieros
- [ ] 🚧 Búsqueda y filtros avanzados
- [ ] 🚧 Emails automáticos
- [ ] 🚧 PDFs de presupuestos/facturas
- [ ] 🚧 Tests completos
- [ ] 🚧 Mejoras UI/UX
- [ ] 🚧 Seguridad adicional (2FA, CORS, etc.)

**Salida:** ERP completo y robusto

---

## 📝 Dependencias y Bloqueadores

### Diagrama de Dependencias

```
PRESUPUESTOS ←─────────────────────────────────────┐
    ↓                                                │
    ├─ Cliente (requiere CRM)                       │
    ├─ Tarifa (modelo simple)                       │
    └─ ItemPresupuesto (modelo simple)              │
         ↓                                           │
    ORDEN REPARACION (requiere Presupuesto)        │
         ├─ Técnico (requiere módulo Taller)       │
         ├─ Refacción (requiere módulo Inventario) │
         └─ Consumo de stock                        │
              ↓                                      │
         CHECKLIST CONTROL (requiere Taller)       │
         INVENTARIO (se consume)                   │
              ↓                                      │
    ORDEN COMPRA (generada automáticamente)        │
         └─ Proveedor (requiere módulo Compras)    │
              ↓                                      │
         RECEPCIÓN (actualiza inventario)          │
              ↓                                      │
    FACTURA (requiere Presupuesto Cerrado) ────────┘
         ├─ Cliente (requiere CRM)
         └─ Pagos
              ↓
    CUENTAS POR COBRAR (requiere Finanzas)
         └─ Reportes
              ↓
    RECLAMO GARANTIA (requiere Presupuesto & Calidad)

ACTIVOS (independiente, pero usado por Taller)
```

---

## 🎯 Resumen Ejecutivo

### Módulos Definitivos (8 total, sin Audatex)

| Fase | Módulo | Entidades | APIs | Prioridad |
|------|--------|-----------|------|-----------|
| 1 | ✅ Presupuestos | 4 | 7 | CORE |
| 1 | ✅ CRM | 2 | 6 | CORE |
| 2 | Taller | 4 | 6 | ALTA |
| 2 | Inventario | 3 | 7 | ALTA |
| 2 | Compras | 3 | 7 | ALTA |
| 3 | Finanzas | 4 | 8 | MEDIA |
| 3 | Calidad | 3 | 6 | MEDIA |
| 3 | Activos | 3 | 7 | MEDIA |

**Total:** 28 entidades principales + 50+ endpoints API

---

**Documento actualizado:** 6 de diciembre de 2025  
**Estado:** Arquitectura Definitiva - Listo para implementación
