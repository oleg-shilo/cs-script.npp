using System;
using System.Collections.Generic;

#pragma warning disable 67

public class TestClassNoNS
{
}

namespace CSScriptIntellisense.Test
{
    /// <summary>
    /// Test values
    /// </summary>
    public enum TestEnum : uint
    {
        /// <summary>Value 1</summary>
        Val1,

        /// <summary>Value 2</summary>
        Val2 = 33,

        /// <summary>Value 3</summary>
        aVal3 = 33,

        /// <summary>Value 4</summary>
        Val4 = 34
    }

    public delegate int? TestDelgt<T>(CustomIndex count, int? contextArg, T parent) where T : struct;

    /// <summary>
    /// Simple class for testing Reflector
    /// </summary>
    public class TestClass1 : List<List<TestClass1>>, IEmptyInterface1, IEmptyInterface2
    {
        /// <summary>
        /// Gets or sets my property.
        /// </summary>
        /// <value>
        /// My property.
        /// </value>
        public int MyProperty { get; set; }

        public const int MyFieldConst = 77;
        public static int MyFieldStat = 33;
        public int MyField = 33333;
        public const char MyChar = '\n';
        public const string MyFieldName = "test\r\ntest";

        public int PropRW { get; set; }

        public static int PropR { get; private set; }

        public event Action OnLoad;

        public static event Action<int> OnLoadStatic;

        static public List<int?> TestMethodWithRefOut(List<int?> nullableIntParam, out int count, ref string name)
        {
            count = 0;
            return null;
        }

        public void TestVoidmethod()
        {
        }

        static public int TestMethod(int intParam = 0)
        {
            return 0;
        }

        static public Dictionary<int, Dictionary<TSource?, TDestination>> TestGenericMethod<TSource, TDestination>(IEnumerable<TSource> intParam) where TSource : struct
        {
            return null;
        }

        public TestClass1()
        {
        }

        protected virtual int MyVirtualProperty { get; set; }

        public TestClass1(int count)
        {
        }

        public class TestNestedClass1
        {
            public string Name { get; set; }
        }

        public class NestedParentClass
        {
            public delegate int TestDelgt3();
        }
    }

    public struct TestStruct1
    {
        public int MyProperty { get; set; }
    }

    public static class TestStaticClass
    {
        public static int MyProperty { get; set; }
    }

    public abstract class TestAbstractClass
    {
        public abstract int MyProperty { get; set; }
        public abstract void MyMethod();    }

    public class CustomIndex
    {
    }


    public static class TestClass27
    {
        public static void TestMethod(this int r, ref string data, out string data2, string separator = "test", char separator2 = 'r', StringComparison sc = StringComparison.Ordinal, params string[] items) { data2 = null; }
    }

    public class TestApiDocClass
    {
        /// <summary>
        /// Generic <see cref="T:WixSharp.WixEntity"/> container for defining WiX <c>Package</c> element attributes.
        /// <para>These attributes are the properties about the package to be placed in the Summary Information Stream.
        /// These are visible from COM through the IStream interface, and these properties can be seen on the package 
        /// in Explorer. </para>
        ///<example>The following is an example of defining the <c>Package</c> attributes.
        ///<code>
        /// var project = 
        ///     new Project("My Product",
        ///         new Dir(@"%ProgramFiles%\My Company\My Product",
        ///         
        ///     ...
        ///         
        /// project.Package.AttributesDefinition = @"AdminImage=Yes;
        ///                                          Comments=Release Candidate;
        ///                                          Description=Fantastic product...";
        ///                                         
        /// Compiler.BuildMsi(project);
        /// </code>
        /// </example>
        /// </summary>
        public void Test()
        {
        }
        /// <summary>
        /// This is the value of the <c>UpgradeCode</c> attribute of the Wix <c>Product</c> 
        /// element. 
        /// <para>Both WiX and MSI consider this element as optional even it is the only available identifier 
        /// for defining relationship between different versions of the same product. Wix# in contrary enforces
        /// that value to allow any future updates of the product being installed.
        /// </para>
        /// <para> 
        /// If user doesn't specify this value Wix# engine will use <see cref="Project.GUID"/> as <c>UpgradeCode</c>.
        /// </para>
        /// </summary>
        public Guid? UpgradeCode;
    }

    public class TestFieldsClass
    {
        public int Count;
        public int Count2 = 77;
        static public int Count3 = 33;
        public const int Count4 = 44;
    }

    public class TestArrayClass
    {
        public int[] ArrayProp { get; set; }

        public int? this[int? index0, CustomIndex index1, string index2, int index3]
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

    }

    public class TestParamArrayClass
    {
        public TestClass1[,] ArrayProp { get; set; }

        public string[,,] Test(int[,] arg, params string[] names)
        {
            return null;
        }
    }



