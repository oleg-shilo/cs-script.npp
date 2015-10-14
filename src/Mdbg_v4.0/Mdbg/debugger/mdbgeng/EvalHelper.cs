using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using Microsoft.Samples.Debugging.MdbgEngine;

//CSScript.Npp related functionality
namespace CSScriptNpp
{
    public class RemoteInspector
    {
        static bool isAgentInjected = false;
        static public string AssemblyFile = Environment.GetEnvironmentVariable("CSSNPP_DBG_AGENTASSEMBLY");
        EvalHelper eval;
        CorValue obj;
        List<CorValue> args = new List<CorValue>();

        public RemoteInspector(EvalHelper eval)
        {
            this.eval = eval;
            if (!isAgentInjected)
            {
                isAgentInjected = true;
                eval.Invoke("System.Reflection.Assembly.LoadFrom", null, eval.CreateString(AssemblyFile));
            }
            obj = eval.Invoke("DbgAgent.ObjectInspector.New", null);
        }

        public RemoteInspector AddArg(CorValue arg)
        {
            args.Add(arg);
            return this;
        }

        void Push(CorValue arg)
        {
            if (arg == null)
                eval.Invoke("DbgAgent.ObjectInspector.StackAddNull", obj);
            else
            {
                string methodName = "DbgAgent.ObjectInspector.StackAdd_";
                switch (arg.Type)
                {
                    case CorElementType.ELEMENT_TYPE_R8:
                        methodName += "R8";
                        break;
                    case CorElementType.ELEMENT_TYPE_R4:
                        methodName += "R4";
                        break;
                    case CorElementType.ELEMENT_TYPE_U8:
                        methodName += "U8";
                        break;
                    case CorElementType.ELEMENT_TYPE_I8:
                        methodName += "I8";
                        break;
                    case CorElementType.ELEMENT_TYPE_U4:
                        methodName += "U4";
                        break;
                    case CorElementType.ELEMENT_TYPE_I4:
                        methodName += "I4";
                        break;
                    case CorElementType.ELEMENT_TYPE_U2:
                        methodName += "U2";
                        break;
                    case CorElementType.ELEMENT_TYPE_I2:
                        methodName += "I2";
                        break;
                    case CorElementType.ELEMENT_TYPE_U1:
                        methodName += "U1";
                        break;
                    case CorElementType.ELEMENT_TYPE_I1:
                        methodName += "I1";
                        break;
                    case CorElementType.ELEMENT_TYPE_CHAR:
                        methodName += "I8";
                        break;
                    case CorElementType.ELEMENT_TYPE_BOOLEAN:
                        methodName += "BOOLEAN";
                        break;
                    default:
                        methodName += "OBJECT";
                        break;
                }

                eval.Invoke(methodName, obj, arg);
            }
        }

        public CorValue Inspect()
        {
            foreach (CorValue arg in args)
                Push(arg);
            args.Clear();
            return eval.Invoke("DbgAgent.ObjectInspector.Inspect", obj);
        }

        public CorValue Invoke(string methodName)
        {
            foreach (CorValue arg in args)
                Push(arg);
            args.Clear();
            return eval.Invoke("DbgAgent.ObjectInspector.InvokeStatic", obj, eval.CreateString(methodName));
        }

        public CorValue Invoke(string methodName, CorValue instance)
        {
            Push(instance);
            foreach (CorValue arg in args)
                Push(arg);
            args.Clear();
            return eval.Invoke("DbgAgent.ObjectInspector.Invoke", obj, eval.CreateString(methodName));
        }

        public CorValue Set(string memberName)
        {
            foreach (CorValue arg in args)
                Push(arg);
            args.Clear();
            return eval.Invoke("DbgAgent.ObjectInspector.SetStatic", obj, eval.CreateString(memberName));
        }

        public CorValue Set(string memberName, CorValue instance)
        {
            Push(instance);
            foreach (CorValue arg in args)
                Push(arg);
            args.Clear();
            return eval.Invoke("DbgAgent.ObjectInspector.Set", obj, eval.CreateString(memberName));
        }


