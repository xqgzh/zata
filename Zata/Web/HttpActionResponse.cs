using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;

namespace Zata.Web
{
    /// <summary>
    /// 用于对HttpResponse进行封装, 以便解决缓存问题
    /// </summary>
    public class HttpActionResponse : IDisposable
    {
        private MemoryStream OutputStream = new MemoryStream();

        private Dictionary<string, string> HeaderDict = new Dictionary<string, string>();

        private HttpCookieCollection Cookies = new HttpCookieCollection();

        public string Charset { get; set; }

        public Encoding ContentEncoding { get; set; }

        public string ContentType { get; set; }

        public Encoding HeaderEncoding { get; set; }

        public void AddCookies(HttpCookie cookie)
        {
            Cookies.Add(cookie);
        }

        public void AppendHeader(string name, string value)
        {
            if (HeaderDict.ContainsKey(name))
                HeaderDict[name] = value;
            else
                HeaderDict.Add(name, value);
        }


        public void BinaryWrite(byte[] buffer)
        {
            OutputStream.Write(buffer, 0, buffer.Length);
        }

        public void Flush(HttpResponse response)
        {
            response.Clear();

            foreach (var k in HeaderDict)
            {
                response.Headers.Remove(k.Key);
                response.Headers.Add(k.Key, k.Value);
            }

            foreach (var c in Cookies.AllKeys)
            {
                response.Cookies.Remove(c);
                response.Cookies.Add(Cookies[c]);
            }

            response.ContentType = ContentType ?? response.ContentType;
            response.ContentEncoding = ContentEncoding ?? response.ContentEncoding;
            response.HeaderEncoding = HeaderEncoding ?? response.HeaderEncoding;
            response.Charset = Charset ?? response.Charset;
            response.BinaryWrite(OutputStream.ToArray());
        }


        public void Dispose()
        {
            if (OutputStream != null)
            {
                OutputStream.Dispose();
            }
        }
    }
}
