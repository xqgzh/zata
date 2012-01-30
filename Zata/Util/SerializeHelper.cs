using System.IO;
using System.Web.Script.Serialization;

namespace Zata.Util
{
    /// <summary>
    /// 序列化辅助类
    /// </summary>
    public static class SerializeHelper
    {
        /// <summary>
        /// 序列化为xml
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToXml(object obj)
        {
            string str = null;
            if (obj != null)
            {
                str = XmlTools.ToXml(obj, true);
            }
            return str;
        }

        /// <summary>
        /// 序列化为json
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(object obj)
        {
            string str = null;
            if (obj != null)
            {
                str = new JavaScriptSerializer().Serialize(obj);
            }
            return str;
        }
    }
}
