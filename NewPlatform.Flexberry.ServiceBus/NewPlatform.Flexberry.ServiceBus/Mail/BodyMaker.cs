namespace NewPlatform.Flexberry.ServiceBus.Mail
{
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// The body maker.
    /// </summary>
    internal class BodyMaker
    {
        /// <summary>
        /// The body.
        /// </summary>
        /// <param name="msgType">The msg type.</param>
        /// <param name="clientId">The client id.</param>
        /// <param name="password">The password.</param>
        /// <param name="msgBody">The msg body.</param>
        /// <param name="msgTags">The msg tags.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string Body(string msgType, string clientId, string password, string msgBody, Dictionary<string, string> msgTags)
        {
            return
                string.Format(
                    "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\" ?>"
                    + "<MessageForESB MessageTypeId=\"{0}\" "
                    + "ClientId=\"{1}\" Password=\"{2}\"><![CDATA[{3}]]>{4}</MessageForESB>",
                    msgType,
                    clientId,
                    password,
                    msgBody,
                    CreateTags(msgTags));
        }

        /// <summary>
        /// The body with group.
        /// </summary>
        /// <param name="msgType">The msg type.</param>
        /// <param name="clientId">The client id.</param>
        /// <param name="password">The password.</param>
        /// <param name="msgBody">The msg body.</param>
        /// <param name="groupName">The group name.</param>
        /// <param name="msgTags">The msg tags.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string BodyWithGroup(
            string msgType,
            string clientId,
            string password,
            string msgBody,
            string groupName,
            Dictionary<string, string> msgTags)
        {
            return string.Format(
                "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\" ?> "
                + "<MessageForESBGroup MessageTypeId=\"{0}\" "
                + "ClientId=\"{1}\" Password=\"{2}\" GroupName=\"{3}\">" + "<![CDATA[{4}]]>{5}</MessageForESBGroup>",
                msgType,
                clientId,
                password,
                groupName,
                msgBody,
                CreateTags(msgTags));
        }

        /// <summary>
        /// The create tags.
        /// </summary>
        /// <param name="msgTags">The msg tags.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string CreateTags(Dictionary<string, string> msgTags)
        {
            if (msgTags == null)
            {
                return string.Empty;
            }

            return msgTags.Count == 0 ? string.Empty : string.Format("<tags>{0}</tags>", GetTagsStr(msgTags));
        }

        /// <summary>
        /// The get tags str.
        /// </summary>
        /// <param name="tags">The tags.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private static string GetTagsStr(IEnumerable<KeyValuePair<string, string>> tags)
        {
            var result = new StringBuilder();

            foreach (var tag in tags)
            {
                result.AppendFormat("<tag><key><![CDATA[{0}]]></key><value><![CDATA[{1}]]></value></tag>", tag.Key, tag.Value);
            }

            return result.ToString();
        }
    }
}