# 🚀 AtelierPro ERP - Arquitectura Empresarial Completa (México)

## 📋 Visión General

Sistema ERP especializado para talleres automotrices en México con capacidad de:
- ✅ Gestión integral de RH
- ✅ Facturación Electrónica (CFDI)
- ✅ Integración bancaria
- ✅ Portal B2B para aseguradoras
- ✅ Almacén avanzado
- ✅ Compliance fiscal mexicano

---

## 🏗️ MÓDULOS EMPRESARIALES (13 TOTAL)

### CORE (Actuales)
1. ✅ **Presupuestos & Siniestros**
2. ✅ **CRM**

### NIVEL 1: Operaciones (Nuevos)
3. 🔧 **Taller** (Órdenes de Reparación)
4. 📦 **Almacén** (MEJORADO)
5. 🛒 **Compras** (MEJORADO)
6. 📋 **Órdenes de Servicio** (NUEVO)

### NIVEL 2: Finanzas & Cumplimiento (Nuevos)
7. 💰 **Finanzas** (MEJORADO)
8. 📄 **Facturación Electrónica** (NUEVO - SAT/CFDI)
9. 🏦 **Tesorería & Pagos** (NUEVO)
10. 📊 **Contabilidad** (NUEVO)

### NIVEL 3: Recursos & Portales (Nuevos)
11. 👥 **Recursos Humanos** (COMPLETO - NUEVO)
12. 🏢 **Portal Aseguradoras** (NUEVO - B2B/API)
13. 📈 **Inteligencia de Negocio** (NUEVO - Dashboards)

---

## 🎯 MEJORAS DETALLADAS POR MÓDULO

---

## 1️⃣ MÓDULO: RECURSOS HUMANOS (COMPLETO)

### Entidades Principales

```csharp
// Estructura organizacional
Departamento
├── Id: Guid
├── Nombre: string
├── Descripcion: string
├── Responsable: Empleado (FK)
├── PresupuestoMensual: decimal
└── Empleados: List<Empleado>

Empleado
├── Id: Guid
├── NumeroEmpleado: string (unique)
├── Nombre: string
├── ApellidoPaterno: string
├── ApellidoMaterno: string
├── RFC: string (unique, validado)
├── CURP: string (unique, validado)
├── FechaNacimiento: DateTime
├── Genero: string (M/F/O)
├── EstadoCivil: string
├── Nacionalidad: string
├── Direccion: string
├── Telefono: string
├── Email: string
├── FechaIngreso: DateTime
├── FechaSalida: DateTime?
├── Estatus: EmpleadoStatus (Activo|Suspendido|Despedido|Renuncia)
├── Puesto: Puesto (FK)
├── Departamento: Departamento (FK)
├── Supervisor: Empleado? (Self-referencing, nullable)
├── SalarioBase: decimal
├── TipoContrato: TipoContrato (Indefinido|Temporal|Practicante)
├── NumeroSSN: string
├── BancoNomina: string
├── CuentaBancaria: string
├── CLABE: string (validado)
├── PermisoManejoLicencia: string?
├── VigenciaLicencia: DateTime?
├── AntecedentesNoDelictivos: bool
├── FechaVerificacion: DateTime?
└── DocumentosPersonal: List<DocumentoPersonal>

Puesto
├── Id: Guid
├── Nombre: string
├── Descripcion: string
├── NivelSalario: int
├── SalarioMinimo: decimal
├── SalarioMaximo: decimal
├── Departamento: Departamento (FK)
└── Responsabilidades: string

// Nómina y pagos
Nomina
├── Id: Guid
├── Periodo: string (Mes-Año)
├── FechaGeneracion: DateTime
├── FechaPago: DateTime
├── Estado: NominaStatus (Borrador|Procesada|Pagada|Cancelada)
├── Items: List<ItemNomina> (1-a-N)
├── MontoTotal: decimal
├── MontoImpuestos: decimal
├── MontoAportaciones: decimal
└── Observaciones: string

ItemNomina
├── Id: Guid
├── Empleado: Empleado (FK)
├── DiasLaborales: int
├── SalarioPeriodo: decimal
├── Ausencias: decimal
├── Bonificaciones: decimal
├── Comisiones: decimal
├── Deducciones: decimal
├── ISR: decimal (calculado)
├── IMSS: decimal (3.625% trabajador + 20.4% patrón)
├── INFONAVIT: decimal (5% patrón)
├── Otros: decimal
├── NetoAPagar: decimal
├── Estatus: ItemNominaStatus (Pendiente|Procesado|Pagado)
└── FechaPago: DateTime?

// Asistencia y puntualidad
Asistencia
├── Id: Guid
├── Empleado: Empleado (FK)
├── Fecha: DateTime
├── HoraEntrada: TimeSpan
├── HoraSalida: TimeSpan?
├── TiempoLaborado: double (horas)
├── TipoRegistro: TipoAsistencia (Normal|Falta|Retardo|PermisoPorHora|Incapacidad)
├── Observaciones: string
└── RegistradoPor: Empleado (FK - supervisor)

Incapacidad
├── Id: Guid
├── Empleado: Empleado (FK)
├── FechaInicio: DateTime
├── FechaFin: DateTime
├── Razon: string (Enfermedad|Accidente|Maternidad|etc)
├── NumeroControlINSS: string
├── Documento: string (URL del archivo)
└── Validada: bool

// Vacaciones y permisos
VacacionPermiso
├── Id: Guid
├── Empleado: Empleado (FK)
├── Tipo: TipoPermiso (Vacaciones|PermisoPersonal|PermisoProfesional|LicenciaNoDr)
├── FechaInicio: DateTime
├── FechaFin: DateTime
├── DiasUtilizados: int
├── DiasDisponibles: int
├── Razon: string
├── Estado: PermisoStatus (Solicitado|Aprobado|Rechazado|Cancelado)
├── AprobadoPor: Empleado (FK)
├── FechaAprobacion: DateTime?
└── Observaciones: string

// Evaluaciones y capacitación
EvaluacionDesempenio
├── Id: Guid
├── Empleado: Empleado (FK)
├── Periodo: int (Trimestre 1-4)
├── Año: int
├── Evaluador: Empleado (FK)
├── PuntajeProductividad: int (1-100)
├── PuntajeCalidad: int
├── PuntajeAsistencia: int
├── PuntajePuntualidad: int
├── PuntajeTrabajoenEquipo: int
├── PuntajeTotal: int
├── Recomendaciones: string
├── Oportunidades: string
└── PromocionPropuesta: bool

CapacitacionEntrenamiento
├── Id: Guid
├── Nombre: string
├── Descripcion: string
├── Temas: List<string>
├── FechaInicio: DateTime
├── FechaFin: DateTime
├── Instructor: string
├── Ubicacion: string
├── Certificado: bool
├── Empleados: List<Empleado> (N-a-N)
└── Observaciones: string

// Documentos personales
DocumentoPersonal
├── Id: Guid
├── Empleado: Empleado (FK)
├── Tipo: TipoDocumento (RFC|CURP|IFE|Licencia|etc)
├── Numero: string
├── FechaExpiracion: DateTime?
├── Documento: string (URL)
└── FechaRegistro: DateTime

// Contactos de emergencia
ContactoEmergencia
├── Id: Guid
├── Empleado: Empleado (FK)
├── Nombre: string
├── Relacion: string
├── Telefono: string
└── Prioridad: int (1-3)
```

