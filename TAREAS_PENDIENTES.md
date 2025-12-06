# 📋 Tareas Pendientes - AtelierPro ERP

## 🎯 Estado Actual
✅ **MVP Completado y Funcional**
- Base de datos con EF Core + SQLite
- API REST con 13 endpoints
- UI Blazor básica
- 17 tests unitarios (100% passing)
- Repositorio en GitHub con SSH

---

## 🔥 Prioridad Alta (Críticas para Producción)

### 1. 🔐 Autenticación y Autorización
**Objetivo:** Proteger la aplicación con sistema de usuarios y roles

**Tareas:**
- [ ] Instalar `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- [ ] Extender `AtelierProDbContext` con `IdentityDbContext`
- [ ] Crear modelos `ApplicationUser` y `ApplicationRole`
- [ ] Agregar tablas de Identity a la BD (Users, Roles, Claims)
- [ ] Implementar páginas de Login/Logout/Registro
- [ ] Crear roles: `Admin`, `Taller`, `Finanzas`, `Cliente`
- [ ] Proteger Controllers con `[Authorize(Roles = "...")]`
- [ ] Agregar políticas de autorización personalizadas
- [ ] Implementar "Recuperar contraseña" por email
- [ ] Tests de autenticación y autorización

**Estimación:** 4-5 horas  
**Prioridad:** ⭐⭐⭐⭐⭐

---

### 2. 🎨 Formularios CRUD Completos en UI
**Objetivo:** Permitir crear/editar entidades desde la interfaz web

**Tareas:**
- [ ] Página `CrearPresupuesto.razor` con formulario completo
- [ ] Página `EditarPresupuesto.razor/{id}` con carga de datos
- [ ] Página `CrearCliente.razor` con validaciones
- [ ] Página `EditarCliente.razor/{id}`
- [ ] Componente reutilizable `VehiculoForm.razor`
- [ ] Componente `ItemPresupuestoEditor.razor` (agregar/quitar items)
- [ ] Validación client-side con DataAnnotations
- [ ] Manejo de errores y mensajes de éxito
- [ ] Confirmación antes de eliminar
- [ ] Navegación coherente entre páginas

**Estimación:** 5-6 horas  
**Prioridad:** ⭐⭐⭐⭐⭐

---

### 3. 🚀 Deployment Inicial
**Objetivo:** Hacer la aplicación accesible en internet

**Opciones:**

#### Opción A: Azure App Service (Recomendado)
- [ ] Crear cuenta Azure (free tier disponible)
- [ ] Crear App Service (Linux + .NET 6)
- [ ] Configurar connection string para BD en Azure
- [ ] Cambiar de SQLite a SQL Server/PostgreSQL
- [ ] Deploy desde GitHub Actions (CI/CD)
- [ ] Configurar dominio personalizado
- [ ] Habilitar SSL/HTTPS
- [ ] Configurar logs y monitoreo

#### Opción B: Railway/Render (Gratuito)
- [ ] Crear cuenta en Railway.app o Render.com
- [ ] Conectar repositorio GitHub
- [ ] Configurar build commands
- [ ] Configurar variables de entorno
- [ ] Deploy automático desde `main` branch

#### Opción C: Docker + VPS
- [ ] Crear `Dockerfile`
- [ ] Crear `docker-compose.yml`
- [ ] Configurar Nginx reverse proxy
- [ ] Configurar SSL con Let's Encrypt
- [ ] Setup en VPS (DigitalOcean/Linode)

**Estimación:** 3-4 horas  
**Prioridad:** ⭐⭐⭐⭐

---

## 📦 Prioridad Media (Expansión de Funcionalidad)

### 4. 📊 Módulo de Inventario
**Objetivo:** Gestión completa de refacciones y stock

**Tareas:**
- [ ] Modelo `Refaccion` con stock actual y mínimo
- [ ] Modelo `MovimientoInventario` (entrada/salida/ajuste)
- [ ] Controlador `InventarioController` con endpoints CRUD
- [ ] Página `ListaInventario.razor` con búsqueda y filtros
- [ ] Alertas de stock bajo (badge rojo si < mínimo)
- [ ] Reporte de movimientos por fecha
- [ ] Integración: descontar stock al cerrar presupuesto
- [ ] Tests para lógica de inventario

**Estimación:** 4-5 horas  
**Prioridad:** ⭐⭐⭐

---

### 5. 🛒 Módulo de Compras
**Objetivo:** Gestión de órdenes de compra y proveedores

**Tareas:**
- [ ] Modelo `Proveedor` (nombre, contacto, rating)
- [ ] Modelo `OrdenCompra` con items y estados
- [ ] Controlador `ComprasController`
- [ ] Página `ListaProveedores.razor`
- [ ] Página `CrearOrdenCompra.razor`
- [ ] Workflow de estados: Pendiente → Enviada → Recibida
- [ ] Generar OC automática desde stock bajo
- [ ] Historial de compras por proveedor
- [ ] Tests para órdenes de compra

**Estimación:** 5-6 horas  
**Prioridad:** ⭐⭐⭐

---

### 6. 📈 Dashboard Mejorado con Métricas
**Objetivo:** Visualización de KPIs del negocio

**Tareas:**
- [ ] Instalar Chart.js o biblioteca similar
- [ ] Gráfica: Presupuestos por estado (pie chart)
- [ ] Gráfica: Ingresos mensuales (line chart)
- [ ] Gráfica: Top 10 refacciones más vendidas
- [ ] Métrica: Tasa de conversión (aprobados/total)
- [ ] Métrica: Tiempo promedio de cierre
- [ ] Métrica: Valor promedio de presupuesto
- [ ] Filtros por rango de fechas
- [ ] Export de reportes a Excel/PDF

**Estimación:** 4-5 horas  
**Prioridad:** ⭐⭐⭐

---

## 🔌 Prioridad Media-Baja (Integraciones)

### 7. 🚗 Integración con Audatex
**Objetivo:** Importar valuaciones de siniestros automáticamente

⚠️ **PAUSADO POR AHORA** - Se implementará en futuro cuando tengas credenciales API

**Tareas (Para futuro):**
- [ ] Obtener credenciales API de Audatex (sandbox)
- [ ] Crear `AudatexService` para HTTP requests
- [ ] Endpoint: Buscar vehículo por VIN
- [ ] Endpoint: Obtener cotización de reparación
- [ ] Mapear respuesta Audatex → Presupuesto
- [ ] Botón "Importar desde Audatex" en UI
- [ ] Manejo de errores y timeouts
- [ ] Cache de respuestas para reducir llamadas
- [ ] Tests con mocks de API

**Estimación:** 6-8 horas  
**Prioridad:** ⭐⭐ (requiere acceso a API)
**Estado:** ❌ NO IMPLEMENTAR POR AHORA

---

### 8. 📧 Notificaciones por Email
**Objetivo:** Enviar emails automáticos a clientes

**Tareas:**
- [ ] Configurar SendGrid o SMTP
- [ ] Servicio `EmailService` con templates
- [ ] Email: Presupuesto aprobado (PDF adjunto)
- [ ] Email: Recordatorio de seguimiento
- [ ] Email: Vehículo listo para entrega
- [ ] Templates HTML con diseño profesional
- [ ] Cola de emails (background job)
- [ ] Logs de envíos exitosos/fallidos

**Estimación:** 3-4 horas  
**Prioridad:** ⭐⭐

---

## 🧪 Prioridad Baja (Calidad y Mantenibilidad)

### 9. 🎯 Aumentar Cobertura de Tests
**Objetivo:** Más confianza para refactorings futuros

**Tareas:**
- [ ] Tests de integración para `ClientesController`
- [ ] Tests de integración para `PresupuestosController`
- [ ] Tests de UI con bUnit
- [ ] Tests end-to-end con Playwright
- [ ] Configurar coverage report (Coverlet)
- [ ] Objetivo: >80% code coverage
- [ ] CI con GitHub Actions (build + test)

**Estimación:** 4-5 horas  
**Prioridad:** ⭐⭐

---

### 10. 📄 Generación de Documentos PDF
**Objetivo:** Exportar presupuestos y reportes a PDF

**Tareas:**
- [ ] Instalar `QuestPDF` o `iTextSharp`
- [ ] Template PDF para presupuestos
- [ ] Incluir logo y datos de empresa
- [ ] Tabla de items con subtotales
- [ ] Firma digital (opcional)
- [ ] Endpoint: `GET /api/presupuestos/{id}/pdf`
- [ ] Botón "Descargar PDF" en UI
- [ ] Tests de generación de PDF

**Estimación:** 3-4 horas  
**Prioridad:** ⭐⭐

---

### 11. 🔍 Búsqueda y Filtros Avanzados
**Objetivo:** Encontrar datos rápidamente

**Tareas:**
- [ ] Búsqueda full-text en Presupuestos (VIN, cliente)
- [ ] Filtros: por estado, fecha, monto
- [ ] Ordenamiento por columnas (ASC/DESC)
- [ ] Paginación con `PagedList`
- [ ] Búsqueda en Clientes (nombre, teléfono, email)
- [ ] Autocompletado en formularios
- [ ] Guardar filtros favoritos

**Estimación:** 3-4 horas  
**Prioridad:** ⭐⭐

---

### 12. 🌐 Internacionalización (i18n)
**Objetivo:** Soporte multi-idioma

**Tareas:**
- [ ] Configurar `IStringLocalizer`
- [ ] Archivos de recursos `.resx` (ES/EN)
- [ ] Traducir UI completa
- [ ] Selector de idioma en navbar
- [ ] Formateo de fechas/números por cultura
- [ ] Tests con diferentes culturas

**Estimación:** 4-5 horas  
**Prioridad:** ⭐ (solo si necesitas inglés)

---

## 🎨 Mejoras de UX/UI

### 13. ✨ Pulir Interfaz
**Tareas:**
- [ ] Tema dark mode
- [ ] Animaciones y transiciones suaves
- [ ] Loading spinners en acciones asíncronas
- [ ] Toasts/notificaciones elegantes
- [ ] Responsive design para móviles
- [ ] Iconos consistentes (Font Awesome)
- [ ] Breadcrumbs de navegación
- [ ] Tooltips informativos

**Estimación:** 3-4 horas  
**Prioridad:** ⭐⭐

---

## 🔧 Mejoras Técnicas

### 14. 🏗️ Refactorings
**Tareas:**
- [ ] Implementar patrón CQRS (opcional)
- [ ] Agregar MediatR para comandos/queries
- [ ] Implementar AutoMapper para DTOs
- [ ] Separar modelos de dominio vs DTOs
- [ ] Agregar FluentValidation
- [ ] Implementar logging estructurado (Serilog)
- [ ] Health checks endpoints
- [ ] Rate limiting con AspNetCoreRateLimit

**Estimación:** 6-8 horas  
**Prioridad:** ⭐ (mejoras arquitectónicas)

---

### 15. 🔒 Seguridad Adicional
**Tareas:**
- [ ] Configurar CORS adecuadamente
- [ ] Implementar CSRF protection
- [ ] Validación de entrada exhaustiva
- [ ] SQL injection prevention (ya cubierto por EF)
- [ ] XSS protection en Razor
- [ ] Auditoría de cambios (quién/cuándo/qué)
- [ ] Two-factor authentication (2FA)
- [ ] Password policy (mínimo 8 caracteres, etc.)

**Estimación:** 4-5 horas  
**Prioridad:** ⭐⭐

---

## 📊 Resumen de Estimaciones

| Categoría | Tareas | Tiempo Total |
|-----------|--------|--------------|
| 🔥 Prioridad Alta | 3 | 12-15 horas |
| 📦 Prioridad Media | 4 | 17-21 horas |
| 🔌 Integraciones | 1 | 3-4 horas (solo emails) |
| 🧪 Calidad | 4 | 13-17 horas |
| 🎨 UX/UI | 1 | 3-4 horas |
| 🔧 Técnicas | 2 | 10-13 horas |
| **TOTAL** | **15** | **58-74 horas** |

> ⚠️ Audatex pausado por ahora (-6-8 horas)

---

## 🎯 Roadmap Sugerido

### **Semana 1-2: MVP Pro (Listo para usar)**
1. Autenticación y roles (Tarea 1)
2. Formularios CRUD (Tarea 2)
3. Deploy inicial (Tarea 3)

### **Semana 3-4: Expansión Core**
4. Módulo Inventario (Tarea 4)
5. Módulo Compras (Tarea 5)
6. Dashboard mejorado (Tarea 6)

### **Semana 5-6: Integraciones y Pulido**
7. Emails automáticos (Tarea 8)
8. PDFs y reportes (Tarea 10)
9. Búsqueda avanzada (Tarea 11)

### **Semana 7-8: Calidad y Optimización**
10. Tests completos (Tarea 9)
11. Búsqueda avanzada (Tarea 11)
12. Mejoras de UX (Tarea 13)

---

## 📝 Notas

- ✅ = Completado
- 🚧 = En progreso
- ⏸️ = En pausa
- ❌ = Bloqueado

**Última actualización:** 6 de diciembre de 2025  
**Responsable:** Equipo AtelierPro
