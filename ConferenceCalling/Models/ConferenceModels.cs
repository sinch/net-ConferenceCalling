using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using Sinch.ServerSdk.Calling;
using Sinch.ServerSdk.Calling.Models;

namespace ConferenceCalling.Models {
    public class ConferenceDetailsViewModel {
        public Conference Conference { get; set; }
        public IParticipant[] Participants { get; set; }
    }

    public class Conference {
        public int Id { get; set; }
        public string ConferenceName { get; set; }
        [MaxLength(4)]
        public string PinCode { get; set; }
        public Guid ConferenceId { get; set; }
    }
    public class ConferenceContext : DbContext {
        public DbSet<Conference> Conferences { get; set; }
        public ConferenceContext() : base("DefaultConnection") {
        }
    }
}