using Xunit;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using System;

namespace NetDaemon.Daemon.Tests.NetDaemonApp
{
    public class AppExtensionsTests
    {

        [Theory]
        [InlineData("ABCDEFGHIJKLMNOPQRSTUVXYZ", "abcdefghijklmnopqrstuvxyz")]
        [InlineData("Å_Ä_Ö", "a_a_o")]
        [InlineData("åäö", "aao")]
        [InlineData("ÈÉÊËĒĔĖĘĚẼẺẸỀẾỄỂỆ", "eeeeeeeeeeeeeeeee")]
        [InlineData("èéêëēĕėęěẽẻẹềếễểệ", "eeeeeeeeeeeeeeeee")]
        public void TestToSafeHomeAssistantEntityIdReturnCorrectString(string convert, string expected)
        {
            //ACT
            string converted = convert.ToSafeHomeAssistantEntityId();

            Assert.Equal(expected.Length, converted.Length);
            Assert.Equal(expected, converted);
        }
    }
}