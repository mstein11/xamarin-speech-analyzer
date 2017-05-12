using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using Happimeter.Server.Models;
using RazorEngine;
using RazorEngine.Templating;

namespace Happimeter.Server.Services
{
    public class EmailManager
    {
        private SmtpClient Client { get; set; }

        public EmailManager(string host, int port, string userName, string password)
        {
            Client = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = host,
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = true,
                Port = port
            };
        }

        public EmailManager()
        {
            Client = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = "smtp.gmail.com",
                Credentials = new NetworkCredential(ConfigurationManager.AppSettings["Happymeter.EmailUser"], ConfigurationManager.AppSettings["Happymeter.EmailPw"]),
                EnableSsl = true,
                Port = 587
            };
            CompileTemplates();
        }

        public void Send(MailMessage message)
        {
            try
            {
                Client.Send(message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public string GetTemplate(string templateKey, object model)
        {
            return Engine.Razor.Run(templateKey, null, model);
        }

        /// <summary>
        /// Compile all templates here. This method is called on startup, so later on the templates just have to be loaded from cache. 
        /// </summary>
        public static void CompileTemplates()
        {
            var movieFile =
                File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, "Templates",
                    "MovieEmail.cshtml"));
            Engine.Razor.Compile(movieFile, MovieEmailViewModel.TemplateKey);
        }

    }
}