    public class TestNestedGrandParentClass
    {
        public class TestNestedParentClass
        {
            static public class TestNestedChildStsticClass
            {
            }

            public class TestNestedChildClass
            {
                public int[] ArrayProp { get; set; }
            }

            public struct TestNestedStruct
            {
            }

            public string Name { get; set; }
        }
        public string Surname { get; set; }
    }

    public class TestNestedGrandParentGenericClass<T0>
    {
        public class TestNestedParentGenericClass<T1>
        {
            static public class TestNestedChildStsticClass
            {
            }

            public class TestNestedChildGenericClass<T2>
            {
                public T0 T0_Prop { get; set; }
                public T1 T1_Prop { get; set; }
                public T2 T2_Prop { get; set; }
                public int[] ArrayProp { get; set; }
            }

            public struct TestNestedStruct
            {
            }

            public string Name { get; set; }
        }
        public string Surname { get; set; }
    }

    public class TestBaseClass { }

    public class TestBaseClassDefC
    {
        public TestBaseClassDefC()
        {
        }

        //public TestBaseClassDefC(int count)
        //{
        //}
    }

    public class TestBaseGenericClass<T> { }
    public class TestBaseGenericClass2<T> where T : TestBaseClass { }
    public class TestBaseGenericClass3<T, T2>
        where T : class, new()
        where T2 : TestBaseClass, IEnumerable<int>, IList<int>
    { }

    public interface IEmptyInterface1 { }

    public interface IEmptyInterface2 { }

    public interface TestInterface
    {
        int MyProperty { get; set; }

        event Action OnLoad;
    }

    public static class ExtensionsClass
    {
        public static bool IsEmpty(this string obj)
        {
            return false;
        }
    }

    public class OperatorsOveloadClass
    {
        public static OperatorsOveloadClass operator +(OperatorsOveloadClass c1, OperatorsOveloadClass c2)
        {
            return null;
        }
    }

    public class DBBool
    {
        public static implicit operator DBBool(bool x) { return null; }
        public static explicit operator bool(DBBool x) { return false; }

        public static DBBool operator +(DBBool x, DBBool y) { return null; }
        public static DBBool operator -(DBBool x, DBBool y) { return null; }
        public static DBBool operator *(DBBool x, DBBool y) { return null; }
        public static DBBool operator /(DBBool x, DBBool y) { return null; }
        public static DBBool operator %(DBBool x, DBBool y) { return null; }
        public static DBBool operator &(DBBool x, DBBool y) { return null; }
        public static DBBool operator |(DBBool x, DBBool y) { return null; }
        public static DBBool operator ^(DBBool x, DBBool y) { return null; }
        public static DBBool operator <<(DBBool x, int y) { return null; }
        public static DBBool operator >>(DBBool x, int y) { return null; }
        public static DBBool operator !(DBBool x) { return null; }
        public static DBBool operator ~(DBBool x) { return null; }
        public static DBBool operator --(DBBool x) { return null; }
        public static DBBool operator ++(DBBool x) { return null; }

        public static DBBool operator ==(DBBool x, DBBool y) { return null; }
        public static DBBool operator !=(DBBool x, DBBool y) { return null; }
        public static DBBool operator <(DBBool x, DBBool y) { return null; }
        public static DBBool operator >(DBBool x, DBBool y) { return null; }
        public static DBBool operator <=(DBBool x, DBBool y) { return null; }
        public static DBBool operator >=(DBBool x, DBBool y) { return null; }

        public static bool operator true(DBBool x) { return false; }
        public static bool operator false(DBBool x) { return false; }

        //public override bool Equals(object obj) { return false; }
        //public override int GetHashCode() { return 0; }
        //public override string ToString() { return "DBBool.Null"; }
    }

    public interface ITestInterface1 { }
    public interface ITestInterface2 { }

    public class GTestClass1<TSource, TDestination, T3> : ITestInterface1, ITestInterface2
            where TSource : IEnumerable<int>, IList<int>
            where T3 : new()
    {
        public int? Count { get; protected set; }
        static public int MyProperty { get; set; }
        static public int MyPropertyIndex { get;  }
        static public int MyPropertyIndex2 { set { }  }

        internal int internalFld;
        private int privateFld;
        protected int protectedFld;

        public int MyField;
        public ICloneable MyCLonabeField;
        public Dictionary<int, string> Map;
        public Dictionary<int, List<string>> MapOfMaps;

        public void AddItems<T>(Dictionary<T, TSource> items, int index)
            where T : class, new()
        {
        }

        static GTestClass1()
        {
        }

        public GTestClass1()
        {
        }

        ~GTestClass1()
        {
        }

        public GTestClass1(IList<TSource> items)
        {
        }
    }
}