using System.Text;

namespace NetDaemon.Extensions.MqttEntityManager.Helpers;

internal static class ByteArrayHelper
{
     public static string SafeToString(byte[]? array)
     {
          try
          {
               if (array == null || array.Length == 0)
                    return "";
               
               return Encoding.UTF8.GetString(array);
          }
          catch (Exception e)
          {
               return "";
          }
     }
}