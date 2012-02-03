﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zata.FastReflection;
using Zata.FastReflection.Caching;
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
        /// A test for GetPropertyValue
        /// </summary>
        [TestMethod()]
        public void GetPropertyValueTest()
        {
            var obj = new DynamicModel();
            string propertyName = "Property";
            object expected = null;
            object actual = obj.GetPropertyValue(propertyName);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SetPropertyValue
        ///</summary>
        [TestMethod()]
        public void SetPropertyValueTest()
        {
            var obj = new DynamicModel();
            string propertyName = "Property";
            object value = "Property";
            obj.SetPropertyValue(propertyName, value);
            Assert.AreEqual(value, obj.GetPropertyValue(propertyName));
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void GetPropertyValuePerformanceTest()
        {
            var obj = new DataObjectModel();
            var cachHost = obj as IAccessorCacheHost;
            object glocalInstance = obj;
            string propertyName = "NameField";
            ExtensionMethods.GetPropertyValue(obj, propertyName);
            var func = (Func<DataObjectModel, string>)Delegate.CreateDelegate(typeof(Func<DataObjectModel, string>), typeof(DataObjectModel).GetMethod("get_ReferenceCount"));


            var directCall = new PerformanceTimer(() => { (obj as DataObjectModel).NameField = (obj as DataObjectModel).Name; }).Run(10000000);
            var delegateCall = new PerformanceTimer(() => { var value = func(obj); }).Run(10000000);
            var interfaceCall = new PerformanceTimer(() => { (obj as IDataObject).SetValue(propertyName, (obj as IDataObject).GetValue(propertyName)); }).Run(10000000);
            var globalCacheCall = new PerformanceTimer(() => { glocalInstance.SetPropertyValue(propertyName, glocalInstance.GetPropertyValue(propertyName)); }).Run(10000000);
            var dynamicCall = new PerformanceTimer(() => { obj.SetPropertyValue(propertyName, obj.GetPropertyValue(propertyName)); }).Run(10000000);
            var genericCall = new PerformanceTimer(() => { obj.SetPropertyValue<DataObjectModel, string>(propertyName, obj.GetPropertyValue<DataObjectModel, string>(propertyName)); }).Run(10000000);
            var inClassCacheCall = new PerformanceTimer(() => { cachHost.SetPropertyValue(propertyName, cachHost.GetPropertyValue(propertyName)); }).Run(10000000);
            var reflectionCall = 0; // new PerformanceTimer(() => { var value = obj.GetPropertyValueByReflection(propertyName); }).Run(10000000);

            Trace.WriteLine(String.Format("直调用时{0}, 委托调用{1}，接口调用用时{2}， 全局缓存{3}，泛型缓存用时{4}， 泛型缓存+调用用时{5}， 类内缓存{6}，反射调用用时{7}", directCall, delegateCall, interfaceCall, globalCacheCall, dynamicCall, genericCall, inClassCacheCall, reflectionCall));
            Assert.IsTrue(dynamicCall > directCall);

            Trace.WriteLine(String.Format("相对直调减速比为{0}", inClassCacheCall.TotalMilliseconds / directCall.TotalMilliseconds));
            Trace.WriteLine(String.Format("相对接口减速比为{0}", inClassCacheCall.TotalMilliseconds / interfaceCall.TotalMilliseconds));
        }

        [TestMethod]
        public void WholeOperationPerformanceTest()
        {
            var obj = new DataObjectModel();
            var time = new PerformanceTimer(() =>
            {
                // 20%
                foreach (var property in obj.GetType().GetProperties())
                {
                    // 80% , 
                    // DataObjectModel有6个属性，也就是说，GetType().GetProperties()的代价，与一次GetPropertyValue相当。
                    // 所以没有必要为属性列表建立独立的缓存。
                    var value = (obj as IAccessorCacheHost).GetPropertyValue(property.Name);
                }
            }).Run(10000000);
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
        public void CopyValueTest()
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
            var obj2 = new DynamicModel();

            obj1.CopyValueTo(obj2);

            Assert.AreEqual(obj1.Name, obj2.Name);
            Assert.AreEqual(obj1.NameField, obj2.NameField);
            Assert.AreEqual(obj1.Key, obj2.Key);
            Assert.AreEqual(obj1.KeyField, obj2.KeyField);
            Assert.AreEqual(obj1.Hash, obj2.Hash);
            Assert.AreEqual(obj1.Namespace, obj2.Namespace);
            Assert.AreEqual(obj1.Property, obj2.Property);
            Assert.AreEqual(obj1.ReferenceCount, obj2.ReferenceCount);
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

            var copyCall = new PerformanceTimer(() => { obj1.CopyValueTo(obj2); }).Run(1000000);
            var cloneCall = new PerformanceTimer(() => { obj2 = obj1.Clone() as DataObjectModel; }).Run(1000000);
            var mannualCall = new PerformanceTimer(() => { MannualCopy(obj1, obj2); }).Run(1000000);
            var interfaceCall = new PerformanceTimer(() => { ReflectionCopy(obj1, obj2); }).Run(1000000);

            Trace.WriteLine(String.Format("接口拷贝用时{0}, 动态拷贝用时:{1}，浅拷贝用时:{2}, 手工赋值用时{3}", interfaceCall, copyCall, cloneCall, mannualCall));

            Trace.WriteLine("动态赋值与IDataObject接口的时间比为: " + copyCall.TotalMilliseconds / interfaceCall.TotalMilliseconds);
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
    }
}
