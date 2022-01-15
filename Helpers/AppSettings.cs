namespace WebApi.Helpers;

public class AppSettings
{
    public string Secret { get; set; }
    public EmailSettings EmailSettings { get; set; }
    
}

public class EmailSettings
{
    public string Host { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool SmtpAuthRequired { get; set; }
    public int Port { get; set; }
    public string SmtpFrom { get; set; }
    public string SecureSocketOptions { get; set; }
}