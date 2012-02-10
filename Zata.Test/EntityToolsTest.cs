using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zata.FastReflection;
using System.Diagnostics;
using Zata.Test.Performance;

namespace Zata.Test
{
    [TestClass]
    public class EntityToolsTest
    {
        [TestMethod]
        public void GetValue()
        {
            DataObjectModel obj = new DataObjectModel();

            string s = "test1234";

            obj.Name = s;

            string v = EntityTools<DataObjectModel>.GetValue(obj, "Name", false) as string;

            Assert.AreEqual(s, v);
        }

        [TestMethod]
        public void GetValueIgnoreCase()
        {
            DataObjectModel obj = new DataObjectModel();

            string s = "test1234";

            obj.Name = s;

            string v = EntityTools<DataObjectModel>.GetValue(obj, "naMe", true) as string;

            Assert.AreEqual(s, v);
        }

        [TestMethod]
        public void SetValue()
        {
            DataObjectModel obj = new DataObjectModel();

            string s = "test1234";

            obj.Name = "temp";
            EntityTools<DataObjectModel>.SetValue(obj, "Name", false, s);

            Assert.AreEqual(s, obj.Name);
        }

        [TestMethod]
        public void SetValueIgnoreCase()
        {
            DataObjectModel obj = new DataObjectModel();

            string s = "test1234";

            obj.Name = "temp";
            EntityTools<DataObjectModel>.SetValue(obj, "namE", true, s);

            Assert.AreEqual(s, obj.Name);
        }

        [TestMethod]
        public void TestEnum()
        {
            DataObjectModel obj = new DataObjectModel();

            obj.MyDay = Day.A1;

            EntityTools<DataObjectModel>.SetValue(obj, "MyDay", false, "A2");

            Assert.AreEqual(Day.A2, obj.MyDay);

            Day d = (Day)EntityTools<DataObjectModel>.GetValue(obj, "myDay", true);

            Assert.AreEqual(d, obj.MyDay);
        }

        [TestMethod]
        public void TestInt32()
        {
            DataObjectModel obj = new DataObjectModel();

            obj.BookCount = 112;

            int i = 234234;

            EntityTools<DataObjectModel>.SetValue(obj, "BookCount", false, i);

            Assert.AreEqual(i, obj.BookCount);

            int d = (int)EntityTools<DataObjectModel>.GetValue(obj, "BookCount", true);

            Assert.AreEqual(d, obj.BookCount);
        }

        [TestMethod]
        public void TestDateTime()
        {
            DataObjectModel obj = new DataObjectModel();

            obj.CreateTime = DateTime.Parse("2010-09-18");

            DateTime i = DateTime.Parse("2012-2-1");

            EntityTools<DataObjectModel>.SetValue(obj, "CreateTime", false, "2012-2-1");

            Assert.AreEqual(i, obj.CreateTime);

            DateTime d = (DateTime)EntityTools<DataObjectModel>.GetValue(obj, "CreateTime", true);

            Assert.AreEqual(d, obj.CreateTime);
        }

        [TestMethod]
        public void TestString()
        {
            DataObjectModel obj = new DataObjectModel();

            obj.CreateTime = DateTime.Parse("2010-09-18");

            DateTime i = DateTime.Parse("2012-2-1");

            EntityTools<DataObjectModel>.SetValueString(obj, "CreateTime", false, "2012-2-1");

            Assert.AreEqual(i, obj.CreateTime);

            string s = EntityTools<DataObjectModel>.GetValueString(obj, "CreateTime", true);

            Assert.AreEqual(s, obj.CreateTime.ToString());
        }

        [TestMethod]
        public void Perf_GetSet()
        {
            Trace.WriteLine("获取/设置性能测试， 忽略大小写");

            var obj = new DataObjectModel();
            object glocalInstance = obj;
            string propertyName = "Name";
            var func = (Func<DataObjectModel, string>)Delegate.CreateDelegate(typeof(Func<DataObjectModel, string>), typeof(DataObjectModel).GetMethod("get_Name"));

            bool Ignore = false;

            TestCase tester = new TestCase();

            tester.Watcher = (i, name, sw) =>
            {
                Trace.WriteLine(string.Format("{0}:{1}", name, sw.Elapsed));
            };

            tester.Build("直接调用时", () => { (obj as DataObjectModel).NameField = (obj as DataObjectModel).Name; });
            tester.Build("委托调用时", () => { var value = func(obj); });
            tester.Build("接口调用时", () => { (obj as IDataObject).SetValue(propertyName, Ignore, (obj as IDataObject).GetValue(propertyName, Ignore)); });
            tester.Build("工具调用时", () => { EntityTools<DataObjectModel>.SetValue(obj, propertyName, Ignore, EntityTools<DataObjectModel>.GetValue(obj, propertyName, Ignore)); });
            tester.Build("字符串工具", () => { EntityTools<DataObjectModel>.SetValueString(obj, propertyName, Ignore, EntityTools<DataObjectModel>.GetValueString(obj, propertyName, Ignore)); });
            tester.Build("空接口调用", () => { obj.SetEntityValue(propertyName, Ignore, obj.GetEntityValue(propertyName, Ignore)); });

            tester.StepWatcher = i =>
            {
                Trace.WriteLine(string.Format("执行{0:###,###,###}次的结果: ", i));
            };

            int times = 10000;

            tester.Execute(1000 * times);

        }

        [TestMethod]
        public void Perf_GetSet_IgnoreCase()
        {

            Trace.WriteLine("获取/设置性能测试， 忽略大小写");

            var obj = new DataObjectModel();
            object glocalInstance = obj;
            string propertyName = "Name";
            var func = (Func<DataObjectModel, string>)Delegate.CreateDelegate(typeof(Func<DataObjectModel, string>), typeof(DataObjectModel).GetMethod("get_Name"));

            bool Ignore = true;

            TestCase tester = new TestCase();

            tester.Watcher = (i, name, sw) =>
            {
                Trace.WriteLine(string.Format("{0}:{1}", name, sw.Elapsed));
            };

            tester.Build("直接调用时", () => { (obj as DataObjectModel).NameField = (obj as DataObjectModel).Name; });
            tester.Build("委托调用时", () => { var value = func(obj); });
            tester.Build("接口调用时", () => { (obj as IDataObject).SetValue(propertyName, Ignore, (obj as IDataObject).GetValue(propertyName, Ignore)); });
            tester.Build("工具调用时", () => { EntityTools<DataObjectModel>.SetValue(obj, propertyName, Ignore, EntityTools<DataObjectModel>.GetValue(obj, propertyName, Ignore)); });
            tester.Build("字符串工具", () => { EntityTools<DataObjectModel>.SetValueString(obj, propertyName, Ignore, EntityTools<DataObjectModel>.GetValueString(obj, propertyName, Ignore)); });
            tester.Build("空接口调用", () => { obj.SetEntityValue(propertyName, Ignore, obj.GetEntityValue(propertyName, Ignore)); });

            tester.StepWatcher = i =>
            {
                Trace.WriteLine(string.Format("执行{0:###,###,###}次的结果: ", i));
            };

            int times = 10000;

            tester.Execute(1000 * times);
        }

        
    }
}