### Servicios RH

```csharp
// Servicios principales
EmpleadoService
- CrearEmpleado(dto) → Validar RFC/CURP
- ActualizarEmpleado(id, dto)
- ObtenerEmpleadosPorDepartamento(deptId)
- ObtenerHistorialLaboral(empleadoId)
- CalcularAntiguedad(fechaIngreso)

NominaService
- GenerarNomina(periodo) → Crear ItemNomina
- CalcularISR(salarioBase, deduccciones)
- CalcularIMSS(salarioBase, tipo)
- CalcularINFONAVIT(salarioBase)
- ProcesarNomina(nominaId)
- ExportarNominaBancaria() → XML/TXT para banco

AsistenciaService
- RegistrarEntrada(empleadoId, hora)
- RegistrarSalida(empleadoId, hora)
- GenerarReporteAsistencia(periodo)
- DetectarRetardos()
- CalcularHorasExtras()

VacacionesService
- SolicitarVacacion(empleadoId, fechas)
- AprobarVacacion(solicitudId, aprobadoPor)
- CalcularDiasDisponibles(empleadoId, año)
- ActualizarDiasVacacionalesAnuales()

CapacitacionService
- AsignarCapacitacion(empleadoId, capacitacionId)
- GenerarCertificado(capacitacionId, empleadoId)
- TrackingCapacitacion(empleadoId)
```

### API Endpoints RH

```
# Empleados
POST   /api/rh/empleados
GET    /api/rh/empleados
GET    /api/rh/empleados/{id}
PUT    /api/rh/empleados/{id}
DELETE /api/rh/empleados/{id}
GET    /api/rh/empleados/departamento/{deptId}
GET    /api/rh/empleados/{id}/historial

# Nómina
POST   /api/rh/nomina/{periodo}
GET    /api/rh/nomina/{nominaId}
POST   /api/rh/nomina/{nominaId}/procesar
POST   /api/rh/nomina/{nominaId}/pagar
GET    /api/rh/nomina/export-bancaria
GET    /api/rh/nomina/historial/{empleadoId}

# Asistencia
POST   /api/rh/asistencia/entrada/{empleadoId}
POST   /api/rh/asistencia/salida/{empleadoId}
GET    /api/rh/asistencia/reporte/{periodo}

# Vacaciones
POST   /api/rh/vacaciones/solicitar
PUT    /api/rh/vacaciones/{solicitudId}/aprobar
GET    /api/rh/vacaciones/disponibles/{empleadoId}

# Capacitación
POST   /api/rh/capacitacion
GET    /api/rh/capacitacion/{id}/empleados
POST   /api/rh/capacitacion/{id}/asignar/{empleadoId}
```

---

## 2️⃣ MÓDULO: FACTURACIÓN ELECTRÓNICA (SAT/CFDI México)

### Entidades

