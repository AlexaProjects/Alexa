/*  
Copyright (C) 2013 Alan Pipitone
    
Al'exa is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Al'exa is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Al'exa.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace AlexaMail
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string ErrorScreenFolder = args[0];
                string smtpHost = args[1];
                int smtpPort = Int32.Parse(args[2]);
                string mailFrom = args[3];
                string[] mailTo = args[4].Split(';');
                string subject = args[5];
                string mailMessage = args[6];
                string user = args[7];
                string password = args[8];

                //init the smtp client
                SmtpClient client = new SmtpClient(smtpHost, smtpPort);

                //create the message object
                MailMessage message = new MailMessage();

                //set the "From" address
                message.From = new MailAddress(mailFrom, "Al'exa");

                foreach (string to in mailTo)
                {
                    message.To.Add(to);
                }

                message.Subject = subject;
                message.Body = mailMessage;

                //get all screenshots
                string[] array1 = Directory.GetFiles(ErrorScreenFolder, "*.jpg");

                foreach (string name in array1)
                {
                    Attachment attach = new Attachment(name);

                    message.Attachments.Add(attach);
                }

                if (user != "none" && password != "none")
                {
                    client.Credentials = new NetworkCredential(user, password);
                }

                client.Send(message);

                message.Dispose();
                client.Dispose();

                foreach (string name in array1)
                {
                    File.Delete(name);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null) Console.WriteLine(ex.InnerException.Message);

            }
            
        }
    }
}
