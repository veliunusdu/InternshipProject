using DevExpress.Xpo;
using Project1.Module.Models.Tenants;

namespace Project1.Module.BusinessObjects
{
    public interface IFirmaAware
    {
        Firma Firma { get; set; }
    }
}
