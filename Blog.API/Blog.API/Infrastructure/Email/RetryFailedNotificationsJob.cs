using Blog.API.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Blog.API.Infrastucture.Email
{
    public class RetryFailedNotificationsJob : IJob
    {
        private readonly BlogDbContext _context;
        private readonly IEmailSender _emailSender;

        public RetryFailedNotificationsJob(BlogDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var failedNotifications = await _context.Notifications
                .Where(n => !n.isPermanentFailure)
                .ToListAsync();

            foreach (var notification in failedNotifications)
            {
                try
                {
                    var post = await _context.Posts
                        .Include(p => p.community)
                        .FirstOrDefaultAsync(p => p.id == notification.postId);

                    if (post == null)
                    {
                        _context.Notifications.Remove(notification);
                        continue;
                    }
                    
                    var subject = $"Новый пост в сообществе {post.community?.name}";
                    var body = $"Здравствуйте!\n\nВ сообществе {post.community?.name} вышел новый пост: {post.title}.\n\nС уважением,\nВаш BLOGAPI.";
                    
                    await _emailSender.SendEmailAsync(notification.subscriberEmail, subject, body);
                    
                    _context.Notifications.Remove(notification);
                }
                catch (Exception ex)
                {
                    var isPermanentFailure = ex.Message.Contains("Recipient address rejected") || 
                                             ex.Message.Contains("550 5.1.1") || 
                                             ex.Message.Contains("non-local recipient verification failed") ||
                                             ex.Message.Contains("Mailbox unavailable");
                    
                    notification.errorMessage = ex.Message;
                    notification.isPermanentFailure = isPermanentFailure;
                }

                await _context.SaveChangesAsync();
            }
        }
    }

}

