using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Data;


namespace Zata.FastReflection
{
    public abstract class EntityTools<T>
    {
        public static readonly Func<T, string, bool, object> GetValue;
        public static readonly Func<T, string, bool, object,bool> SetValue;
        public static readonly Func<T, string, bool, string> GetValueString;
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
    }

}
