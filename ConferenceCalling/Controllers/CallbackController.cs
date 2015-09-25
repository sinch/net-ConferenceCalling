using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ConferenceCalling.Models;
using Sinch.ServerSdk;
using Sinch.ServerSdk.Calling.Callbacks.Request;
using Sinch.ServerSdk.Calling.Callbacks.Response;
using Sinch.ServerSdk.Calling.Models;
using Sinch.ServerSdk.Models;

namespace ConferenceCalling.Controllers {
    public class CallbackController : ApiController {
        [HttpPost]
        public async Task<SvamletModel> Post(CallbackEventModel model) {
            var sinch = SinchFactory.CreateCallbackResponseFactory(Locale.EnUs);
            var reader = sinch.CreateEventReader();
            var evt = reader.ReadModel(model);
            var builder = sinch.CreateIceSvamletBuilder();
            switch (evt.Event) {
                case Event.IncomingCall:
                    builder.AddNumberInputMenu("menu1", "Enter 4 digits", 4, "Enter 4 digits", 3, TimeSpan.FromSeconds(60));
                    builder.RunMenu("menu1");
                    break;
                case Event.PromptInput:
                    using (var db = new ConferenceContext()) {

                        var conference = db.Conferences.FirstOrDefault(c => c.PinCode == model.MenuResult.Value);
                        if (conference != null)
                        {
                            builder.ConnectConference(conference.ConferenceId.ToString());
                        } else {
                            builder.Say("Invalid code").Hangup(HangupCause.Normal);
                        }
                    }

                    break;
                case Event.AnsweredCall:
                    builder.Continue();
                    break;
                default:
                    break;
            }
            return builder.Build().Model;
        }
    }
}
