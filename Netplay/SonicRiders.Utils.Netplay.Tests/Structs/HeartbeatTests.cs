using System;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Xunit;

namespace Riders.Netplay.Messages.Tests.Structs
{
    public class HeartbeatTests
    {

        [Fact]
        public void Standard()
        {
            var timeNow  = DateTime.UtcNow;
            var timeSoon = DateTime.UtcNow.AddSeconds(5);

            var hbNow  = new Heartbeat(timeNow, 300);
            var hbSoon = new Heartbeat(timeSoon, 300);

            Assert.False(hbSoon.IsCheating(hbNow, out _));
        }

        [Fact]
        public void SpeedHack3Percent()
        {
            var timeNow = DateTime.UtcNow;
            var timeSoon = DateTime.UtcNow.AddSeconds(5);

            var hbNow = new Heartbeat(timeNow, (short) (300 * 1.03));
            var hbSoon = new Heartbeat(timeSoon, (short) (300 * 1.03));

            Assert.True(hbSoon.IsCheating(hbNow, out _));
        }

        [Fact]
        public void SpeedHackMarginOfError()
        {
            var timeNow = DateTime.UtcNow;
            var timeSoon = DateTime.UtcNow.AddSeconds(5);

            var hbNow = new Heartbeat(timeNow, (short)(300 * 1.015));
            var hbSoon = new Heartbeat(timeSoon, (short)(300 * 1.015));

            Assert.False(hbSoon.IsCheating(hbNow, out _));
        }


    }
}