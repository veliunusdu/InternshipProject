using System;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using Project1.Module.BusinessObjects.Security;

namespace Project1.Module.Security
{
    public class CurrentUserFirmaOperator : ICustomFunctionOperator, ICustomFunctionOperatorFormattable
    {
        public string Name => "CurrentUserFirma";

        public Type ResultType(params Type[] operands) => typeof(object);

        public object Evaluate(params object[] operands)
        {
            var user = SecuritySystem.CurrentUser as ApplicationUser;
            return user?.Firma?.Oid; // SessionMixingException'ı önlemek için Oid döner
        }

        public string Format(Type providerType, params string[] operands)
        {
            var user = SecuritySystem.CurrentUser as ApplicationUser;
            if (user?.Firma != null)
            {
                return $"'{user.Firma.Oid}'";
            }
            return "NULL";
        }

        public static void Register()
        {
            if (CriteriaOperator.GetCustomFunction("CurrentUserFirma") == null)
            {
                CriteriaOperator.RegisterCustomFunction(new CurrentUserFirmaOperator());
            }
        }
    }
}
