using Ryujinx.HLE.HOS.Services.Arp;

namespace Ryujinx.HLE.HOS.Services.Bcat.ServiceCreator
{
    internal class IDeliveryCacheDirectoryService : IpcService
    {
        private ApplicationLaunchProperty applicationLaunchProperty;

        public IDeliveryCacheDirectoryService(ApplicationLaunchProperty applicationLaunchProperty)
        {
            this.applicationLaunchProperty = applicationLaunchProperty;
        }



    }
}
