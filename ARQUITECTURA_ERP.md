# 🏗️ Arquitectura Completa del ERP AtelierPro

## 📊 Visión General del Sistema

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         AtelierPro ERP System                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  FRONTEND (Blazor Server)  │  BACKEND (API REST)  │  DATA (SQLite/SQL) │
│  ─────────────────────────────────────────────────────────────────────  │
│                                                                         │
│  • Dashboard                • Controllers         • Contexto DB         │
│  • Páginas Razor            • Services            • Modelos             │
│  • Componentes              • Repositories        • Relaciones          │
│  • UI Interactiva           • Lógica Negocio      • Validaciones        │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 🎯 Módulos del ERP (Mapa Completo)

### 1️⃣ **MÓDULO CORE: Presupuestos & Siniestros**

**Responsabilidad:** Gestión integral de presupuestos de reparación desde cotización hasta facturación

#### Entidades Relacionadas:
```
Presupuesto (CORE)
├── Vehiculo (1-a-1)
│   ├── VIN
│   ├── Versión
│   ├── Antigüedad (años)
│   └── Valor Residual (para depreciación)
├── Items (1-a-N)
│   ├── ItemPresupuesto (Pieza/ManoObra/Pintura)
│   ├── Código
│   ├── Descripción
│   ├── TiempoAsignado (horas)
│   ├── PrecioUnitario
│   ├── PorcentajeAjuste
│   ├── RequierePintura
│   ├── RequiereDesmontajeDoble
│   └── RequiereAlineacion
├── Tarifa (1-a-1)
│   ├── PrecioManoObraHora
│   ├── PrecioPinturaHora
│   ├── TasaIVA (0.16 = 16%)
│   ├── FactorRecargo
│   └── FactorDescuento
└── Estado (Enum: Borrador → Aprobado → Cerrado → Facturado)

Presupuesto.Subtotal = SUM(ItemPresupuesto.CostoAjustado)
Presupuesto.IVA = Subtotal * Tarifa.TasaIVA
Presupuesto.Total = Subtotal + IVA
```

#### Servicios Principales:
- **PresupuestoService**: Cálculos de totales, margen, IVA
- **PresupuestoRepository**: CRUD + filtrado por estado
- **ReglaService**: 
  - Depreciation: 10% por año (máx 50%)
  - Auto-complements: Pintura automática si RequierePintura=true
- **WorkflowService**: Transiciones de estado con validación

#### API Endpoints Actuales:
```
✅ GET    /api/presupuestos
✅ GET    /api/presupuestos/{id}
✅ GET    /api/presupuestos/estado/{estado}
✅ POST   /api/presupuestos
✅ PUT    /api/presupuestos/{id}
✅ DELETE /api/presupuestos/{id}
✅ POST   /api/presupuestos/{id}/cambiar-estado
```

#### Lógica de Negocio Principal:
1. **Crear** presupuesto en estado "Borrador"
2. **Aplicar reglas** de negocio (depreciación, complementos)
3. **Calcular** totales con IVA y márgenes
4. **Cambiar estado** con validaciones de workflow
5. **Cerrar** presupuesto cuando se completa la reparación
6. **Facturar** para generar venta

---

### 2️⃣ **MÓDULO CRM: Clientes & Relaciones**

**Responsabilidad:** Gestión de datos de clientes, historial e interacciones

#### Entidades Relacionadas:
```
Cliente (CORE)
├── Id (Guid)
├── Nombre
├── Historial (texto)
├── Preferencias (texto)
├── NPS (Net Promoter Score 0-10)
├── TasaRetencion (%)
└── Interacciones (1-a-N)
    ├── Interaccion
    │   ├── Id
    │   ├── Fecha
    │   ├── Tipo (llamada/email/reunión)
    │   └── Resultado

Presupuesto ←→ Cliente (1-a-N)
OrdenReparacion ←→ Cliente (1-a-N)
Factura ←→ Cliente (1-a-N) [FUTURA]
```

#### Servicios Principales:
- **ClienteService**: Operaciones CRM
- **ClienteRepository**: CRUD + búsqueda
- Cálculos: NPS promedio, tasa de retención

