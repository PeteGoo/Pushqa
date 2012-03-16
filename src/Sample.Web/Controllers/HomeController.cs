using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sample.Web.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Index/

        public ActionResult Index() {
            
            return View();
        }

        public ActionResult IntervalWithSkipAndTake() {
            return View();
        }

        public ActionResult ProcessInfo() {
            return View();
        }

        public ActionResult StockTicker() {
            return View();
        }
    }
}
