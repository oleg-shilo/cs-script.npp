using DbgAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

#pragma warning disable 414

namespace CSScriptIntellisense.Test
{
    public class DbgAgentTests
    {
        static string testObjTypeName = typeof(TestObject).FullName;
        static string testObjSubTypeName = typeof(TestObjectSub).FullName;

        class TestObject
        {
            public string Name = "nm";
            int privateIndex = 0;
            static int privateIndexStat = 0;
            int INDEX { get; set; }
            static int INDEXSTAT = 0;
            public string NAME { get; set; }
            public int ReadOnlyName { get; private set; }
        }

        class TestObjectSub : TestObject
        {
        }

        [Fact]
        public void CanSetPublicField()
        {
            var obj = new TestObject();
            var dbg = new ObjectInspector();

            dbg.StackAdd_OBJECT(obj);
            dbg.StackAdd_OBJECT("new name");
            object result = dbg.Set(testObjTypeName + ".Name");

            Assert.Equal("new name", result);
        }

        [Fact]
        public void CanSetNestedPublicField()
        {
            var obj = new TestObjectSub();
            var dbg = new ObjectInspector();

            dbg.StackAdd_OBJECT(obj);
            dbg.StackAdd_OBJECT("new name");
            object result = dbg.Set(testObjSubTypeName + ".Name");

            Assert.Equal("new name", result);
        }

        [Fact]
        public void CanSetPrivateField()
        {
            
            var obj = new TestObject();
            var dbg = new ObjectInspector();

            dbg.StackAdd_OBJECT(obj);
            dbg.StackAdd_OBJECT(777);
            object result = dbg.Set(testObjTypeName + ".privateIndex");

            Assert.Equal(777, result);
        }

        [Fact]
        public void CanSetStaticPrivateField()
        {
            var dbg = new ObjectInspector();

            dbg.StackAdd_OBJECT(333);
            object result = dbg.SetStatic(testObjTypeName+".privateIndexStat");

            Assert.Equal(333, result);
        }

        [Fact]
        public void CanSetNestedStaticPrivateField()
        {
            var dbg = new ObjectInspector();

            dbg.StackAdd_OBJECT(333);
            object result = dbg.SetStatic(testObjSubTypeName + ".privateIndexStat");

            Assert.Equal(333, result);
        }

        //---------------------------
        [Fact]
        public void CanSetPublicProp()
        {
            var obj = new TestObject();
            var dbg = new ObjectInspector();

            dbg.StackAdd_OBJECT(obj);
            dbg.StackAdd_OBJECT("new name");
            object result = dbg.Set(testObjTypeName + ".NAME");

            Assert.Equal("new name", result);
        }

        [Fact]
        public void CanSetNestedPublicProp()
        {
            var obj = new TestObjectSub();
            var dbg = new ObjectInspector();

            dbg.StackAdd_OBJECT(obj);
            dbg.StackAdd_OBJECT("new name");
            object result = dbg.Set(testObjSubTypeName + ".NAME");

            Assert.Equal("new name", result);
        }

        [Fact]
        public void CanSetPrivateProp()
        {
            var obj = new TestObject();
            var dbg = new ObjectInspector();

            dbg.StackAdd_OBJECT(obj);
            dbg.StackAdd_OBJECT(777);
            object result = dbg.Set(testObjTypeName + ".INDEX");

            Assert.Equal(777, result);
        }

        [Fact]
        public void CanSetStaticPrivateProp()
        {
            var dbg = new ObjectInspector();

            dbg.StackAdd_OBJECT(333);
            object result = dbg.SetStatic(testObjTypeName + ".INDEXSTAT");

            Assert.Equal(333, result);
        }

        [Fact]
        public void CanSetNestedStaticPrivateProp()
        {
            var dbg = new ObjectInspector();

            dbg.StackAdd_OBJECT(333);
            object result = dbg.SetStatic(testObjSubTypeName + ".INDEXSTAT");

            Assert.Equal(333, result);
        }

        [Fact]
        public void CanHandleSettingReadOnlyProp()
        {
            var dbg = new ObjectInspector();

            dbg.StackAdd_OBJECT(333);
            object result = dbg.SetStatic(testObjTypeName + ".ReadOnlyName");

            Assert.Equal("<error>", result);
        }
    }
}