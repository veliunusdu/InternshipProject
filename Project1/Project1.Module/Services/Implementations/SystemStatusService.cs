using Project1.Module.Services.Interfaces;

namespace Project1.Module.Services.Implementations
{
    public class SystemStatusService : ISystemStatusService
    {
        private bool _isActive = true;

        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

        public bool Toggle()
        {
            _isActive = !_isActive;
            return _isActive;
        }
    }
}
