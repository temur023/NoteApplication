using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;

namespace NoteApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmailController:ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SendEmail(string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse("brianne60@ethereal.email"));
        email.To.Add(MailboxAddress.Parse("brianne60@ethereal.email"));
        email.Subject = "Test Email Subject";
        email.Body = new TextPart(TextFormat.Html) { Text = body };

        using var smtp = new SmtpClient();

        // IMPORTANT: only for Ethereal / local dev
        smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

        await smtp.ConnectAsync("smtp.ethereal.email", 587, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync("brianne60@ethereal.email", "MVb6tUkr6tkB74HxT7");
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);

        return Ok();
    }

}