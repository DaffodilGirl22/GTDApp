using GTDApp.Data;

namespace GTDApp.Services
{
    public class ServiceContext : IServiceContext
    {
        public ServiceContext(DatabaseContext context)
        {
            InboxServ = new InboxService(context);
        }

        public IInboxService InboxServ { get; set; }
    }

}