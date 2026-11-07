using System.Net;
using System.Net.Mail;

public static class MailSender
{
    public static void Mail(string subject, string body, string UserEmail)
    {
        using var smtp = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential("eeemo8021@gmail.com", "kjkxsfrwhqavjiru"),
            EnableSsl = true
        };

        var message = new MailMessage("eeemo8021@gmail.com", UserEmail, subject, body)
        {
            IsBodyHtml = true
        };
        smtp.Send(message);
    }
}