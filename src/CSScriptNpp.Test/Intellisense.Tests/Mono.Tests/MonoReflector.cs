using CSScriptIntellisense;
using ICSharpCode.NRefactory.Documentation;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Testing;
using UltraSharp.Cecil;
using Xunit;

//using Mono.CSharp;
namespace Testing
{
    public class MonoReflector
    {
        public MonoReflector()
        {
            //RoslynHost.Init();
        }

        static string[] usedNamespaces = new string[] { "Testing" };

        static ITypeDefinition LoadLocalType(string name)
        {
            var types = LocalTypes.ToArray();
            return types.Where(t => t.ReflectionName.EndsWith(name)).First();
        }

        public static ITypeDefinition LoadType<T>()
        {
            var types = LoadTypesOf<T>().ToArray();
            string name = typeof(T).FullName;
            return types.Where(t => t.ReflectionName == name).First();
        }

        static IEnumerable<ITypeDefinition> localTypes;

        static IEnumerable<ITypeDefinition> LocalTypes
        {
            get
            {
                if (localTypes == null)
                    localTypes = LoadTypes();
                return localTypes;
            }
        }

        static IEnumerable<ITypeDefinition> LoadTypes()
        {
            var tt = typeof(File).Assembly.Location;

            Reflector.OutputDir = Environment.CurrentDirectory;

            var asmFile = Assembly.GetExecutingAssembly().Location;
            //asmFile = "".GetType().Assembly.Location;

            string xmlFile = Path.GetFullPath(Path.GetFileNameWithoutExtension(asmFile) + ".xml");

            XmlDocumentationProvider doc = MonoCompletionEngine.GetXmlDocumentation(asmFile);
            if (doc == null && File.Exists(xmlFile))
                doc = new XmlDocumentationProvider(xmlFile);

            var loader = new CecilLoader { DocumentationProvider = doc };
            var unresolvedAsm = loader.LoadAssemblyFile(asmFile);
            var compilation = new SimpleCompilation(unresolvedAsm);
            var context = new SimpleTypeResolveContext(compilation);
            var asm = unresolvedAsm.Resolve(context);
            return asm.GetAllTypeDefinitions().ToArray();
        }

        public static IEnumerable<ITypeDefinition> LoadTypesOf<T>()
        {
            Reflector.OutputDir = Environment.CurrentDirectory;

            var asmFile = typeof(T).Assembly.Location;
            string xmlFile = Path.GetFullPath(Path.GetFileNameWithoutExtension(asmFile) + ".xml");

            XmlDocumentationProvider doc = MonoCompletionEngine.GetXmlDocumentation(asmFile);
            if (doc == null && File.Exists(xmlFile))
                doc = new XmlDocumentationProvider(xmlFile);

            var loader = new CecilLoader { DocumentationProvider = doc };
            var unresolvedAsm = loader.LoadAssemblyFile(asmFile);
            var compilation = new SimpleCompilation(unresolvedAsm);
            var context = new SimpleTypeResolveContext(compilation);
            var asm = unresolvedAsm.Resolve(context);
            return asm.GetAllTypeDefinitions().ToArray();
        }