```csharp
// Configuración CFDI
ConfiguracionSAT
├── Id: Guid
├── RFC: string (del taller)
├── RazonSocial: string
├── CURP: string (del representante legal)
├── Direccion: string
├── TipoSociedad: string (SA|SAPI|LLC|etc)
├── RegimenFiscal: string (601-Residente|605-Extranjero)
├── SeriesCFDI: List<SerieCFDI>
├── CertificadoDigital: string (archivo .cer)
├── LlavePrivada: string (archivo .key)
├── PasswordCertificado: string (encrypted)
├── ProveedorCertificacion: string (Thales|ValidacionSAT|etc)
└── FechaVigenciaDesde: DateTime

SerieCFDI
├── Id: Guid
├── Serie: string (A-Z, máx 25 caracteres)
├── FolioActual: long
├── FolioFinal: long
└── Vigente: bool

// Factura electrónica
FacturaElectronica (extends Factura)
├── Id: Guid
├── NumeroFactura: string (Serie + Folio)
├── Folio: long
├── Serie: string
├── UUID: string (Unique, asignado por SAT)
├── FechaEmision: DateTime
├── FechaVencimiento: DateTime
├── FormaPago: FormaPagoSAT (Enum: 01=Efectivo, 03=Transferencia, etc)
├── MethodoPago: MethodoPagoSAT (Enum: PUE=Pago en una emisión, PPD=Pago programado)
├── Moneda: string (MXN|USD|EUR)
├── TipoCambio: decimal (si no es MXN)
├── CondicionesPago: string
├── ReferenciaPago: string
├── Receptor: ReceptorCFDI
├── Items: List<ItemFacturaElectronica>
├── Subtotal: decimal
├── Descuentos: decimal
├── Impuestos: List<ImpuestoCFDI>
├── Total: decimal
├── EstadoCFDI: EstadoCFDI (Vigente|Cancelada)
├── FechaCancelacion: DateTime?
├── MotivoCancelacion: string
├── FolioCancelacion: string?
├── Observaciones: string
├── XMLGenerado: string (almacenar XML completo)
├── XMLTimbrado: string (XML con timbrado SAT)
├── URLDescarga: string (donde descargar XML)
├── StatusSAT: string (Vigente|Cancelada|etc)
└── AceptacionCliente: AceptacionCFDI?

ReceptorCFDI
├── Id: Guid
├── RFC: string (del cliente)
├── RazonSocial: string
├── Direccion: string
├── UsoCFDI: UsoCFDI (Enum: G01=Adquisición Mercancías, etc)
├── TipoPersona: TipoPersona (Física|Moral)
├── RegímenFiscal: string
└── Pais: string (default: MX)

ItemFacturaElectronica
├── Id: Guid
├── ClaveProdServ: string (SAT - 18 dígitos)
├── Descripcion: string
├── ClaveUnidad: string (SAT - Pieza=H87, Hora=HUR, etc)
├── Cantidad: decimal
├── PrecioUnitario: decimal
├── Subtotal: decimal
├── DescuentoItem: decimal
├── TotalItem: decimal
├── NumSerieProducto: string?
└── NumPartida: int?

ImpuestoCFDI
├── Id: Guid
├── Tipo: TipoImpuesto (IVA|IEPS|ISR)
├── Base: decimal
├── Tasa: decimal (0.16 para IVA)
├── Importe: decimal
└── TrasladoRetencion: string (Traslado|Retención)

AceptacionCFDI
├── Id: Guid
├── FacturaElectronica: FacturaElectronica (FK)
├── FechaAceptacion: DateTime
├── RespuestaAceptacion: RespuestaAceptacion (Aceptada|Rechazo|QuejaEnCorreccion)
├── MotivosRechazo: List<MotivoRechazo>
├── Observaciones: string
└── UsuarioCliente: Usuario (FK)

// Nota de crédito/débito
NotaCreditoDebito
├── Id: Guid
├── Tipo: TipoNota (NotaCredito|NotaDebito)
├── FacturaOriginal: FacturaElectronica (FK)
├── Motivo: MotivoNota (01=Devolución|02=Descuento|03=Ajuste Precio|etc)
├── Descripcion: string
├── Items: List<ItemNotaCreditoDebito>
├── MontoOriginal: decimal
├── MontoAjuste: decimal
├── NumeroNotaCFDI: string
├── XMLGenerado: string
└── FechaEmision: DateTime

// Complementos CFDI
ComplementoPago
├── Id: Guid
├── NumeroOperacion: string
├── Fecha: DateTime
├── RFCEmisor: string
├── RFCReceptor: string
├── Monto: decimal
├── FormaPago: FormaPagoSAT
├── BancoOrdenante: string?
├── CuentaOrdenante: string?
├── BancoReceptor: string?
├── CuentaReceptor: string?
└── FacturasRelacionadas: List<FacturaElectronica>
```

### Servicios de Facturación Electrónica

```csharp
FacturaElectronicaService
- GenerarCFDI(presupuestoId) → Crear FacturaElectronica
- TimbrarCFDI(facturaId) → Conectar con PAC (Proveedor Autorizado Certificación)
- ObtenerXMLTimbrado(facturaId) → Recuperar del PAC
- DescargarPDF(facturaId) → Generar PDF con código QR
- DescargarXML(facturaId) → Descargar XML original
- CancelarCFDI(facturaId, motivo) → Cancelación ante SAT
- ValidarFormatoXML(xml) → Validación local
- EnviarCorreo(facturaId, emailCliente) → Enviar CFDI por email

CFDIComplementoService
- GenerarComplementoPago(pagosIds) → Crear complemento de pago
- AgregarComplementoPago(cfdiiId, comploId) → Relacionar complementos
- ValidarComplementoPago(complo) → Validar estructura

NotaCreditoService
- GenerarNotaCredito(facturaId, motivo, items)
- GenerarNotaDebito(facturaId, motivo, items)
- TimbrarNota(notaId)

CancelacionService
- SolicitarCancelacion(facturaId, motivo)
- AcusarReciboCancelacion(folioSAT)
- ObtenerStatusCancelacion(facturaId)

PACIntegrationService (Conexión con PAC)
- TimbrarCFDI(xml) → Llamada a PAC (Thales, FINKOK, etc)
- CancelarCFDI(uuid, motivo) → SAT
- ObtenerEstatus(uuid) → Query a SAT
- DescargarXMLTimbrado(uuid) → Desde PAC
```

### Flujo de Facturación CFDI

```
1. Presupuesto CERRADO
   ↓
2. FacturaElectronicaService.GenerarCFDI()
   ├─ Crear FacturaElectronica
   ├─ Asignar Folio = FolioActual++
   ├─ AsignarUUID (temporal)
   ├─ Generar XML (estructura SAT)
   └─ Estado = EnProceso
   ↓
3. FacturaElectronicaService.TimbrarCFDI()
   ├─ Conectar a PAC (vía WebService)
   ├─ Enviar XML sin sellar
   ├─ PAC valida contra SAT
   ├─ PAC retorna XML timbrado + UUID real
   ├─ Almacenar XML timbrado
   └─ Estado = Timbrada
   ↓
4. FacturaElectronicaService.DescargarPDF()
   ├─ Generar PDF con datos CFDI + código QR
   ├─ QR contiene: UUID + RFC + Monto
   └─ Retornar PDF
   ↓
5. EmailService.EnviarFactura()
   ├─ Enviar XML al cliente
   ├─ Enviar PDF descargable
   ├─ Incluir link a descarga
   └─ AceptacionCFDI.UsuarioCliente ← Cliente
   ↓
6. (Opcional) CancelacionService.SolicitarCancelacion()
   ├─ Solo si presupuesto es rechazado
   ├─ Conectar a SAT
   ├─ Motivo de cancelación
   └─ Estado = Cancelada
```

