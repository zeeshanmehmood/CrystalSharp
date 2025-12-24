using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace CrystalSharp.Common.Extensions
{
    public static class ObjectExtensions
    {
        extension(object o)
        {
            public dynamic AsDynamic()
            {
                return PrivateReflectionDynamicObject.WrapObjectIfNeeded(o);
            }

            public bool IsOfType<T>()
            {
                return o is T;
            }

            public object CopyTo(Type destinationObject)
            {
                object copy = o.CopyFromSource(o, destinationObject);

                return copy;
            }

            public T CopyTo<T>()
            {
                Type destinationObject = typeof(T);
                T copy = (T)o.CopyFromSource(o, destinationObject);

                return copy;
            }

            public object CopyFromSource(object source, Type destinationObject)
            {
                var destination = Activator.CreateInstance(destinationObject);

                foreach (PropertyInfo sourceProperty in source.GetType().GetProperties())
                {
                    foreach (PropertyInfo destinationProperty in destination.GetType().GetProperties())
                    {
                        if (destinationProperty.Name == sourceProperty.Name
                            && destinationProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                        {
                            if (destinationProperty.CanWrite)
                            {
                                if (Nullable.GetUnderlyingType(destinationProperty.PropertyType) != null)
                                {
                                    var targetValue = sourceProperty.GetValue(source);

                                    if (targetValue is not null)
                                    {
                                        object changedType = destinationProperty.PropertyType == typeof(DateTime) || destinationProperty.PropertyType == typeof(DateTime?)
                                        ? DateTime.Parse(targetValue.ToString())
                                        : Convert.ChangeType(targetValue, Nullable.GetUnderlyingType(destinationProperty.PropertyType));
                                        destinationProperty.SetValue(destination, changedType, null);
                                    }
                                }
                                else
                                {
                                    if (destinationProperty.PropertyType.BaseType is not null && destinationProperty.PropertyType.BaseType.Equals(typeof(Enum)))
                                    {
                                        destinationProperty.SetValue(destination, Enum.ToObject(destinationProperty.PropertyType, sourceProperty.GetValue(source)), null);
                                    }
                                    else
                                    {
                                        destinationProperty.SetValue(destination, sourceProperty.GetValue(source), null);
                                    }
                                }
                            }

                            break;
                        }
                    }
                }

                return destination;
            }

            public IDictionary<string, T> ToDictionary<T>()
            {
                ArgumentNullException.ThrowIfNull(o);

                Dictionary<string, T> dictionary = [];

                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(o))
                {
                    object value = property.GetValue(o);

                    if (value.IsOfType<T>())
                    {
                        dictionary.Add(property.Name, (T)value);
                    }
                }

                return dictionary;
            }
        }

        extension<TKey, TValue>(IDictionary<TKey, TValue> source)
        {
            public T ToObject<T>() where T : class
            {
                T destinationObject = Activator.CreateInstance<T>();
                Type destinationType = destinationObject.GetType();

                foreach (KeyValuePair<TKey, TValue> item in source)
                {
                    destinationType.GetProperty(item.Key.ToString()).SetValue(destinationObject, item.Value, null);
                }

                return destinationObject;
            }
        }
    }
}
