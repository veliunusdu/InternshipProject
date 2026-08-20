using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Updating;

namespace Project1.Module
{
    public sealed class Project1Module : ModuleBase
    {
        public Project1Module()
        {
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.SystemModule.SystemModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.ConditionalAppearance.ConditionalAppearanceModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Validation.ValidationModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Security.SecurityModule));
            Security.CurrentCustomerOidOperator.Register();
        }

        public override void CustomizeTypesInfo(DevExpress.ExpressApp.DC.ITypesInfo typesInfo)
        {
            base.CustomizeTypesInfo(typesInfo);
            Security.CurrentCustomerOidOperator.Register();
        }

        public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB)
        {
            ModuleUpdater updater = new DatabaseUpdate.Updater(objectSpace, versionFromDB);
            return new ModuleUpdater[] { updater };
        }

    }
}