### API Endpoints Facturación

```
# Generación y timbrado
POST   /api/facturacion/generar/{presupuestoId}
GET    /api/facturacion/{facturaId}
POST   /api/facturacion/{facturaId}/timbrar
POST   /api/facturacion/{facturaId}/cancelar

# Descargas
GET    /api/facturacion/{facturaId}/xml
GET    /api/facturacion/{facturaId}/pdf
GET    /api/facturacion/{facturaId}/zip (xml+pdf+acuse)

# Complementos
POST   /api/facturacion/{facturaId}/complemento-pago
GET    /api/facturacion/complementos/{periodo}

# Notas de crédito/débito
POST   /api/facturacion/{facturaId}/nota-credito
POST   /api/facturacion/{facturaId}/nota-debito
GET    /api/facturacion/notas/{periodo}

# Estado y seguimiento
GET    /api/facturacion/{uuid}/estado-sat
GET    /api/facturacion/pendientes-timbrado
GET    /api/facturacion/canceladas/{periodo}
```

---

## 3️⃣ MÓDULO: TESORERÍA & PAGOS BANCARIOS

### Entidades

```csharp
ConfiguracionBancaria
├── Id: Guid
├── Banco: string (BBVA|Santander|Banamex|etc)
├── NumCuenta: string
├── CLABE: string (18 dígitos)
├── RFC: string
├── UsuarioOnline: string (encrypted)
├── PasswordOnline: string (encrypted)
├── ApiKey: string (encrypted)
├── ApiSecret: string (encrypted)
├── Endpoint: string (URL conexión banco)
├── SaldoActual: decimal
├── FechaActualizacion: DateTime
└── Vigente: bool

// Movimientos bancarios
MovimientoBancario
├── Id: Guid
├── Referencia: string
├── Tipo: TipoMovimiento (Entrada|Salida|Transferencia|Comisión)
├── Monto: decimal
├── Fecha: DateTime
├── Descripcion: string
├── Transaccion: Transaccion? (FK - nullable)
├── FacturaRelacionada: FacturaElectronica? (FK)
├── StatusBanco: string (Procesado|Pendiente|Rechazado)
└── NotaBanco: string

// Pagos a proveedores
PagoProveedor
├── Id: Guid
├── Proveedor: Proveedor (FK)
├── MontoTotal: decimal
├── OrdenesCom praPagadas: List<OrdenCompra> (N-a-N)
├── FechaPagoSolicitada: DateTime
├── FechaPagoRealizada: DateTime?
├── Metodo: MetodoPago (Transferencia|Cheque|Efectivo)
├── Referencia: string (NumTransferencia|NumCheque)
├── Banco: string
├── CuentaOrigen: string (CLABE del taller)
├── CuentaDestino: string (CLABE del proveedor)
├── Estado: EstadoPago (Pendiente|Procesado|Confirmado|Rechazado)
├── StatusBanco: string
├── DocumentoSoporte: string (Comprobante pago)
└── Observaciones: string

RequisicionPago
├── Id: Guid
├── Numero: string
├── Fecha: DateTime
├── Solicitante: Empleado (FK)
├── MontoSolicitado: decimal
├── Justificacion: string
├── Archivo: string (URL)
├── Estado: EstadoRequisicion (Solicitada|Aprobada|Rechazada|Pagada)
├── AprobadoPor: Usuario (FK)
├── FechaAprobacion: DateTime?
├── PagoProcesado: PagoProveedor? (FK)
└── ObservacionesRe chazo: string

// Reconciliación bancaria
ReconciliacionBancaria
├── Id: Guid
├── Periodo: string (Mes-Año)
├── FechaInicio: DateTime
├── FechaFin: DateTime
├── SaldoBancoSegun: decimal
├── SaldoLibrosSegun: decimal
├── Diferencia: decimal
├── Partidas Conciliadas: List<PartidaConciliada>
├── PartidasPendientes: List<PartidaPendiente>
├── Estado: EstadoReconciliacion (EnProceso|Conciliada|Diferencias)
├── Reconciliador: Usuario (FK)
├── FechaReconciliacion: DateTime?
└── Observaciones: string

PartidaConciliada
├── Id: Guid
├── MovimientoBancario: MovimientoBancario (FK)
├── RegistroContable: RegistroContable (FK)
├── FechaConciliacion: DateTime
└── Reconciliador: Usuario

PartidaPendiente
├── Id: Guid
├── Tipo: string (BancoNoLibros|LibrosNoBanco)
├── Descripcion: string
├── Monto: decimal
├── Dias: int
├── Accion: string (Investigar|Ajuste|etc)
└── Responsable: Usuario

// Flujo de caja
ProyeccionFlujoCaja
├── Id: Guid
├── Periodo: int (Mes)
├── Año: int
├── EntradasEsperadas: decimal
├── SalidasEsperadas: decimal
├── SaldoFinal: decimal
├── ActualizadoAl: DateTime
└── Escenarios: List<EscenarioFlujoCaja>

EscenarioFlujoCaja
├── Optimista: decimal
├── Pesimista: decimal
├── RealmenteRealizado: decimal?
└── Varianza: decimal?
```

### Servicios de Tesorería

