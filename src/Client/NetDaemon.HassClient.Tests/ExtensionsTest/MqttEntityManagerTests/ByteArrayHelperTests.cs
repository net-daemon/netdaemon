using System.Collections;
using NetDaemon.Extensions.MqttEntityManager.Helpers;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class ByteArrayHelperTests
{
    sealed class GoodData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return [null!, ""];
            yield return [new byte[] { 0x68, 0x65, 0x6c, 0x6c, 0x6f}, "hello"];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    [Theory]
    [ClassData(typeof(GoodData))]
    public void CanParse(byte[]? array, string expected)
    {
        ByteArrayHelper.SafeToString(array).Should().Be(expected);
    }
}
