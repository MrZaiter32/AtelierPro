# 🚀 Guía de Implementación en AtelierPRO ERP

## Arquitectura Recomendada: Microservicio

```
┌─────────────────────────┐         HTTP REST          ┌──────────────────────┐
│   AtelierPRO ERP        │  ←────────────────────→   │  Catálogos API       │
│   (C# - Puerto 8080)    │      JSON Request/Response │  (Python - :5000)    │
│                         │                            │                      │
│   - Almacén             │                            │  - Selenium          │
│   - Ventas              │                            │  - Scrapers          │
│   - Compras             │                            │  - FinditParts       │
│   - Catálogos ←──────────────────────────────────────┤  - FleetPride (soon) │
└─────────────────────────┘                            └──────────────────────┘
```

---

## 📋 Paso a Paso

### **Paso 1: Configurar API Python como Servicio**

#### 1.1 Verificar que la API funciona:

```bash
cd /home/n3thun73r/Documentos/catalogoerp/catalogoerp
source venv/bin/activate
python api_scraper.py
```

Deberías ver:
```
🚀 FinditParts Scraper API
======================================================================
Endpoints disponibles:
  GET  /health                - Health check
  ...
```

#### 1.2 Probar desde terminal:

```bash
# En otra terminal
curl http://localhost:5000/health
```

Respuesta esperada:
```json
{
  "status": "OK",
  "service": "FinditParts Scraper API",
  "version": "1.0"
}
```

#### 1.3 Configurar como servicio systemd (para que inicie automáticamente):

```bash
sudo nano /etc/systemd/system/catalogos-api.service
```

Contenido:
```ini
[Unit]
Description=Catálogos API - Scraper FinditParts
After=network.target

[Service]
Type=simple
User=n3thun73r
WorkingDirectory=/home/n3thun73r/Documentos/catalogoerp/catalogoerp
Environment="PATH=/home/n3thun73r/Documentos/catalogoerp/catalogoerp/venv/bin"
ExecStart=/home/n3thun73r/Documentos/catalogoerp/catalogoerp/venv/bin/python api_scraper.py
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

Activar el servicio:
```bash
sudo systemctl daemon-reload
sudo systemctl enable catalogos-api
sudo systemctl start catalogos-api
sudo systemctl status catalogos-api
```

---

### **Paso 2: Integrar en AtelierPRO ERP**

#### 2.1 Ubicar tu repositorio de AtelierPRO:

```bash
# ¿Dónde está tu ERP?
cd /ruta/a/tu/AtelierPRO

# Verificar estructura
ls -la
```

#### 2.2 Crear estructura de carpetas:

```bash
# Desde tu repo de AtelierPRO
mkdir -p Almacen/Services/Catalogos
mkdir -p Almacen/Controllers
mkdir -p Almacen/Models
```

#### 2.3 Copiar archivos C#:

```bash
# Copiar servicios
cp /home/n3thun73r/Documentos/catalogoerp/catalogoerp/CSharp_AtelierPRO/ICatalogoProveedorService.cs \
   ./Almacen/Services/Catalogos/

cp /home/n3thun73r/Documentos/catalogoerp/catalogoerp/CSharp_AtelierPRO/FinditPartsCatalogoService.cs \
   ./Almacen/Services/Catalogos/

cp /home/n3thun73r/Documentos/catalogoerp/catalogoerp/CSharp_AtelierPRO/CatalogosManager.cs \
   ./Almacen/Services/Catalogos/

# Copiar controller
cp /home/n3thun73r/Documentos/catalogoerp/catalogoerp/CSharp_AtelierPRO/CatalogosController.cs \
   ./Almacen/Controllers/
```

#### 2.4 Configurar en tu proyecto:

**Opción A: `appsettings.json` (.NET Core)**
```json
{
  "CatalogosAPI": {
    "BaseUrl": "http://localhost:5000",
    "Timeout": 60
  }
}
```

**Opción B: `App.config` (.NET Framework)**
```xml
<configuration>
  <appSettings>
    <add key="CatalogosAPI.BaseUrl" value="http://localhost:5000" />
    <add key="CatalogosAPI.Timeout" value="60" />
  </appSettings>
</configuration>
```

#### 2.5 Modificar `FinditPartsCatalogoService.cs`:

```csharp
public FinditPartsCatalogoService()
{
    // Leer desde configuración
    var baseUrl = ConfigurationManager.AppSettings["CatalogosAPI.BaseUrl"] 
                  ?? "http://localhost:5000";
    
    _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(60)
    };
    _apiBaseUrl = baseUrl;
}
```

---

### **Paso 3: Adaptar a tu Base de Datos**

Necesitas adaptar 3 cosas a tu esquema existente:

#### 3.1 Tabla de Productos (probablemente ya existe)

```sql
-- Si no existe, agregar columnas:
ALTER TABLE Productos 
ADD ProveedorCatalogo NVARCHAR(50),
    UrlCatalogo NVARCHAR(500),
    FechaActualizacionCatalogo DATETIME;
