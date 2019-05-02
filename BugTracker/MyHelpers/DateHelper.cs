using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace BugTracker.MyHelpers
{
    [NotMapped]
    public static class DateHelper
    {
        public static string GetDateAndTime(this DateTime date) => $"{date.ToString("m") + ", " + date.ToShortTimeString()} | {date.Year}";

        public static string GetDateTimeFromNow(this DateTime date, string justUpdatedString = "Just Updated")
        {
            DateTime now = DateTime.Now;
            DateTime today = DateTime.Today;

            int postDay = today.Day - date.Day;
            postDay = postDay < 0 ? postDay * -1 : postDay; // ensure postDay is positive
            TimeSpan postTime = now.TimeOfDay - date.TimeOfDay;

            if (postDay >= 1)
            {
                return $"{postDay} {(postDay == 1 ? "day" : "days")} ago";
            }
            else if (postDay == 0 && postTime.Hours >= 1)
            {
                return $"{postTime.Hours} {(postTime.Hours == 1 ? "hour" : "hours")} ago";
            }
            else if (postTime.Hours == 0 && postTime.Minutes >= 1)
            {
                return $"{postTime.Minutes} {(postTime.Minutes == 1 ? "minute" : "minutes")} ago";
            }
            else if (postTime.Minutes == 0 && postTime.Seconds >= 1)
            {
                return $"{postTime.Seconds} {(postTime.Seconds == 1 ? "second" : "seconds")} ago";
            }
            else
            {
                return justUpdatedString;
            }
        }
    }
}