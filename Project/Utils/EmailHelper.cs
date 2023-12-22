using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
namespace Project.Utils;

public class EmailHelper
{
    public async Task SendEmailAsync(MailRequestViewModel mailRequestViewModel)
    {
        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(Constants.mail); // Hansı mail tərəfindən göndəriləcək

        email.To.Add(MailboxAddress.Parse(mailRequestViewModel.ToEmail)); //  Mail Kimə göndəriləcək

        email.Subject = mailRequestViewModel.Subject; // Subject nədir onu daxil edirik

        var builder = new BodyBuilder(); // İnstance almalıyıq

        builder.HtmlBody = mailRequestViewModel.Body;
         
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        smtp.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true; // Bypass certificate validation


        await smtp.ConnectAsync(Constants.host, Constants.port, SecureSocketOptions.StartTls); // smtp`ə qoşulduq

        await smtp.AuthenticateAsync(Constants.mail, Constants.password); // Mail`ə login oluruq

        await smtp.SendAsync(email); // mailə mesajı göndəririk

        smtp.Disconnect(true); // smtp`ə disconnect etdik 
    }
}