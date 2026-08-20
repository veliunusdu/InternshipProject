#nullable enable
using System;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using Project1.Module.BusinessObjects.Security;

namespace Project1.Module.Security
{
    /// <summary>
    /// XPO ve XAF Criteria motoruna 'CurrentCustomerOid()' fonksiyonunu ekler.
    /// Oturum açmış olan müşterinin (ApplicationUser.Musteri) Oid değerini dinamik olarak döndürür.
    /// </summary>
    public class CurrentCustomerOidOperator : ICustomFunctionOperatorBrowsable, ICustomFunctionOperatorFormattable
    {
        public const string OperatorName = "CurrentCustomerOid";

        static CurrentCustomerOidOperator()
        {
            Register();
        }

        public static void Register()
        {
            var instance = new CurrentCustomerOidOperator();
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

        public string Description => "Oturum açmış müşterinin (ApplicationUser.Musteri) Oid değerini döndürür.";

        public FunctionCategory Category => FunctionCategory.All;

        public Type ResultType(params Type[] operands) => typeof(Guid?);

        public static Guid? GetCurrentCustomerOid()
        {
            if (SecuritySystem.CurrentUser is ApplicationUser appUser && appUser.Musteri != null)
            {
                return appUser.Musteri.Oid;
            }
            return null;
        }

        public object? Evaluate(params object[] operands)
        {
            return GetCurrentCustomerOid();
        }

        public string Format(Type providerType, params string[] operands)
        {
            var customerOid = GetCurrentCustomerOid();
            if (customerOid.HasValue)
            {
                return $"'{customerOid.Value}'";
            }
            return "NULL";
        }
    }
}
