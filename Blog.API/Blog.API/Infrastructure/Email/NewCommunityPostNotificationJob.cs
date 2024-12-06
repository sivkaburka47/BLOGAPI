using System.Linq;
using Blog.API.Data;
using Blog.API.Models.DB;
using Blog.API.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Blog.API.Infrastucture.Email
{
    public class NewCommunityPostNotificationJob : IJob
    {
        private readonly BlogDbContext _context;
        private readonly IEmailSender _emailSender;

        public NewCommunityPostNotificationJob(BlogDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var lastTriggerTime = context.PreviousFireTimeUtc?.UtcDateTime ?? DateTime.UtcNow.AddMinutes(-5);
            var currentTriggerTime = context.FireTimeUtc.UtcDateTime;

            var newPosts = await _context.Posts
                .Where(p => p.createTime >= lastTriggerTime && p.createTime < currentTriggerTime)
                .Include(p => p.community)
                .ToListAsync();

            foreach (var post in newPosts)
            {
                if (post.communityId.HasValue)
                {
                    var subscribers = await _context.CommunityUsers
                        .Where(cu =>
                            cu.communityId == post.communityId.Value &&
                            cu.communityRoles.Contains(CommunityRole.Subscriber))
                        .Include(cu => cu.user)
                        .Select(cu => cu.user)
                        .ToListAsync();

                    foreach (var subscriber in subscribers)
                    {
                        try
                        {
                            var subject = $"Новый пост в сообществе {post.community?.name}";
                            var body = $"Здравствуйте, {subscriber.fullName}!\n\nВ сообществе {post.community?.name} вышел новый пост: {post.title}.\n\nС уважением,\nВаш BLOGAPI.";

                            await _emailSender.SendEmailAsync(subscriber.email, subject, body);
                        }
                        catch (Exception ex)
                        {
                            var isPermanentFailure = ex.Message.Contains("Recipient address rejected") || 
                                                     ex.Message.Contains("550 5.1.1") || 
                                                     ex.Message.Contains("non-local recipient verification failed") ||
                                                     ex.Message.Contains("Mailbox unavailable");
                            
                            var notification = new Notification
                            {
                                postId = post.id,
                                subscriberEmail = subscriber.email,
                                createdAt = DateTime.UtcNow,
                                errorMessage = ex.Message,
                                isPermanentFailure = isPermanentFailure
                            };

                            _context.Notifications.Add(notification);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
        }
    }
}