#### API Endpoints Actuales:
```
✅ GET    /api/clientes
✅ GET    /api/clientes/{id}
✅ POST   /api/clientes
✅ PUT    /api/clientes/{id}
✅ DELETE /api/clientes/{id}
✅ GET    /api/clientes/estadisticas
```

#### Lógica de Negocio Principal:
1. **Crear/actualizar** datos de clientes
2. **Registrar interacciones** (llamadas, emails, etc.)
3. **Calcular métricas** de relación (NPS, retención)
4. **Historial completo** de compras

---

### 3️⃣ **MÓDULO TALLER: Órdenes de Reparación**

**Responsabilidad:** Asignación de trabajos, seguimiento técnico y HH

#### Entidades Relacionadas:
```
OrdenReparacion (FUTURA)
├── Id (Guid)
├── Presupuesto (FK) ← Vinculada a presupuesto aprobado
├── TecnicoAsignado (nombre)
├── Inicio (DateTime)
├── Fin (DateTime nullable)
├── HorasReales (double)
└── Items de trabajo (1-a-N)

Presupuesto.Items (planeado) ← OrdenReparacion.Items (real)
```

#### Servicios Necesarios:
- **OrdenReparacionService**: CRUD y seguimiento
- **TecnicoService**: Gestión de técnicos disponibles
- Cálculos: Tiempo vs planificado, productividad por técnico

#### API Endpoints Futuros:
```
POST   /api/ordenes-reparacion/{presupuestoId}
GET    /api/ordenes-reparacion
GET    /api/ordenes-reparacion/{id}
PUT    /api/ordenes-reparacion/{id}
POST   /api/ordenes-reparacion/{id}/finalizar
```

---

### 4️⃣ **MÓDULO INVENTARIO: Refacciones & Stock**

**Responsabilidad:** Control de stock, alertas y movimientos

#### Entidades Relacionadas:
```
Refaccion (FUTURA)
├── SKU (Primary Key)
├── Nombre
├── Descripción
├── StockActual
├── StockMinimo
├── CostoPromedio
├── Proveedor (FK)
└── MovimientosInventario (1-a-N)

MovimientoInventario (FUTURA)
├── Id (Guid)
├── Refaccion (FK)
├── Tipo (Entrada/Salida/Ajuste)
├── Cantidad
├── Fecha
├── Referencia (Presupuesto/OrdenCompra)
└── Usuario

ItemPresupuesto ←→ Refaccion (cuando Tipo=Pieza)
```

#### Servicios Necesarios:
- **InventarioService**: Gestión de stock
- **MovimientoInventarioService**: Registro de movimientos
- Alertas: Stock bajo, vencimiento, reorden

#### API Endpoints Futuros:
```
GET    /api/inventario
GET    /api/inventario/{sku}
GET    /api/inventario/alertas
POST   /api/inventario/{sku}/movimiento
PUT    /api/inventario/{sku}
```

---

### 5️⃣ **MÓDULO COMPRAS: Órdenes de Compra & Proveedores**

**Responsabilidad:** Gestión de proveedores y órdenes de reabastecimiento

#### Entidades Relacionadas:
```
Proveedor (FUTURA)
├── Id (Guid)
├── Nombre
├── Contacto
├── Email
├── Teléfono
├── Dirección
├── Rating (1-5 estrellas)
└── OrdenesCo mpra (1-a-N)

OrdenCompra (FUTURA)
├── Id (Guid)
├── Proveedor (FK)
├── Fecha
├── Items (1-a-N)
│   ├── Refaccion
│   ├── CantidadSolicitada
│   ├── PrecioUnitario
│   └── Cantidad Recibida
├── Estado (Pendiente → Enviada → Recibida → Facturada)
├── Monto Total
└── FechaEntrega

Refaccion ←→ Proveedor (1-a-N)
OrdenCompra.Items ←→ Refaccion (N-a-1)
```

#### Servicios Necesarios:
- **ProveedorService**: Gestión de proveedores
- **OrdenCompraService**: CRUD y workflow
- Generación automática desde stock bajo

