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
                ActionSelector
                    or AreaSelector
                    or AddonSelector
                    or EntitySelector
                    or DeviceSelector
                    or ObjectSelector
                    or TargetSelector
                    or TextSelector
                    or null => typeof(string),
                BooleanSelector => typeof(bool),
                NumberSelector => typeof(long),
                TimeSelector => typeof(DateTime),
                SelectSelector => typeof(List<string>),
                _ => typeof(string)
            };
        }
    }
}