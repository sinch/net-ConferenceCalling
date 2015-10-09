using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ConferenceCalling.Models;
using Newtonsoft.Json;
using Sinch.ServerSdk;
using Sinch.ServerSdk.Calling;

namespace ConferenceCalling.Controllers {
    public class HomeController : Controller {
        private string appKey;
        private string appSecret;

        public HomeController() {
            appKey = ConfigurationManager.AppSettings["applicationKey"];
            appSecret = ConfigurationManager.AppSettings["applicationSecret"];
        }
        public ActionResult Index() {
            //using (var db = new ConferenceContext())
            //{
            //    var model = db.Conferences.ToList();
            //    return View(model);
            //}
            return View();
        }

        [Route("~/Conference")]
        public ActionResult JoinConference() {
            ViewBag.applicationKey = appKey;

            return View();

        }
        [Route("~/GetTicket")]
        [HttpGet]
        public JsonResult GetTicket(string id) {

            LoginObject loginObject = new LoginObject(appKey, appSecret);
            loginObject.userTicket = loginObject.Signature(id);
            
            return Json(loginObject, JsonRequestBehavior.AllowGet);

        }
    }
}