#### API Endpoints Futuros:
```
GET    /api/proveedores
POST   /api/proveedores
GET    /api/ordenes-compra
POST   /api/ordenes-compra
PUT    /api/ordenes-compra/{id}
POST   /api/ordenes-compra/{id}/recibir
```

---

### 6️⃣ **MÓDULO CALIDAD: Control de Calidad**

**Responsabilidad:** Inspecciones, checklists y garantía

#### Entidades Relacionadas:
```
ChecklistControl (EXISTE EN MODELO)
├── Id (Guid)
├── Responsable
├── Fecha
├── Aprobado (bool)
├── Observaciones
└── OrdenReparacion (FK) [FUTURA]

ReclamoGarantia (EXISTE EN MODELO)
├── Id (Guid)
├── Presupuesto (FK) [FUTURA]
├── Motivo
├── Resolucion
└── Fecha

QualityCheckItem (FUTURA)
├── Aspecto a verificar
├── Cumplido (bool)
├── Foto/Evidencia
└── Anotaciones
```

#### Servicios Necesarios:
- **QualityControlService**: Gestión de checklists
- **WarrantyService**: Gestión de reclamos y garantía

#### API Endpoints Futuros:
```
POST   /api/ordenes-reparacion/{id}/control-calidad
GET    /api/control-calidad/{id}
POST   /api/reclamos-garantia
GET    /api/reclamos-garantia
```

---

### 7️⃣ **MÓDULO ASSETS: Activos & Calibración**

**Responsabilidad:** Gestión de herramientas y equipos calibrables

#### Entidades Relacionadas:
```
Activo (EXISTE EN MODELO)
├── Id (Guid)
├── Nombre
├── Tipo (Herramienta/Equipo)
├── UltimaCalibracion (DateTime)
├── FrecuenciaCalibracion (TimeSpan)
└── CalibracionVencida (propiedad calculada)

CalibrationRecord (FUTURA)
├── Id (Guid)
├── Activo (FK)
├── FechasCalibracion
├── Próxima Calibración
└── CertificadoCalibración
```

#### Servicios Necesarios:
- **AssetService**: Gestión de activos
- **CalibrationService**: Seguimiento de calibraciones

#### API Endpoints Futuros:
```
GET    /api/activos
POST   /api/activos
GET    /api/activos/calibraciones-vencidas
POST   /api/activos/{id}/calibrar
```

---

### 8️⃣ **MÓDULO FINANZAS: Facturación & Pagos**

**Responsabilidad:** Facturación, cobros, flujo de caja

#### Entidades Relacionadas:
```
Factura (FUTURA)
├── Id (Guid)
├── Presupuesto (FK - 1-a-1 after cierre)
├── Cliente (FK)
├── NumeroFactura
├── Fecha
├── Monto
├── Impuestos
├── Total
├── Vencimiento
└── Estado (Pendiente → Pagada → Vencida)

Pago (FUTURA)
├── Id (Guid)
├── Factura (FK)
├── Monto
├── Fecha
├── Método (Efectivo/Tarjeta/Transferencia)
└── Referencia

CuentaPorCobrar (FUTURA)
├── Cliente
├── Monto Pendiente
├── Días Vencida
├── Último Pago
```

#### Servicios Necesarios:
- **InvoiceService**: Generación de facturas desde presupuestos
- **PaymentService**: Registro de pagos
- **FinanceReportService**: Reportes financieros

#### API Endpoints Futuros:
```
GET    /api/facturas
POST   /api/facturas/{presupuestoId}
POST   /api/pagos
GET    /api/cuentas-por-cobrar
POST   /api/reportes/flujo-caja
```

---

### 9️⃣ **MÓDULO RH: Recursos Humanos** (Opcional)

**Responsabilidad:** Gestión de técnicos, asignación de trabajos

#### Entidades Relacionadas:
```
Tecnico (FUTURA)
├── Id (Guid)
├── Nombre
├── Email
├── Teléfono
├── Especialización
├── DisponibilidadActual
├── HorasTrabajadasMes
└── OrdenesCo signadas (1-a-N)

Disponibilidad (FUTURA)
├── Tecnico (FK)
├── Día
├── HorasDisponibles
└── OrdenesAsignadas
```

---

## 🔄 Flujos Principales de Negocio

