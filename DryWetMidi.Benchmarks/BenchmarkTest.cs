﻿using System;
using System.Text;
using BenchmarkDotNet.Running;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks
{
    public abstract class BenchmarkTest
    {
        #region Constants

        internal const string TestCategory = "Benchmark";

        #endregion

        #region Properties

        public TestContext TestContext { get; set; }

        #endregion

        #region Methods

        [SetUp]
        public void SetupTest()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }

        protected void RunBenchmarks<TBenchmarks>()
        {
            var summary = BenchmarkRunner.Run<TBenchmarks>();

            var validationErrorsStringBuilder = new StringBuilder();

            foreach (var error in summary.ValidationErrors)
            {
                var benchmarkDisplayInfo = error.Benchmark?.DisplayInfo;
                var isCritical = error.IsCritical;
                var message = error.Message;

                validationErrorsStringBuilder.AppendLine($"[{benchmarkDisplayInfo} | Critical={isCritical}]: {message}. ");
            }

            var validationError = validationErrorsStringBuilder.ToString().Trim();

            Assert.IsEmpty(validationError, validationError);
        }

        #endregion
    }
}