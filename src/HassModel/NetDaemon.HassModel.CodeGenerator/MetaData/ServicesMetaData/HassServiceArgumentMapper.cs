namespace NetDaemon.HassModel.CodeGenerator.Helpers;

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
            NumberSelector { Step: not null } s when s.Step % 1 == 0 => typeof(long),
            NumberSelector => typeof(double),
            EntitySelector => typeof(string),
            DeviceSelector => typeof(string),
            AreaSelector => typeof(string),
            _ => selectorObject.Type switch
            {
                "color_rgb" => typeof(IReadOnlyCollection<int>),
                "boolean" => typeof(bool),
                "date" => typeof(DateOnly),
                "time" => typeof(TimeOnly),
                "datetime" => typeof(DateTime),
                "text" => typeof(string),
                _ => typeof(object)
            },
        };

        return selectorObject?.Multiple ?? false ? typeof(IEnumerable<>).MakeGenericType(clrType) : clrType;
    }
}
