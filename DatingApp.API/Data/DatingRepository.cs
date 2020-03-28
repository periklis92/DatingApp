using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext context;
        public DatingRepository(DataContext _context)
        {
            context = _context;

        }

        public void Add<T>(T entity) where T : class
        {
            context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            context.Remove(entity);
        }

        public async Task<Like> GetLike(int userID, int recipientID)
        {
            return await context.Likes.FirstOrDefaultAsync(u => 
                u.LikerID == userID && u.LikeeID == recipientID);
        }

        public async Task<Photo> GetMainPhoto(int userID)
        {
            return await context.Photos.Where(u => u.UserID == userID)
                                        .FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await context.Photos.FirstOrDefaultAsync(p => p.ID == id);
            
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.ID == id);

            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = context.Users.AsQueryable();

            users = users.Where(u => u.ID != userParams.UserID && u.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserID, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.ID));
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserID, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.ID));
            }
            
            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);

                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }
            
            switch (userParams.OrderBy)
            {
                case "created":
                    users = users.OrderByDescending(u => u.Created);
                    break;
                
                default:
                    users = users.OrderByDescending(u => u.LastActive);
                    break;
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.ID == id);

            if (likers)
                return user.Likers.Where(u => u.LikeeID == id).Select(i => i.LikerID);
            else
                return user.Likees.Where(u => u.LikerID == id).Select(i => i.LikeeID);
        }

        public async Task<bool> SaveAll()
        {
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await context.Messages.FirstOrDefaultAsync(m => m.ID == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = context.Messages.AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(u => u.RecipientID == messageParams.UserID
                        && !u.RecipientDeleted);
                    break;
                case "Outbox":
                    messages = messages.Where(u => u.SenderID == messageParams.UserID 
                        && !u.SenderDeleted);
                    break;
                default:
                    messages = messages.Where(u => u.RecipientID == messageParams.UserID 
                        && !u.RecipientDeleted && u.IsRead == false);
                    break;
            }

            messages = messages.OrderByDescending(d => d.DateSent);

            return await PagedList<Message>.CreateAsync(messages, 
                messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userID, int recipientID)
        {
            var messages = await context.Messages
                .Where(m => m.RecipientID == userID && !m.RecipientDeleted && m.SenderID == recipientID ||
                    m.RecipientID == recipientID && !m.SenderDeleted && m.SenderID == userID)
                .OrderByDescending(m => m.DateSent)
                .ToListAsync();

            return messages;
        }
    }
}