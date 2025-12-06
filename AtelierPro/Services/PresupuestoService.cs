using AtelierPro.Models;
using System.Collections.Generic;
using System.Linq;

namespace AtelierPro.Services
{
    public class PresupuestoService
    {
        private readonly ReglaService _reglaService;

        public PresupuestoService(ReglaService reglaService)
        {
            _reglaService = reglaService;
        }

        public void AgregarItem(Presupuesto presupuesto, ItemPresupuesto item, Tarifa tarifa)
        {
            presupuesto.Items.Add(item);
        }

        /// <summary>
        /// Calcula totales del presupuesto aplicando reglas de negocio, IVA y factores.
        /// TasaIva debe estar en formato decimal (ej: 0.16 para 16%).
        /// </summary>
        public Presupuesto CalcularTotales(Presupuesto presupuesto, Tarifa tarifa)
        {
            // Aplicar reglas de negocio antes de calcular
            presupuesto = _reglaService.AplicarReglas(presupuesto, tarifa);

            // Calcular IVA y Total (TasaIva en formato decimal: 0.16 = 16%)
            presupuesto.IvaAplicado = presupuesto.Subtotal * tarifa.TasaIva;
            presupuesto.TotalFinal = presupuesto.Subtotal + presupuesto.IvaAplicado;
            
            return presupuesto;
        }

        /// <summary>
        /// Calcula el margen promedio (diferencia entre TotalFinal y Subtotal).
        /// </summary>
        public decimal CalcularMargen(List<Presupuesto> presupuestos)
        {
            if (presupuestos == null || !presupuestos.Any())
            {
                return 0;
            }

            decimal margenTotal = 0;
            foreach (var p in presupuestos)
            {
                margenTotal += p.IvaAplicado; // El margen es el IVA aplicado
            }

            return margenTotal / presupuestos.Count;
        }
    }
}