using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CrystalSharp.Envoy.Extensions
{
    public static class EnvoyServiceCollectionExtensions
    {
        private static readonly Func<Type, bool> isConcreteType = type => { return !type.IsInterface && !type.IsAbstract; };
        private static readonly Func<Type, bool> isOpenGenericType = type => { return type.IsGenericTypeDefinition || type.ContainsGenericParameters; };
        private static readonly Func<Type, Type, bool> isAssignableFromInterface = (concreteType, templateType) =>
        {
            if (concreteType is null) return false;

            if (concreteType == templateType) return true;

            return templateType.IsAssignableFrom(concreteType);
        };
        private static readonly Func<Type, Type, bool> isRelated = (concreteType, interfaceType) =>
        {
            Type @interface = interfaceType.GetGenericTypeDefinition();
            Type[] interfaceArguments = @interface.GenericTypeArguments;
            Type[] concreteArguments = concreteType.GenericTypeArguments;

            return IsValid(concreteType, interfaceType, isAssignableFromInterface) && interfaceArguments.Length == concreteArguments.Length;
        };
        private static readonly Func<Type, Type, bool> isValidConcreteType = (concreteType, templateType) =>
        {
            return concreteType is not null && templateType.IsInterface && IsValid(concreteType, isConcreteType);
        };
        private static readonly Func<Type, Type, bool> isValidBaseType = (concreteType, templateType) =>
        {
            return concreteType.BaseType!.IsGenericType && (concreteType.BaseType!.GetGenericTypeDefinition() == templateType);
        };

        public static void AddEnvoy(this IServiceCollection services, params Assembly[] assembliesToScan)
        {
            Assembly[] assemblies = [.. assembliesToScan.Distinct()];

            RegisterRequiredService(services);
            RegisterImplementations(typeof(IRequestHandler<,>), services, assemblies, false);
            RegisterImplementations(typeof(INotificationHandler<>), services, assemblies, true);
        }

        private static void RegisterRequiredService(IServiceCollection services)
        {
            services.TryAdd(new ServiceDescriptor(typeof(IEnvoy), typeof(EnvoyImpl), ServiceLifetime.Transient));
        }

        private static void RegisterImplementations(Type contractInterface,
            IServiceCollection services,
            IEnumerable<Assembly> assemblies,
            bool allowDuplicate)
        {
            List<Type> concreteTypes = [];
            List<Type> interfaces = [];

            IEnumerable<Type> types = assemblies.SelectMany(a => a.DefinedTypes).Where(t => !IsValid(t, isOpenGenericType));

            foreach (Type type in types)
            {
                IEnumerable<Type> interfaceTypes = DiscoverInterfaces(type, contractInterface);

                if (interfaceTypes.Any())
                {
                    AddConcreteTypeToList(concreteTypes, type);

                    foreach (Type interfaceType in interfaceTypes)
                    {
                        AddDistinctInterface(interfaces, interfaceType);
                    }
                }
            }

            IEnumerable<KeyValuePair<Type, Type>> typesToRegister = GetTypesToRegister(concreteTypes, interfaces, allowDuplicate);

            foreach (KeyValuePair<Type, Type> registrationItem in typesToRegister)
            {
                try
                {
                    services.AddTransient(registrationItem.Key, registrationItem.Value);
                }
                catch
                {
                    //
                }
            }
        }

        private static List<KeyValuePair<Type, Type>> GetTypesToRegister(IEnumerable<Type> concreteTypes, IEnumerable<Type> interfaces, bool allowDuplicate)
        {
            List<KeyValuePair<Type, Type>> types = [];

            foreach (Type @interface in interfaces)
            {
                List<Type> matchingTypes = [.. concreteTypes.Where(t => IsValid(t, @interface, isAssignableFromInterface))];

                if (allowDuplicate)
                {
                    if (matchingTypes.Count != 0)
                    {
                        types.AddRange(matchingTypes.Select(t => new KeyValuePair<Type, Type>(@interface, t)));
                    }
                }
                else
                {
                    if (matchingTypes.Count > 1)
                    {
                        matchingTypes.RemoveAll(t => !IsIdenticalWithInterfaceArguments(t, @interface));
                    }

                    types.AddRange(matchingTypes.Select(t => new KeyValuePair<Type, Type>(@interface, t)));
                }

                if (!IsValid(@interface, isOpenGenericType))
                {
                    types.AddRange(GetRelatedConcreteTypes(@interface, concreteTypes));
                }
            }

            return types;
        }

        private static bool IsValid(Type type, Func<Type, bool> condition)
        {
            return condition(type);
        }

        private static bool IsValid(Type firstType, Type secondType, Func<Type, Type, bool> condition)
        {
            return condition(firstType, secondType);
        }

        private static void AddConcreteTypeToList(List<Type> list, Type concreteType)
        {
            if (IsValid(concreteType, isConcreteType))
            {
                list.Add(concreteType);
            }
        }

        private static void AddDistinctInterface(List<Type> list, Type @interface)
        {
            if (!list.Contains(@interface))
            {
                list.Add(@interface);
            }
        }

        private static IList<KeyValuePair<Type, Type>> GetRelatedConcreteTypes(Type @interface, IEnumerable<Type> concretes)
        {
            IEnumerable<Type> types = concretes.Where(t => IsValid(t, isOpenGenericType) && IsValid(t, @interface, isRelated));
            IList<KeyValuePair<Type, Type>> concreteTypes = [];

            foreach (Type type in types)
            {
                concreteTypes.Add(new KeyValuePair<Type, Type>(@interface, type.MakeGenericType(@interface.GenericTypeArguments)));
            }

            return concreteTypes;
        }

        private static bool IsIdenticalWithInterfaceArguments(Type handlerType, Type handlerInterface)
        {
            bool isIdentital = handlerType.IsInterface && handlerType.GenericTypeArguments.SequenceEqual(handlerInterface.GenericTypeArguments);

            if (!isIdentital)
            {
                isIdentital = IsIdenticalWithInterfaceArguments(handlerType.GetInterface(handlerInterface.Name), handlerInterface);
            }

            return isIdentital;
        }

        private static IEnumerable<Type> DiscoverInterfaces(Type concreteType, Type templateType)
        {
            if (concreteType is null) yield break;

            if (!IsValid(concreteType, isConcreteType)) yield break;

            if (isValidConcreteType(concreteType, templateType))
            {
                IEnumerable<Type> interfaceTypes = GetInterfacesFromConcreteType(concreteType, templateType);

                foreach (Type interfaceType in interfaceTypes)
                {
                    yield return interfaceType;
                }
            }
            else if (isValidBaseType(concreteType, templateType))
            {
                yield return concreteType.BaseType;
            }

            if (concreteType.BaseType == typeof(object)) yield break;

            foreach (Type interfaceType in DiscoverInterfaces(concreteType.BaseType!, templateType))
            {
                yield return interfaceType;
            }
        }

        private static IEnumerable<Type> GetInterfacesFromConcreteType(Type concreteType, Type interfaceType)
        {
            return concreteType.GetInterfaces().Where(t => t.IsGenericType && (t.GetGenericTypeDefinition() == interfaceType));
        }
    }
}
