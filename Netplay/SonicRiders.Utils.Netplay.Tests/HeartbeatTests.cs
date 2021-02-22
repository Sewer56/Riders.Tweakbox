using System;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Xunit;

namespace Riders.Netplay.Messages.Tests
{
    public class HeartbeatTests
    {
        [Fact]
        public void Standard()
        {
            var timeNow  = DateTime.UtcNow;
            var timeSoon = DateTime.UtcNow.AddSeconds(5);

            var hbNow  = new AntiCheatHeartbeat(timeNow, 300);
            var hbSoon = new AntiCheatHeartbeat(timeSoon, 300);

            Assert.False(hbSoon.IsCheating(hbNow, out _));
        }

        [Fact]
        public void SpeedHack3Percent()
        {
            var timeNow = DateTime.UtcNow;
            var timeSoon = DateTime.UtcNow.AddSeconds(5);

            var hbNow = new AntiCheatHeartbeat(timeNow, (short) (300 * 1.03));
            var hbSoon = new AntiCheatHeartbeat(timeSoon, (short) (300 * 1.03));

            Assert.True(hbSoon.IsCheating(hbNow, out _));
        }

        [Fact]
        public void SpeedHackMarginOfError()
        {
            var timeNow = DateTime.UtcNow;
            var timeSoon = DateTime.UtcNow.AddSeconds(5);

            var hbNow = new AntiCheatHeartbeat(timeNow, (short)(300 * 1.015));
            var hbSoon = new AntiCheatHeartbeat(timeSoon, (short)(300 * 1.015));

            Assert.False(hbSoon.IsCheating(hbNow, out _));
        }
    }
}