using System.Text;

namespace NetDaemon.Extensions.MqttEntityManager.Helpers;

/// <summary>
/// Helpers for byte arrays
/// </summary>
internal static class ByteArrayHelper
{
     /// <summary>
     /// Convert a byte array to a string, or to an empty string if the array is not valid UTF8
     /// </summary>
     /// <param name="array">The byte array to convert.</param>
     /// <returns>The UTF-8 string, or an empty string when the array is null, empty, or invalid.</returns>
     public static string SafeToString(byte[]? array)
     {
          try
          {
               if (array == null || array.Length == 0)
                    return "";
               
               return Encoding.UTF8.GetString(array);
          }
          catch (Exception)
          {
               return "";
          }
     }
}
