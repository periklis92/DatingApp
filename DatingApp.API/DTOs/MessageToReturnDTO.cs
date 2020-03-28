using System;

namespace DatingApp.API.DTOs
{
    public class MessageToReturnDTO
    {
        public int ID { get; set; }
        public int SenderID { get; set; }
        public string SenderKnownAs { get; set; }
        public string SenderPhotoUrl { get; set; }
        public int ReciepientID { get; set; }
        public string ReciepientPhotoUrl { get; set; }
        public string RecipientKnownAs { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime DateSent { get; set; }
    }
}