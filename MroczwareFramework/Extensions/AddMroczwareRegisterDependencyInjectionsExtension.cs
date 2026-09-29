using Microsoft.Extensions.DependencyInjection;
using MroczwareFramework.Attributes.DependencyInjection;
using MroczwareFramework.Enums.DependencyInjection;
using System.Reflection;

namespace MroczwareFramework.Extensions
{
    /// <summary>
    /// Rejstracja serwisów z wywolujace metodę aplikacji w kontenerze DI
    /// </summary>
    public static class AddMroczwareRegisterDependencyInjectionsExtension
    {
        public static IServiceCollection AddMroczwareRegisterDependencyInjections(this IServiceCollection services, Assembly assembly)
        {
            IEnumerable<Type> assemblyTypes = assembly
                .GetTypes()
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    t.GetCustomAttribute<DependencyInjectionAttribute>() is not null &&
                    t.GetInterfaces().Any()
                )
                .Distinct();

            foreach (Type type in assemblyTypes)
            {
                DependencyInjectionAttribute? attribute = type.GetCustomAttribute<DependencyInjectionAttribute>();
                Type typeInterface = type.GetInterface($"I{type.Name}")
                    ?? throw new Exception("Serwis musi implementowac interface");

                if (attribute is null) continue;
                switch (attribute.DependencyType)
                {
                    case DependencyInjectionTypeEnum.Singleton:
                        services.AddSingleton(typeInterface, type);
                        break;
                    case DependencyInjectionTypeEnum.Transient:
                        services.AddTransient(typeInterface, type);
                        break;
                    case DependencyInjectionTypeEnum.Scope:
                        services.AddScoped(typeInterface, type);
                        break;
                    default:
                        throw new Exception("Nie sprecyzowano typu rejstracji do DI");
                }
                ;
            }
            ;

            return services;
        }
    }
}
