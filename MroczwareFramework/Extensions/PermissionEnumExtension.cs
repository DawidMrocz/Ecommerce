namespace Mobile.Data.Extensions
{
    using System;
    using System.ComponentModel;
    using System.Reflection;

    namespace Mobile.Data.DataModels.Contract
    {
        public static class PermissionEnumExtension
        {
            public static string GetPermissionCode(this Enum value)
            {
                FieldInfo? field = value.GetType().GetField(value.ToString())
                    ?? throw new Exception("Nie znaleziono pola");

                DescriptionAttribute? attribute = field.GetCustomAttribute<DescriptionAttribute>()
                    ?? throw new Exception($"Nie przypisano atrybutu z kodem uprawnienia");

                return attribute.Description;
            }
        }
    }
}
