using MroczwareFramework.Models.Template;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace MroczwareFramework.Services.EmailService
{
    internal partial class EmailService<TDbContext>
    {
        public async Task SendEmail(
            IEnumerable<string> to,
             string templateString,
             string? from = null,
             string cultureStrongName = "PL",
             bool bodyAsHtml = true,
             Dictionary<string, string>? bodyParams = null,
             Dictionary<string, string>? subjectParams = null,
             IEnumerable<Attachment>? attachmets = null,
             IEnumerable<string>? cc = null,
             IEnumerable<string>? bcc = null,
             IEnumerable<string>? replyTo = null,
             MailPriority emailPriority = MailPriority.Normal
             )
        {
            TemplateModel templateMailAddress = await _templateService.Get(templateString, cultureStrongName)
                ?? throw new Exception("Nie znaleziono szablonu");

            if (templateMailAddress.Body is null || templateMailAddress.Subject is null) throw new Exception("Subject or body not provided");

            if (bodyParams is not null)
                foreach (KeyValuePair<string, string> parameter in bodyParams)
                    templateMailAddress.Body = templateMailAddress.Body.Replace("{{" + parameter.Key + "}}", parameter.Value);

            if (subjectParams is not null)
                foreach (KeyValuePair<string, string> parameter in subjectParams)
                    templateMailAddress.Subject = templateMailAddress.Subject.Replace("{{" + parameter.Key + "}}", parameter.Value);

            using SmtpClient smtpClient = new();

            try
            {
                smtpClient.Host = _configuration["Email:Server"];
                smtpClient.Port = int.Parse(_configuration["Email:Port"]);
                smtpClient.Timeout = 12000;
                smtpClient.EnableSsl = bool.Parse(_configuration["Enable:SSL"]);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(
                    _configuration["Email:User"],
                    _configuration["Email:Password"]
                );

                // Tutaj możesz wysłać e-mail
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Błąd parsowania konfiguracji: {ex.Message}");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"Błąd SMTP: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Nieoczekiwany błąd: {ex.Message}");
            }


            using MailMessage message = new()
            {
                SubjectEncoding = Encoding.UTF8,
                IsBodyHtml = bodyAsHtml,
                BodyEncoding = Encoding.UTF8,
                Priority = emailPriority,
                Subject = templateMailAddress.Subject,
                Body = templateMailAddress.Body,
                From = new MailAddress(from),
            };

            foreach (string receiver in to) message.To.Add(new MailAddress(receiver));
            if (cc is not null) foreach (string item in cc) message.CC.Add(new MailAddress(item));
            if (bcc is not null) foreach (string item in bcc) message.Bcc.Add(new MailAddress(item));
            if (replyTo is not null) foreach (string item in replyTo) message.ReplyToList.Add(new MailAddress(item));

            //DODANIE ZAŁĄCZNIKÓW
            if (attachmets is not null)
                foreach (Attachment attachmet in attachmets)
                    message.Attachments.Add(new Attachment(attachmet.ContentStream, attachmet.Name));

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.DefaultConnectionLimit = 100;
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; };
                smtpClient.Send(message);
            }
            catch (SmtpFailedRecipientsException ex)
            {
                foreach (SmtpFailedRecipientException innerEx in ex.InnerExceptions)
                {
                    SmtpStatusCode status = innerEx.StatusCode;
                    if (status is SmtpStatusCode.MailboxBusy or SmtpStatusCode.MailboxUnavailable)
                    {
                        Thread.Sleep(5000);
                        smtpClient.Send(message);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to deliver message to {innerEx.FailedRecipient}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mail sending error: {ex.Message}");
            }
        }

        public async Task SendEmail(
             string to,
             string templateString,
             string? from = null,
             string cultureStrongName = "PL",
             bool bodyAsHtml = true,
             Dictionary<string, string>? bodyParams = null,
             Dictionary<string, string>? subjectParams = null,
             IEnumerable<Attachment>? attachmets = null,
             IEnumerable<string>? cc = null,
             IEnumerable<string>? bcc = null,
             IEnumerable<string>? replyTo = null,
             MailPriority emailPriority = MailPriority.Normal
             )
        {
            TemplateModel templateMailAddress = await _templateService.Get(templateString, cultureStrongName)
                ?? throw new Exception("Nie znaleziono szablonu");

            if (templateMailAddress.Body is null || templateMailAddress.Subject is null) throw new Exception("Subject or body not provided");

            // Kopie szablonu do edycji
            string bodyToSend = templateMailAddress.Body;
            string subjectToSend = templateMailAddress.Subject;

            // Podstawienie parametrów
            if (bodyParams is not null)
                foreach (var parameter in bodyParams)
                    bodyToSend = bodyToSend.Replace("{{" + parameter.Key + "}}", parameter.Value);

            if (subjectParams is not null)
                foreach (var parameter in subjectParams)
                    subjectToSend = subjectToSend.Replace("{{" + parameter.Key + "}}", parameter.Value);
            using SmtpClient smtpClient = new();

            try
            {
                smtpClient.Host = _configuration["Email:Server"];
                smtpClient.Port = int.Parse(_configuration["Email:Port"]);
                smtpClient.Timeout = 12000;
                smtpClient.EnableSsl = bool.Parse(_configuration["Email:SSL"]);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(
                    _configuration["Email:User"],
                    _configuration["Email:Password"]
                );

                // Tutaj możesz wysłać e-mail
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Błąd parsowania konfiguracji: {ex.Message}");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"Błąd SMTP: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Nieoczekiwany błąd: {ex.Message}");
            }


            using MailMessage message = new()
            {
                SubjectEncoding = Encoding.UTF8,
                IsBodyHtml = bodyAsHtml,
                BodyEncoding = Encoding.UTF8,
                Priority = emailPriority,
                Subject = subjectToSend,
                Body = bodyToSend,
                From = new MailAddress(from),
            };

            message.To.Add(new MailAddress(to));
            if (cc is not null) foreach (string item in cc) message.CC.Add(new MailAddress(item));
            if (bcc is not null) foreach (string item in bcc) message.Bcc.Add(new MailAddress(item));
            if (replyTo is not null) foreach (string item in replyTo) message.ReplyToList.Add(new MailAddress(item));

            //DODANIE ZAŁĄCZNIKÓW
            if (attachmets is not null)
                foreach (Attachment attachmet in attachmets)
                    message.Attachments.Add(new Attachment(attachmet.ContentStream, attachmet.Name));

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.DefaultConnectionLimit = 100;
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; };
                smtpClient.Send(message);
            }
            catch (SmtpFailedRecipientsException ex)
            {
                foreach (SmtpFailedRecipientException innerEx in ex.InnerExceptions)
                {
                    SmtpStatusCode status = innerEx.StatusCode;
                    if (status is SmtpStatusCode.MailboxBusy or SmtpStatusCode.MailboxUnavailable)
                    {
                        Thread.Sleep(5000);
                        smtpClient.Send(message);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to deliver message to {innerEx.FailedRecipient}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mail sending error: {ex.Message}");
            }
        }
    }
}