        [Fact(Skip = "waiting for Syntaxer migration")]
        public void Reflector_Reconstruct_Enum()
        {
            ITypeDefinition enumType = LoadLocalType("TestEnum");
            ITypeDefinition nonEnumType = LoadLocalType("TestClass1");

            Assert.True(enumType.IsEnum());
            Assert.False(nonEnumType.IsEnum());

            string code = new Reflector().Process(enumType)
                                         .Code;

            Assert.Equal(code,
@"namespace CSScriptIntellisense.Test
{
    /// Test values
    public enum TestEnum
    {
        /// Value 1
        Val1 = 0,
        /// Value 2
        Val2 = 33,
        /// Value 3
        Val3 = 33,
        /// Value 4
        Val4 = 34,
    }
}");
        }

        [Fact(Skip = "waiting for Syntaxer migration")]
        public void Reflector_Reconstruct_GenericClass()
        {
            ITypeDefinition type = LoadLocalType("Test.GTestClass1`2");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public class GTestClass1<TSource, TDestination> where TSource: TestBaseClass, System.Collections.Generic.IEnumerable<int>, System.Collections.Generic.IList<int> : object
    {
        public int MyField;
        public System.ICloneable MyCLonabeField;
        public System.Collections.Generic.Dictionary<int, string> Map;
        public System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>> MapOfMaps;
        public int? Count { get; set; }
        public static int MyProperty { get; set; }
        public GTestClass1() {}
        public GTestClass1(System.Collections.Generic.IList<TSource> items) {}
        public void AddItems<T>(System.Collections.Generic.Dictionary<T, TSource> items) where T: class, new() {}
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_StringSplit()
        {
            var tt = Regex.Replace("public System.String[] Split(char[] separator , System.StringSplitOptions options)", @"\b(System.String)\b", "string");

            ITypeDefinition type = LoadType<string>();

            string code = new Reflector().Process(type)
                                         .Code;
        }

        [Fact]
        public void Reflector_Reconstruct_OperatorOverloads()
        {
            ITypeDefinition type = LoadLocalType("Test.OperatorsOveloadClass");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public class OperatorsOveloadClass : object
    {
        public static OperatorsOveloadClass operator +(OperatorsOveloadClass c1, OperatorsOveloadClass c2) {}
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_Indexers()
        {
            ITypeDefinition type = LoadLocalType("Test.TestArrayClass");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public class TestArrayClass : object
    {
        public int[] ArrayProp { get; set; }
        public int? this[int? index0, CustomIndex index1, string index2, int index3] { get; set; }
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_UnresolvedType()
        {
            ITypeDefinition type = LoadLocalType("Test.TestArrayClass");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public class TestArrayClass : object
    {
        public int[] ArrayProp { get; set; }
        public int? this[int? index0, CustomIndex index1, string index2, int index3] { get; set; }
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_ExtensionMethods()
        {
            ITypeDefinition type = LoadLocalType("Test.ExtensionsClass");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public static class ExtensionsClass : object
    {
        public static bool IsEmpty(this string obj) {}
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_StaticClass()
        {
            ITypeDefinition type = LoadLocalType("Test.TestStaticClass");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public static class TestStaticClass : object
    {
        public static int MyProperty { get; set; }
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_DoesNotPickInheritedMembers()
        {
            ITypeDefinition type = LoadType<FileInfo>();

            string code = new Reflector().Process(type)
                                         .Code;

            Assert.DoesNotContain(code, "public abstract string Name { get; }");
        }

        [Fact]
        public void Reflector_Reconstruct_AbstractClass()
        {
            ITypeDefinition type = LoadLocalType("Test.TestAbstractClass");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public abstract class TestAbstractClass : object
    {
        public abstract int MyProperty { get; set; }
        public abstract void MyMethod() {}
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_ParamArrayClass()
        {
            ITypeDefinition type = LoadLocalType("Test.TestParamArrayClass");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public class TestParamArrayClass : object
    {
        public TestClass1[,] ArrayProp { get; set; }
        public string[,,] Test(int[,] arg, params string[] names) {}
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_Interface()
        {
            ITypeDefinition type = LoadLocalType("Test.TestInterface");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public interface TestInterface
    {
        int MyProperty { get; set; }
        event System.Action OnLoad;
    }
}", code);
        }

        [Fact(Skip = "waiting for Syntaxer migration")]
        public void Reflector_Reconstruct_ComplexDocumentation()
        {
            ITypeDefinition type = LoadLocalType("TestApiDocClass");
            var reflector = new Reflector(usedNamespaces);

            string code = reflector.Process(type)
                                   .Code;

            Assert.Equal(@"namespace CSScriptIntellisense.Test
{
    public class TestApiDocClass : object
    {
        /// This is the value of the UpgradeCode attribute of the Wix Product element.
        /// Both WiX and MSI consider this element as optional even it is the only available identifier for defining relationship between different versions of the same product. Wix# in contrary enforces that value to allow any future updates of the product being installed.
        ///  If user doesn't specify this value Wix# engine will use !:Project.GUID as UpgradeCode.
        public System.Guid? UpgradeCode;
        /// Generic WixSharp.WixEntity container for defining WiX Package element attributes.
        /// These attributes are the properties about the package to be placed in the Summary Information Stream. These are visible from COM through the IStream interface, and these properties can be seen on the package in Explorer.
        /// The following is an example of defining the Package attributes.
        ///
        ///              var project =
        ///                  new Project(""My Product"",
        ///                      new Dir(@""%ProgramFiles%\My Company\My Product"",
        ///
        ///                  ...
        ///
        ///              project.Package.AttributesDefinition = @""AdminImage=Yes;
        ///                                                       Comments=Release Candidate;
        ///                                                       Description=Fantastic product..."";
        ///
        ///              Compiler.BuildMsi(project);
        public void Test() {}
    }
}", code);
        }

        [Fact(Skip = "waiting for Syntaxer migration")]
        public void Reflector_Reconstruct_Class()
        {
            ITypeDefinition type = LoadLocalType("TestClass1");
            var reflector = new Reflector(usedNamespaces);

            string code = reflector.Process(type)
                                   .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    /// Simple class for testing Reflector
    public class TestClass1 : System.Collections.Generic.List<System.Collections.Generic.List<TestClass1>>, IEmptyInterface1, IEmptyInterface2
    {
        public partial class TestNestedClass1 { }
        public const int MyFieldConst = 77;
        public static int MyFieldStat;
        public int MyField;
        public const string MyFieldName = ""test"";
        /// Gets or sets my property.
        /// --------------------------
        /// My property.
        public int MyProperty { get; set; }
        public int PropRW { get; set; }
        public static int PropR { get; }
        public virtual int MyVirtualProperty { }
        public event System.Action OnLoad;
        public static event System.Action<int> OnLoadStatic;
        public TestClass1() {}
        public TestClass1(int count) {}
        public static System.Collections.Generic.List<int?> TestMethodWithRefOut(System.Collections.Generic.List<int?> nullableIntParam, out int count, ref string name) {}
        public void TestVoidmethod() {}
        public static int TestMethod(int intParam = 0) {}
        public static System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<TSource?, TDestination>> TestGenericMethod<TSource, TDestination>(System.Collections.Generic.IEnumerable<TSource> intParam) where TSource: struct {}
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_AssemblyInfo()
        {
            ITypeDefinition type = LoadLocalType("TestClass1");
            var reflector = new Reflector(usedNamespaces);

            string[] code = reflector.Process(type.ParentAssembly, type, type.Members[2])
                                     .Code
                                     .GetLines();
            Assert.True(code.Length > 3);
            Assert.Equal(@"//", code[0]);
            Assert.True(code[1].StartsWith("// This file has been decompiled from "));
            Assert.True(code[1].EndsWith("CSScriptNpp.Test.dll"));
            Assert.Equal(@"//", code[2]);
        }

        [Fact(Skip = "waiting for Syntaxer migration")]
        public void Reflector_Reconstruct_WithLookupPositions()
        {
            ITypeDefinition type = LoadLocalType("TestClass1");
            var reflector = new Reflector(usedNamespaces);

            Func<string, Reflector.Result> ReconstructAndLookup = (name) =>
                    reflector.Process(null, type, type.Members.Where(m => m.Name == name).FirstOrDefault());

            string code = ReconstructAndLookup("").Code;
            int pos = ReconstructAndLookup("PropRW").MemberPosition;

            Assert.Equal(400, ReconstructAndLookup("MyFieldName").MemberPosition);
            Assert.Equal(84, ReconstructAndLookup("").MemberPosition);
            Assert.Equal(601, ReconstructAndLookup("PropRW").MemberPosition);
            Assert.Equal(1086, ReconstructAndLookup("TestVoidmethod").MemberPosition);
            Assert.Equal(734, ReconstructAndLookup("OnLoad").MemberPosition);
        }

        [Fact]
        public void Reflector_Reconstruct_WithLookupPositionsEnums()
        {
            ITypeDefinition type = LoadLocalType("TestEnum");
            var reflector = new Reflector(usedNamespaces);

            Func<string, Reflector.Result> ReconstructAndLookup = (name) =>
                    reflector.Process(null, type, type.Members.Where(m => m.Name == name).FirstOrDefault());

            var result = ReconstructAndLookup("");

            Assert.Equal(61, ReconstructAndLookup("").MemberPosition);
            Assert.Equal(115, ReconstructAndLookup("Val1").MemberPosition);
        }

        [Fact]
        public void Reflector_Reconstruct_WithLookupPositionsNestedType()
        {
            ITypeDefinition type = LoadLocalType("TestNestedClass1");
            var reflector = new Reflector(usedNamespaces);

            Func<string, Reflector.Result> ReconstructAndLookup = (name) =>
                    reflector.Process(null, type, type.Members.Where(m => m.Name == name).FirstOrDefault());

            Assert.Equal(84, ReconstructAndLookup("").MemberPosition);
        }

        [Fact]
        public void Reflector_Reconstruct_Struct()
        {
            ITypeDefinition type = LoadLocalType("TestStruct1");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public sealed struct TestStruct1 : System.ValueType
    {
        public int MyProperty { get; set; }
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_Delegate()
        {
            ITypeDefinition type = LoadLocalType("TestDelgt`1");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public int? TestDelgt<T>(CustomIndex count, int? contextArg, T parent) where T: struct;
}", code);
        }

        [Fact(Skip = "waiting for Syntaxer migration")]
        public void Reflector_Reconstruct_Delegate2()
        {
            ITypeDefinition type = LoadLocalType("TestDelgt3");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public partial class TestClass1
    {
        public int TestDelgt3();
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_NestedClassParent()
        {
            ITypeDefinition type = LoadLocalType("TestNestedParentClass");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public partial class TestNestedGrandParentClass
    {
        public class TestNestedParentClass : object
        {
            public static partial class TestNestedChildStsticClass { }
            public partial class TestNestedChildClass { }
            public partial struct TestNestedStruct { }
            public string Name { get; set; }
        }
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_HidingDefaultConstructors()
        {
            ITypeDefinition type = LoadLocalType("TestBaseClassDefC");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public class TestBaseClassDefC : object
    {
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_NestedGenericClassParent()
        {
            ITypeDefinition type = LoadLocalType("TestNestedParentGenericClass`1");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public partial class TestNestedGrandParentGenericClass
    {
        public class TestNestedParentGenericClass<T0, T1> : object
        {
            public partial class TestNestedChildStsticClass { }
            public partial class TestNestedChildGenericClass { }
            public partial struct TestNestedStruct { }
            public string Name { get; set; }
        }
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_NestedGenericClassConstraints()
        {
            ITypeDefinition type = LoadLocalType("TestBaseGenericClass3`2");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public class TestBaseGenericClass3<T, T2> where T: class, new() where T2: TestBaseClass, System.Collections.Generic.IEnumerable<int>, System.Collections.Generic.IList<int> : object
    {
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_NestedGenericClass()
        {
            ITypeDefinition type = LoadLocalType("TestNestedChildGenericClass`1");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public partial class TestNestedGrandParentGenericClass
    {
        public partial class TestNestedParentGenericClass
        {
            public class TestNestedChildGenericClass<T0, T1, T2> : object
            {
                public T0 T0_Prop { get; set; }
                public T1 T1_Prop { get; set; }
                public T2 T2_Prop { get; set; }
                public int[] ArrayProp { get; set; }
            }
        }
    }
}", code);
        }

        [Fact]
        public void Reflector_Reconstruct_SyetsTaskClass()
        {
            ITypeDefinition type = LoadType<System.Threading.Tasks.Task>();

            string code = new Reflector().Process(type)
                                         .Code;
        }

        [Fact]
        public void Reflector_Reconstruct_FieldsClass()
        {
            ITypeDefinition type = LoadLocalType("TestFieldsClass");

            string code = new Reflector().Process(type)
                                         .Code;
            Assert.Equal(
@"namespace CSScriptIntellisense.Test
{
    public class TestFieldsClass : object
    {
        public int Count;
        public int Count2;
        public static int Count3;
        public const int Count4 = 44;
    }
}", code);
        }

        [Fact]
        public void ResolveMember()
        {
            SimpleCodeCompletion.ResetProject();

            //str|ing( => 85
            //Tes|t(   => 116
            //Con|sole => 132
            //Console.Write|Line => 142
            //Test|Cls => 172
            //int.Ma|xValue => 202
            //Even|tHandler => 223
            //System.IO.File.De|lete => 267

            var result = SimpleCodeCompletion.ResolveMember(@"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        Test();
        Console.WriteLine(""test"");
        TestCls t;
        int m = int.MaxValue;
        EventHandler handler;
        System.IO.File.Delete(""file"");
        System.Threading.Tasks.Task.Run(()=>Thread.Sleep(1000));
    }

    static void Test()
    {
    }
}

class TestCls<T>
{
}", 267, "test.cs");
            Assert.True(result.FileName.EndsWith(".cs"));
            Assert.Equal(240, result.BeginLine);
            Assert.Equal(240, result.EndLine);
        }

        [Fact(Skip = "waiting for Syntaxer migration")]
        public void ResolveConstructor()
        {
            var tt = AppDomain.CurrentDomain.BaseDirectory;
            tt = Environment.CurrentDirectory;
            tt = Assembly.GetExecutingAssembly().Location;

            SimpleCodeCompletion.ResetProject();

            //Te|st(   => 127

            var result = SimpleCodeCompletion.ResolveMember(@"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        var t = new Test();
        Console.WriteLine(""test"");
        System.IO.File.Delete(""file"");
    }
}

class Test
{
    //public Test(int count){}
}", 127, "test.cs");

            Assert.True(result.FileName.EndsWith(".cs"));
            Assert.Equal(15, result.BeginLine);
            Assert.Equal(18, result.EndLine);
        }

        [Fact]
        public void Testbed()
        {
            SimpleCodeCompletion.ResetProject();

            //str|ing( => 85
            //Tes|t(   => 116
            //Con|sole => 132
            //Console.Write|Line => 142
            //Test|Cls => 172
            //int.Ma|xValue => 202
            //Even|tHandler => 223
            //System.IO.File.De|lete => 267

            var result = SimpleCodeCompletion.ResolveMember(@"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        Test();
        Console.WriteLine(""test"");
        TestCls t;
        int m = int.MaxValue;
        EventHandler handler;
        System.IO.File.Delete(""file"")
    }

    static void Test()
    {
    }
}

class TestCls<T>
{
}", 85, "test.cs");
        }
    }
}