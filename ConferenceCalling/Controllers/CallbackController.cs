using System;
using System.Data.Entity;
using System.Linq;
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
                    if (model.OriginationType == "MXP") {
                        await ConnectToConference(model.To.Endpoint, "", builder);
                    } else {
                        builder.AddNumberInputMenu("menu1", "Enter 4 digit pin", 4, "Enter 4 digit pin", 3,
                            TimeSpan.FromSeconds(60));
                        builder.RunMenu("menu1");
                    }
                    break;
                case Event.PromptInput:
                    await ConnectToConference(model.MenuResult.Value, model.From, builder);
                    break;
                case Event.AnsweredCall:
                    builder.Continue();
                    break;
                case Event.DisconnectedCall:
                    break;
                default:
                    break;
            }
            return builder.Build().Model;
        }

        private async Task ConnectToConference(string  pinCode, string cli, IIceSvamletBuilder builder) {
            using (var db = new ConferenceContext()) {

                var conference =
                    await db.Conferences.FirstOrDefaultAsync(c => c.PinCode == pinCode && c.ConferenceDate == DateTime.Today);
                if (conference != null) {
                    builder.ConnectConference(conference.ConferenceId.ToString()).WithCli(cli);
                    builder.Say(", Welcome to the conference");
                } else {
                    builder.Say("Invalid code").Hangup(HangupCause.Normal);
                }
            }
        }
    }


}