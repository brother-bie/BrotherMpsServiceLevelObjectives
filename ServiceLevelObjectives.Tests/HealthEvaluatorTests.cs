using Brother.MPS.ServiceLevelObjectives.Entities;
using Brother.MPS.ServiceLevelObjectives.Rules;

namespace Brother.MPS.ServiceLevelObjectives.Tests
{
    [TestFixture]
    internal class HealthEvaluatorTests
    {
        private readonly HealthEvaluator _evaluator = new();

        private static JobMetrics HealthyMetrics()
        {
            return new JobMetrics
            {
                CountryCode = "GB",
                Service = ServiceType.AccountCreation,

                TotalJobs = 10000,
                SuccessfulJobs = 9980,
                FailedJobs = 20,
                RetriedJobs = 100,

                JobsCompletedWithinSla = 9950,

                PendingJobs = 50,
                NormalBacklog = 100,

                Sla = TimeSpan.FromMinutes(30),
                OldestPendingJobAge = TimeSpan.FromMinutes(20),

                CriticalFailure = false
            };
        }

        [Test]
        public void Healthy_metrics_should_be_green()
        {
            var result = _evaluator.Evaluate(HealthyMetrics());

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Green));
            Assert.That(result.Reasons.Count == 0);
        }

        [Test]
        public void Success_rate_below_98_percent_should_be_red()
        {
            var metrics = HealthyMetrics() with
            {
                TotalJobs = 10000,
                SuccessfulJobs = 970,
                FailedJobs = 30
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Red));
            Assert.That(result.Reasons, Does.Not.Contain("Success rate"));
        }

        [Test]
        public void Success_rate_between_98_and_99_5_should_be_amber()
        {
            var metrics = HealthyMetrics() with
            {
                TotalJobs = 10000,
                SuccessfulJobs = 9800,
                FailedJobs = 10
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Amber));
        }

        [Test]
        public void Success_rate_at_99_5_percent_should_be_green()
        {
            var metrics = HealthyMetrics() with
            {
                TotalJobs = 10000,
                SuccessfulJobs = 9950,
                FailedJobs = 5
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Green));
        }

        [Test]
        public void Retry_rate_above_5_percent_should_be_red()
        {
            var metrics = HealthyMetrics() with
            {
                TotalJobs = 10000,
                RetriedJobs = 510
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Red));
        }

        [Test]
        public void Retry_rate_between_2_and_5_percent_should_be_amber()
        {
            var metrics = HealthyMetrics() with
            {
                TotalJobs = 10000,
                RetriedJobs = 300
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Amber));
        }

        [Test]
        public void Retry_rate_below_2_percent_should_be_green()
        {
            var metrics = HealthyMetrics() with
            {
                TotalJobs = 10000,
                RetriedJobs = 19
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Green));
        }

        [Test]
        public void Sla_compliance_below_95_percent_should_be_red()
        {
            var metrics = HealthyMetrics() with
            {
                TotalJobs = 10000,
                JobsCompletedWithinSla = 940
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Red));
        }

        [Test]
        public void Sla_compliance_between_95_and_99_percent_should_be_amber()
        {
            var metrics = HealthyMetrics() with
            {
                TotalJobs = 10000,
                JobsCompletedWithinSla = 9700
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Amber));
        }

        [Test]
        public void Sla_compliance_at_99_percent_should_be_green()
        {
            var metrics = HealthyMetrics() with
            {
                TotalJobs = 10000,
                JobsCompletedWithinSla = 9900
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Green));
        }

        [Test]
        public void Backlog_below_1_5_times_normal_should_be_green()
        {
            var metrics = HealthyMetrics() with
            {
                PendingJobs = 149,
                NormalBacklog = 100
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Green));
        }

        [Test]
        public void Backlog_between_1_5_and_3_times_normal_should_be_amber()
        {
            var metrics = HealthyMetrics() with
            {
                PendingJobs = 200,
                NormalBacklog = 100
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Amber));
        }

        [Test]
        public void Backlog_at_3_times_normal_should_be_red()
        {
            var metrics = HealthyMetrics() with
            {
                PendingJobs = 300,
                NormalBacklog = 100
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Red));
        }

        [Test]
        public void Oldest_pending_job_within_sla_should_be_green()
        {
            var metrics = HealthyMetrics() with
            {
                Sla = TimeSpan.FromMinutes(30),
                OldestPendingJobAge = TimeSpan.FromMinutes(30)
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Green));
        }

        [Test]
        public void Oldest_pending_job_between_1_and_2_times_sla_should_be_amber()
        {
            var metrics = HealthyMetrics() with
            {
                Sla = TimeSpan.FromMinutes(30),
                OldestPendingJobAge = TimeSpan.FromMinutes(45)
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Amber));
        }

        [Test]
        public void Oldest_pending_job_above_2_times_sla_should_be_red()
        {
            var metrics = HealthyMetrics() with
            {
                Sla = TimeSpan.FromMinutes(30),
                OldestPendingJobAge = TimeSpan.FromMinutes(61)
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Red));
        }

        [Test]
        public void Critical_failure_should_make_result_red()
        {
            var metrics = HealthyMetrics() with
            {
                CriticalFailure = true
            };

            var result = _evaluator.Evaluate(metrics);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Red));
            Assert.That(result.Reasons, Does.Contain("A critical failure was detected."));
        }
    }
}
