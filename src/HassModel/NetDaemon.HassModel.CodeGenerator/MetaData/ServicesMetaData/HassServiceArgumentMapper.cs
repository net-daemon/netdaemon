﻿namespace NetDaemon.HassModel.CodeGenerator.Helpers;

internal static class HassServiceArgumentMapper
{
    public static ServiceArgument Map(HassServiceField field)
    {
        return new ServiceArgument
        {
            HaName = field.Field,
            ClrType = GetClrTypeFromSelector(field.Selector),
            Required = field.Required == true,
            Comment = field.Description + (string.IsNullOrWhiteSpace(field.Example?.ToString()) ? "" : $" eg: {field.Example}")
        };
    }
    private static Type GetClrTypeFromSelector(Selector? selectorObject)
    {
        var clrType =  selectorObject switch
        {
            null => typeof(object),
            NumberSelector s when (s.Step ?? 1) % 1 != 0 => typeof(double),
            NumberSelector => typeof(long),
            EntitySelector => typeof(string),
            DeviceSelector => typeof(string),
            AreaSelector => typeof(string),
            _ => selectorObject.Type switch
            {
                "boolean" => typeof(bool),
                "object" => typeof(object),
                "time" => typeof(DateTime), // Maybe TimeOnly??,
                "text" => typeof(string),
                _ => typeof(object)
            },
        };

        return selectorObject?.Multiple ?? false ? typeof(IEnumerable<>).MakeGenericType(clrType) : clrType;
    }
}