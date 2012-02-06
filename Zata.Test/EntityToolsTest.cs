﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zata.FastReflection;
using System;

namespace Zata.Test
{
    /// <summary>
    /// This is a test class for EntityToolsTest and is intended
    /// to contain all EntityToolsTest Unit Tests
    /// </summary>
    [TestClass]
    public class EntityToolsTest
    {
        [TestMethod]
        public void CopyToTest()
        {
            var a = new DataObjectModel() { Name = "Name", NameField = "NameField" };
            var b = new DynamicModel();
            EntityTools<DataObjectModel, DynamicModel>.CopyTo(a, b);
            Assert.AreEqual(EntityTools<DataObjectModel>.GetValue(a, "Name"), EntityTools<DynamicModel>.GetValue(b, "Name"));
            Assert.AreEqual(EntityTools<DataObjectModel>.GetValue(a, "NameField"), EntityTools<DynamicModel>.GetValue(b, "NameField"));
        }

        [TestMethod]
        public void CompatibleNameTest()
        {
            var a = new DynamicModel() { Property = "Value" };
            Assert.AreEqual("Value", EntityTools<DynamicModel>.GetValue(a, "Prop"));
            EntityTools<DynamicModel>.SetValue(a, "Prop", "New");
            Assert.AreEqual("New", EntityTools<DynamicModel>.GetValue(a, "Prop"));
            Assert.AreEqual("New", EntityTools<DynamicModel>.GetValueIgnoreCase(a, "pRop"));
        }

        [TestMethod]
        public void CompatibleCopyTest()
        {
            var a = new AClass() { Property = "P" };
            var b = new BClass();
            EntityTools<AClass, BClass>.CopyTo(a, b);
            Assert.AreEqual("P", b.Prop);
            Assert.AreEqual("P", EntityTools<BClass>.GetValueIgnoreCase(b, "pp"));
        }

        public class AClass
        {
            public string A { get; set; }

            public string Property { get; set; }

            public DateTime DateTime { get; set; }
        }

        public class BClass
        {
            public string a { get; set; }

            [CompatibleName("Pp")]
            [CompatibleName("Property")]
            public string Prop { get; set; }

            public string DateTime { get; set; }
        }

        [TestMethod]
        public void IgnoreCaseTest()
        {
            var a = new AClass() { A = "AValue" };
            Assert.AreEqual("AValue", EntityTools<AClass>.GetValue(a, "A"));
            Assert.AreEqual(null, EntityTools<AClass>.GetValue(a, "a"));
            Assert.AreEqual("AValue", EntityTools<AClass>.GetValueIgnoreCase(a, "a"));
            var b = new BClass();
            EntityTools<AClass, BClass>.CopyTo(a, b);
            Assert.AreEqual(null, EntityTools<BClass>.GetValue(b, "a"));
            EntityTools<AClass, BClass>.CopyTo(a, b, true);
            Assert.AreEqual("AValue", EntityTools<BClass>.GetValue(b, "a"));
        }

        [TestMethod]
        public void ConvertableTest()
        {
            var b = new BClass();
            EntityTools<BClass>.SetValue(b, "DateTime", DateTime.Now);
            Assert.IsNotNull(b.DateTime);
        }
    }
}