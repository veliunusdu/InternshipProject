#nullable enable
using System;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using Project1.Module.BusinessObjects.Security;

namespace Project1.Module.Security
{
    /// <summary>
    /// XPO ve XAF Criteria motoruna 'CurrentFirmaOid()' fonksiyonunu ekler.
    /// Oturum açmış olan kullanıcının (ApplicationUser.Firma) Oid değerini dinamik olarak döndürür.
    /// </summary>
    public class CurrentFirmaOidOperator : ICustomFunctionOperatorBrowsable, ICustomFunctionOperatorFormattable
    {
        public const string OperatorName = "CurrentFirmaOid";

        static CurrentFirmaOidOperator()
        {
            Register();
        }

        public static void Register()
        {
            var instance = new CurrentFirmaOidOperator();
            if (CriteriaOperator.GetCustomFunction(OperatorName) == null)
            {
                CriteriaOperator.RegisterCustomFunction(instance);
            }
        }

        public string Name => OperatorName;

        public int MinOperandCount => 0;

        public int MaxOperandCount => 0;

        public bool IsValidOperandCount(int count) => count == 0;

        public bool IsValidOperandType(int operandIndex, int operandCount, Type type) => true;

        public string Description => "Oturum açmış kullanıcının (ApplicationUser.Firma) Oid değerini döndürür.";

        public FunctionCategory Category => FunctionCategory.All;

        public Type ResultType(params Type[] operands) => typeof(Guid?);

        public static Guid? GetCurrentFirmaOid()
        {
            if (SecuritySystem.CurrentUser is ApplicationUser appUser && appUser.Firma != null)
            {
                return appUser.Firma.Oid;
            }
            return null;
        }

        public object? Evaluate(params object[] operands)
        {
            return GetCurrentFirmaOid();
        }

        public string Format(Type providerType, params string[] operands)
        {
            var firmaOid = GetCurrentFirmaOid();
            if (firmaOid.HasValue)
            {
                return $"'{firmaOid.Value}'";
            }
            return "NULL";
        }
    }
}