```csharp
PagoProveedorService
- CrearRequisicionPago(proveedor, montos, concepto)
- AprobarRequisicion(requisicionId, aprobador)
- ProcesarPago(requisicionId) → Transferencia bancaria
- ConfirmarPago(pagoId, comprobanteBank)
- ObtenerStatusPago(pagoId)

ReconciliacionBancariaService
- ObtenerSaldoBanco() → WebService del banco
- ObtenerMovimientosBank(fechaInicio, fechaFin)
- CrearPartidaConciliada(movBanco, registroContable)
- IdentificarDiferencias()
- GenerarReporteReconciliacion()
- AlertarDiferencias() → Email a contador

FlujoCajaService
- ProyectarFlujoCaja(periodo) → Análisis de entradas/salidas
- CalcularRequerimiento(periodo) → Dinero mínimo necesario
- AvisarFaltante() → Si proyección indica falta de liquídez
- CompararConRealizado()

TransferenciaBancariaService
- CrearTransferencia(origen, destino, monto, concepto)
- EnviarABanco() → Vía API bancaria
- TrackearTransferencia(referenciaTransferencia)
- ReintentoAutomatic() → Si falla
```

### API Endpoints Tesorería

```
# Configuración
POST   /api/tesoreria/config-bancaria
GET    /api/tesoreria/saldo-actual
PUT    /api/tesoreria/saldo-actualizar (forzar sincronización)

# Pagos a proveedores
POST   /api/tesoreria/requisicion-pago
GET    /api/tesoreria/requisiciones/{estado}
PUT    /api/tesoreria/requisicion/{id}/aprobar
POST   /api/tesoreria/requisicion/{id}/pagar
GET    /api/tesoreria/pagos/{periodo}
GET    /api/tesoreria/pagos/proveedor/{proveedorId}

# Reconciliación
POST   /api/tesoreria/reconciliacion/{periodo}
GET    /api/tesoreria/reconciliacion/{id}
POST   /api/tesoreria/reconciliacion/{id}/conciliar
GET    /api/tesoreria/reconciliacion/pendientes

# Flujo de caja
GET    /api/tesoreria/flujo-caja/{periodo}
GET    /api/tesoreria/flujo-caja/proyeccion/{meses}
GET    /api/tesoreria/flujo-caja/analisis

# Movimientos
GET    /api/tesoreria/movimientos/{periodo}
GET    /api/tesoreria/movimientos/banco
```

---

## 4️⃣ MÓDULO: ALMACÉN (MEJORADO)

### Extensiones a Refacción

```csharp
Refaccion (MEJORADO)
├── Sku: string (PK)
├── Nombre: string
├── ... (campos anteriores)
├── CodigoBarras: string
├── CodigoQR: string
├── UbicacionAlmacen: string (Pasillo-Estante-Nivel)
├── StockBloqueado: int (reservado para órdenes)
├── StockDañado: int (no disponible)
├── FechaIngreso: DateTime
├── FechaSalida: DateTime?
├── ProveedorPrincipal: Proveedor (FK)
├── CostoUltima Entrada: decimal
├── CostoProm edio: decimal (PEPS)
├── PeriodicidadRecuento: int (días)
├── UltimoRecuento: DateTime?
└── ContenedorMaterial: string? (si aplica)

// Nueva entidad: Inventario por ubicación
InventarioUbicacion
├── Id: Guid
├── Refaccion: Refaccion (FK)
├── Ubicacion: string
├── Stock: int
├── FechaActualizacion: DateTime
└── VerificadoPor: Empleado (FK)

// Ciclo de conteos
CuentoFísico
├── Id: Guid
├── Tipo: TipoCuento (Diario|Semanal|Mensual|Total)
├── Fecha: DateTime
├── FechaInicio: DateTime
├── FechaFin: DateTime?
├── Personal: List<Empleado>
├── Refacciones: List<ItemCuentoFísico>
├── Estado: EstadoCuento (Planificado|EnCurso|Completado|Verificado)
├── Diferencias: List<DiferenciaCuento>
└── Observaciones: string

ItemCuentoFísico
├── Refaccion: Refaccion
├── StockSistema: int
├── StockContado: int
├── Diferencia: int
├── UsuarioConteo: Empleado (FK)
├── Horaconteo: DateTime
└── Observaciones: string

DiferenciaCuento
├── Refaccion: Refaccion
├── DiferenciaCantidad: int
├── Causa: string (Robo|Error|Daño|Error Sistema)
├── Accion: string (Ajuste|Investigación)
└── Responsable: Empleado
```

### Entidades de Órdenes de Servicio

```csharp
// Órdenes de servicio (para trabajos indirectos)
OrdenServicio
├── Id: Guid
├── Numero: string
├── ClienteId: Guid (FK) → Cliente (puede ser interno)
├── FechaSolicitud: DateTime
├── Descripcion: string
├── Tipo: TipoServicio (Mantenimiento|Reparación|Adecuación|Otros)
├── Contacto: string (nombre persona)
├── Telefono: string
├── Ubicacion: string
├── Items: List<ItemOrdenServicio>
├── MontoPresupuestado: decimal
├── MontoReal: decimal?
├── FechaEjecucion: DateTime?
├── FechaComplecion: DateTime?
├── Responsable: Empleado (FK)
├── Estado: EstadoOrdenServicio (Registrada|Asignada|EnEjecución|Completada|Cancelada)
├── RequierePresupuesto: bool
├── PresupuestoGenerado: Presupuesto? (FK)
├── ReporteComplecion: ReporteServicio?
└── Observaciones: string

ItemOrdenServicio
├── Id: Guid
├── Descripcion: string
├── Cantidad: int
├── PrecioUnitario: decimal
├── Subtotal: decimal
├── TipoItem: TipoItemServicio (Material|ManoObra|Herramientas)
├── Refaccion: Refaccion? (FK - si es material)
└── Observaciones: string

ReporteServicio
├── Id: Guid
├── OrdenServicio: OrdenServicio (FK)
├── FechaInicio: DateTime
├── FechaFin: DateTime
├── TiempoTotal: double (horas)
├── PersonalAsignado: List<Empleado>
├── Descripcion: string
├── Observaciones: string
├── FotosBefore: List<string> (URLs)
├── FotosAfter: List<string> (URLs)
├── Firmas: List<FirmaAprobacion>
└── AprobadoPor: Usuario (FK)

FirmaAprobacion
├── Nombre: string
├── Rol: string
├── Fecha: DateTime
└── ImagenSignatura: string (base64)
```

