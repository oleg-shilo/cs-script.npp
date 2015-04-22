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
        EvalHelper eval;
        CorValue obj;
        List<CorValue> args = new List<CorValue>();

        public RemoteInspector(EvalHelper eval)
        {
            this.eval = eval;
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
                eval.Invoke("DbgAgent.ObjectInspector.StackAddInt32", obj, arg);
        }

        public CorValue Invoke(string methodName)
        {
            foreach (CorValue arg in args)
                Push(arg);
            return eval.Invoke("DbgAgent.ObjectInspector.InvokeStatic", obj, eval.CreateString(methodName));
        }

        public CorValue Invoke(string methodName, CorValue instance)
        {
            Push(instance);
            foreach (CorValue arg in args)
                Push(arg);
            return eval.Invoke("DbgAgent.ObjectInspector.Invoke", obj, eval.CreateString(methodName));
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
            return ParseValue("\"" + value + "\"");
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

        public class ExpressionParingResult
        {
            public string Method;
            public CorValue[] Arguments;
            public CorValue Instance;
        }

        public ExpressionParingResult ParseExpression(string expression)
        {
            var result = new ExpressionParingResult();

            int bracketIndex = expression.IndexOf('(');
            string methodName = expression.Substring(0, bracketIndex).Trim();
            string args = expression.Substring(bracketIndex).Replace("(", "").Replace(")", "").Trim();

            string[] methodParts = methodName.Split('.');

            //<<TypeName>|<CodeReference>>.<MethodName>

            string reference = string.Join(".", methodParts.Take(methodParts.Length - 1).ToArray());
            try
            {
                var instance = process.ResolveVariable(reference, process.Threads.Active.CurrentFrame);
                if (instance != null)
                {
                    result.Instance = instance.CorValue;
                    result.Method = methodParts.Last();

                }
            }
            catch { }

            if (result.Instance == null)
            {
                //MDbgFunction func = this.ResolveFunctionNameFromScope(methodName);
                //if (null == func)
                //    throw new Exception(String.Format(CultureInfo.InvariantCulture, "Could not resolve {0}", new Object[] { methodName }));

                result.Method = methodName;

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


        CorValue GetResult()
        {
            process.Go().WaitOne();

            // now display result of the funceval
            if (!(process.StopReason is EvalCompleteStopReason))
            {
                // we could have received also EvalExceptionStopReason but it's derived from EvalCompleteStopReason
                //WriteOutput("Func-eval not fully completed and debuggee has stopped");
                //WriteOutput("Result of funceval won't be printed when finished.");
            }
            else
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

