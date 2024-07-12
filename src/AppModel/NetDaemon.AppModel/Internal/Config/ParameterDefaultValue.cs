using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace NetDaemon.AppModel.Internal.Config;

internal static class ParameterDefaultValue
{
    public static bool TryGetDefaultValue(ParameterInfo parameter, out object? defaultValue)
    {
        bool tryToGetDefaultValue;
        bool flag = CheckHasDefaultValue(parameter, out tryToGetDefaultValue);
        defaultValue = null;
        if (flag)
        {
            if (tryToGetDefaultValue)
            {
                defaultValue = parameter.DefaultValue;
            }

            bool flag2 = parameter.ParameterType.IsGenericType && parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (defaultValue == null && parameter.ParameterType.IsValueType && !flag2)
            {
                defaultValue = CreateValueType(parameter.ParameterType);
            }

            if (defaultValue != null && flag2)
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                Type underlyingType = Nullable.GetUnderlyingType(parameter.ParameterType);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                if (underlyingType != null && underlyingType.IsEnum)
                {
                    defaultValue = Enum.ToObject(underlyingType, defaultValue);
                }
            }
        }

        return flag;
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2067:UnrecognizedReflectionPattern", Justification = "CreateValueType is only called on a ValueType. You can always create an instance of a ValueType.")]
        static object? CreateValueType(Type t)
        {
            return RuntimeHelpers.GetUninitializedObject(t);
        }
    }

    public static bool CheckHasDefaultValue(ParameterInfo parameter, out bool tryToGetDefaultValue)
    {
        tryToGetDefaultValue = true;
        return parameter.HasDefaultValue;
    }
}
