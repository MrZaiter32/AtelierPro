using AtelierPro.Models;

namespace AtelierPro.Services
{
    public class ReglaService
    {
        public Presupuesto AplicarReglas(Presupuesto presupuesto, Tarifa tarifa)
        {
            if (presupuesto.Vehiculo == null)
            {
                return presupuesto; // No se pueden aplicar reglas sin vehículo
            }

            // 1. Regla de Depreciación por Antigüedad
            AplicarDepreciacion(presupuesto.Items, presupuesto.Vehiculo.AntiguedadAnios);

            // 2. Regla de Complementos Automáticos
            AplicarComplementos(presupuesto.Items, tarifa);

            return presupuesto;
        }

        private void AplicarDepreciacion(IList<ItemPresupuesto> items, int antiguedadAnios)
        {
            // Lógica de ejemplo: 10% de depreciación por año, max 50%
            if (antiguedadAnios > 0)
            {
                var factorDepreciacion = Math.Min(antiguedadAnios * 0.10m, 0.50m);
                foreach (var item in items.Where(i => i.Tipo == TipoItemPresupuesto.Pieza))
                {
                    // El ajuste se representa como un descuento (negativo)
                    item.PorcentajeAjuste = -factorDepreciacion * 100;
                }
            }
        }

        private void AplicarComplementos(IList<ItemPresupuesto> items, Tarifa tarifa)
        {
            var itemsOriginales = items.ToList(); // Copia para evitar modificar la colección mientras se itera
            foreach (var item in itemsOriginales.Where(i => i.Tipo == TipoItemPresupuesto.Pieza))
            {
                // Si la pieza requiere pintura, añadir item de pintura
                if (item.RequierePintura && !items.Any(i => i.Tipo == TipoItemPresupuesto.Pintura && i.Descripcion.Contains(item.Codigo)))
                {
                    items.Add(new ItemPresupuesto
                    {
                        Tipo = TipoItemPresupuesto.Pintura,
                        Codigo = $"PINT-{item.Codigo}",
                        Descripcion = $"Pintura para {item.Descripcion}",
                        TiempoAsignadoHoras = 1.5, // Ejemplo
                        PrecioUnitario = tarifa.PrecioPinturaHora
                    });
                }
                
                // Aquí se podrían añadir más lógicas para desmontajes dobles, alineación, etc.
            }
        }
    }
}