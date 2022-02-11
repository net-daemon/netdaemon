using System.ComponentModel;
using System.Dynamic;

namespace NetDaemon.Extensions.MqttEntityManager.Helpers;

/// <summary>
/// Helper utilities for dynamics
/// </summary>
public class DynamicHelpers
{
    /// <summary>
    /// Return true if the specified dynamic object has a property of this name
    /// </summary>
    /// <param name="o"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
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