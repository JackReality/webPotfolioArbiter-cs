using DotNetEnv;
using MailKit.Net.Smtp;
using MimeKit;

namespace Portfolio.Services;

/// <summary>
/// Service d'envoi d'emails via SMTP (configuré via les variables d'environnement dans .env).
/// Utilise MailKit, la bibliothèque standard moderne pour l'envoi d'emails en .NET.
/// </summary>
public class EmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;
    private readonly string _fromName;
    private readonly string _fromEmail;

    public EmailService()
    {
        // Lecture des identifiants SMTP depuis le fichier .env
        _smtpHost = Env.GetString("SMTP_HOST", "mail.infomaniak.com");
        _smtpPort = Env.GetInt("SMTP_PORT", 465); // Port SSL par défaut pour Infomaniak
        _smtpUser = Env.GetString("SMTP_USER", "");
        _smtpPassword = Env.GetString("SMTP_PASSWORD", "");
        
        // MAIL_FROM peut être au format "Nom <email@domaine.com>" ou juste "email@domaine.com"
        var mailFrom = Env.GetString("MAIL_FROM", "Portfolio Arbiter <noreply@portfolioarbiter.com>");
        
        // Extraction simple du nom et de l'email si le format "Nom <email>" est utilisé
        if (mailFrom.Contains('<') && mailFrom.Contains('>'))
        {
            _fromName = mailFrom.Split('<')[0].Trim();
            _fromEmail = mailFrom.Split('<')[1].Replace(">", "").Trim();
        }
        else
        {
            _fromName = "Portfolio Arbiter";
            _fromEmail = mailFrom.Trim();
        }
    }

    /// <summary>
    /// Envoie un email à un destinataire donné.
    /// </summary>
    /// <param name="toEmail">Adresse email du destinataire</param>
    /// <param name="subject">Objet de l'email</param>
    /// <param name="htmlBody">Contenu HTML de l'email</param>
    /// <param name="replyTo">
    /// Adresse de réponse facultative : si fournie, un clic sur « Répondre » dans le client
    /// mail visera cette adresse (et non le From). Utile pour le formulaire de contact, où
    /// l'on veut répondre directement au visiteur. Laisser null pour le comportement habituel.
    /// </param>
    public async Task EnvoyerEmailAsync(string toEmail, string subject, string htmlBody, string? replyTo = null)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        if (!string.IsNullOrWhiteSpace(replyTo))
        {
            message.ReplyTo.Add(new MailboxAddress("", replyTo));
        }
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            // Connexion au serveur SMTP (Auto négocie SSL sur 465 ou StartTLS sur 587)
            await client.ConnectAsync(_smtpHost, _smtpPort, MailKit.Security.SecureSocketOptions.Auto);
            
            // Authentification
            if (!string.IsNullOrEmpty(_smtpUser) && !string.IsNullOrEmpty(_smtpPassword))
            {
                await client.AuthenticateAsync(_smtpUser, _smtpPassword);
            }

            // Envoi
            await client.SendAsync(message);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}