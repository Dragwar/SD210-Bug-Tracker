using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models.Domain;
using BugTracker.MyHelpers;
using BugTracker.MyHelpers.DB_Repositories;

namespace BugTracker.Models.Filters.Actions
{
    public class Benchmark : ActionFilterAttribute, IActionFilter, IResultFilter
    {
        private readonly HttpServerUtilityWrapper HttpServerUtilityWrapper;
        private readonly FileSystemRepository FileSystemRepository;
        private readonly Stopwatch TotalStopwatch = new Stopwatch();
        private readonly Stopwatch Stopwatch = new Stopwatch();

        private string BenchmarkName { get; }
        private List<ActionResultBenchmark> Benchmarks { get; set; }

        private const string Controller = "controller";
        private const string Action = "action";
        private const string BenchmarkFolder = CONSTANTS.BenchmarkFolder;
        private long TempNum { get; set; }

        public Benchmark([Optional] string benchmarkName)
        {
            HttpServerUtilityWrapper = new HttpServerUtilityWrapper(HttpContext.Current.Server);
            FileSystemRepository = new FileSystemRepository(HttpServerUtilityWrapper, BenchmarkFolder);
            BenchmarkName = benchmarkName ?? $"{HttpServerUtilityWrapper.MachineName}_Benchmark";

            (List<ActionResultBenchmark> result, bool hasLoaded, _) = FileSystemRepository.LoadJsonFile<List<ActionResultBenchmark>>(BenchmarkName);
            Benchmarks = hasLoaded ? result : new List<ActionResultBenchmark>();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            TotalStopwatch.Reset();
            Stopwatch.Reset();
            TotalStopwatch.Start();
            Stopwatch.Start();
        }
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            Stopwatch.Stop();
            long actionMilliseconds = Stopwatch.ElapsedMilliseconds;
            TempNum = actionMilliseconds;
        }
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            Stopwatch.Reset();
            Stopwatch.Start();
        }
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            TotalStopwatch.Stop();
            Stopwatch.Stop();

            long grandTotalMilliseconds = TotalStopwatch.ElapsedMilliseconds;
            long resultMilliseconds = Stopwatch.ElapsedMilliseconds;

            filterContext.RouteData.Values.TryGetValue(Controller, out object conName);
            filterContext.RouteData.Values.TryGetValue(Action, out object actName);
            string controllerName = conName as string ?? throw new Exception();
            string actionName = actName as string ?? throw new Exception();

            Benchmarks.Add(new ActionResultBenchmark(
                controllerName: controllerName,
                actionName: actionName,
                grandTotalMilliseconds: grandTotalMilliseconds,
                actionMilliseconds: TempNum, // more accurate to compared to subtracting "grandTotal" and "result"
                resultMilliseconds: resultMilliseconds));

            (bool hasSaved, string filePath, string resultMessage) = FileSystemRepository.SaveJsonFile(BenchmarkName, Benchmarks);

            if (!hasSaved)
            {
                throw new Exception($"Error - log file not saved ({filePath}): {resultMessage}");
            }
        }
    }
}