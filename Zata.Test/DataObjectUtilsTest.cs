using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zata.FastReflection;
using Zata.Test;

namespace ObjKnife.Test
{
    /// <summary>
    /// This is a test class for DataObjectUtilsTest and is intended
    /// to contain all DataObjectUtilsTest Unit Tests
    /// </summary>
    [TestClass()]
    public class DataObjectUtilsTest
    {
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void GetPropertyValuePerformanceTest()
        {
            var obj = new DataObjectModel();
            object glocalInstance = obj;
            string propertyName = "NameField";
            var func = (Func<DataObjectModel, string>)Delegate.CreateDelegate(typeof(Func<DataObjectModel, string>), typeof(DataObjectModel).GetMethod("get_ReferenceCount"));
            EntityTools<DataObjectModel>.SetValue(obj, propertyName, EntityTools<DataObjectModel>.GetValue(obj, propertyName));
            obj.SetEntityValue(propertyName, obj.GetEntityValue(propertyName)); 

            var directCall = new PerformanceTimer(() => { (obj as DataObjectModel).NameField = (obj as DataObjectModel).Name; }).Run(10000000);
            var delegateCall = new PerformanceTimer(() => { var value = func(obj); }).Run(10000000);
            var interfaceCall = new PerformanceTimer(() => { (obj as IDataObject).SetValue(propertyName, (obj as IDataObject).GetValue(propertyName)); }).Run(10000000);
            var reflectionCall = 0; // new PerformanceTimer(() => { var value = obj.GetPropertyValueByReflection(propertyName); }).Run(10000000);
            var entityToolsCall = new PerformanceTimer(() => { EntityTools<DataObjectModel>.SetValue(obj, propertyName, EntityTools<DataObjectModel>.GetValue(obj, propertyName)); }).Run(10000000);
            var entityToolsICCall = new PerformanceTimer(() => { EntityTools<DataObjectModel>.SetValueIgnoreCase(obj, propertyName, EntityTools<DataObjectModel>.GetValueIgnoreCase(obj, propertyName)); }).Run(10000000);
            var iEntityCall = new PerformanceTimer(() => { obj.SetEntityValue(propertyName, obj.GetEntityValue(propertyName)); }).Run(10000000);

            Trace.WriteLine(String.Format(
                "直调用时{0}, 委托调用{1}，接口调用用时{2}， 反射调用用时{3}, entityToolsCall{4}, entityICCall{5}, iEntityCall{6}", 
                directCall, delegateCall, interfaceCall, reflectionCall, entityToolsCall, entityToolsICCall, iEntityCall));

            Trace.WriteLine(String.Format("entityToolsCall相对直调减速比为{0:##.##}, 相对接口减速比: {1:##.##}", 
                entityToolsCall.TotalMilliseconds / directCall.TotalMilliseconds, 
                entityToolsCall.TotalMilliseconds / interfaceCall.TotalMilliseconds));
            Trace.WriteLine(String.Format("iEntityCall相对直调减速比为{0:##.##}, 相对接口减速比: {1:##.##}",
                iEntityCall.TotalMilliseconds / directCall.TotalMilliseconds,
                iEntityCall.TotalMilliseconds / interfaceCall.TotalMilliseconds));
        }

        [TestMethod]
        public void RuntimeType()
        {
            var obj = new DataObjectModel();
            var obj2 = new DataObjectModel();
            var type1 = obj.GetType();
            var type2 = obj2.GetType();

            var properties1 = type1.GetProperties();
            var properties2 = type2.GetProperties();

            Assert.IsTrue(Object.ReferenceEquals(type1, type2));
            Assert.IsFalse(Object.ReferenceEquals(properties1, properties2));
        }

        [TestMethod]
        public void CallGenericMethodByReflection()
        {
            GenericMehod<BindingFlags>(BindingFlags.Public);
            var type = this.GetType();
            var method = type.GetMethod("GenericMehod");
            method.MakeGenericMethod(typeof(BindingFlags)).Invoke(this, new object[] { BindingFlags.Public });
        }

