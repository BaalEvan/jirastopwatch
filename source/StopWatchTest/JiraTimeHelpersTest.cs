namespace StopWatchTest
{
    using NUnit.Framework;
    using StopWatch;
    using System;
    using System.Globalization;
    using System.Threading;

    [TestFixture]
    public class JiraTimeHelpersTest
    {
        [SetUp]
        public void Setup()
        {
            JiraTimeHelpers.Configuration = null;
        }

        [Test]
        public void DateTimeToJiraDateTime_HandlesTimeZones()
        {
            Assert.That(JiraTimeHelpers.DateTimeToJiraDateTime(new DateTimeOffset(2015, 09, 20, 16, 40, 51, TimeSpan.Zero)), Is.EqualTo("2015-09-20T16:40:51.000+0000"));
            Assert.That(JiraTimeHelpers.DateTimeToJiraDateTime(new DateTimeOffset(2015, 09, 20, 16, 40, 51, TimeSpan.FromHours(1))), Is.EqualTo("2015-09-20T16:40:51.000+0100"));
            Assert.That(JiraTimeHelpers.DateTimeToJiraDateTime(new DateTimeOffset(2015, 09, 20, 16, 40, 51, TimeSpan.FromMinutes(9 * 60 + 30))), Is.EqualTo("2015-09-20T16:40:51.000+0930"));
        }

        [Test]
        public void DateTimeToJiraDateTime_IgnoreRegionalSettings()
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("bn-BD");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("bn-BD");
                var s = JiraTimeHelpers.DateTimeToJiraDateTime(DateTimeOffset.Now);
                Assert.That(JiraTimeHelpers.DateTimeToJiraDateTime(new DateTimeOffset(2015, 09, 20, 16, 40, 51, TimeSpan.Zero)), Is.EqualTo("2015-09-20T16:40:51.000+0000"));
                Assert.That(JiraTimeHelpers.DateTimeToJiraDateTime(new DateTimeOffset(2015, 09, 20, 16, 40, 51, TimeSpan.FromHours(1))), Is.EqualTo("2015-09-20T16:40:51.000+0100"));
                Assert.That(JiraTimeHelpers.DateTimeToJiraDateTime(new DateTimeOffset(2015, 09, 20, 16, 40, 51, TimeSpan.FromMinutes(9 * 60 + 30))), Is.EqualTo("2015-09-20T16:40:51.000+0930"));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
                Thread.CurrentThread.CurrentUICulture = currentUICulture;
            }
        }

        [Test]
        public void TimeSpanToJira_FormatsDaysHoursMinutes()
        {
            Assert.That(JiraTimeHelpers.TimeSpanToJiraTime(new TimeSpan(12, 7, 0)), Is.EqualTo("12h 7m"));
            Assert.That(JiraTimeHelpers.TimeSpanToJiraTime(new TimeSpan(9, 15, 0)), Is.EqualTo("9h 15m"));
            Assert.That(JiraTimeHelpers.TimeSpanToJiraTime(new TimeSpan(1, 2, 5, 0)), Is.EqualTo("1d 2h 5m"));
            Assert.That(JiraTimeHelpers.TimeSpanToJiraTime(new TimeSpan(21, 4, 0, 0)), Is.EqualTo("21d 4h 0m"));
        }


        [Test]
        public void JiraTimeToTimeSpan_InvalidMinutesFails()
        {
            Assert.IsNull(JiraTimeHelpers.JiraTimeToTimeSpan("m"));
            Assert.IsNull(JiraTimeHelpers.JiraTimeToTimeSpan("2 m"));
        }

        [Test]
        public void JiraTimeToTimeSpan_InvalidHoursFails()
        {
            Assert.IsNull(JiraTimeHelpers.JiraTimeToTimeSpan("h"));
            Assert.IsNull(JiraTimeHelpers.JiraTimeToTimeSpan("8 h"));
        }

        /*
        [Test]
        public void JiraTimeToTimeSpan_ValidHoursWithInvalidMinutesFails()
        {
            Assert.IsNull(JiraTimeHelpers.JiraTimeToTimeSpan("2h 5"));
            Assert.IsNull(JiraTimeHelpers.JiraTimeToTimeSpan("2h m"));
        }

        [Test]
        public void JiraTimeToTimeSpan_InvalidHoursWithValidMinutesFails()
        {
            Assert.IsNull(JiraTimeHelpers.JiraTimeToTimeSpan("2 5m"));
            Assert.IsNull(JiraTimeHelpers.JiraTimeToTimeSpan("h 5m"));
        }
        */

        [Test]
        public void JiraTimeToTimeSpan_ParsesJiraStyleTimespan()
        {
            Assert.AreEqual(120, JiraTimeHelpers.JiraTimeToTimeSpan("2h").Value.TotalMinutes);
            Assert.AreEqual(125, JiraTimeHelpers.JiraTimeToTimeSpan("2h 5m").Value.TotalMinutes);
            Assert.AreEqual(5, JiraTimeHelpers.JiraTimeToTimeSpan("5m").Value.TotalMinutes);
            Assert.AreEqual(0, JiraTimeHelpers.JiraTimeToTimeSpan("0").Value.TotalMinutes);
        }

        [Test]
        public void JiraTimeToTimeSpan_ParsesDecimalHours()
        {
            Assert.AreEqual(150, JiraTimeHelpers.JiraTimeToTimeSpan("2.5h").Value.TotalMinutes);
        }

        [Test]
        public void JiraTimeToTimeSpan_IgnoresDecimalValueForMinutes()
        {
            Assert.AreEqual(600, JiraTimeHelpers.JiraTimeToTimeSpan("10.5m").Value.TotalSeconds);
        }

        [Test]
        public void JiraTimeToTimeSpan_AllowsMinutesBeforeHours()
        {
            Assert.AreEqual(125, JiraTimeHelpers.JiraTimeToTimeSpan("5m 2h").Value.TotalMinutes);
        }

        [Test]
        public void JiraTimeToTimeSpan_AllowsSillyValues()
        {
            Assert.AreEqual(120, JiraTimeHelpers.JiraTimeToTimeSpan("2h 0m").Value.TotalMinutes);
            Assert.AreEqual(5, JiraTimeHelpers.JiraTimeToTimeSpan("0h 5m").Value.TotalMinutes);
        }

        [Test]
        public void JiraTimeToTimeSpan_AllowsMultipleWhitespace()
        {
            Assert.AreEqual(65, JiraTimeHelpers.JiraTimeToTimeSpan("1h      5m").Value.TotalMinutes);
            Assert.AreEqual(125, JiraTimeHelpers.JiraTimeToTimeSpan("    2h   5m    ").Value.TotalMinutes);
        }

        [Test]
        public void JiraTimeToTimeSpan_AllowsNoWhitespace()
        {
            Assert.AreEqual((0 * 8 + 2) * 60 + 5, JiraTimeHelpers.JiraTimeToTimeSpan("2h5m").Value.TotalMinutes);
            Assert.AreEqual((1 * 8 + 2) * 60 + 5, JiraTimeHelpers.JiraTimeToTimeSpan("1d2h5m").Value.TotalMinutes);
        }

        [Test]
        public void JiraTimeToTimeSpan_AllowsDays()
        {
            Assert.AreEqual((1 * 8 + 2) * 60 + 5, JiraTimeHelpers.JiraTimeToTimeSpan("1d 2h 5m").Value.TotalMinutes);
            Assert.AreEqual((1 * 8 + 2) * 60, JiraTimeHelpers.JiraTimeToTimeSpan("1d 2h").Value.TotalMinutes);
            Assert.AreEqual((1 * 8) * 60 + 5, JiraTimeHelpers.JiraTimeToTimeSpan("1d 5m").Value.TotalMinutes);
        }

        [Test]
        public void JiraTimeToTimeSpan_TwoDaysCheck()
        {
            Assert.AreEqual((2 * 8 + 1) * 60 + 4, JiraTimeHelpers.JiraTimeToTimeSpan("2d 1h 4m").Value.TotalMinutes);
        }

        [Test]
        public void JiraRoundUpTo6Or15M_Correct()
        {
            Assert.That(new TimeSpan(0, 6, 0), Is.EqualTo(JiraTimeHelpers.RoundUpTo6Or15M(new TimeSpan(0, 0, 1))));
            Assert.That(new TimeSpan(0, 6, 0), Is.EqualTo(JiraTimeHelpers.RoundUpTo6Or15M(new TimeSpan(0, 1, 0))));
            Assert.That(new TimeSpan(0, 6, 0), Is.EqualTo(JiraTimeHelpers.RoundUpTo6Or15M(new TimeSpan(0, 5, 0))));
            Assert.That(new TimeSpan(0, 6, 0), Is.EqualTo(JiraTimeHelpers.RoundUpTo6Or15M(new TimeSpan(0, 6, 0))));
            Assert.That(new TimeSpan(0, 15, 0), Is.EqualTo(JiraTimeHelpers.RoundUpTo6Or15M(new TimeSpan(0, 6, 1))));
            Assert.That(new TimeSpan(0, 15, 0), Is.EqualTo(JiraTimeHelpers.RoundUpTo6Or15M(new TimeSpan(0, 15, 0))));
            Assert.That(new TimeSpan(0, 30, 0), Is.EqualTo(JiraTimeHelpers.RoundUpTo6Or15M(new TimeSpan(0, 15, 1))));
            Assert.That(new TimeSpan(1, 0, 0), Is.EqualTo(JiraTimeHelpers.RoundUpTo6Or15M(new TimeSpan(0, 59, 1))));
            Assert.That(new TimeSpan(1, 15, 0), Is.EqualTo(JiraTimeHelpers.RoundUpTo6Or15M(new TimeSpan(0, 60, 1))));
        }
    }
}
