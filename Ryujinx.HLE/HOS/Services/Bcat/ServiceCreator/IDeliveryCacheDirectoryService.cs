using System;
using System.Collections.Generic;
using System.Text;
using Ryujinx.HLE.HOS.Services.Arp;

namespace Ryujinx.HLE.HOS.Services.Bcat.ServiceCreator
{
    class IDeliveryCacheDirectoryService : IpcService
    {
    
      public IDeliveryCacheDirectoryService(ServiceCtx context, ApplicationLaunchProperty applicationLaunchProperty)
      {
      
      }
      [Command(0)]
      public ResultCode Open(serviceCtx Context)
      {
            return ResultCode.Success;
       }
       
       [Command(1)]
       public ResultCode Open(serviceCtx Context)
      {
            return ResultCode.Success;
       }
      
      [Command(2)]
       public ResultCode Open(serviceCtx Context)
      {
            return ResultCode.Success;
       }
