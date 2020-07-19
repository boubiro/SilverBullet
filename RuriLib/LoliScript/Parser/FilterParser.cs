using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace RuriLib.LS
{
    /// <summary>
    /// Parses a Parameter(s)
    /// </summary>
    public static class FilterParser
    {
        public static object ParseObject(ref object[] values, Type parameterType,
            bool convertOneVal = true)
        {
            if (parameterType.GetTypeInfo().IsEnum)
                return Enum.Parse(parameterType, values[0].ToString(), true);

            if (values.Length == 1 && convertOneVal)
            {
                object ct = null;
                try
                {
                    ct = Convert.ChangeType(values[0], parameterType);
                }
                catch
                {
                    if (parameterType.IsClass ||
                        parameterType.IsEnum ||
                        parameterType.IsValueType)
                    {
                        ct = ParseObject(ref values,
                            parameterType,
                            false);
                    }
                }
                try { values = values.RemoveAt(0); } catch { }
                return ct;
            }

            else if ((parameterType.IsClass || parameterType.IsValueType) &&
                     !parameterType.IsAbstract &&
                     !parameterType.IsInterface &&
                     !parameterType.IsPrimitive)
            {
                var constructors = parameterType.GetConstructors();
                ConstructorInfo constructor = null;
                ParameterInfo[] ctorParams = null;
                var ctorIndex = 0;
                while (ctorIndex < constructors.Length)
                {
                    constructor = constructors[ctorIndex];
                    ctorParams = constructor.GetParameters();
                    if (ctorParams.Length == values.Length)
                    {
                        break;
                    }
                    else if (ctorParams.Length == 1)
                    {
                        if (ParameterIsForValue(ctorParams, values[0]))
                        {
                            break;
                        }
                    }
                    ctorIndex++;
                    constructor = null;
                }
                if (constructor == null) { constructor = constructors.FirstOrDefault(); }
                List<object> parameterObject = new List<object>();
                object constructorObject = null;
                if (ctorParams?.Length > 0)
                {
                    for (var j = 0; j < ctorParams.Length; j++)
                    {
                        //change type -> value type to param type
                        try
                        {
                            parameterObject.Add(ChangeType(values[j],
                           ctorParams[j].ParameterType));
                        }
                        catch (IndexOutOfRangeException)
                        {
                            parameterObject.Add(ChangeType(values.First(),
                                ctorParams[j].ParameterType));
                        }
                        catch (InvalidCastException)
                        {
                            if (ctorParams[j].ParameterType.IsClass ||
                                ctorParams[j].ParameterType.IsEnum ||
                                ctorParams[j].ParameterType.IsValueType)
                            {
                                parameterObject.Add(ParseObject(ref values, ctorParams[j].ParameterType));
                            }
                        }
                    }
                    for (var r = 0; r < parameterObject.Count; r++)
                        try { RemoveValue(ref values); } catch { }
                    //get first constructor from constructors
                    //invoke -> set params object in first constructor
                    constructorObject = constructor.Invoke(parameterObject.ToArray());
                }
                else
                {
                    if (parameterType.IsColor())
                    {
                        constructorObject = CreateColor(ref values);
                    }
                }
                //create instance of class with constructor
                return ChangeType(constructorObject, parameterType);
            }
            var ct2 = ChangeType(values[0], parameterType);
            RemoveValue(ref values);
            return ct2;
        }

        private static Color CreateColor(ref object[] values, bool alpha = false)
        {
            var alphaColor = 255;
            if (values.Length == 4 && alpha)
            {
                try
                {
                    alphaColor = int.Parse(values[0].ToString());
                    RemoveValue(ref values);
                }
                catch { }
            }
            var red = int.Parse(values[0].ToString());
            RemoveValue(ref values);
            var green = int.Parse(values[0].ToString());
            RemoveValue(ref values);
            var blue = int.Parse(values[0].ToString());
            RemoveValue(ref values);
            return Color.FromArgb(alphaColor, red, green, blue);
        }

        private static object ChangeType(object value, Type parameterType)
        {
            if (parameterType.GetTypeInfo().IsEnum)
                return Enum.Parse(parameterType, value.ToString(), true);
            else if (parameterType.IsNullableEnum(out Type enumType))
            {
                try
                {
                    return Enum.Parse(enumType, value.ToString());
                }
                catch (ArgumentException) { return null; }
            }
            else if (parameterType.IsNullable(out Type nullableType))
            {
                if (nullableType.IsValueType)
                {
                    var obj = new[] { value };
                    try
                    {
                        return ParseObject(ref obj, nullableType);
                    }
                    catch (ArgumentException)
                    {
                        return null;
                    }
                }
                try
                {
                    return Convert.ChangeType(value, nullableType);
                }
                catch(ArgumentException) { return null; }
            }
            try
            {
                return Convert.ChangeType(value, parameterType);
            }
            catch (ArgumentException) { return null; }
            catch (FormatException) { return null; }
        }

        private static object ChangeType(object[] values, Type parameterType)
        {
            if (parameterType.GetTypeInfo().IsEnum)
                return Enum.Parse(parameterType, values[0].ToString(), true);

            var objList = new List<object>();

            for (var i = 0; i < values.Length; i++)
            {
                return Convert.ChangeType(values[i], parameterType);
            }
            return objList.ToArray();
        }

        private static void RemoveValue(ref object[] values, int index = 0)
        {
            values = values.RemoveAt(index);
        }

        private static bool ParameterIsForValue(ParameterInfo[] parameters,
            object value)
        {
            try
            {
                var cp = parameters.FirstOrDefault().ParameterType;
                return cp ==
                     ChangeType(value, cp)
                     .GetType();
            }
            catch (Exception ex) { return false; }
        }

    }
}
