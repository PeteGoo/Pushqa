using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using Pushqa.Server.SignalR;
using SignalR.Hosting.AspNet.Routing;
using SignalR.Transports;

namespace Sample.Web {
    public class Global : System.Web.HttpApplication {

        protected void Application_Start(object sender, EventArgs e) {
            // Determines how long to wait between long polling connections to consider a connection "dead"
            TransportHeartBeat.Instance.DisconnectTimeout = TimeSpan.FromSeconds(10);

            // Determines how often to check connection status.
            TransportHeartBeat.Instance.HeartBeatInterval = TimeSpan.FromSeconds(10);
            RouteTable.Routes.MapConnection<QueryablePushService<MyPushContext>>("events", "events/{*operation}");
        }

        protected void Session_Start(object sender, EventArgs e) {

        }

        protected void Application_BeginRequest(object sender, EventArgs e) {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e) {

        }

        protected void Application_Error(object sender, EventArgs e) {

        }

        protected void Session_End(object sender, EventArgs e) {

        }

        protected void Application_End(object sender, EventArgs e) {

        }
    }
}