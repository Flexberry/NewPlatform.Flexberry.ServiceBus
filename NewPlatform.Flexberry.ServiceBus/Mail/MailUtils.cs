namespace NewPlatform.Flexberry.ServiceBus.Mail
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using Components;
    using OpenPop.Mime;
    using OpenPop.Mime.Header;
    using OpenPop.Pop3;

    /// <summary>
    /// The sb service.
    /// </summary>
    internal static class MailUtils
    {
        /// <summary>
        /// The del directory.
        /// </summary>
        /// <param name="tempDirectory">
        /// The temp directory.
        /// </param>
        private static void DelDirectory(string tempDirectory)
        {
            foreach (string dirFileName in Directory.GetFiles(Directory.GetCurrentDirectory() + "\\" + tempDirectory))
            {
                File.Delete(dirFileName);
            }

            Directory.Delete(Directory.GetCurrentDirectory() + "\\" + tempDirectory);
        }

        /// <summary>
        /// The delete stored messages.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="msgs">The msgs.</param>
        private static void DeleteStoredMessages(Pop3Client client, IEnumerable<Header> msgs)
        {
            foreach (Header msg in from header in msgs where header.stored select header)
            {
                client.DeleteMessage(msg.num);
            }
        }

        /// <summary>
        /// The get attachment.
        /// </summary>
        /// <param name="attachment">The attachment.</param>
        /// <param name="password">The password.</param>
        /// <returns>The <see cref="byte[]"/>.</returns>
        private static byte[] GetAttachment(string attachment, string password)
        {
            string zipFileName = "TempArchiveFileName.7z";
            string tempDirectory = "TempDirectory";
            string fileName = "File.txt";
            File.WriteAllBytes(zipFileName, Convert.FromBase64String(attachment));

            string str = string.Format(
                "e -p{0} -o{1} -y {2}",
                password,
                "\"" + Directory.GetCurrentDirectory() + "\\" + tempDirectory + "\"",
                "\"" + Directory.GetCurrentDirectory() + "\\" + zipFileName + "\"");

            var info = new ProcessStartInfo { WindowStyle = ProcessWindowStyle.Hidden, FileName = @"7z\7z.exe", Arguments = str };
            var proc = Process.Start(info);
            proc?.WaitForExit();

            File.Delete(zipFileName);

            byte[] bytes = File.ReadAllBytes(Directory.GetCurrentDirectory() + "\\" + tempDirectory + "\\" + fileName);

            DelDirectory(tempDirectory);

            return bytes;
        }

        /// <summary>
        /// The get attachment from many.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="password">The password.</param>
        /// <returns>The <see cref="byte[]"/>.</returns>
        private static byte[] GetAttachmentFromMany(IEnumerable<Attach> list, string password)
        {
            string zipFileName = "TempArchiveFileName.7z";
            string tempDirectory = "TempDirectory";
            string fileName = "File.txt";

            foreach (Attach attach in list)
            {
                File.WriteAllBytes(zipFileName + GetNum(attach.num), Convert.FromBase64String(attach.str));
            }

            string str = String.Format(
                "e -p{0} -o{1} -y {2}",
                password,
                "\"" + Directory.GetCurrentDirectory() + "\\" + tempDirectory + "\"",
                "\"" + Directory.GetCurrentDirectory() + "\\" + zipFileName + ".001\"");

            var info = new ProcessStartInfo { WindowStyle = ProcessWindowStyle.Hidden, FileName = @"7z\7z.exe", Arguments = str };
            var proc = Process.Start(info);

            proc?.WaitForExit();

            foreach (Attach attach in list)
            {
                File.Delete(zipFileName + GetNum(attach.num));
            }

            byte[] bytes = File.ReadAllBytes(Directory.GetCurrentDirectory() + "\\" + tempDirectory + "\\" + fileName);

            DelDirectory(tempDirectory);

            return bytes;
        }

        /// <summary>
        /// The get data from body.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="password">The password.</param>
        /// <param name="group">The group.</param>
        /// <returns>The <see cref="MessageForESB"/>.</returns>
        private static MessageForESB GetDataFromBody(Message message, out string password, out string group)
        {
            var xDoc = new XmlDocument
                {
                    InnerXml = Encoding.UTF8.GetString(Encoding.GetEncoding(1251).GetBytes(message.ToMailMessage().Body)).Trim()
                };

            var msg = new MessageForESB();

            group = string.Empty;
            password = string.Empty;

            foreach (XmlAttribute attr in xDoc.ChildNodes[1].Attributes)
            {
                switch (attr.Name)
                {
                    case "GroupName":
                        group = attr.Value;
                        break;

                    case "ClientId":
                        msg.ClientID = attr.Value;
                        break;

                    case "MessageTypeId":
                        msg.MessageTypeID = attr.Value;
                        break;

                    case "Password":
                        password = attr.Value;
                        break;
                }
            }

            msg.Body = xDoc.ChildNodes[1].FirstChild.Value;

            msg.Tags = new Dictionary<string, string>();

            if (xDoc.ChildNodes[1].ChildNodes[1] != null)
            {
                foreach (object node in xDoc.ChildNodes[1].ChildNodes[1].ChildNodes)
                {
                    msg.Tags.Add(((XmlNode)node).FirstChild.InnerText, ((XmlNode)node).ChildNodes[1].InnerText);
                }
            }

            return msg;
        }

        /// <summary>
        /// The get msgs for sb.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private static List<Header> GetMsgsForSB(Pop3Client client)
        {
            var headers = new List<Header>();
            for (int i = 0; i < client.GetMessageCount(); i++)
            {
                MessageHeader msg = client.GetMessageHeaders(i + 1);

                if (msg.Subject.Contains("MESSAGE_FROM_BUS_CLIENT"))
                {
                    headers.Add(new Header { msg = client.GetMessage(i + 1), num = i + 1 });
                }
            }

            return headers;
        }

        /// <summary>
        /// The get num.
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private static string GetNum(string num)
        {
            switch (num.Length)
            {
                case 1:
                    return ".00" + num;

                case 2:
                    return ".0" + num;

                case 3:
                    return "." + num;
            }

            return "001";
        }

        /// <summary>
        /// The receive mess.
        /// </summary>
        public static void ReceiveMess(IReceivingManager receivingManager, ILogger logger)
        {
            string mailServer = ConfigurationManager.AppSettings["MailServer"];
            int mailPort = Convert.ToInt32(ConfigurationManager.AppSettings["MailPort"]);
            string mailLogin = ConfigurationManager.AppSettings["MailLogin"];
            string mailPassword = Encoding.Unicode.GetString(Convert.FromBase64String(ConfigurationManager.AppSettings["MailPassword"]));

            var client = new Pop3Client();
            try
            {
                client.Connect(mailServer, mailPort, false);
                client.Authenticate(mailLogin, mailPassword);

                List<Header> msgs = GetMsgsForSB(client);
                foreach (Header header in msgs)
                {
                    if (!header.stored)
                    {
                        if (header.msg.Headers.Subject.Contains("OUT_OF"))
                        {
                            Header tmpHeader = header;
                            List<Header> curMsgs =
                                msgs.Where(
                                    msg => msg.msg.Headers.Subject.Contains(tmpHeader.msg.Headers.Subject.Split('_')[4]))
                                    .ToList();

                            if (curMsgs.Count() == Convert.ToInt32(header.msg.Headers.Subject.Split('_')[8]))
                            {
                                SaveMsgToDBFromManyAttachs(curMsgs, receivingManager);

                                foreach (Header msg in curMsgs)
                                {
                                    msg.stored = true;
                                }
                            }
                        }
                        else
                        {
                            SaveMsgToDB(header.msg, header.msg.Headers.Subject.Split('_')[4], receivingManager);

                            header.stored = true;
                        }
                    }
                }

                DeleteStoredMessages(client, msgs);
            }
            catch (Exception e)
            {
                logger.LogUnhandledException(e);
            }
            finally
            {
                if (client.Connected)
                {
                    client.Disconnect();
                }
            }
        }

        /// <summary>
        /// The save msg to db.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="fileName">The file name.</param>
        private static void SaveMsgToDB(Message message, string fileName, IReceivingManager receivingManager)
        {
            string password;
            string group;

            MessageForESB msg = GetDataFromBody(message, out password, out group);

            if (fileName != "empty")
            {
                var attachments = message.FindAllAttachments();
                msg.Attachment = GetAttachment(attachments[0].GetBodyAsText(), password);
            }

            if (group == string.Empty)
            {
                receivingManager.AcceptMessage(msg);
            }
            else
            {
                receivingManager.AcceptMessage(msg, group);
            }
        }

        /// <summary>
        /// The save msg to db from many attachs.
        /// </summary>
        /// <param name="msgs">The msgs.</param>
        private static void SaveMsgToDBFromManyAttachs(IList<Header> msgs, IReceivingManager receivingManager)
        {
            string password;
            string group;

            MessageForESB msg = GetDataFromBody(msgs[0].msg, out password, out group);

            var attList = new List<Attach>();
            foreach (Header header in msgs)
            {
                var attachments = header.msg.FindAllAttachments();
                attList.Add(
                    new Attach
                        {
                            str = attachments[1].GetBodyAsText(),
                            num = header.msg.Headers.Subject.Split('_')[5]
                        });
            }

            msg.Attachment = GetAttachmentFromMany(attList, password);

            if (group == string.Empty)
            {
                receivingManager.AcceptMessage(msg);
            }
            else
            {
                receivingManager.AcceptMessage(msg, group);
            }
        }
    }
}