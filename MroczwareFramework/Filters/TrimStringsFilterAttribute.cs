using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace MroczwareFramework.Filters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TrimStringsFilterAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var parameter in context.ActionArguments.Where(arg => arg.Value is string))
            {

                if (parameter.Value is string stringValue)
                    context.ActionArguments[parameter.Key] = stringValue.Trim();

                if (parameter.Value != null)
                    TrimAllString(parameter.Value);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }

        private void TrimAllString(object? obj)
        {
            if (obj == null) return;

            PropertyInfo[] properties = obj.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(string) && property.CanRead && property.CanWrite)
                {
                    string? currentValue = property.GetValue(obj) as string;
                    if (currentValue is not null)
                    {
                        string trimmedStringValue = currentValue.Trim();
                        property.SetValue(obj, trimmedStringValue);
                    }
                }
            }
        }
    }
}
