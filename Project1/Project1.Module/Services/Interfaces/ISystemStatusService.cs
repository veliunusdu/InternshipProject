namespace Project1.Module.Services.Interfaces
{
    public interface ISystemStatusService
    {
        bool IsActive { get; set; }
        bool Toggle();
    }
}