```

#### 3.2 Tabla de Referencias Cruzadas (crear si no existe)

```sql
CREATE TABLE ReferenciasAlternativas (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProductoId INT NOT NULL,
    FabricanteRef NVARCHAR(100),
    PartNumberRef NVARCHAR(100),
    Tipo NVARCHAR(50), -- 'Equivalente', 'Alternativo', 'Reemplazo'
    FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
);
```

#### 3.3 Modificar `CatalogosController.cs`:

Busca la función `GuardarProductoEnBDAsync()` y adapta a TU modelo:

```csharp
private async Task<bool> GuardarProductoEnBDAsync(ProductoCatalogo producto)
{
    try
    {
        // ADAPTAR A TU MODELO
        // Ejemplo genérico:
        
        var query = @"
            INSERT INTO Productos 
            (Codigo, Nombre, Fabricante, Descripcion, 
             ProveedorCatalogo, UrlCatalogo, FechaAlta)
            VALUES 
            (@Codigo, @Nombre, @Fabricante, @Descripcion, 
             @Proveedor, @Url, @FechaAlta);
            SELECT SCOPE_IDENTITY();
        ";

        using (var connection = new SqlConnection(_connectionString))
        {
            var productoId = await connection.ExecuteScalarAsync<int>(query, new
            {
                Codigo = producto.PartNumber,
                Nombre = producto.Description,
                Fabricante = producto.Manufacturer,
                Descripcion = producto.AdditionalInfo,
                Proveedor = producto.Proveedor,
                Url = producto.Url,
                FechaAlta = DateTime.Now
            });

            // Guardar referencias cruzadas
            if (producto.CrossReferences != null && producto.CrossReferences.Any())
            {
                var queryRefs = @"
                    INSERT INTO ReferenciasAlternativas 
                    (ProductoId, FabricanteRef, PartNumberRef, Tipo)
                    VALUES (@ProductoId, @Fabricante, @PartNumber, @Tipo)
                ";

                foreach (var crossRef in producto.CrossReferences)
                {
                    await connection.ExecuteAsync(queryRefs, new
                    {
                        ProductoId = productoId,
                        Fabricante = crossRef.Manufacturer,
                        PartNumber = crossRef.PartNumber,
                        Tipo = crossRef.Tipo
                    });
                }
            }

            return true;
        }
    }
    catch (Exception ex)
    {
        throw new Exception($"Error guardando en BD: {ex.Message}", ex);
    }
}
```

---

### **Paso 4: Agregar UI en tu Módulo de Almacén**

#### Opción A: WinForms

```csharp
// En tu Form existente de Almacén (ej: frmAlmacen.cs)
public partial class frmAlmacen : Form
{
    private CatalogosController _catalogosController;

    public frmAlmacen()
    {
        InitializeComponent();
        _catalogosController = new CatalogosController(_almacenRepository);
    }

    // Agregar un botón "Consultar Catálogos"
    private async void btnConsultarCatalogos_Click(object sender, EventArgs e)
    {
        var frmCatalogos = new frmConsultaCatalogos(_catalogosController);
        frmCatalogos.ShowDialog();
        
        // Si se importó algo, recargar grid
        if (frmCatalogos.ProductoImportado)
        {
            CargarProductos();
        }
    }
}
```

Crear nuevo formulario `frmConsultaCatalogos.cs`:

```csharp
public partial class frmConsultaCatalogos : Form
{
    private CatalogosController _controller;
    public bool ProductoImportado { get; private set; }

    public frmConsultaCatalogos(CatalogosController controller)
    {
        InitializeComponent();
        _controller = controller;
    }

