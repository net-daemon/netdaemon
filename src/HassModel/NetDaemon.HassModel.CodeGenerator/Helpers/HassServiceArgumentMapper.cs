using System;
using JoySoftware.HomeAssistant.Model;

namespace NetDaemon.HassModel.CodeGenerator.Helpers
{
    internal static class HassServiceArgumentMapper
    {
        public static ServiceArgument Map(HassServiceField field)
        {
            Type type = GetTypeFromSelector(field.Selector);

            return new ServiceArgument
            {
                HaName = field.Field!,
                Type = type,
                Required = field.Required == true,
                Comment = field.Description + (string.IsNullOrWhiteSpace(field.Example?.ToString()) ? "" : $" eg: {field.Example}")
            };
        }
        private static Type GetTypeFromSelector(object? selectorObject)
        {
            return selectorObject switch
            {
                BooleanSelector => typeof(bool),
                NumberSelector s when (s.Step ?? 1) % 1 != 0 => typeof(double),
                NumberSelector => typeof(long),
                TimeSelector => typeof(DateTime),
                ObjectSelector => typeof(object),
                _ => typeof(string)
            };
        }
    }
}