        public CorValue TestPrimitives()
        {
            this.AddArg(eval.Invoke("DbgAgent.ObjectInspector.Test_Get_I1", obj));
            this.AddArg(eval.Invoke("DbgAgent.ObjectInspector.Test_Get_U1", obj));
            this.AddArg(eval.Invoke("DbgAgent.ObjectInspector.Test_Get_I2", obj));
            this.AddArg(eval.Invoke("DbgAgent.ObjectInspector.Test_Get_I4", obj));
            this.AddArg(eval.Invoke("DbgAgent.ObjectInspector.Test_Get_I8", obj));
            this.AddArg(eval.Invoke("DbgAgent.ObjectInspector.Test_Get_U2", obj));
            this.AddArg(eval.Invoke("DbgAgent.ObjectInspector.Test_Get_U4", obj));
            this.AddArg(eval.Invoke("DbgAgent.ObjectInspector.Test_Get_U8", obj));
            this.AddArg(eval.Invoke("DbgAgent.ObjectInspector.Test_Get_R4", obj));
            this.AddArg(eval.Invoke("DbgAgent.ObjectInspector.Test_Get_R8", obj));
            this.AddArg(eval.Invoke("DbgAgent.ObjectInspector.Test_Get_BOOLEAN", obj));
            this.AddArg(eval.Invoke("DbgAgent.ObjectInspector.Test_Get_CHAR", obj));

            return Invoke("TestPrimitives", obj);
        }
    }

    public class EvalHelper
    {
        MDbgProcess process;
        CorEval eval;

        public RemoteInspector CreateInspector()
        {
            return new RemoteInspector(this);
        }

        public EvalHelper(MDbgProcess process)
        {
            this.process = process;
            eval = process.Threads.Active.CorThread.CreateEval();
        }

        public CorValue ParseValue(string name)
        {
            return process.m_engine.ParseExpression(name, process, process.Threads.Active.CurrentFrame);
        }

        public CorValue CreateString(string value)
        {
            CorEval eval = process.Threads.Active.CorThread.CreateEval();
            eval.NewString(value);
            return GetResult();

            //return ParseValue("\"" + value + "\"");
        }

        public CorValue Invoke(string methodName, CorValue instance, params CorValue[] args)
        {
            var allArgs = new List<CorValue>();
            if (instance != null)
                allArgs.Add(instance);
            allArgs.AddRange(args);

            MDbgFunction func = process.ResolveFunctionNameFromScope(methodName);
            if (null == func)
                throw new Exception(String.Format(CultureInfo.InvariantCulture, "Could not resolve {0}", new Object[] { methodName }));

            CorEval eval = process.Threads.Active.CorThread.CreateEval();

            // Get Variables
            ArrayList vars = new ArrayList();

            String arg;
            foreach (var v in allArgs)
            {
                if (v is CorGenericValue)
                {
                    vars.Add(v as CorValue);
                }
                else
                {
                    CorHeapValue hv = v.CastToHeapValue();
                    if (hv != null)
                    {
                        // we cannot pass directly heap values, we need to pass reference to heap valus
                        CorReferenceValue myref = eval.CreateValue(CorElementType.ELEMENT_TYPE_CLASS, null).CastToReferenceValue();
                        myref.Value = hv.Address;
                        vars.Add(myref);
                    }
                    else
                    {
                        vars.Add(v);
                    }
                }
            }

            eval.CallFunction(func.CorFunction, (CorValue[])vars.ToArray(typeof(CorValue)));
            return GetResult();
        }

        public class ExpressionParsingResult
        {
            public string Member;
            public CorValue[] Arguments;
            public CorValue Instance;
            public bool IsLocalVariable;
            public bool IsSetter;
            public string Expression;
            public string ExpressionArgs;
            public string ExpressionValue;
        }



