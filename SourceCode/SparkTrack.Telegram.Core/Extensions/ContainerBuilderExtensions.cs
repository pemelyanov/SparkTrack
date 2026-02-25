namespace SparkTrack.Telegram.Core.Extensions;

using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;
using EventHandlers;

public static class ContainerBuilderExtensions
{
    public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
        RegisterAssemblyAssignableGenericWithArguments(
            this ContainerBuilder builder,
            Assembly assembly,
            Type genericType,
            HashSet<Type> genericArguments,
            params Type[] asBonusImplementations
        )
    {
        var registration = builder.RegisterAssemblyTypes(assembly)
            .Where(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                    i.GetGenericTypeDefinition() == genericType
                )
                .Select(i => i.GetGenericArguments()[0])
                .Any(genericArguments.Contains)
            )
            .As(t => t.GetInterfaces()
                .First(i => i.IsGenericType &&
                    i.GetGenericTypeDefinition() == genericType &&
                    genericArguments.Contains(i.GetGenericArguments()[0])
                )
            );

        foreach (Type bonusImplementation in asBonusImplementations)
            registration = registration.As(bonusImplementation);

        return registration;
    }
    
    public static void RegisterGenericTypes(
        this ContainerBuilder builder,
        Type implementationType,
        Type interfaceType,
        HashSet<Type> genericArguments,
        Action<IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>> registrationCallback)
    {
        foreach (var genericArg in genericArguments)
        {
            var closedImplementation = implementationType.MakeGenericType(genericArg);
            var closedInterface = interfaceType.MakeGenericType(genericArg);

            var registration = builder.RegisterType(closedImplementation)
                .As(closedInterface);

            registrationCallback(registration);
        }
    }
}