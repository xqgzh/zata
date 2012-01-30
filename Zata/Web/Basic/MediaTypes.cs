using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zata.Web.Basic
{
    public static class MediaTypes
    {
        #region Bambook中MimeTypes的类型映射

        /// <summary>
        /// 在bambook中, 存在一些项目特点的类型, 通过此字典与标准的Mime类型进行映射
        /// </summary>
        public static Dictionary<string, string> BambookMimeTypes = new Dictionary<string, string>();

        static MediaTypes()
        {
            BambookMimeTypes.Add(Text.Xml, Text.Xml);
            BambookMimeTypes.Add(MediaTypes.Application.Json, MediaTypes.Application.Json);
            BambookMimeTypes.Add(MediaTypes.Application.OctetStream, MediaTypes.Application.OctetStream);

            //兼容处理
            BambookMimeTypes.Add("xml", Text.Xml);
            BambookMimeTypes.Add("json", MediaTypes.Application.Json);
            BambookMimeTypes.Add("binary1", MediaTypes.Application.OctetStream);
        }

        #endregion

        /// <summary>
        /// Application Section
        /// </summary>
        public static class Application
        {
            /// <summary>
            /// application/json
            /// </summary>
            public const string Json = "application/json";

            /// <summary>
            /// application/octet-stream
            /// </summary>
            public const string OctetStream = "application/octet-stream";

            /// <summary>
            /// application/json-rpc
            /// </summary>
            public const string JsonRpc = "application/json-rpc";
        }

        /// <summary>
        /// Text Section
        /// </summary>
        public static class Text
        {
            /// <summary>
            /// text/xml
            /// </summary>
            public const string Xml = "text/xml";

            /// <summary>
            /// text/plain
            /// </summary>
            public const string Plain = "text/plain";

            /// <summary>
            /// text/html
            /// </summary>
            public const string Html = "text/html";
        }
    }
}
