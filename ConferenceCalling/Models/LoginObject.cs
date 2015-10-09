using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConferenceCalling.Models {
    

    public class LoginObject {
        private readonly string _key;
        private readonly string _secret;

        public LoginObject(string key, string secret) {
            _key = key;
            _secret = secret;
        }

        [JsonProperty("userTicket")]
        public string userTicket { get; set; }
        public string Signature(string userId) {
            UserTicket userTicket = new UserTicket();
            userTicket.Identity = new Identity { Type = "username", Endpoint = userId };
            userTicket.ApplicationKey = _key;
            userTicket.Created = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
            userTicket.ExpiresIn = 3600;
            Debug.WriteLine(DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            var json = JsonConvert.SerializeObject(userTicket).Replace(" ", "");
            var ticketData = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
            var sha256 = new HMACSHA256(Convert.FromBase64String(_secret));
            var signature = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(json)));
            Debug.WriteLine(json);
            return ticketData + ":" + signature;
        }
    }

    public class UserTicket {
        [JsonProperty("identity")]
        public Identity Identity { get; set; }
        [JsonProperty("applicationkey")]
        public string ApplicationKey { get; set; }
        [JsonProperty("created")]
        public string Created { get; set; }
        [JsonProperty("expiresIn")]
        public int ExpiresIn { get; set; }
        
    }

    public class Identity {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
    }
  
}
