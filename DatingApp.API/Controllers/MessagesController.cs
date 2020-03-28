using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/user/{userID}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository repo;
        private readonly IMapper mapper;

        public MessagesController(IDatingRepository _repo, IMapper _mapper)
        {
            mapper = _mapper;
            repo = _repo;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userID, int id)
        {
            if (userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await repo.GetMessage(id);

            if (messageFromRepo == null)
                return NotFound();

            return Ok(messageFromRepo);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userID, [FromQuery]MessageParams messageParams)
        {
            if (userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageParams.UserID = userID;

            var messagesFromRepo = await repo.GetMessagesForUser(messageParams);

            var messages = mapper.Map<IEnumerable<MessageToReturnDTO>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize, 
            messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);

            return Ok(messages);
        }

        [HttpGet("thread/{recipientID}")]
        public async Task<IActionResult> GetMessageThread(int userID, int recipientID)
        {
            if (userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messagesFromRepo = await repo.GetMessageThread(userID, recipientID);

            var messageThread = mapper.Map<IEnumerable<MessageToReturnDTO>>(messagesFromRepo);

            return Ok(messageThread);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userID, MessageForCreationDTO messageForCreationDTO)
        {
            var user = await repo.GetUser(userID);

            if (userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageForCreationDTO.SenderID = userID;

            var recipient = await repo.GetUser(messageForCreationDTO.RecipientID);

            if (recipient == null)
                return BadRequest("Couldn't find user!");

            var message = mapper.Map<Message>(messageForCreationDTO);

            repo.Add(message);

            if (await repo.SaveAll()){
                var messageToReturn = mapper.Map<MessageToReturnDTO>(message);
                return CreatedAtRoute("GetMessage", 
                    new {userID, id = message.ID}, messageToReturn);
            }

            throw new Exception("Creating the message failed on save!");
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userID)
        {
            if (userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await repo.GetMessage(id);

            if (messageFromRepo.SenderID == userID)
                messageFromRepo.SenderDeleted = true;
            else if (messageFromRepo.RecipientID == userID)
                messageFromRepo.RecipientDeleted = true;

            if (messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
                repo.Delete(messageFromRepo);

            if (await repo.SaveAll())
                return NoContent();

            throw new Exception("Error deleting the message");
        } 

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userID, int id)
        {
            if (userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var message = await repo.GetMessage(id);

            if (message.RecipientID != userID)
                return Unauthorized();

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await repo.SaveAll();

            return NoContent();
        }
    }
}