        [TestMethod]
        public void CopyValuePerformanceTest()
        {
            var obj1 = new DataObjectModel()
            {
                Name = "Name",
                Key = "Key",
                Hash = "Hash",
                KeyField = "KeyField",
                NameField = "NameField",
                Namespace = "Namespace",
                Property = "Property",
                ReferenceCount = "FDf"
            };

            var obj2 = new DataObjectModel()
            {
                Name = "Name",
                Key = "Key",
                Hash = "Hash",
                KeyField = "KeyField",
                NameField = "NameField",
                Namespace = "Namespace",
                Property = "Property",
                ReferenceCount = "FDf"
            };

            var cloneCall = new PerformanceTimer(() => obj2 = obj1.Clone() as DataObjectModel).Run(1000000);
            var mannualCall = new PerformanceTimer(() => MannualCopy(obj1, obj2)).Run(1000000);
            var interfaceCall = new PerformanceTimer(() => ReflectionCopy(obj1, obj2)).Run(1000000);
            var emitCall = new PerformanceTimer(() => EntityTools<DataObjectModel, DataObjectModel>.CopyTo(obj1, obj2, true)).Run(1000000);

            Trace.WriteLine(String.Format("接口拷贝用时{0}，浅拷贝用时:{1}, 手工赋值用时{2}, Emit用时{3}", interfaceCall, cloneCall, mannualCall, emitCall));
        }

        private void MannualCopy(DataObjectModel source, DataObjectModel target)
        {
            target.NameField = source.NameField;
            target.Name = source.Name;
            target.Key = source.Key;
            target.KeyField = source.KeyField;
            target.Hash = source.Hash;
            target.Namespace = source.Namespace;
            target.Property = source.Property;
            target.ReferenceCount = source.ReferenceCount;
        }

        private void ReflectionCopy(IDataObject source, IDataObject target)
        {
            var sourceType = source.GetType();
            foreach (var memberInfo in sourceType.GetProperties())
            {
                target.SetValue(memberInfo.Name, source.GetValue(memberInfo.Name));
            }
            foreach (var memberInfo in sourceType.GetFields())
            {
                target.SetValue(memberInfo.Name, source.GetValue(memberInfo.Name));
            }
        }

        public void GenericMehod<T>(T e) where T : struct
        {
        }

        [TestMethod]
        public void TestPerfGet4Set()
        {
            var obj = new DataObjectModel();
            object glocalInstance = obj;
            string propertyName = "NameField";
            var func = (Func<DataObjectModel, string>)Delegate.CreateDelegate(typeof(Func<DataObjectModel, string>), typeof(DataObjectModel).GetMethod("get_ReferenceCount"));

            TestCase tester = new TestCase();

            tester.Watcher = (i, name, sw) =>
            {
                Trace.WriteLine(string.Format("{0}:{1:##.##}", name.PadingLeft(20, '.'), sw.ElapsedTicks.ToString().PadingLeft(20, '.')));
            };

            tester.Build("直接调用", () => { (obj as DataObjectModel).NameField = (obj as DataObjectModel).Name; });
            tester.Build("委托调用", () => { var value = func(obj); });
            tester.Build("接口调用用时", () => { (obj as IDataObject).SetValue(propertyName, (obj as IDataObject).GetValue(propertyName)); });
            tester.Build("反射调用用时", () => { /*var value = obj.GetPropertyValueByReflection(propertyName);*/ });
            tester.Build("EntityTools<T>调用", () => { EntityTools<DataObjectModel>.SetValue(obj, propertyName, EntityTools<DataObjectModel>.GetValue(obj, propertyName)); });
            tester.Build("IEntity<T>调用", () => { obj.SetEntityValue(propertyName, obj.GetEntityValue(propertyName)); });

            tester.StepWatcher = i =>
            {
                Trace.WriteLine(string.Format("执行{0}次的结果: ", i));
            };

            int times = 10000;

            tester.Execute(10 * times, 100 * times);

        }



        public class TestCase
        {
            public Dictionary<string, Action> Methods = new Dictionary<string, Action>();

            public Action<int, string, Stopwatch> Watcher;
            public Action<int> StepWatcher;

            public TestCase Build(string name, Action action)
            {
                Methods.Add(name, action);
                return this;
            }

            public void Execute(params int[] MaxTimeList)
            {
                Stopwatch sw = new Stopwatch();

                int iCount = Methods.Keys.Count;

                for (int Step = 0; Step < MaxTimeList.Length; Step++ )
                {
                    int MaxTimes = MaxTimeList[Step];

                    if (StepWatcher != null)
                        StepWatcher(MaxTimes);

                    foreach (var name in Methods.Keys)
                    {
                        var action = Methods[name];
                        action();
                        sw.Reset();
                        sw.Start();
                        for (int i = 0; i < MaxTimes; i++)
                        {
                            action();
                        }
                        sw.Stop();

                        Watcher(Step, name, sw);
                    }

                }
            }
        }
    }

    public static class StringExtension
    {
        public static string PadingLeft(this string str, int Width, char c)
        {
            int w = Width * 3 - Encoding.UTF8.GetByteCount(str);

            return str.PadLeft(w, c);
        }
    }

}
