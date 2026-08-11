using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Updating;

namespace Project1.Blazor.Server
{
    [ToolboxItemFilter("Xaf.Platform.Blazor")]
    public sealed class Project1BlazorModule : ModuleBase
    {
        public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB)
        {
            return ModuleUpdater.EmptyModuleUpdaters;
        }
    }
}
