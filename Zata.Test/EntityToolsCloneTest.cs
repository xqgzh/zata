using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zata.FastReflection;
using System;
using System.Collections.Generic;

namespace Zata.Test
{
    /// <summary>
    /// This is a test class for EntityToolsTest and is intended
    /// to contain all EntityToolsTest Unit Tests
    /// </summary>
    [TestClass]
    public class EntityToolsCloneTest
    {
        [TestMethod]
        public void CopyToTest()
        {
            var a = new DataObjectModel() { Name = "Name", NameField = "NameField" };
            var b = new DynamicModel();
            EntityTools<DataObjectModel, DynamicModel>.CopyTo(a, b);
            Assert.AreEqual(EntityTools<DataObjectModel>.GetValue(a, "Name", false), EntityTools<DynamicModel>.GetValue(b, "Name", false));
            Assert.AreEqual(EntityTools<DataObjectModel>.GetValue(a, "NameField", false), EntityTools<DynamicModel>.GetValue(b, "NameField", false));
        }

        [TestMethod]
        public void CompatibleNameTest()
        {
            var a = new DynamicModel() { Property = "Value" };
            Assert.AreEqual("Value", EntityTools<DynamicModel>.GetValue(a, "Prop", false));
            EntityTools<DynamicModel>.SetValue(a, "Prop", false, "New");
            Assert.AreEqual("New", EntityTools<DynamicModel>.GetValue(a, "Prop", false));
        }

        [TestMethod]
        public void CompatibleCopyTest()
        {
            var a = new AClass() { Property = "P" };
            var b = new BClass();
            EntityTools<AClass, BClass>.CopyTo(a, b);
            Assert.AreEqual("P", b.Prop);
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

            [EntityAlias("Pp")]
            [EntityAlias("Property")]
            public string Prop { get; set; }

            public string DateTime { get; set; }

            public int Age { get; set; }
        }

        [TestMethod]
        public void IgnoreCaseTest()
        {
            var a = new AClass() { A = "AValue" };
            Assert.AreEqual("AValue", EntityTools<AClass>.GetValue(a, "A", false));
            Assert.AreEqual(null, EntityTools<AClass>.GetValue(a, "a", false));
            var b = new BClass();
            EntityTools<AClass, BClass>.CopyTo(a, b);
            Assert.AreEqual(null, EntityTools<BClass>.GetValue(b, "a", false));
            EntityTools<AClass, BClass>.CopyTo(a, b, true);
            Assert.AreEqual("AValue", EntityTools<BClass>.GetValue(b, "a", false));
        }

        [TestMethod]
        public void ConvertableTest()
        {
            var b = new BClass();
            EntityTools<BClass>.SetValue(b, "DateTime", false, DateTime.Now);
            EntityTools<BClass>.SetValue(b, "Age", false, "12");
            Assert.IsNotNull(b.DateTime);
            Assert.AreEqual(12, b.Age);
        }

        [TestMethod]
        public void ToUpperPerformanceTest()
        {
            Dictionary<string, int> hash = new Dictionary<string, int>();
            hash.Add("a", "a".GetHashCode());
            hash.Add("b", "a".GetHashCode());
            hash.Add("c", "a".GetHashCode());
            hash.Add("d", "a".GetHashCode());
            hash.Add("e", "a".GetHashCode());
            var temp = 1;
            var temp2 = "";

            var hashTime = new PerformanceTimer(() => temp = hash["a"]).Run(10000000);
            var upperTime = new PerformanceTimer(() => temp2 = "abcdef".ToUpperInvariant()).Run(10000000);
            var lowerTime = new PerformanceTimer(() => temp2 = "abcdef".ToLowerInvariant()).Run(10000000);

            Console.WriteLine(String.Format("Hash用时{0}，ToUpper用时{1}，ToLower用时{2}", hashTime, upperTime, lowerTime));
        }
    }
}