### Flujo 1: Del Presupuesto a la Reparación
```
1. CLIENTE solicita presupuesto
   ↓
2. PRESUPUESTO creado (Borrador)
   ├─ Vehiculo registrado
   ├─ Items añadidos
   └─ Tarifa aplicada
   ↓
3. REGLAS aplicadas (depreciation, complements)
   ↓
4. CÁLCULO de total con IVA
   ↓
5. CLIENTE aprueba → Estado "Aprobado"
   ↓
6. ORDEN DE REPARACIÓN creada
   ├─ Técnico asignado
   └─ Herramientas reservadas
   ↓
7. REPARACIÓN en progreso
   ├─ Refacciones descontadas de inventario
   ├─ Horas reales registradas
   └─ Control de calidad
   ↓
8. REPARACIÓN completada
   ├─ Presupuesto → Estado "Cerrado"
   └─ Checklist de calidad aprobado
   ↓
9. FACTURACIÓN
   ├─ Factura generada
   └─ Presupuesto → Estado "Facturado"
   ↓
10. PAGO recibido
```

### Flujo 2: Gestión de Inventario
```
STOCK BAJO detectado (< StockMinimo)
   ↓
ORDEN DE COMPRA generada automáticamente
   ↓
PROVEEDOR notificado (email/API)
   ↓
ORDEN enviada → Estado "Enviada"
   ↓
RECEPCIÓN → Refacción recibida
   ↓
STOCK actualizado
   ↓
MOVIMIENTO registrado en histórico
```

### Flujo 3: Control de Calidad
```
ORDEN REPARACIÓN completada
   ↓
CHECKLIST DE CALIDAD
   ├─ Inspecciones visuales
   ├─ Pruebas funcionales
   └─ Documentación
   ↓
SI Aprobado → Presupuesto a "Cerrado"
   ↓
SI No Aprobado → Retrabajos necesarios
   ↓
RECLAMO DE GARANTÍA (si aplica)
```

---

## 🎨 Modelo Entidad-Relación (ER)

```
┌──────────────────┐         ┌──────────────────┐
│   Vehiculo       │◄────1───┤   Presupuesto    │
│ (PK: VIN)        │         │ (PK: Id)         │
└──────────────────┘         └────────┬─────────┘
                                      │
                          ┌───────────┼───────────┐
                          │           │           │
                       1-N│           │           │1-N
                    ┌─────▼──────┐   │  ┌─────────▼──────┐
                    │ItemPresup.│   │  │  Tarifa        │
                    │(PK: Id)   │   │  │ (PK: Id)       │
                    └────────────┘   └──┤ TasaIVA: 0.16  │
                                       └────────────────┘

┌──────────────────┐
│   Cliente        │◄────1──┬──── Presupuesto
│ (PK: Id)         │        └───── OrdenReparacion
│                  │        ┌───── Factura
└────────┬─────────┘        └───── Pago
         │
      1-N│
    ┌────▼──────────┐
    │Interaccion    │
    │(PK: Id)       │
    └───────────────┘

┌──────────────────┐         ┌──────────────────┐
│  OrdenReparacion │────1────┤  Presupuesto     │
│  (PK: Id)        │         │ (Estado=Aprobado)│
└────────┬─────────┘         └──────────────────┘
         │
      1-N│
    ┌────▼──────────┐
    │   Tecnico     │
    │(PK: Id)       │
    └───────────────┘

┌──────────────────┐      1-N  ┌──────────────────┐
│   Proveedor      │◄─────────┤   OrdenCompra    │
│  (PK: Id)        │          │ (PK: Id)         │
└──────────────────┘          └────────┬─────────┘
                                       │
                                    1-N│
                              ┌────────▼────────┐
                              │  Refaccion      │
                              │ (PK: SKU)       │
                              │ StockActual     │
                              │ StockMinimo     │
                              └─────────────────┘
                                      │
                                   1-N│
                              ┌────────▼──────────────┐
                              │ MovimientoInventario  │
                              │ (PK: Id)              │
                              └───────────────────────┘

┌──────────────────┐      1-N  ┌──────────────────┐
│     Factura      │◄─────────┤  Presupuesto     │
│  (PK: NumFact)   │          │ (Estado=Cerrado) │
└────────┬─────────┘          └──────────────────┘
         │
      1-N│
    ┌────▼──────────┐
    │     Pago      │
    │   (PK: Id)    │
    └───────────────┘
```

