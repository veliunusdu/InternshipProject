namespace Project1.Core.Services.Interfaces
{
    public interface ISystemStatusService
    {
        bool IsActive { get; set; }
        bool Toggle();
    }
}
