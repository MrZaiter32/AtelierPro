using System;
using System.Threading.Tasks;

namespace AtelierPro.Services
{
    /// <summary>
    /// Servicio simple para mostrar un indicador global de "ocupado" (spinner circular)
    /// mientras se ejecutan operaciones asincrónicas largas.
    /// </summary>
    public class BusyService
    {
        private bool _isBusy;
        private string _message = string.Empty;

        public bool IsBusy => _isBusy;
        public string Message => _message;

        public event Action? OnChange;

        public IDisposable Start(string message = "Cargando...")
        {
            SetBusy(true, message);
            return new BusyHandle(this);
        }

        public async Task RunAsync(Func<Task> action, string message = "Procesando...")
        {
            SetBusy(true, message);
            try
            {
                await action();
            }
            finally
            {
                SetBusy(false, string.Empty);
            }
        }

        private void SetBusy(bool busy, string message)
        {
            _isBusy = busy;
            _message = message;
            OnChange?.Invoke();
        }

        private sealed class BusyHandle : IDisposable
        {
            private readonly BusyService _service;
            private bool _disposed;

            public BusyHandle(BusyService service)
            {
                _service = service;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _service.SetBusy(false, string.Empty);
            }
        }
    }
}