### Servicios de Almacén

```csharp
AlmacenService
- ObtenerStockDisponible(sku) → Stock - Bloqueado - Dañado
- ActualizarUbicacion(sku, ubicacionNueva)
- BloquearStock(sku, cantidad, razon)
- DesbloquearStock(sku, cantidad)
- RegistrarRefaccion(dto) → con validación de datos
- ActualizarCostos(sku) → PEPS
- ObtenerRefaccionesProximas Caducar()

CuentoFísicoService
- CrearPlanificacion(tipo, fechas)
- AsignarPersonal(cuentoId, empleados)
- GenerarFormulaiosCuento()
- RegistrarConteo(itemCuentoId, cantidad)
- CalcularDiferencias()
- IdentificarCausas() → Análisis
- GenerarReporteDiferencias()

RecuadreinventarioService
- DetectarItems Faltantes()
- ReportarAlmacenista()
- SugerirAjustes()

OrdenServicioService
- CrearOrdenServicio(dto)
- GenerarPresupuesto(ordenServicioId)
- AsignarResponsable(ordenServicioId, empleadoId)
- CompletarOrdenServicio(ordenServicioId, reporteDto)
```

### API Endpoints Almacén

```
# Refacciones
GET    /api/almacen/refacciones
GET    /api/almacen/refacciones/{sku}
PUT    /api/almacen/refacciones/{sku}/ubicacion
GET    /api/almacen/stock-disponible/{sku}
GET    /api/almacen/refacciones/proximas-caducar

# Cuentos físicos
POST   /api/almacen/cuento-fisico
GET    /api/almacen/cuento-fisico/{id}
POST   /api/almacen/cuento-fisico/{id}/item
GET    /api/almacen/cuento-fisico/{id}/diferencias
POST   /api/almacen/cuento-fisico/{id}/completar

# Órdenes de servicio
POST   /api/almacen/orden-servicio
GET    /api/almacen/orden-servicio/{id}
POST   /api/almacen/orden-servicio/{id}/completar
GET    /api/almacen/orden-servicio/por-responsable/{empleadoId}
```

---

## 5️⃣ MÓDULO: PORTAL ASEGURADORAS (B2B/API)

### Estructura

```csharp
// Configuración de aseguradoras
Aseguradora
├── Id: Guid
├── Nombre: string
├── RFC: string
├── CodigoCertificacion: string (interno)
├── ContactoPrincipal: string
├── Telefono: string
├── Email: string
├── PortalURL: string
├── ApiEndpoint: string
├── ApiKey: string (encrypted)
├── ApiSecret: string (encrypted)
├── Estatus: bool (Activa)
├── PorcentajeComision: decimal?
├── TiempoMaximoRespuesta: int (horas para presupuesto)
├── CondicionesPago: string
├── DocumentacionRequerida: List<string>
└── ContactosAutorizados: List<ContactoAseguradora>

ContactoAseguradora
├── Nombre: string
├── Rol: string
├── Email: string
├── Telefono: string
└── Activo: bool

// Órdenes de siniestro desde API
SiniestroRecibido
├── Id: Guid
├── NumeroSiniestro: string (de la aseguradora)
├── Aseguradora: Aseguradora (FK)
├── FechaRecepcion: DateTime
├── FechaOcurrencia: DateTime
├── TipoSiniestro: TipoSiniestro (Accidente|Robo|Desastre Natural|etc)
├── NumeroPóliza: string
├── Pólizante: string (nombre cliente)
├── Vehiculo: VehiculoSiniestro
│   ├── VIN: string
│   ├── Placa: string
│   ├── Marca: string
│   ├── Modelo: string
│   ├── Año: int
│   └── Descripcion: string
├── Daños: string (descripción de daños)
├── FotosDelSiniestro: List<string> (URLs)
├── MontoAsegurado: decimal
├── MontoLimiteReparacion: decimal
├── TasaDeducible: decimal
├── DatosPerito: PeritoPasante? (si fue peritado)
├── EstadoRecepcion: EstadoSiniestro (RecibidoAPI|Validado|ConPresupuesto|RechazadoAPI)
├── FechaUltimaActualizacion: DateTime
├── ConcatenacionesJSON: string (JSON completo de aseguradora)
└── ErroresValidacion: List<ErrorValidacion>?

DatosPerito
├── Nombre: string
├── RFC: string
├── NumeroExpedientePeritaje: string
├── FechaPeritaje: DateTime
├── DiagnosticoPreliminar: string
├── PresupuestoPerito: decimal
├── DocumentoPeritaje: string (URL)
└── Observaciones: string

// Presupuesto enviado a aseguradora
PresupuestoEnviado
├── Id: Guid
├── SiniestroRecibido: SiniestroRecibido (FK)
├── Presupuesto: Presupuesto (FK)
├── FechaEnvio: DateTime
├── NumeroSeguimientoAseguradora: string (ID remoto)
├── ConFormatos: bool (XML/JSON)
├── URLDescargaAseguradora: string
├── EstadoRespuestaAseguradora: EstadoRespuestaAseguradora (Recibido|EnRevision|Aprobado|RechazadoAseg|RechazadoTaller)
├── FechaRespuesta: DateTime?
├── Observaciones: string
├── RespuestasAdjuntas: List<string> (URLs de documentos)
├── MaximoPermitido: decimal (asignado por aseguradora)
└── ComentariosAseguradora: string

// Portal B2B
PortalAseguradora
├── Id: Guid
├── Aseguradora: Aseguradora (FK)
├── UsuariosPortal: List<UsuarioPortalAseguradora>
├── FechaCreacion: DateTime
├── URLAcceso: string
├── APIKey: string (para conexiones)
└── Activo: bool

UsuarioPortalAseguradora
├── Id: Guid
├── PortalAseguradora: PortalAseguradora (FK)
├── NombreUsuario: string
├── Email: string
├── Password: string (encrypted)
├── Rol: string (Cajero|Estimador|Adjunto|Admin)
├── Permisos: List<string> (ver órdenes|ver presupuestos|etc)
├── UltimoLogin: DateTime?
├── Activo: bool
└── FechaRegistro: DateTime

// Automatización API
IntegracionAseguradora
├── Id: Guid
├── Aseguradora: Aseguradora (FK)
├── TipoDatos: TipoIntegracion (JSON|XML|EDI|REST)
├── FrecuenciaActualizacion: int (minutos)
├── UltimaActualizacion: DateTime
├── ProximaActualizacion: DateTime
├── Activa: bool
└── LogErrores: List<ErrorIntegracion>

ErrorIntegracion
├── Id: Guid
├── Timestamp: DateTime
├── Tipo: string (ConexionError|ParseError|ValidacionError)
├── Mensaje: string
├── Detalles: string
└── Resuelta: bool
```

