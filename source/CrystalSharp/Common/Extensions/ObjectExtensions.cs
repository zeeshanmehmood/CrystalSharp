// The MIT License (MIT)
//
// Copyright (c) 2024 Zeeshan Mehmood
// https://github.com/zeeshanmehmood/CrystalSharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace CrystalSharp.Common.Extensions
{
    public static class ObjectExtensions
    {
        public static dynamic AsDynamic(this object o)
        {
            return PrivateReflectionDynamicObject.WrapObjectIfNeeded(o);
        }

        private static bool IsOfType<T>(object value)
        {
            return value is T;
        }

        public static object CopyTo(this object source, Type destinationObject)
        {
            object copy = CopyFromSource(source, destinationObject);

            return copy;
        }

        public static T CopyTo<T>(this object source)
        {
            Type destinationObject = typeof(T);
            T copy = (T)CopyFromSource(source, destinationObject);

            return copy;
        }

        public static object CopyFromSource(object source, Type destinationObject)
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

                                if (targetValue != null)
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

        public static IDictionary<string, object> ToDictionary(this object source)
        {
            return source.ToDictionary<object>();
        }

        public static IDictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            IDictionary<string, T> dictionary = new Dictionary<string, T>();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                object value = property.GetValue(source);

                if (IsOfType<T>(value))
                {
                    dictionary.Add(property.Name, (T)value);
                }
            }

            return dictionary;
        }

        public static T ToObject<T>(this IDictionary<string, object> source) where T : class
        {
            return ToObject<T, string, object>(source);
        }

        public static T ToObject<T, TKey, TValue>(this IDictionary<TKey, TValue> source) where T : class
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
