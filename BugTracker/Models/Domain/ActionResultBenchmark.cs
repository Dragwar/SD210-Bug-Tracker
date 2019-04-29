using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;

namespace BugTracker.Models.Domain
{
    [NotMapped]
    [Serializable]
    public class ActionResultBenchmark
    {
        public Guid Id { get; }
        public string Name { get; }
        public string ControllerName { get; }
        public string ActionName { get; }
        public long TotalActionResultMilliseconds { get; }
        public long GrandTotalMilliseconds { get; }
        public long NoiseMilliseconds { get; }
        public long ActionMilliseconds { get; }
        public long ResultMilliseconds { get; }
        public DateTime Created { get; }

        /// <summary>All parameters is needed for serializing/deserializing this class to and from JSON with "<see cref="Newtonsoft.Json"/>"</summary>
        public ActionResultBenchmark(
            string controllerName,
            string actionName,
            long grandTotalMilliseconds,
            long actionMilliseconds,
            long resultMilliseconds,
            [Optional] Guid id,
            [Optional] DateTime created,
            [Optional] string name)
        {
            ControllerName = controllerName ?? throw new ArgumentNullException(nameof(controllerName));
            ActionName = actionName ?? throw new ArgumentNullException(nameof(actionName));

            if (!IsNumbersPositive(grandTotalMilliseconds, actionMilliseconds, resultMilliseconds)) throw new ArgumentOutOfRangeException("Numbers can't be below zero");

            Id = id == null || id == Guid.Empty ? Guid.NewGuid() : id;
            Name = !string.IsNullOrWhiteSpace(name) ? name : Id.ToString().ToUpper().Substring(0, 4);
            Created = created == null || created == default ? DateTime.Now : created;

            ActionMilliseconds = actionMilliseconds;
            ResultMilliseconds = resultMilliseconds;
            TotalActionResultMilliseconds = actionMilliseconds + resultMilliseconds;
            GrandTotalMilliseconds = grandTotalMilliseconds;
            NoiseMilliseconds = GrandTotalMilliseconds - TotalActionResultMilliseconds;
        }

        private bool IsNumbersPositive(params long[] numbers) => numbers.All(num => num >= 0);
        public override string ToString() => $"Date: {Created.ToShortDateString()}, GrandTotal: {GrandTotalMilliseconds}, Action: {ActionMilliseconds}, Result: {ResultMilliseconds}";
    }
}