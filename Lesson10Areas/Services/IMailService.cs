namespace Lesson10Areas.Services
{
    public interface IMailService
    {
        bool SendEmailAsync(MailRequest mailRequest);
    }
}
