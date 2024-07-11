using Auction_Marketplace.Services.Jobs;
using Microsoft.Extensions.Hosting;
using Quartz.Spi;
using Quartz;

namespace Auction_Marketplace.Services.Implementation
{
    public class QuartzHostedService : IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        private readonly IEnumerable<JobSchedule> _jobSchedules;

        public QuartzHostedService(ISchedulerFactory schedulerFactory, IJobFactory jobFactory, IEnumerable<JobSchedule> jobSchedules)
        {
            _schedulerFactory = schedulerFactory;
            _jobFactory = jobFactory;
            _jobSchedules = jobSchedules;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            scheduler.JobFactory = _jobFactory;

            foreach (var jobSchedule in _jobSchedules)
            {
                var job = CreateJob(jobSchedule.JobType);
                var trigger = CreateTrigger(jobSchedule);

                await scheduler.ScheduleJob(job, trigger, cancellationToken);
            }

            await scheduler.Start(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static IJobDetail CreateJob(Type jobType) =>
            JobBuilder
                .Create(jobType)
                .WithIdentity(jobType.FullName)
                .Build();

        private static ITrigger CreateTrigger(JobSchedule schedule) =>
            TriggerBuilder
                .Create()
                .WithIdentity($"{schedule.JobType.FullName}.trigger")
                .WithCronSchedule(schedule.CronExpression)
                .Build();
    }
}
