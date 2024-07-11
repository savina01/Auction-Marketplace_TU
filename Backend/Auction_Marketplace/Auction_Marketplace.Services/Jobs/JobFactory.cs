using Quartz.Spi;
using Quartz;

namespace Auction_Marketplace.Services.Jobs
{
    public class JobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public JobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                var jobDetail = bundle.JobDetail;
                var jobType = jobDetail.JobType;
                var job = (IJob)_serviceProvider.GetService(jobType);

                if (job == null)
                {
                    throw new InvalidOperationException($"Could not resolve job of type {jobType.FullName}");
                }

                return job;
            }
            catch (Exception ex)
            {
                throw new SchedulerException($"Problem instantiating class '{bundle.JobDetail.JobType.FullName}'", ex);
            }
        }

        public void ReturnJob(IJob job) { }
    }
}