### Servicios Portal Aseguradoras

```csharp
SiniestroAPIService
- RecibirSiniestroDesdeAPI(json) → Parse y validar
- ValidarDatos(siniestro) → RFC, VIN, etc
- CrearSiniestroDesdeAPI(dto) → Crear entidad
- EnviarConfirmacion(aseguradoraId, numeroSiniestro)
- NotificarErroresValidacion()

PresupuestoAseguradoraService
- GenerarPresupuestoParaAseguradora(siniestroId)
- AplicarLimitesAseguradora(presupuestoId, limites)
- ExportarPresupuesto(presupuestoId, formato:XML|JSON)
- EnviarPresupuestoAPI(aseguradoraId, presupuestoId)
- TrackearEstado(presupuestoId)
- RecebirRespuesta(numeroSeguimiento)
- ProcesarAprobacion()
- ProcesarRechazo()

PortalAseguradoraService
- CrearUsuarioPortal(aseguradoraId, email, rol)
- GenerarCredenciales()
- OtorgarPermisos(usuarioId, permisos)
- TrackearAccesos()
- RevocarAcceso(usuarioId)

IntegracionAPIService
- SincronizarSiniestros(aseguradoraId) → Polling o Webhooks
- ProcesarRespuestaPresupuesto()
- ActualizarEstados()
- ReintentarFallos()
- GenerarReporteIntegracion()
```

### API Endpoints Portal Aseguradoras

```
# Recepción de siniestros (desde aseguradora)
POST   /api/aseguradoras/webhook/siniestro
POST   /api/aseguradoras/api/siniestro (REST alternativo)
GET    /api/aseguradoras/siniestro/{numeroSiniestro}/estado

# Gestión en portal
POST   /api/aseguradoras/portal/login
GET    /api/aseguradoras/portal/mis-ordenes
GET    /api/aseguradoras/portal/orden/{id}
GET    /api/aseguradoras/portal/orden/{id}/presupuesto
PUT    /api/aseguradoras/portal/orden/{id}/aprobar
PUT    /api/aseguradoras/portal/orden/{id}/rechazar

# API para aseguradoras (consumir desde su lado)
GET    /api/aseguradoras/presupuesto/{id}/xml
GET    /api/aseguradoras/presupuesto/{id}/json
POST   /api/aseguradoras/presupuesto/{id}/respuesta

# Configuración y sincronización
POST   /api/admin/aseguradoras
GET    /api/admin/aseguradoras/{id}
PUT    /api/admin/aseguradoras/{id}
POST   /api/admin/aseguradoras/{id}/sincronizar
GET    /api/admin/aseguradoras/{id}/log-integracion
```

### Flujo de Siniestro desde API

```
1. ASEGURADORA envía siniestro por API/Webhook
   ↓
2. Sistema recibe y parsea JSON/XML
   ↓
3. SiniestroAPIService.ValidarDatos()
   ├─ Validar RFC
   ├─ Validar VIN
   ├─ Validar números de póliza
   └─ Si error → Responder error 400 + detalle
   ↓
4. SiniestroRecibido creado en BD
   ├─ Estado = RecibidoAPI
   ├─ AseguradoraId asignado
   └─ Notificar a admin
   ↓
5. Sistema muestra en panel interno
   ├─ Taller revisa
   ├─ Crea Presupuesto
   └─ Aplica LimitesAseguradora (monto máximo)
   ↓
6. PresupuestoAseguradoraService.EnviarPresupuestoAPI()
   ├─ Exportar a XML o JSON
   ├─ Firmar digitalmente (si aplica)
   ├─ Enviar a endpoint de aseguradora
   └─ Obtener numero seguimiento
   ↓
7. Aseguradora revisa en portal
   ├─ Aprueba o rechaza
   └─ Responde por API
   ↓
8. Sistema recibe respuesta
   ├─ Si Aprobado → Presupuesto.Estado = AprobadoAseguradora
   ├─ Si Rechazado → Notificar taller
   └─ GenerarReporteIntegracion()
```

---

## 6️⃣ MÓDULO: CONTABILIDAD

### Entidades Básicas

