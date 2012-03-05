using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Data;
using System.Web;
using System.Collections;


namespace Zata.FastReflection
{
    public abstract class EntityTools<T>
    {
        /// <summary>
        /// 获取指定属性的值 (string name, bool IgnoreCase)
        /// </summary>
        public static readonly Func<T, string, bool, object> GetValue;

        /// <summary>
        /// 设置指定属性的值 (string name, bool IgnoreCase, object value)
        /// </summary>
        public static readonly Func<T, string, bool, object,bool> SetValue;

        /// <summary>
        /// 获取指定属性的值, 结果转换为字符串 (string name, bool IgnoreCase)
        /// </summary>
        public static readonly Func<T, string, bool, string> GetValueString;

        /// <summary>
        /// 设置指定属性的值, 输入为字符串 (string name, bool IgnoreCase, string value)
        /// </summary>
        public static readonly Func<T, string, bool, string, bool> SetValueString;

        public static int PropertyCount;
        public static int FieldCount;
        public static int FieldOrPropertyCount;

        public static readonly string[] FeildOrPropertys;
        public static readonly string[] Propertys;
        public static readonly string[] Fields;

        static EntityTools()
        {
            EntityToolsInternal.GetFieldPropertys<T>(ref FieldCount, ref PropertyCount, ref Fields, ref Propertys, ref FeildOrPropertys);

            FieldOrPropertyCount = PropertyCount + FieldCount;


            GetValue = EntityToolsInternal.GetValueFunction<T, object>();
            SetValue = EntityToolsInternal.SetValueFunction<T, object>();
            GetValueString = EntityToolsInternal.GetValueFunction<T, string>();
            SetValueString = EntityToolsInternal.SetValueFunction<T, string>();
        }

        #region From

        /// <summary>
        /// 从IDataRecord中解析对象属性
        /// </summary>
        /// <param name="record"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public int FromIDataRecord(IDataRecord record, T target)
        {
            int totalCount = 0;

            for (int i = 0; i < record.FieldCount; i++)
            {
                string name = record.GetName(i);

                if (string.IsNullOrEmpty(name) || record.IsDBNull(i))
                    continue;

                if (SetValue(target, name, true, record.GetValue(i))) totalCount++;
            }

            return totalCount;
        }

        /// <summary>
        /// 从DataRow中解析对象属性
        /// </summary>
        /// <param name="record"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public int FromDataRow(DataRow record, T target)
        {
            int totalCount = 0;


            for (int i = 0, j = record.Table.Columns.Count; i < j; i++)
            {
                string name = record.Table.Columns[i].ColumnName;

                if (string.IsNullOrEmpty(name) || record.IsNull(i))
                    continue;

                if (SetValue(target, name, true, record[i])) totalCount++;
            }

            return totalCount;
        }

        /// <summary>
        /// 从HttpRequest中解析对象属性(解析POST, GET, Server-Varable, Cookies四个集合, 返回结果
        /// </summary>
        /// <param name="record"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public int FromHttpRequest(HttpRequest request, T target)
        {
            int totalCount = 0;

            for (int i = 0, j = request.Params.Count; i < j; i++)
            {
                string name = request.Params.GetKey(i);

                string values = request.Params.Get(i);

                if (SetValueString(target, name, true, values)) totalCount++;
            }

            return totalCount;
        }

        /// <summary>
        /// 从HttpRequest中解析对象属性(解析POST集合, 返回结果
        /// </summary>
        /// <param name="record"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public int FromHttpRequestPost(HttpRequest request, T target)
        {
            int totalCount = 0;

            for (int i = 0, j = request.Form.AllKeys.Length; i < j; i++)
            {
                string name = request.Form.GetKey(i);

                string values = request.Form.Get(i);

                if (SetValueString(target, name, true, values)) totalCount++;
            }

            return totalCount;
        }

        /// <summary>
        /// 从HttpRequest中解析对象属性(解析POST集合, 返回结果
        /// </summary>
        /// <param name="record"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public int FromHttpRequestGet(HttpRequest request, T target)
        {
            int totalCount = 0;

            for (int i = 0, j = request.QueryString.AllKeys.Length; i < j; i++)
            {
                string name = request.QueryString.GetKey(i);

                string values = request.QueryString.Get(i);

                if (SetValueString(target, name, true, values)) totalCount++;
            }

            return totalCount;
        }

        /// <summary>
        /// 从HttpRequest中解析对象属性(解析Cookie集合, 返回结果
        /// </summary>
        /// <param name="record"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public int FromHttpRequestCookies(HttpRequest request, T target)
        {
            int totalCount = 0;

            for (int i = 0, j = request.Cookies.AllKeys.Length; i < j; i++)
            {
                var cookie = request.Cookies.Get(i);
                string name = cookie.Name;

                string value = cookie.Value;

                if (SetValueString(target, name, true, value)) totalCount++;
            }

            return totalCount;
        }

        #endregion

        #region To

        /// <summary>
        /// 从IDataRecord中解析对象属性
        /// </summary>
        /// <param name="target"></param>
        /// <param name="DbDataParameterList"></param>
        /// <returns></returns>
        public int ToIDbParameterList(T target, IList DbDataParameterList)
        {
            int totalCount = 0;

            foreach (IDbDataParameter p in DbDataParameterList)
            {
                if (p.Direction != ParameterDirection.Input)
                    continue;

                string pName = EntityToolsInternal.GetParameterName(p);

                object o = GetValue(target, pName, false);

                if (o == null)
                    p.Value = DBNull.Value;
                else
                    p.Value = o;
            }

            return totalCount;
        }

        #endregion
    }

}