        public ExpressionParsingResult ParseExpression(string expression)
        {
            var result = new ExpressionParsingResult();

            result.IsSetter = expression.Contains("=");
            result.Expression = expression;

            int bracketIndex = expression.IndexOfAny(new[] { '(', '=' });
            string methodName = expression.Substring(0, bracketIndex).Trim();
            string args = expression.Substring(bracketIndex).Replace("(", "").Replace(")", "").Replace("=", "").Trim();

            string[] methodParts = methodName.Split('.');

            if (methodParts.Length == 1) //myVar=3
                result.IsLocalVariable = true;

            result.ExpressionValue = args;


            string reference;

            if (methodParts.Length == 1)
            {
                //varName
                reference = methodParts[0];
            }
            else
            {
                //<<TypeName>|<CodeReference>>.<MethodName>
                reference = string.Join(".", methodParts.Take(methodParts.Length - 1).ToArray());

            }
            try
            {
                var instance = process.ResolveVariable(reference, process.Threads.Active.CurrentFrame);
                if (instance != null)
                {
                    result.Instance = instance.CorValue;
                    result.Member = methodParts.Last();

                }
            }
            catch { }

            if (result.Instance == null)
            {
                //MDbgFunction func = this.ResolveFunctionNameFromScope(methodName);
                //if (null == func)
                //    throw new Exception(String.Format(CultureInfo.InvariantCulture, "Could not resolve {0}", new Object[] { methodName }));

                result.Member = methodName;

            }

            CorEval eval = process.Threads.Active.CorThread.CreateEval();

            // Get Variables
            ArrayList vars = new ArrayList();
            String arg;
            if (args.Length != 0)
                foreach (var item in args.Split(','))
                {
                    arg = item.Trim();

                    CorValue v = process.m_engine.ParseExpression(arg, process, process.Threads.Active.CurrentFrame);

                    if (v == null)
                    {
                        throw new Exception("Cannot resolve expression or variable " + arg);
                    }

                    if (v is CorGenericValue)
                    {
                        vars.Add(v as CorValue);
                    }

                    else
                    {
                        CorHeapValue hv = v.CastToHeapValue();
                        if (hv != null)
                        {
                            // we cannot pass directly heap values, we need to pass reference to heap valus
                            CorReferenceValue myref = eval.CreateValue(CorElementType.ELEMENT_TYPE_CLASS, null).CastToReferenceValue();
                            myref.Value = hv.Address;
                            vars.Add(myref);
                        }
                        else
                        {
                            vars.Add(v);
                        }
                    }

                }

            result.Arguments = (CorValue[])vars.ToArray(typeof(CorValue));
            return result;
        }

        public CorValue TypeMemberExpressionInvoke(ExpressionParsingResult info)
        {
            var inspector = CreateInspector();
            foreach (CorValue arg in info.Arguments)
                inspector.AddArg(arg);

            CorValue result;
            if (info.IsSetter)
            {
                if (info.Instance != null)
                    result = inspector.Set(info.Member, info.Instance);
                else
                    result = inspector.Set(info.Member);
            }
            else
            {
                if (info.Instance != null)
                    result = inspector.Invoke(info.Member, info.Instance);
                else
                    result = inspector.Invoke(info.Member);
            }
            return result;
        }

