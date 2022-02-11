using System.ComponentModel;
using System.Dynamic;

namespace NetDaemon.Extensions.MqttEntityManager.Helpers;

public class DynamicHelpers
{
    public static bool PropertyExists(dynamic o, string propertyName)
    {
        if (o == null)
            return false;
        
        var properties = TypeDescriptor.GetProperties(o.GetType());
        foreach (var property in properties)
        {
            if (property.Name == propertyName)
                return true;
        }
        return false;
    }
}