---

## 📊 Estado Actual vs. Faltante

### ✅ **IMPLEMENTADO**
- [x] Core: Presupuestos (CRUD completo)
- [x] CRM: Clientes (CRUD básico)
- [x] Tarífas y reglas de cálculo
- [x] Workflow de estados
- [x] API REST (13 endpoints)
- [x] UI Blazor (Dashboard + Listas)
- [x] Tests unitarios (17 tests)

### 🚧 **EN CONSTRUCCIÓN**
- [ ] Autenticación & Autorización
- [ ] Formularios CRUD en UI (crear/editar)
- [ ] Deployment inicial

### ❌ **FALTANTE**
- [ ] Módulo Taller (Órdenes de Reparación)
- [ ] Módulo Inventario (Refacciones & Stock)
- [ ] Módulo Compras (Órdenes & Proveedores)
- [ ] Módulo Finanzas (Facturas & Pagos)
- [ ] Módulo Calidad (Checklists & Reclamos)
- [ ] Módulo Assets (Activos & Calibración)
- [ ] Integración Audatex
- [ ] Notificaciones (Email/SMS)
- [ ] Dashboard con gráficas
- [ ] Búsqueda y filtros avanzados

---

## 🔗 Dependencias Entre Módulos

```
Presupuesto (CORE)
├─ Depende de: Tarifa, Cliente, Vehiculo
├─ Usado por: Orden Reparación, Factura, Pago
├─ Genera: Movimiento Inventario, Checklist Calidad
└─ Dispara: Flujo de Reparación

Orden Reparación
├─ Depende de: Presupuesto (Aprobado)
├─ Asigna: Técnico, Activos
├─ Genera: Movimiento Inventario
└─ Precede: Facturación

Inventario
├─ Monitorea: Stock de Refacciones
├─ Genera: Orden Compra automática
├─ Consume: Durante Orden Reparación
└─ Alimenta: Análisis de costo

Orden Compra
├─ Depende de: Stock bajo, Proveedor
├─ Actualiza: Inventario
└─ Precede: Control de Calidad de entrada

Facturación
├─ Depende de: Presupuesto (Cerrado)
├─ Registra: Pago
└─ Genera: Reporte Financiero
```

---

## 🎯 Prioridades de Desarrollo

**Fase 1: MVP Producción** (Actual)
- [x] Presupuestos básico
- [x] CRM básico
- [ ] Autenticación
- [ ] Formularios CRUD en UI
- [ ] Deployment

**Fase 2: ERP Core** (Semanas 3-4)
- [ ] Orden Reparación
- [ ] Inventario
- [ ] Compras

**Fase 3: Operaciones** (Semanas 5-6)
- [ ] Facturación
- [ ] Control de Calidad
- [ ] Integración Audatex

**Fase 4: Inteligencia** (Semanas 7-8)
- [ ] Dashboard con KPIs
- [ ] Reportes avanzados
- [ ] Búsqueda y filtros

**Fase 5: Mejora Continua** (Weeks 9+)
- [ ] 2FA y seguridad adicional
- [ ] Notificaciones automáticas
- [ ] Mobile app
- [ ] Analytics

---

## 📝 Notas Arquitectónicas

1. **Patrón Repository**: Abstracción de datos para facilitar testing y cambios de BD
2. **Servicios con Lógica**: Separación clara entre CRUD y reglas de negocio
3. **Enums para Estados**: Type-safe workflow (Borrador, Aprobado, etc.)
4. **DTOs Futuros**: Separar modelos de dominio de modelos API (para évitar over-sharing)
5. **Auditoría**: Agregar CreatedAt, UpdatedAt, ModifiedBy a entidades críticas
6. **Soft Delete**: Implementar borrado lógico en lugar de físico (para auditoría)

---

**Última actualización:** 6 de diciembre de 2025  
**Estado:** Arquitectura v1.0 - Lista para implementación
