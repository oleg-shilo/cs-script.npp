using System;
using System.IO;
using Xunit;
using RoslynIntellisense;
using CSScriptIntellisense.Test;

namespace Testing
{
    public class RoslynReflector : RoslynHost
    {
        [Fact]
        public void Reconstruct_Enum()
        {
            var enumSymbol = LoadType<CSScriptIntellisense.Test.TestEnum>();

            string code = enumSymbol.Reconstruct();

            Assert.Equal(code,
@"using System;

namespace CSScriptIntellisense.Test
{
    // Test values
    public enum TestEnum
    {
        // Value 1
        Val1 = 0,
        // Value 2
        Val2 = 33,
        // Value 3
        aVal3 = 33,
        // Value 4
        Val4 = 34,
    }
}");
        }

        [Fact]
        public void Reconstruct_NullableStruct()
        {
            var enumSymbol = LoadType("System.Nulla|ble<int>");

            string code = enumSymbol.Reconstruct(false);

            Assert.Equal(@"namespace System
{
    public sealed struct Nullable<T>
        where T: struct
    {
        public Nullable();
        public Nullable(T value);

        public bool HasValue { get; }
        public T Value { get; }

        public override bool Equals(object other);
        public override int GetHashCode();
        public T GetValueOrDefault();
        public T GetValueOrDefault(T defaultValue);
        public override string ToString();

        public static explicit operator T(Nullable<T> value);
        public static implicit operator Nullable<T>(T value);
    }
}", code);
        }

        [Fact]
        public void Reconstruct_NestedEnumAsParam()
        {
            var enumSymbol = LoadExpression("var ttt = Environment.GetFolder|Path(Environment.SpecialFolder.AdminTools);");

            string code = enumSymbol.Reconstruct(false);

            Assert.StartsWith(@"using System.Collections;

namespace System
{
    public static class Environment
    {
        public static string CommandLine { get; }
        public static string CurrentDirectory { get; set; }
        public static int CurrentManagedThreadId { get; }", code);
        }

        [Fact]
        public void Reconstruct_NestedEnum()
        {
            var enumSymbol = LoadExpression("var ttt = Environment.GetFolderPath(Environment.SpecialFolder.Ad|minTools);");

            string code = enumSymbol.Reconstruct(false);
            { }
            Assert.StartsWith(@"namespace System
{
    public class Environment
    {
        public enum SpecialFolder
        {
            ApplicationData = 26,
            CommonApplicationData = 35,
            LocalApplicationData = 28,
            Cookies = 33,
            Desktop = 0,
            Favorites = 6,
            History = 34,
            InternetCache = 32,
            Programs = 2,
            MyComputer = 17,
            MyMusic = 13,
            MyPictures = 39,
            MyVideos = 14,
            Recent = 8,
", code);
        }

        [Fact]
        public void Reconstruct_GenericClass()
        {
            var enumSymbol = LoadType("CSScriptIntellisense.Test.GTest|Class1<List<int>, int, object>>");

            string code = enumSymbol.Reconstruct(false);

            Assert.Equal(
@"using System;
using System.Collections.Generic;

namespace CSScriptIntellisense.Test
{
    public class GTestClass1<TSource, TDestination, T3>: ITestInterface1, ITestInterface2
        where TSource: IEnumerable<int>, IList<int>
        where T3: new()
    {
        public GTestClass1();
        public GTestClass1(IList<TSource> items);

        protected ~GTestClass1();

        public Dictionary<int, string> Map;
        public Dictionary<int, List<string>> MapOfMaps;
        public ICloneable MyCLonabeField;
        public int MyField;
        protected int protectedFld;

        public int? Count { get; protected set; }
        public static int MyProperty { get; set; }
        public static int MyPropertyIndex { get; }
        public static int MyPropertyIndex2 { set; }

        public void AddItems<T>(Dictionary<T, TSource> items, int index) where T: class, new();
    }
}", code);
        }

        [Fact]
        public void Reconstruct_OperatorOverloads()
        {
            var enumSymbol = LoadType<DBBool>();

            string code = enumSymbol.Reconstruct(false);

            Assert.Equal(
@"using System;

namespace CSScriptIntellisense.Test
{
    public class DBBool
    {
        public static DBBool operator +(DBBool x, DBBool y);
        public static DBBool operator &(DBBool x, DBBool y);
        public static DBBool operator |(DBBool x, DBBool y);
        public static DBBool operator --(DBBool x);
        public static DBBool operator /(DBBool x, DBBool y);
        public static DBBool operator ==(DBBool x, DBBool y);
        public static DBBool operator ^(DBBool x, DBBool y);
        public static DBBool operator >(DBBool x, DBBool y);
        public static DBBool operator >=(DBBool x, DBBool y);
        public static DBBool operator ++(DBBool x);
        public static DBBool operator !=(DBBool x, DBBool y);
        public static DBBool operator <<(DBBool x, int y);
        public static DBBool operator <(DBBool x, DBBool y);
        public static DBBool operator <=(DBBool x, DBBool y);
        public static DBBool operator !(DBBool x);
        public static DBBool operator %(DBBool x, DBBool y);
        public static DBBool operator *(DBBool x, DBBool y);
        public static DBBool operator ~(DBBool x);
        public static DBBool operator >>(DBBool x, int y);
        public static DBBool operator -(DBBool x, DBBool y);

        public static bool operator false(DBBool x);
        public static bool operator true(DBBool x);

        public static explicit operator bool(DBBool x);
        public static implicit operator DBBool(bool x);
    }
}", code);
        }

        [Fact]
        public void Reconstruct_Indexers()
        {
            var type = LoadType("CSScriptIntellisense.Test.TestArrayCla|ss");

            string code = type.Reconstruct(false);

            Assert.Equal(
@"using System;

namespace CSScriptIntellisense.Test
{
    public class TestArrayClass
    {
        public int[] ArrayProp { get; set; }
        public int? this[int? index0, CustomIndex index1, string index2, int index3] { get; set; }
    }
}", code);
        }

        [Fact]
        public void Reconstruct_ExtensionMethodsClass()
        {
            var symbol = LoadType("CSScriptIntellisense.Test.ExtensionsCla|ss");

            string code = symbol.Reconstruct(false);

            Assert.Equal(@"using System;

namespace CSScriptIntellisense.Test
{
    public static class ExtensionsClass
    {
        public static bool IsEmpty(this string obj);
    }
}", code);
        }

        [Fact]
        public void Reconstruct_SrcExtensionMethodUse()
        {
            var symbol = LoadExpression("var empty = \"test\".IsEmp|ty()");

            string code = symbol.Reconstruct(false);

            Assert.Equal(@"using System;

namespace CSScriptIntellisense.Test
{
    public static class ExtensionsClass
    {
        public static bool IsEmpty(this string obj);
    }
}", code);
        }

        [Fact]
        public void Reconstruct_AsmExtensionMethodUse()
        {
            var symbol = LoadExpression("var t = \"dd\".ToA|rray();");

            int pos;
            string code = symbol.Reconstruct(out pos, false);

            string fragment = code.Substring(pos).TrimStart();

            Assert.StartsWith(@"public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source);", fragment);
        }

        [Fact]
        public void Reconstruct_MethodParamModifiers()
        {
            var symbol = LoadType("CSScriptIntellisense.Test.TestClass|27");

            string code = symbol.Reconstruct(false);

            Assert.Equal(@"using System;

namespace CSScriptIntellisense.Test
{
    public static class TestClass27
    {
        public static void TestMethod(this int r, ref string data, out string data2, string separator = ""test"", char separator2 = 'r', StringComparison sc = 4, params string[] items);
    }
}", code);
        }

        [Fact]
        public void Reconstruct_StaticClass()
        {
            var symbol = LoadType("CSScriptIntellisense.Test.TestS|taticClass");

            string code = symbol.Reconstruct(false);

            Assert.Equal(@"using System;

namespace CSScriptIntellisense.Test
{
    public static class TestStaticClass
    {
        public static int MyProperty { get; set; }
    }
}", code);
        }

        [Fact]
        public void Reconstruct_DoesNotPickInheritedMembers()
        {
            var type = LoadType<FileInfo>();

            string code = type.Reconstruct(false);

            Assert.DoesNotContain(code, "public DateTime CreationTime { get; set; }");
        }

        [Fact]
        public void Reconstruct_AbstractClass()
        {
            var type = LoadType("CSScriptIntellisense.Test.TestAbstractCl|ass");

            string code = type.Reconstruct(false);

            Assert.Equal(
@"using System;

namespace CSScriptIntellisense.Test
{
    public abstract class TestAbstractClass
    {
        protected TestAbstractClass();

        public abstract int MyProperty { get; set; }

        public abstract void MyMethod();
    }
}", code);
        }

        [Fact]
        public void Reconstruct_ParamArrayClass()
        {
            var type = LoadType("CSScriptIntellisense.Test.TestParamArrayCla|ss");

            string code = type.Reconstruct(false);

            Assert.Equal(
@"using System;

namespace CSScriptIntellisense.Test
{
    public class TestParamArrayClass
    {
        public TestClass1[,] ArrayProp { get; set; }

        public string[,,] Test(int[,] arg, params string[] names);
    }
}", code);
        }

        [Fact]
        public void Reconstruct_Interface()
        {
            var type = LoadType("CSScriptIntellisense.Test.TestInterf|ace");

            string code = type.Reconstruct(false);

            Assert.Equal(
@"using System;

namespace CSScriptIntellisense.Test
{
    public interface TestInterface
    {
        event Action OnLoad;

        int MyProperty { get; set; }
    }
}", code);
        }

        [Fact(Skip = "waiting for Syntaxer migration")]
        public void Reconstruct_ComplexDocumentation()
        {
            var type = LoadType<TestApiDocClass>();

            string code = type.Reconstruct();

            Assert.Equal(@"using System;

namespace CSScriptIntellisense.Test
{
    public class TestApiDocClass
    {
        // This is the value of the
        // UpgradeCode attribute of the Wix Product element.
        // Both WiX and MSI consider this element as optional even it is the only available identifier for defining relationship between different versions of the same product. Wix# in contrary enforces that value to allow any future updates of the product being installed.
        //  If user doesn't specify this value Wix# engine will use !:Project.GUID as UpgradeCode.
        public Guid? UpgradeCode;

        // Generic
        // WixSharp.WixEntity container for defining WiX Package element attributes.
        // These attributes are the properties about the package to be placed in the Summary Information Stream. These are visible from COM through the IStream interface, and these properties can be seen on the package in Explorer.
        // The following is an example of defining the Package attributes.
        //
        //              var project =
        //                  new Project(""My Product"",
        //                      new Dir(@""%ProgramFiles%\My Company\My Product"",
        //
        //                  ...
        //
        //              project.Package.AttributesDefinition = @""AdminImage=Yes;
        //                                                       Comments=Release Candidate;
        //                                                       Description=Fantastic product..."";
        //
        //              Compiler.BuildMsi(project);
        public void Test();
    }
}", code);
        }

        [Fact]
        public void Reconstruct_Class()
        {
            var type = LoadType<TestClass1>();

            string code = type.Reconstruct(false);

            Assert.Equal(
@"using System;
using System.Collections.Generic;

namespace CSScriptIntellisense.Test
{
    public class TestClass1 : List<List<TestClass1>>, IList, ICollection, IList, ICollection, IReadOnlyList, IReadOnlyCollection, IEnumerable, IEnumerable, IEmptyInterface1, IEmptyInterface2
    {
        public TestClass1();
        public TestClass1(int count);

        public static const char MyChar = '\n';
        public int MyField;
        public static const int MyFieldConst = 77;
        public static const string MyFieldName = ""test\r\ntest"";
        public static int MyFieldStat;

        public event Action OnLoad;
        public static event Action<int> OnLoadStatic;

        public int MyProperty { get; set; }
        public static int PropR { get; }
        public int PropRW { get; set; }
        protected virtual int MyVirtualProperty { protected get; protected set; }

        public static Dictionary<int, Dictionary<TSource?, TDestination>> TestGenericMethod<TSource, TDestination>(IEnumerable<TSource> intParam) where TSource: struct;
        public static int TestMethod(int intParam = 0);
        public static List<int?> TestMethodWithRefOut(List<int?> nullableIntParam, out int count, ref string name);
        public void TestVoidmethod();

        public class NestedParentClass { /*hidden*/ }
        public class TestNestedClass1 { /*hidden*/ }
    }
}", code);
        }

        //        [Fact]
        //        public void Reconstruct_AssemblyInfo()
        //        {
        //            var type = LoadType("TestClass1");
        //            var reflector = new Reflector(usedNamespaces);

        //            string[] code = reflector.Process(type.ParentAssembly, type, type.Members[2])
        //                                     .Code
        //                                     .GetLines();
        //            Assert.True(code.Length > 3);
        //            Assert.Equal(@"//", code[0]);
        //            Assert.True(code[1].StartsWith("// This file has been decompiled from "));
        //            Assert.True(code[1].EndsWith("CSScriptNpp.Test.dll"));
        //            Assert.Equal(@"//", code[2]);
        //        }

        [Fact]
        public void Reconstruct_Struct()
        {
            var type = LoadType<TestStruct1>();

            string code = type.Reconstruct(false);

            Assert.Equal(
@"using System;

namespace CSScriptIntellisense.Test
{
    public sealed struct TestStruct1
    {
        public int MyProperty { get; set; }
    }
}", code);
        }

        [Fact]
        public void Reconstruct_Delegate()
        {
            var type = LoadType("CSScriptIntellisense.Test.TestDel|gt<int>");

            string code = type.Reconstruct(false);

            Assert.Equal(
@"using System;

namespace CSScriptIntellisense.Test
{
    public delegate int? TestDelgt<T>(CustomIndex count, int? contextArg, T parent) where T: struct;
}", code);
        }

        [Fact]
        public void Reconstruct_Delegate2()
        {
            var type = LoadType("CSScriptIntellisense.Test.TestClass1.NestedParentClass.TestDelgt|3");

            string code = type.Reconstruct(false);

            Assert.Equal(
@"using System;

namespace CSScriptIntellisense.Test
{
    public class NestedParentClass
    {
        public class TestClass1
        {
            public delegate int TestDelgt3();
        }
    }
}", code);
        }

        [Fact]
        public void Reconstruct_NestedClassParent()
        {
            var type = LoadType("CSScriptIntellisense.Test.TestNestedGrandParentClass.TestNestedParentCla|ss");

            string code = type.Reconstruct(false);

            Assert.Equal(
@"using System;

namespace CSScriptIntellisense.Test
{
    public class TestNestedGrandParentClass
    {
        public class TestNestedParentClass
        {
            public string Name { get; set; }

            public class TestNestedChildClass { /*hidden*/ }
            public static class TestNestedChildStsticClass { /*hidden*/ }
            public sealed struct TestNestedStruct { /*hidden*/ }
        }
    }
}", code);
        }

        [Fact]
        public void Reconstruct_HidingDefaultConstructors()
        {
            var type = LoadType<TestBaseClassDefC>();

            string code = type.Reconstruct(false);

            Assert.Equal(
@"using System;

namespace CSScriptIntellisense.Test
{
    public class TestBaseClassDefC
    {
    }
}", code);
        }

        [Fact]
        public void Reconstruct_NestedGenericClassConstraints()
        {
            var type = LoadType("CSScriptIntellisense.Test.TestBaseGeneri|cClass3<string, List<int>>");

            string code = type.Reconstruct(false);

            Assert.Equal(
@"using System;

namespace CSScriptIntellisense.Test
{
    public class TestBaseGenericClass3<T, T2>
        where T: class, new()
        where T2: TestBaseClass, IEnumerable<int>, IList<int>
    {
    }
}", code);
        }

        [Fact]
        public void Reconstruct_FieldsClass()
        {
            var type = LoadType<TestFieldsClass>();

            string code = type.Reconstruct(false);

            Assert.Equal(
@"using System;

namespace CSScriptIntellisense.Test
{
    public class TestFieldsClass
    {
        public int Count;
        public int Count2;
        public static int Count3;
        public static const int Count4 = 44;
    }
}", code);
        }

        [Fact]
        public void Testbed()
        {
        }
    }
}