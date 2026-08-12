namespace Project1.Module.Services
{
    public interface ISystemStatusService
    {
        bool IsActive { get; set; }
        bool Toggle();
    }

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
