using AtelierPro.Models;

namespace AtelierPro.Services
{
    public class WorkflowService
    {
        public bool AprobarPresupuesto(Presupuesto presupuesto)
        {
            if (presupuesto.Estado == EstadoPresupuesto.Borrador)
            {
                presupuesto.Estado = EstadoPresupuesto.Aprobado;
                // Aquí podría ir lógica adicional, como notificar a otros sistemas.
                return true;
            }
            return false;
        }

        public bool CerrarPresupuesto(Presupuesto presupuesto)
        {
            if (presupuesto.Estado == EstadoPresupuesto.Aprobado)
            {
                presupuesto.Estado = EstadoPresupuesto.Cerrado;
                // Listo para ser facturado.
                return true;
            }
            return false;
        }

        public bool FacturarPresupuesto(Presupuesto presupuesto)
        {
            if (presupuesto.Estado == EstadoPresupuesto.Cerrado)
            {
                presupuesto.Estado = EstadoPresupuesto.Facturado;
                return true;
            }
            return false;
        }

        public void CambiarEstado(Presupuesto presupuesto, EstadoPresupuesto nuevoEstado)
        {
            bool exito = false;
            switch (nuevoEstado)
            {
                case EstadoPresupuesto.Aprobado:
                    exito = AprobarPresupuesto(presupuesto);
                    break;
                case EstadoPresupuesto.Cerrado:
                    exito = CerrarPresupuesto(presupuesto);
                    break;
                case EstadoPresupuesto.Facturado:
                    exito = FacturarPresupuesto(presupuesto);
                    break;
                default:
                    throw new InvalidOperationException($"Estado {nuevoEstado} no es válido");
            }

            if (!exito)
            {
                throw new InvalidOperationException(
                    $"No se puede cambiar el presupuesto del estado {presupuesto.Estado} a {nuevoEstado}");
            }
        }
    }
}