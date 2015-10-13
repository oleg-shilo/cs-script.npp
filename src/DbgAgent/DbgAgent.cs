using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace DbgAgent
{
    public class ObjectInspector
    {
        ArrayList stack = new ArrayList();

        public void StackAdd_I1(SByte arg) { stack.Add(arg); }

        public void StackAdd_U1(Byte arg) { stack.Add(arg); }

        public void StackAdd_I2(Int16 arg) { stack.Add(arg); }

        public void StackAdd_I4(Int32 arg) { stack.Add(arg); }

        public void StackAdd_I8(Int64 arg) { stack.Add(arg); }

        public void StackAdd_U2(UInt16 arg) { stack.Add(arg); }

        public void StackAdd_U4(UInt32 arg) { stack.Add(arg); }

        public void StackAdd_U8(UInt64 arg) { stack.Add(arg); }

        public void StackAdd_R4(Single arg) { stack.Add(arg); }

        public void StackAdd_R8(Double arg) { stack.Add(arg); }

        public void StackAdd_BOOLEAN(Boolean arg) { stack.Add(arg); }

        public void StackAdd_CHAR(Char arg) { stack.Add(arg); }

        public void StackAdd_OBJECT(Object arg) { stack.Add(arg); }

        public object Set(string memberFullName)
        {
            return Invoke(memberFullName, false);
        }

        public object SetStatic(string memberFullName)
        {
            return Invoke(memberFullName, true);
        }

        object Set(string methodFullName, bool isStatic)
        {
            string[] parts = methodFullName.Split('.');

            string memberName = parts.Last();
            object instance = null;
            Type type = null;

            object[] args;
            if (isStatic)
            {
                instance = null;
                string typeName = string.Join(".", parts.Reverse().Skip(1).Reverse().ToArray());
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetType(typeName);
                    if (type != null)
                        break;
                }
            }
            else
            {
                instance = stack[0];
                stack.RemoveAt(0);
                type = instance.GetType();
            }
            args = stack.ToArray();

            if (type == null)
                throw new Exception("Cannot evaluate " + methodFullName);

            return "ttt";

            MethodInfo method = null;
            foreach (var m in type.GetMethods())
            {
                int i = 0;
                bool missmatch = false;
                var methodArgs = m.GetParameters();

                if (m.Name == memberName && methodArgs.Length == args.Length)
                {
                    foreach (ParameterInfo p in m.GetParameters())
                    {
                        if (p.ParameterType != args[i++].GetType())
                        {
                            missmatch = true;
                            break;
                        }
                    }
                    if (!missmatch)
                        method = m;
                }
            }

            if (method == null)
                throw new Exception("Cannot find method " + methodFullName);

            object result = method.Invoke(instance, args);
            return result;
        }

        public object Invoke(string methodFullName)
        {
            return Invoke(methodFullName, false);
        }

        public object InvokeStatic(string methodFullName)
        {
            return Invoke(methodFullName, true);
        }

        object Invoke(string methodFullName, bool isStatic)
        {
            string[] parts = methodFullName.Split('.');

            string methodName = parts.Last();
            object instance = null;
            Type type = null;

            object[] args;
            if (isStatic)
            {
                instance = null;
                string typeName = string.Join(".", parts.Reverse().Skip(1).Reverse().ToArray());
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetType(typeName);
                    if (type != null)
                        break;
                }
            }
            else
            {
                instance = stack[0];
                stack.RemoveAt(0);
                type = instance.GetType();
            }
            args = stack.ToArray();

            if (type == null)
                throw new Exception("Cannot evaluate " + methodFullName);

            MethodInfo method = null;
            foreach (var m in type.GetMethods())
            {
                int i = 0;
                bool missmatch = false;
                var methodArgs = m.GetParameters();

                if (m.Name == methodName && methodArgs.Length == args.Length)
                {
                    foreach (ParameterInfo p in m.GetParameters())
                    {
                        if (p.ParameterType != args[i++].GetType())
                        {
                            missmatch = true;
                            break;
                        }
                    }
                    if (!missmatch)
                        method = m;
                }
            }

            if (method == null)
                throw new Exception("Cannot find method " + methodFullName);

            object result = method.Invoke(instance, args);
            return result;
        }

        public static ObjectInspector New()
        {
            return new ObjectInspector();
        }

        ///////////////////////////

        public object Test_Get_I1() { return SByte.MaxValue; }
        public object Test_Get_U1() { return Byte.MaxValue; }
        public object Test_Get_I2() { return Int16.MaxValue; }
        public object Test_Get_I4() { return Int32.MaxValue; }
        public object Test_Get_I8() { return Int64.MaxValue; }
        public object Test_Get_U2() { return UInt16.MaxValue; }
        public object Test_Get_U4() { return UInt32.MaxValue; }
        public object Test_Get_U8() { return UInt64.MaxValue; }
        public object Test_Get_R4() { return Single.MaxValue; }
        public object Test_Get_R8() { return Double.MaxValue; }
        public object Test_Get_BOOLEAN() { return true; }
        public object Test_Get_CHAR() { return Char.MaxValue; }


        public object TestPrimitives(SByte a1, Byte a2, Int16 a3, Int32 a4, Int64 a5, UInt16 a6, UInt32 a7, UInt64 a8, Single a9, Double a10, bool a11, Char a12)
        {
            if (a1 == SByte.MaxValue &&
                a2 == Byte.MaxValue &&
                a3 == Int16.MaxValue &&
                a4 == Int32.MaxValue &&
                a5 == Int64.MaxValue &&
                a6 == UInt16.MaxValue &&
                a7 == UInt32.MaxValue &&
                a8 == UInt64.MaxValue &&
                a9 == Single.MaxValue &&
                a10 == Double.MaxValue &&
                a11 == true &&
                a12 == Char.MaxValue)
                return "success";
            else
                return "error";
        }

        public object TestPrimitives1()//SByte a, Byte b, Int16 c, Int32 i444)//, Int64 i123, UInt16 i2, UInt32 i4, UInt64 u2, Single s, Double d, bool bu, Char ch)
        {
            return "success";
        }
    }
}
