using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Security.Cryptography;
using Zata.Util;

namespace Zata.Web.Protocols
{
    class BasicHttpMethodResponse
    {
        static byte[] zipPrefix = { (byte)'Z', (byte)'i', (byte)'p', (byte)' ' };
        static byte[] prefix = Encoding.UTF8.GetBytes("N$E_$nd@");
        static byte[] suffix = Encoding.UTF8.GetBytes("B@mb!0!0!k!");

        #region 根据Reques上下文信息格式化返回值


        /// <summary>
        /// 根据Reques上下文信息格式化返回值
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="httpRequest"></param>
        /// <param name="httpResponse"></param>
        /// <param name="httpMethodContext"></param>
        public void ProcessResponse(HttpActionContext context)
        {
            //0 业务逻辑检查, 检查方法的返回值是否合法
            //1 生成结果的二进制字节流
            //2 检查返回值是否需要压缩
            //3 检查返回值是否需要签名
            //4 写入结果到ResponseStream中
            context.ResponseStream = new MemoryStream();

            context.HttpContext.Response.Clear();
            object o = context.Result;
            string acceptType = GetAcceptType(context.HttpContext.Request);

            //获取返回结果的二进制数据
            byte[] ResultBinary = GetResultBinary(o, acceptType);

            //bool IsCompressed = IsNeedCompress(httpContext.Request, 0, acceptType, ResultBinary);

            //需要进行压缩
            //if (IsCompressed)
            //    ResultBinary = CompressHelper.Compress(ResultBinary, RestConfigInfo.Data.CompressLevel);

            //string SignatureType = GetSignatureType(httpContext.Request, acceptType);
            //byte[] SingnedBinary = null;

            //签名
            //switch (SignatureType)
            //{
            //    case "1":
            //        SingnedBinary = ComputeStaticSHA1(prefix, suffix, ResultBinary);
            //        break;
            //    case "2":
            //        //注意:实现动态sig时要注意对于可缓存接口使用该方式可能导致问题,应约束使得可缓存接口无法使用sig验证
            //        SingnedBinary = ComputeDynamicSHA1(ResultBinary);
            //        break;
            //    default:
            //        break;
            //}

            //写入Response
            context.HttpContext.Response.ContentEncoding = Encoding.UTF8;


            context.HttpContext.Response.ContentType = acceptType;
            context.ResponseStream.Write(ResultBinary, 0, ResultBinary.Length);
            //context.HttpContext.Response.BinaryWrite(ResultBinary);
            //context.HttpContext.Response.Flush();
        }

        #endregion

        #region 获取客户端指定返回的MIME类型

        /// <summary>
        /// 获取客户端指定返回的MIME类型
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        static string GetAcceptType(HttpRequest httpRequest)
        {
            /* 
             * 1. 首先检查xml参数确定是否返回xml
             * 2. 其次检查format参数确定返回类型
             * 3. 上述都不存在, 返回json格式
             * 4. 上述返回类型, 在返回值压缩和签名的情况下可能会变为application/octet-stream
             * 
             * 在2,3之间新增逻辑
             * 2.1. 根据Request.AcceptTypes来决定返回类型
             */
            string acceptMediaTypes = string.Empty;

            if (httpRequest["xml"] == "1")
                acceptMediaTypes = MediaTypes.Text.Xml;
            else
                acceptMediaTypes = httpRequest["format"];

            if (string.IsNullOrEmpty(acceptMediaTypes))
            {
                if (httpRequest.AcceptTypes != null && httpRequest.AcceptTypes.Length > 0)
                {
                    acceptMediaTypes = httpRequest.AcceptTypes[0];

                    int subIndex = acceptMediaTypes.IndexOf(";");

                    if (subIndex > 0)
                        acceptMediaTypes = acceptMediaTypes.Substring(0, subIndex);
                }
                else
                    acceptMediaTypes = "json";
            }

            acceptMediaTypes = acceptMediaTypes.ToLower();

            if (MediaTypes.MimeTypesDictionary.ContainsKey(acceptMediaTypes))
                acceptMediaTypes = MediaTypes.MimeTypesDictionary[acceptMediaTypes];
            else //默认返回XML文本
                acceptMediaTypes = MediaTypes.Application.Json;

            return acceptMediaTypes;
        }

        #endregion

        #region 将返回对象转换为二进制字节流

        /// <summary>
        /// 将返回对象转换为二进制字节流
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="o"></param>
        /// <param name="acceptType"></param>
        /// <returns></returns>
        private static byte[] GetResultBinary(object o, string acceptType)
        {
            switch (acceptType)
            {
                case MediaTypes.Application.Json:
                    return Encoding.UTF8.GetBytes(SerializeHelper.ToJson(o));
                case MediaTypes.Text.Xml:
                    return Encoding.UTF8.GetBytes(SerializeHelper.ToXml(o));
                default:
                    break;
            }

            //对于字符串, 返回UTF8二进制
            if (o is string)
                return Encoding.UTF8.GetBytes((string)o);

            //默认返回JSON数组
            return Encoding.UTF8.GetBytes(SerializeHelper.ToJson(o));
        }

        #endregion

        #region 检查是否需要压缩

        /// <summary>
        /// 检查是否需要压缩
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="CompressThreshold"></param>
        /// <param name="acceptType"></param>
        /// <param name="resultBinary"></param>
        /// <returns></returns>
        static bool IsNeedCompress(HttpRequest httpRequest, int CompressThreshold, string acceptType, byte[] resultBinary)
        {
            if (acceptType != MediaTypes.Application.Json && acceptType != MediaTypes.Text.Xml)
                return false;

            return "1".Equals(httpRequest["compress"]) && resultBinary.Length > CompressThreshold;
        }

        #endregion

        #region 获取客户端要求的签名类型

        /// <summary>
        /// 获取客户端要求的签名类型
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="acceptType"></param>
        /// <returns></returns>
        private static string GetSignatureType(HttpRequest httpRequest, string acceptType)
        {
            if (acceptType != MediaTypes.Application.Json &&
                acceptType != MediaTypes.Text.Xml &&
                acceptType != MediaTypes.Application.OctetStream)
                return string.Empty;

            return httpRequest["signature"];
        }

        #endregion

        #region 计算静态SHA1签名

        /// <summary>
        /// 计算静态SHA1签名
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private static byte[] ComputeDynamicSHA1(byte[] result)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            using (MemoryStream stream = new MemoryStream(result))
            {
                //TODO: 实现动态服务器校验时需要修改缓存策略，使得即使缓存命中也为用户生成一份sha1校验头
                //stream.Write(rsa, 0, rsa.Length);
                stream.Position = 0;
                return sha1.ComputeHash(stream);
            }
        }

        #endregion

        #region 计算动态SHA1签名

        /// <summary>
        /// 计算动态SHA1签名
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private static byte[] ComputeStaticSHA1(byte[] prefix, byte[] suffix, byte[] result)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(prefix, 0, prefix.Length);
                stream.Write(result, 0, result.Length);
                stream.Write(suffix, 0, suffix.Length);
                stream.Position = 0;
                return sha1.ComputeHash(stream);
            }
        }

        #endregion

    }
}
