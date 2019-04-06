using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace BugTracker.MyHelpers
{

    [NotMapped]
    public static class DateHelper
    {
        public static string GetDateAndTime(this DateTime Date) => $"{Date.ToString("m") + ", " + Date.ToShortTimeString()} | {Date.Year}";

        public static string GetDateTimeFromNow(this DateTime Date, string justUpdatedString = "Just Updated")
        {
            int postDay = DateTime.Today.Day - Date.Day;
            TimeSpan postTime = DateTime.Now.TimeOfDay - Date.TimeOfDay;
            if (postDay >= 1)
            {
                return $"{postDay} day(s) ago";
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