    private async void btnBuscar_Click(object sender, EventArgs e)
    {
        var partNumber = txtPartNumber.Text.Trim();
        var manufacturer = txtManufacturer.Text.Trim();

        if (string.IsNullOrEmpty(partNumber))
        {
            MessageBox.Show("Ingrese un Part Number", "Atención");
            return;
        }

        // Mostrar loading
        lblStatus.Text = "Buscando en catálogos en línea...";
        btnBuscar.Enabled = false;

        try
        {
            var resultado = await _controller.BuscarProductoAsync(partNumber, manufacturer);

            if (resultado.Success)
            {
                dgvResultados.DataSource = resultado.Productos.Select(p => new
                {
                    Proveedor = p.Proveedor,
                    PartNumber = p.PartNumber,
                    Fabricante = p.Manufacturer,
                    Descripción = p.Description,
                    CrossRefs = string.Join(", ", p.CrossReferences.Select(cr => cr.ToString()))
                }).ToList();

                lblStatus.Text = $"✓ {resultado.TotalResultados} productos encontrados";
            }
            else
            {
                MessageBox.Show(resultado.Mensaje, "Sin resultados");
                lblStatus.Text = "Sin resultados";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error");
            lblStatus.Text = "Error en la búsqueda";
        }
        finally
        {
            btnBuscar.Enabled = true;
        }
    }

    private async void btnImportar_Click(object sender, EventArgs e)
    {
        if (dgvResultados.SelectedRows.Count == 0)
        {
            MessageBox.Show("Seleccione un producto", "Atención");
            return;
        }

        var fila = dgvResultados.SelectedRows[0];
        var url = fila.Cells["Url"].Value.ToString(); // Agregar columna URL oculta

        try
        {
            var producto = await _controller.ObtenerDetallesProductoAsync(url);
            
            var confirmar = MessageBox.Show(
                $"¿Importar este producto al inventario?\n\n" +
                $"Part Number: {producto.PartNumber}\n" +
                $"Fabricante: {producto.Manufacturer}\n" +
                $"Descripción: {producto.Description}",
                "Confirmar importación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmar == DialogResult.Yes)
            {
                var importado = await _controller.ImportarProductoAlInventarioAsync(producto);

                if (importado)
                {
                    MessageBox.Show("✓ Producto importado exitosamente", "Éxito");
                    ProductoImportado = true;
                    this.Close();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error");
        }
    }
}
```

#### Opción B: WPF

```xaml
<!-- Views/CatalogosView.xaml -->
<Window x:Class="AtelierPRO.Almacen.Views.CatalogosView"
        Title="Consultar Catálogos en Línea" Height="600" Width="900">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Búsqueda -->
        <StackPanel Grid.Row="0" Margin="10">
            <TextBlock Text="Part Number:" FontWeight="Bold"/>
            <TextBox Name="txtPartNumber" Margin="0,5,0,10"/>
            
            <TextBlock Text="Fabricante (opcional):" FontWeight="Bold"/>
            <TextBox Name="txtManufacturer" Margin="0,5,0,10"/>
            
            <Button Name="btnBuscar" Content="Buscar en Catálogos" 
                    Click="btnBuscar_Click" Padding="10,5"/>
        </StackPanel>

        <!-- Resultados -->
        <DataGrid Grid.Row="1" Name="dgResultados" AutoGenerateColumns="False" Margin="10">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Proveedor" Binding="{Binding Proveedor}"/>
                <DataGridTextColumn Header="Part Number" Binding="{Binding PartNumber}"/>
                <DataGridTextColumn Header="Fabricante" Binding="{Binding Manufacturer}"/>
                <DataGridTextColumn Header="Descripción" Binding="{Binding Description}" Width="*"/>
            </DataGrid.Columns>
        </DataGrid>

        <!-- Acciones -->
        <StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Right" Margin="10">
            <Button Name="btnImportar" Content="Importar Seleccionado" 
                    Click="btnImportar_Click" Padding="10,5" Margin="0,0,10,0"/>
            <Button Content="Cerrar" Click="btnCerrar_Click" Padding="10,5"/>
        </StackPanel>
    </Grid>
</Window>
```

---

### **Paso 5: Prueba de Integración**

#### 5.1 Verificar que la API funciona:

```bash
curl http://localhost:5000/health
```

#### 5.2 Desde tu ERP, ejecutar:

```csharp
// Código de prueba
var controller = new CatalogosController(_almacenRepository);

// Verificar servicios
var servicios = await controller.VerificarServiciosAsync();
foreach (var s in servicios)
{
    Console.WriteLine($"{s.Key}: {(s.Value ? "✓" : "✗")}");
}

// Buscar producto
var resultado = await controller.BuscarProductoAsync("R537001", "Meritor");
Console.WriteLine($"Encontrados: {resultado.TotalResultados}");
```

---

### **Paso 6: Deployment en Producción**

#### 6.1 Si ambos están en el mismo servidor:

```
Servidor Local
├── API Python:     http://localhost:5000
└── ERP C#:         http://localhost:8080
```

#### 6.2 Si están en servidores separados:

```
Servidor Python:    http://192.168.1.100:5000
Servidor ERP:       http://192.168.1.101:8080
```

Cambiar configuración:
```json
{
  "CatalogosAPI": {
    "BaseUrl": "http://192.168.1.100:5000"
  }
}
```

#### 6.3 Con Docker (opcional):

```dockerfile
# Dockerfile para la API
FROM python:3.13-slim

WORKDIR /app
COPY requirements.txt .
RUN pip install -r requirements.txt

COPY . .

EXPOSE 5000
CMD ["python", "api_scraper.py"]
```

```bash
docker build -t catalogos-api .
docker run -d -p 5000:5000 --name catalogos-api catalogos-api
```

---

## ✅ Checklist de Implementación

- [ ] API Python funcionando en `http://localhost:5000`
- [ ] Archivos C# copiados al proyecto AtelierPRO
- [ ] Configuración de URL en `appsettings.json`
- [ ] Adaptado `GuardarProductoEnBDAsync()` a tu BD
- [ ] Agregado botón/menú en UI de Almacén
- [ ] Probado buscar producto
- [ ] Probado importar producto
- [ ] Verificado que guarda en tu BD correctamente
- [ ] (Opcional) Configurado como servicio systemd

---

## 🆘 Soporte

Si tienes dudas específicas sobre tu base de datos o estructura de AtelierPRO, comparte:

1. El esquema de tu tabla `Productos`
2. Cómo es tu `_almacenRepository` actual
3. Si usas Entity Framework, Dapper, ADO.NET, etc.

¡Y te ayudo a adaptar el código! 🚀
