using System;
using Google.Protobuf.WellKnownTypes;

namespace SysCribBackend.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime ToDateTime(this Timestamp timestamp)
        {
            return timestamp.ToDateTime();
        }

        public static string ToFormattedDate(this Timestamp timestamp)
        {
            var dateTime = timestamp.ToDateTime();
            return dateTime.ToString("yyyy-MM-dd"); // Format as Year-Month-Day
        }
    }
}
