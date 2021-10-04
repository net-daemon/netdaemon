using System;
using System.Collections.Generic;
using JoySoftware.HomeAssistant.Model;
namespace NetDaemon.Service.App.CodeGeneration.Helpers
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
                Required = field.Required == true
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
                _ => typeof(string)
            };
        }
    }
}