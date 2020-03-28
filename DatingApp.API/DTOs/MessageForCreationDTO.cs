using System;

namespace DatingApp.API.DTOs
{
    public class MessageForCreationDTO
    {
        public int SenderID { get; set; }
        public int RecipientID { get; set; }
        public DateTime DateSent { get; set; }
        public string Content { get; set; }
        public MessageForCreationDTO()
        {
            DateSent = DateTime.Now;
        }
    }
}