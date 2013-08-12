using Microsoft.AspNet.SignalR;
using Owin;
using Pushqa.Server.SignalR;
using Sample.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.SelfHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapConnection<QueryablePushService<MyPushContext>>("/events", new ConnectionConfiguration { EnableCrossDomain = true });
        }
    }
}
