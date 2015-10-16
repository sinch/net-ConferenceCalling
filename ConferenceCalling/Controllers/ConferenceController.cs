using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Castle.Components.DictionaryAdapter;
using ConferenceCalling.Models;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;

namespace ConferenceCalling.Controllers {

    public class ConferenceController : Controller {
        private string appKey;
        private string appSecret;

        public ConferenceController() {
            appKey = ConfigurationManager.AppSettings["applicationKey"];
            appSecret = ConfigurationManager.AppSettings["applicationSecret"];
        }
        [Route("~/Conference")]
        public ActionResult JoinConference(string id) {
            ViewBag.applicationKey = appKey;
            if (string.IsNullOrEmpty(id)) {
                ViewBag.id = id;
            }
            return View();
        }

        [Authorize]
        [Route("~/Conference/Create")]
        [HttpGet]
        public async Task<ActionResult> Create() {
            var model = new CreateConferenceModel();
            model.Participants = new EditableList<ConferenceAtendee>();
            model.Conference = new Conference();
            model.Conference.ConferenceDate = DateTime.Today.AddDays(1);
            model.Conference.ConferenceTime = DateTime.Now.TimeOfDay;
            model.Conference.OwnerId = User.Identity.Name;

            string code = "";
            using (var db = new ConferenceContext()) {
                Random rng = new Random();
                int value = rng.Next(100, 9999); //1
                code = value.ToString("0000");
                while (db.Conferences.Any(m => m.PinCode == code && m.ConferenceDate == DateTime.Today)) {
                    value = rng.Next(100, 9999); //1
                    code = value.ToString("0000");
                }

            }
            model.Conference.PinCode = code;
            for (int i = 0; i < 9; i++) {
                model.Participants.Add(new ConferenceAtendee());
            }
            return View(model);
        }
        [Authorize]
        [Route("~/Conference/Create")]
        [HttpPost]
        public async Task<ActionResult> Create(CreateConferenceModel model) {
            using (var db = new ConferenceContext()) {
                // lets add a new guid to the model to ensure that all conferences are uniq
                model.Conference.ConferenceId = Guid.NewGuid();
                var dt = new DateTime(model.Conference.ConferenceTime.Ticks);
                var utcdate = model.Conference.ConferenceDate.AddTicks(model.Conference.ConferenceTime.Ticks).ToUniversalTime();
                model.Conference.ConferenceTime = new TimeSpan(utcdate.TimeOfDay.Ticks);
                model.Conference.ConferenceDate = utcdate.Date;
                model.Conference.OwnerId = User.Identity.Name;
                // loop thru all participants and add by id
                List<ConferenceAtendee> cp = new List<ConferenceAtendee>();
                foreach (var a in model.Participants.Where(m => m.Name != "")) {
                    if (!db.ConferenceAtendees.Any(m => m.Email == a.Email || m.Phone == a.Phone)) {
                        db.ConferenceAtendees.Add(a);
                        await db.SaveChangesAsync();
                        cp.Add(a);
                    } else {
                        cp.Add(db.ConferenceAtendees.FirstOrDefault(m => m.Email == a.Email || m.Phone == a.Phone));
                    }
                }
                model.Conference.Attendees = cp;
                db.Conferences.Add(model.Conference);
                await db.SaveChangesAsync();
            }
            //return tohome controller
            return View("Confirmed", model);
        }

        [Authorize]
        [Route("~/Conference/My")]
        public ActionResult MyConferences() {
            using (var db = new ConferenceContext())
            {
               var conferences= db.Conferences.Where(m => m.OwnerId == User.Identity.Name).ToList();
                return View(conferences);
            }
        }
        

        [Route("~/Conference/calendar/{id}")]
        public FileResult Calendar(string id) {
            using (var db = new ConferenceContext())
            {
                var iCal = new iCalendar {
                    Method = "PUBLISH",
                    Version = "2.0"
                };
                var confguid = new Guid(id);
                var conference = db.Conferences.FirstOrDefault(m => m.ConferenceId == confguid);
                var utcdate = conference.ConferenceDate.AddTicks(conference.ConferenceTime.Ticks);
                var evt = iCal.Create<Event>();
                
                evt.Start = new iCalDateTime(utcdate);
                evt.Duration = TimeSpan.FromHours(1);
                evt.End = new iCalDateTime(utcdate.AddHours(1));
                evt.Location = "http://demo.sinch.com/conference/" + conference.ConferenceId;
                evt.Description = "Conference pin: " + conference.PinCode;
                evt.Organizer = new Organizer(conference.OwnerId);
                evt.Summary = conference.ConferenceName;
                evt.UID = conference.ConferenceId.ToString();
                evt.Alarms.Add(new Alarm {
                    Duration = new TimeSpan(0, 15, 0),
                    Trigger = new Trigger(new TimeSpan(0, 15, 0)),
                    Action = AlarmAction.Display,
                    Description = "Reminder"
                });
                string output = new iCalendarSerializer().SerializeToString(iCal);
                new iCalendarSerializer().Serialize(iCal, "c:\\temp\\test.ics");
                var bytes = Encoding.UTF8.GetBytes(output);
                return File(bytes, "text/calendar", conference.ConferenceName + ".ics");
            }
        }

        [Route("~/GetTicket")]
        [HttpGet]
        public JsonResult GetTicket(string id) {
            var loginObject = new LoginObject(appKey, appSecret);
            loginObject.userTicket = loginObject.Signature(id);

            return Json(loginObject, JsonRequestBehavior.AllowGet);
        }


    }
}