using System.Configuration;
using System.Web.Mvc;
using ConferenceCalling.Models;

namespace ConferenceCalling.Controllers {
    public class HomeController : Controller {
        private readonly string appKey;
        private readonly string appSecret;

      

        public ActionResult Index() {
            //using (var db = new ConferenceContext())
            //{
            //    var model = db.Conferences.ToList();
            //    return View(model);
            //}
            return View();
        }

      
    }
}