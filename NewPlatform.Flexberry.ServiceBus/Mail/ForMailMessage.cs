namespace NewPlatform.Flexberry.ServiceBus.Mail
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Mail;
    using System.Text;

    ///<summary>
    ///класс-обертка для сообщения, отправляемого электронной почтой в корпоративную сервисную шину
    ///</summary>
    internal class ForMailMessage
    {
        ///<summary>
        ///если задать имя группы, то будет отправлено СообщениеСГруппой
        ///</summary>
        public string Group { get; set; }

        ///<summary>
        ///тело сообщения может содержать произвольный текст. не рекомендуется передавать большие объемы данных - лучше для этого использовать вложение
        ///</summary>
        public string Body { get; set; }

        ///<summary>
        ///словарь для произвольных пар "ключ-значение". в тэге также можно найти некоторую служебную информацию, например, путь или имя отправителя
        ///</summary>
        public Dictionary<string,string> Tags { get; set; }

        ///<summary>
        ///произвольное вложение. рекомендуется использовать для передачи файлов и больших объемов данных
        ///</summary>
        public byte[] Attachment { get; set; }

        ///<summary>
        ///идентификатор типа сообщения (тип Guid)
        ///</summary>
        public string MsgTypeID { get; set; }

        ///<summary>
        ///идентификатор отправителя (тип Guid)
        ///</summary>
        public string ClientID { get; set; }

        private int quote = 1024;

        ///<summary>
        ///квота позволяет ограничить размер пересылаемых почтой пакетов с данными. задается в байтах. по умолчанию 1024
        ///</summary>
        public int Quote
        {
            get { return quote; }
            set { quote = value; }
        }

        private string password = "123@asd";

        ///<summary>
        ///пароль используется при архивировании вложения с целью повышения уровня защищенности пакетов при пересылке. задан программно по умолчанию
        ///</summary>
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        private int tryCount = 10;

        ///<summary>
        ///в случае возникновения исключения будет произведено указанное количество повторов
        ///</summary>
        public int TryCount
        {
            get { return tryCount; }
            set { tryCount = value; }
        }

        private const string tempFileName = "File.txt";
        private string zipFileName;

        ///<summary>
        ///отправляет сообщение на электронный почтовый ящик
        ///</summary>
        ///<param name="mailTo">адрес почтового ящика получателя (на который отправляется сообщение)</param>
        ///<param name="mailFrom">адрес почтового ящика отправителя (проверяется регулярным выражением)</param>
        ///<param name="mailServerName">имя почтового сервера отправителя</param>
        public void Send(string mailTo, string mailFrom, string mailServerName)
        {
            CreatePackages();

            var list = GetNeedSendFiles();

            using (var message = new MailMessage(mailFrom, mailTo, string.Empty, CreateBody()))
            {
                var mailClient = new SmtpClient(mailServerName) {UseDefaultCredentials = true};

                if (list == null || list.Count == 0)
                {
                    message.Subject = "MESSAGE_FROM_BUS_CLIENT_";

                    var sb = new StringBuilder("MESSAGE_FROM_BUS_CLIENT_");

                    sb.Append(zipFileName);

                    message.Attachments.Add(new Attachment(zipFileName));

                    mailClient.Send(message);
                }
                else
                {
                    var i = 1;
                    foreach (var fileName in list)
                    {
                        message.Attachments.Clear();

                        using (var att = new Attachment(fileName))
                        {
                            var sb = new StringBuilder("MESSAGE_FROM_BUS_CLIENT_");

                            sb.Append(fileName.Substring(fileName.LastIndexOf("\\") + 1).Split('.')[0]);

                            if (list.Count > 1)
                            {
                                sb.Append("_").Append(i++).Append("_OUT_OF_").Append(list.Count);
                            }

                            message.Subject = sb.ToString();

                            message.Attachments.Add(att);

                            var j = TryCount;
                            while(j > 0)
                            {
                                try
                                {
                                    if (j > 0)
                                    {
                                        mailClient.Send(message);

                                        j = 0;
                                    }
                                }
                                catch
                                {
                                    j--;

                                    if (j == 0)
                                        throw new Exception("Не удалось подключиться к почтовому серверу");
                                }
                            }

                            att.Dispose();
                        }
                    }
                }

                message.Dispose();
            }

            if (zipFileName == "empty")
                Attachment = null;

            DeletePackages();
        }

        public void Send(string mailTo, string mailFrom, string mailServerName, bool useSSL)
        {
            CreatePackages();

            var list = GetNeedSendFiles();

            using (var message = new MailMessage(mailFrom, mailTo, string.Empty, CreateBody()))
            {
                var mailClient = new SmtpClient(mailServerName)
                    {UseDefaultCredentials = true, EnableSsl = useSSL};

                if (list == null || list.Count == 0)
                {
                    message.Subject = "MESSAGE_FROM_BUS_CLIENT_";

                    var sb = new StringBuilder("MESSAGE_FROM_BUS_CLIENT_");

                    sb.Append(zipFileName);

                    message.Attachments.Add(new Attachment(zipFileName));

                    mailClient.Send(message);
                }
                else
                {
                    var i = 1;
                    foreach (var fileName in list)
                    {
                        message.Attachments.Clear();

                        using (var att = new Attachment(fileName))
                        {
                            var sb = new StringBuilder("MESSAGE_FROM_BUS_CLIENT_");

                            sb.Append(fileName.Substring(fileName.LastIndexOf("\\") + 1).Split('.')[0]);

                            if (list.Count > 1)
                            {
                                sb.Append("_").Append(i++).Append("_OUT_OF_").Append(list.Count);
                            }

                            message.Subject = sb.ToString();

                            message.Attachments.Add(att);

                            var j = TryCount;
                            while (j > 0)
                            {
                                try
                                {
                                    if (j > 0)
                                    {
                                        mailClient.Send(message);

                                        j = 0;
                                    }
                                }
                                catch
                                {
                                    j--;

                                    if (j == 0)
                                        throw new Exception("Не удалось подключиться к почтовому серверу");
                                }
                            }

                            att.Dispose();
                        }
                    }
                }

                message.Dispose();
            }

            if (zipFileName == "empty")
                Attachment = null;

            DeletePackages();
        }

        private List<string> GetNeedSendFiles()
        {
            if (Attachment == null) return null;

            var list = new List<string>();

            foreach (var fileName in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                if (fileName.Contains(zipFileName))
                {
                    list.Add(fileName);
                }
            }
            return list;
        }

        private void DeletePackages()
        {
            if (zipFileName == null) return;

            foreach (var fileName in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                if (fileName.Contains(zipFileName))
                {
                    File.Delete(fileName);
                }
            }
        }

        private string CreateBody()
        {
            return string.IsNullOrEmpty(Group)
                       ? BodyMaker.Body(MsgTypeID, ClientID, Password, Body, Tags)
                       : BodyMaker.BodyWithGroup(MsgTypeID, ClientID, Password, Body, Group, Tags);
        }

        private void CreatePackages()
        {
            if (Attachment == null)
            {
                Attachment = Encoding.UTF8.GetBytes("empty string");

                zipFileName = "empty";
            }
            else
            {
                zipFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".7z";
            }

            File.WriteAllBytes(tempFileName, Attachment);

            var arg = $"a -t7z -mx=7 -ms=on -v{Quote} -p{Password} -mhe {zipFileName} {tempFileName}";
            var info = new ProcessStartInfo { WindowStyle = ProcessWindowStyle.Hidden, FileName = @"7z\7z.exe", Arguments = arg };
            var proc = Process.Start(info);

            proc?.WaitForExit();

            File.Delete(tempFileName);
        }
    }
}
