using System.Net.Mail;

namespace MroczwareFramework.Services.EmailService
{
    public interface IEmailService
    {
        Task SendEmail(IEnumerable<string> to, string templateString, string? from = null, string cultureStrongName = "PL", bool bodyAsHtml = true, Dictionary<string, string>? bodyParams = null, Dictionary<string, string>? subjectParams = null, IEnumerable<Attachment>? attachmets = null, IEnumerable<string>? cc = null, IEnumerable<string>? bcc = null, IEnumerable<string>? replyTo = null, MailPriority emailPriority = MailPriority.Normal);
        Task SendEmail(string to, string templateString, string? from = null, string cultureStrongName = "PL", bool bodyAsHtml = true, Dictionary<string, string>? bodyParams = null, Dictionary<string, string>? subjectParams = null, IEnumerable<Attachment>? attachmets = null, IEnumerable<string>? cc = null, IEnumerable<string>? bcc = null, IEnumerable<string>? replyTo = null, MailPriority emailPriority = MailPriority.Normal);
    }
}