        public CorValue VariableExpressionInvoke(ExpressionParsingResult info)
        {
            //based on MdbgCommands.SetCmd(string arguments)
            if (info.Instance == null)
                throw new Exception("Cannot resolve variable ");

            if (info.Arguments == null || !info.Arguments.Any())
                throw new Exception("Cannot resolve arguments ");

            // Arguments has to be in the form of variable=varName, variable=value or variable=(<type>)value, 
            // where we use the ldasm naming convention (e.g. "int", "sbyte", "ushort", etc...) for <type>.
            // Example inputs: var=myInt, var=45, var=(long)45

            MDbgValue lsMVar = new MDbgValue(process, info.Instance);
            CorValue val = info.Arguments.First();

            CorGenericValue valGeneric = val as CorGenericValue;
            bool bIsReferenceValue = val is CorReferenceValue;

            if (lsMVar != null)
            {
                if (valGeneric != null)
                {
                    CorValue lsVar = lsMVar.CorValue;
                    if (lsVar == null)
                        throw new Exception("cannot set constant values to unavailable variables");

                    // val is a primitive value                    
                    CorGenericValue lsGenVal = lsVar.CastToGenericValue();
                    if (lsGenVal == null)
                        throw new Exception("cannot set constant values to non-primitive values");

                    try
                    {
                        // We want to allow some type coercion. Eg, casting between integer precisions.
                        lsMVar.Value = val; // This may do type coercion
                    }
                    catch (MDbgValueWrongTypeException)
                    {
                        throw new Exception(String.Format("Type mismatch. Can't convert from {0} to {1}", val.Type, lsGenVal.Type));
                    }
                }
                else if (bIsReferenceValue)
                {
                    //reget variable
                    lsMVar = process.ResolveVariable(info.Member, process.Threads.Active.CurrentFrame);
                    lsMVar.Value = val;
                }
                else
                {
                    if (val.CastToHeapValue() != null)
                    {
                        throw new Exception("Heap values should be assigned only to debugger variables");
                    }
                    if (val.CastToGenericValue() != null)
                    {
                        lsMVar.Value = val.CastToGenericValue();
                    }
                    else
                    {
                        lsMVar.Value = val.CastToReferenceValue();
                    }
                }
            }

            // as a last thing we do is to return new value of the variable
            lsMVar = process.ResolveVariable(info.Member, process.Threads.Active.CurrentFrame);
            return lsMVar.CorValue;
        }

        CorValue GetResult()
        {
            process.Go().WaitOne();

            // now display result of the funceval
            if (process.StopReason is EvalCompleteStopReason)
            {
                eval = (process.StopReason as EvalCompleteStopReason).Eval;
                Debug.Assert(eval != null);

                return eval.Result;
            }
            return null;
        }
    }
}

//public CorValue BoxCorValue(CorValue value)
//{
//    //Value.cs - zoom-decompiler
//    // Box value type
//    byte[] rawValue = value.CastToGenericValue().GetRawValue();
//    // The type must not be a primive type (always true in current design)
//    //ICorDebugReferenceValue corRefValue = eval.NewObjectNoConstructor(this.Type).CorReferenceValue;
//    ICorDebugReferenceValue corRefValue = eval.NewObjectNoConstructor(process.ResolveClass("System.Int32")).CorReferenceValue;
//    // Make the reference to box permanent
//    //corRefValue = ((ICorDebugHeapValue2)corRefValue.Dereference()).CreateHandle(CorDebugHandleType.HANDLE_STRONG);
//    //// Create new value
//    //Value newValue = new Value(appDomain, corRefValue);
//    //// Copy the data inside the box
//    //newValue.CorGenericValue.SetRawValue(rawValue);
//    return newValue;
//}


//https://groups.google.com/forum/#!topic/microsoft.public.dotnet.framework.clr/fsVRhRIHBkQ
/*Use ICorDebugEval::NewObjectNoConstructor to create a new Int32. (The 
function has func-eval semantics, so you'll have to continue the process 
and get the result from an EvalComplete callback.)
On the resulting ICorDebugBoxValue, call GetObject to get the 
underlying integer.

On the resulting ICorDebugValue, get the ICorDebugGenericValue 
interface and set the integer to 12.

Pass the ICorDebugBoxValue to ICorDebugEval::CallFunction.

Jonathan*/
//public CorValue Box(int value)
//{
//    var cls = process.ResolveClass("System.Int32");
//    eval.NewObjectNoContstructor(cls);
//    var val = GetResult();

//    var valBoxed = val.CastToReferenceValue();
//    var corVal = valBoxed.Dereference();
//    var ee = corVal.CastToGenericValue();
//    ee.SetValue(value);

//    var h = corVal.CastToHeapValue()
//                   .CreateHandle(CorDebugHandleType.HANDLE_STRONG);


//    var tttt = h.CastToGenericValue();


//    var gv = corVal.CastToGenericValue(); 
//    gv.SetValue(value);

//    //var obj = val.GetObject();
//    //obj.S 
//    //var gv = val.CastToGenericValue();
//    //gv.SetValue(value);
//    //return gv;
//    return val;
//}