```csharp
CuentaContable
├── Id: Guid
├── Codigo: string (formato SAT: XXXX-XXX-XXX)
├── Nombre: string
├── Tipo: TipoCuenta (Activo|Pasivo|Capital|Ingreso|Gasto)
├── Subcuenta: CuentaContable? (self-referencing)
├── Saldo: decimal
├── SaldoDeudor: bool
├── FechaSaldo: DateTime
└── Activa: bool

RegistroContable
├── Id: Guid
├── Numero: string (número de asiento)
├── Fecha: DateTime
├── Concepto: string
├── Referencia: string (FacturaId|PagoId|etc)
├── Particip: List<Participa> (1-a-N)
├── Acreedor: decimal (SUM de créditos)
├── Deudor: decimal (SUM de débitos)
├── Cuadrado: bool (Acreedor == Deudor)
├── Estado: EstadoAsiento (Borrador|Confirmado|Cancelado)
└── RegistradoPor: Usuario

Participa
├── Cuenta: CuentaContable (FK)
├── Tipo: TipoPartida (Debito|Credito)
├── Monto: decimal
└── Observaciones: string

// Reportes contables
BalanceComprobacion
├── Id: Guid
├── Periodo: string (Mes-Año)
├── FechaGeneracion: DateTime
├── Cuentas: List<RenglonBalance>
├── TotalDeudor: decimal
├── TotalAcreedor: decimal
├── Cuadrado: bool
└── AprobadoPor: Usuario?

EstadoResultados
├── Id: Guid
├── Periodo: string
├── Ingresos: decimal
├── CostoVentas: decimal
├── UtilidadBruta: decimal
├── GastosOperacionales: decimal
├── Utilidad Operacional: decimal
├── GastosFinancieros: decimal
├── UtilidadAntesImpuestos: decimal
├── ISR: decimal
├── PTU: decimal
├── UtilidadNeta: decimal
└── GeneradoEl: DateTime
```

---

## 📊 DASHBOARDS ESPECIALIZADOS

### 1. Dashboard Gerencial
- KPIs financieros
- Presupuestos vs facturas
- Margen de ganancia
- Clientes por vencer
- Top técnicos por productividad

### 2. Dashboard Finanzas
- Flujo de caja en tiempo real
- Cuentas por cobrar vencidas
- Cuentas por pagar
- Balance general
- Estado de resultados

### 3. Dashboard Almacén
- Stock bajo
- Rotación de inventario
- Diferencias en conteos
- Órdenes pendientes recepción

### 4. Dashboard Aseguradoras
- Siniestros en proceso
- Presupuestos pendientes respuesta
- Tasas de aprobación/rechazo
- Tiempos de respuesta

### 5. Dashboard RH
- Nómina del mes
- Asistencia/ausentismo
- Evaluaciones pendientes
- Vacaciones próximas

---

## 🔄 FLUJO INTEGRADO COMPLETO

```
CLIENTE (Aseguradora) 
    ↓
ORDEN DE SINIESTRO (vía API/Portal)
    ├─ SiniestroRecibido creado
    ├─ Validación automática
    └─ Notificación al taller
         ↓
    PRESUPUESTO creado
    ├─ ReglaService aplica reglas
    ├─ Límites de aseguradora aplicados
    ├─ FacturaElectronicaService prepara CFDI
    └─ Enviado a aseguradora (API/Email)
         ↓
    ASEGURADORA responde (Aprobado/Rechazado)
         ├─ Si Aprobado
         │   └─ ORDEN REPARACIÓN creada
         │       ├─ AlmacenService bloquea stock
         │       ├─ TecnicoService asigna responsable
         │       ├─ MovimientoInventario registra salida
         │       └─ OrdenReparacion.Estado = EnProgreso
         │            ↓
         │       REPARACIÓN se ejecuta
         │       ├─ AsistenciaService registra HH
         │       ├─ QualityControlService crea checklist
         │       ├─ WarrantyService (si aplica)
         │       └─ OrdenReparacion.Estado = Completada
         │            ↓
         │       FACTURACIÓN
         │       ├─ FacturaElectronicaService.GenerarCFDI()
         │       ├─ TimbradoSAT (PAC)
         │       ├─ Email a cliente + aseguradora
         │       ├─ RegistroContable para asiento
         │       └─ CuentasPorCobrar actualizada
         │            ↓
         │       PAGO recibido
         │       ├─ PagoService.Registrar()
         │       ├─ ReconciliacionBancariaService.Conciliar()
         │       └─ FinanceReportService.Actualizar()
         │
         └─ Si Rechazado
             └─ NotificaciónTaller + Opciones
                 ├─ Rehacer presupuesto
                 ├─ Ajustar montos
                 └─ Rechazar siniestro
```

---

## 💼 FASES DE IMPLEMENTACIÓN MEJORADA

### FASE 0: Infraestructura (1-2 semanas)
- [x] MVP Core (Presupuestos + CRM)
- [ ] Autenticación & Roles
- [ ] Formularios CRUD en UI
- [ ] Deployment inicial

### FASE 1: Operaciones (2-3 semanas)
- [ ] Taller (Órdenes de Reparación)
- [ ] Almacén (MEJORADO)
- [ ] Compras (MEJORADO)
- [ ] Órdenes de Servicio

### FASE 2: Finanzas Fiscal (3-4 semanas)
- [ ] Facturación Electrónica (SAT/CFDI)
- [ ] Tesorería & Pagos Bancarios
- [ ] Contabilidad básica
- [ ] Reconciliación bancaria

### FASE 3: RH & Portales (3-4 semanas)
- [ ] Recursos Humanos COMPLETO
- [ ] Portal Aseguradoras (B2B/API)
- [ ] Integración API con aseguradoras
- [ ] Webhooks y sincronización

### FASE 4: Inteligencia (2-3 semanas)
- [ ] Dashboards especializados
- [ ] Reportes avanzados
- [ ] Exportación (Excel/PDF)
- [ ] Alertas automáticas

### FASE 5: Optimización (Ongoing)
- [ ] Performance tuning
- [ ] Seguridad adicional (2FA, auditoría)
- [ ] Mobile app (opcional)
- [ ] BI & Analytics

---

## 🎯 ESTIMACIÓN TOTAL

| Fase | Módulos | Horas |
|------|---------|-------|
| 0 | MVP Pro | 12-15 |
| 1 | Operaciones | 20-25 |
| 2 | Finanzas Fiscal | 30-40 |
| 3 | RH & Portales | 35-45 |
| 4 | Inteligencia | 20-25 |
| 5 | Optimización | 15-20 |
| **TOTAL** | **13 módulos** | **132-170 horas** |

---

**Documento: Arquitectura Empresarial Completa**  
**Versión:** 2.0 - Con todas las mejoras solicitadas  
**Estado:** Listo para implementación sistémica
