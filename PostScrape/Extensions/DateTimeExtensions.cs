using System;
using NodaTime;
using NodaTime.Extensions;

namespace PostScrape.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts a non-local-time DateTime to a local-time DateTime based on the
        /// specified timezone. The returned object will be of Unspecified DateTimeKind 
        /// which represents local time agnostic to servers timezone. To be used when
        /// we want to convert UTC to local time somewhere in the world.
        /// </summary>
        /// <param name="dateTimeOffset">Non-local DateTime as UTC or Unspecified DateTimeKind.</param>
        /// <param name="timezone">Timezone name (in TZDB format).</param>
        /// <returns>Local DateTime as Unspecified DateTimeKind.</returns>
        public static DateTime ToZone(this DateTimeOffset dateTimeOffset, string timezone)
        {
            if (dateTimeOffset.DateTime.Kind == DateTimeKind.Local)
                throw new ArgumentException("Expected non-local kind of DateTime");

            var zone = DateTimeZoneProviders.Tzdb[timezone];
            var instant = dateTimeOffset.ToInstant();
            var inZone = instant.InZone(zone);
            var unspecified = inZone.ToDateTimeUnspecified();

            return unspecified;
        }
    }
}