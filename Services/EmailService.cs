namespace WebApi.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using WebApi.Helpers;
using WebApi.Models.Emails;

public interface IEmailService
{
    MimeMessage Compose(Addressee to,
        string subject,
        string body,
        string from = "",
        TextFormat textFormat = TextFormat.Plain);
    Task SendAsync(MimeMessage message);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<AppSettings> appSettings, ILogger<EmailService> logger)
    {        
        _emailSettings = appSettings.Value.EmailSettings;
        _logger = logger;
    }

    /// <summary>
    /// <para>Composes a new <see cref="MimeMessage"/> for sending thru email.</para>
    /// </summary>
    /// <param name="to">Who the mail is sent to.</param>
    /// <param name="subject">The email subject</param>
    /// <param name="body">The email contents.</param>
    /// <param name="from">
    /// <para>(Overridable)</para>
    /// <para>Who the mail is from.</para>
    /// <para>Defaults to the setting: <c>SmtpFrom</c> in <see cref="EmailSettings"/></para>
    /// </param>
    /// <param name="textFormat">The text format of the email. Must be a valid enum of <see cref="TextFormat"/></param>
    /// <returns>A fully constructed <see cref="MimeMessage"/> ready to transmit.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public MimeMessage Compose(
        Addressee to,
        string subject,
        string body,
        string from = "",
        TextFormat textFormat = TextFormat.Plain)
    {
        try
        {
            // Parameter validation

            if (string.IsNullOrEmpty(body))
                throw new ArgumentNullException(nameof(body));

            if (string.IsNullOrEmpty(from))
                from = _emailSettings.SmtpFrom;

            if (string.IsNullOrEmpty(to.Email))
                throw new ArgumentNullException(nameof(to.Email));

            // Constructing the message
            MimeMessage message = new();
            message.From.Add(new MailboxAddress(null, from));
            message.To.Add(new MailboxAddress(to.Name, to.Email));

            message.Subject = subject ?? "Hello from the User API!";
            message.Body = new TextPart(textFormat)
            {
                Text = body,
            };

            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// <para>Asynchronously sends an email message.</para>
    /// </summary>
    /// <param name="message">A fully constructed <see cref="MimeMessage"/></param>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task SendAsync(MimeMessage message)
    {
        try
        {
            // We shouldn't be sending empty messages!
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // Instantiating a new SmtpClient instance
            using SmtpClient client = new();

            // Getting Secure Socket Options from App Settings
            SecureSocketOptions secureSocketOptions = ParseTlsSettings(_emailSettings.SecureSocketOptions);

            // Connecting to SMTP Host...
            await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, secureSocketOptions);

            // If SMTP Host requires Authentication, do so here.
            // This is configurable in App Settings.
            if (_emailSettings.SmtpAuthRequired)
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            
            // Send the message!
            await client.SendAsync(message);

            // We're done, so disconnect from the SMTP Host.
            // `true` means a `QUIT` command will be issued to the SMTP host.
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// <para>Parses the <c>SecureSocketOptions</c> setting in <see cref="EmailSettings"/></para>
    /// </summary>
    /// <param name="tlsSettings">The string for the <c>SecureSocketOptions</c> setting in <see cref="EmailSettings"/></param>
    /// <returns>
    /// <para>A <see cref="SecureSocketOptions"/> value corresponding to the passed <paramref name="tlsSettings"/></para>
    /// <para>Or <see cref="SecureSocketOptions.Auto"/> otherwise.</para>
    /// </returns>
    private static SecureSocketOptions ParseTlsSettings(string tlsSettings)
    {
        // If `tlsSettings` is either empty, malformed, or null,
        // then return `SecureSocketOptions.Auto`.
        if (string.IsNullOrEmpty(tlsSettings) || string.IsNullOrWhiteSpace(tlsSettings))
            return SecureSocketOptions.Auto;

        // Use pattern matching to get the correct value.
        // `tlsSettings` is converted to lowercase for standarized matching.
        //
        // If no matches are found, then the default is `SecureSocketOptions.Auto'.
        SecureSocketOptions result = tlsSettings.ToLower() switch
        {
            "none" => SecureSocketOptions.None,
            "auto" => SecureSocketOptions.Auto,
            "sslonconnect" => SecureSocketOptions.SslOnConnect,
            "starttls" => SecureSocketOptions.StartTls,
            "starttlswhenavailable" => SecureSocketOptions.StartTlsWhenAvailable,
            _ => SecureSocketOptions.Auto,
        };

        return result;
    }

}