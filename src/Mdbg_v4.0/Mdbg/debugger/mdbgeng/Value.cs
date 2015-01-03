//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using Microsoft.Samples.Debugging.CorMetadata;

namespace Microsoft.Samples.Debugging.MdbgEngine
{
    /// <summary>
    /// MDbg Value class.
    /// </summary>
    public sealed class MDbgValue : MarshalByRefObject
    {
        /// <summary>
        /// Creates a new instance of the MDbgValue Object.
        /// This constructor is public so that applications can use this class to print values (CorValue).
        /// CorValue's can be returned for example by funceval(CorEval.Result).
        /// </summary>
        /// <param name="process">The Process that will own the Value.</param>
        /// <param name="value">The CorValue that this MDbgValue will start with.</param>
        public MDbgValue(MDbgProcess process, CorValue value)
        {
            // value can be null, but we should always know what process we are
            // looking at.
            Debug.Assert(process != null);
            Initialize(process, null, value);
        }

        /// <summary>
        /// Creates a new instance of the MDbgValue Object.
        /// This constructor is public so that applications can use this class to print values (CorValue).
        /// CorValue's can be returned for example by funceval(CorEval.Result).
        /// </summary>
        /// <param name="process">The Process that will own the Value.</param>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The CorValue that this MDbgValue will start with.</param>
        public MDbgValue(MDbgProcess process, string name, CorValue value)
        {
            Debug.Assert(process != null && name != null);
            // corValue can be null for native variables in MC++
            Initialize(process, name, value);
        }

        private void Initialize(MDbgProcess process, string name, CorValue value)
        {
            m_process = process;
            m_name = name;
            m_corValue = value;
        }

        /// <summary>
        /// The CorValue stored in the MDbgValue.
        /// </summary>
        /// <value>The CorValue.</value>
        public CorValue CorValue
        {
            get
            {
                return m_corValue;
            }
        }

        public bool IsDictionaryEntryType
        {
            get { return TypeName.StartsWith("System.Collections.Generic.Dictionary+Entry<"); }
        }

        public bool IsListType
        {
            get { return TypeName.StartsWith("System.Collections.Generic.List<"); }
        }

        public bool IsDictionaryType
        {
            get { return TypeName.StartsWith("System.Collections.Generic.Dictionary<"); }
        }

        //zos; CSScript.Npp related changes
        public string DisplayValue
        {
            get
            {
                try
                {
                    if (IsArrayType)
                    {
                        //it is always single dimension array
                        int length = Dereference(CorValue, null).CastToArrayValue()
                                                                .GetDimensions()[0];
                        return "{" + this.TypeName.Replace("[]", "[" + length + "]") + "}";
                    }
                    else if (IsListType)
                    {
                        return "Count = " + this.GetFieldValue<int>("_size");
                    }
                    else if (IsDictionaryType)
                    {
                        return "Count = " + this.GetFieldValue<int>("count");
                    }
                    else if (IsDictionaryEntryType)
                    {
                        return "[" + this.GetField("key").GetStringValue(false) + ", " + this.GetField("value").GetStringValue(false) + "]";
                    }
                }
                catch { }
                return null;
            }
        }

        /// <summary>
        /// The Process that owns this Value.
        /// </summary>
        /// <value>The Process.</value>
        public MDbgProcess Process
        {
            get
            {
                return m_process;
            }
        }

        /// <summary>
        /// The Name of this Value.
        /// </summary>
        /// <value>The Name.</value>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// The Name of this Type.
        /// </summary>
        /// <value>The TypeName.</value>
        public string TypeName
        {
            get
            {
                if (CorValue == null)
                {
                    return "<N/A>";
                }

                // Every Value should have a non-null type associated with it.
                CorType t = CorValue.ExactType;
                return InternalUtil.PrintCorType(m_process, t);
            }
        }

        public bool IsStatic { get; set; }
        public bool IsProperty { get; set; }
        public bool IsFake { get; set; }
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Is this type a complex type.
        /// </summary>
        /// <value>true if it is complex, else false.</value>
        public bool IsComplexType
        {
            get
            {
                if (CorValue == null)
                    return false;
                CorValue value;
                try
                {
                    value = Dereference(CorValue, null);
                }
                catch (COMException ce)
                {
                    if (ce.ErrorCode == (int)HResult.CORDBG_E_BAD_REFERENCE_VALUE)
                        return false;
                    throw;
                }
                if (value == null)
                    return false;
                return (value.Type == CorElementType.ELEMENT_TYPE_CLASS ||
                        value.Type == CorElementType.ELEMENT_TYPE_VALUETYPE);
            }
        }

        /// <summary>
        /// Is this type an array type.
        /// </summary>
        /// <value>true if it is an array type, else false.</value>
        public bool IsArrayType
        {
            get
            {
                if (CorValue == null)
                    return false;
                CorValue value;
                try
                {
                    value = Dereference(CorValue, null);
                }
                catch (COMException ce)
                {
                    if (ce.ErrorCode == (int)HResult.CORDBG_E_BAD_REFERENCE_VALUE)
                        return false;
                    throw;
                }

                if (value == null)
                    return false;
                Debug.Assert(value != null);
                return (value.Type == CorElementType.ELEMENT_TYPE_SZARRAY ||
                        value.Type == CorElementType.ELEMENT_TYPE_ARRAY);
            }
        }

        /// <summary>
        /// Is this Value Null.
        /// </summary>
        /// <value>true if it is Null, else false.</value>
        public bool IsNull
        {
            get
            {
                if (CorValue == null)
                    return true;
                CorValue value;
                try
                {
                    value = Dereference(CorValue, null);
                }
                catch (COMException ce)
                {
                    if (ce.ErrorCode == (int)HResult.CORDBG_E_BAD_REFERENCE_VALUE)
                        return false;
                    throw;
                }
                return (value == null);
            }
        }

        /// <summary>
        /// Gets the Value.
        /// </summary>
        /// <param name="expand">Should it expand inner objects.</param>
        /// <returns>A string representation of the Value.</returns>
        public string GetStringValue(bool expand)
        {
            return GetStringValue(expand ? 1 : 0);
        }

        /// <summary>
        /// Gets the Value.
        /// </summary>
        /// <param name="expandDepth">How deep inner objects should be expanded. Value
        /// 0 means don't expand at all.</param>
        /// <returns>A string representation of the Value.</returns>
        public string GetStringValue(int expandDepth)
        {
            // by default we can do funcevals.
            return GetStringValue(expandDepth, true);
        }

        /// <summary>
        /// Gets the Value.
        /// </summary>
        /// <param name="expandDepth">How deep inner objects should be expanded. Value
        /// 0 means don't expand at all.</param>
        /// <param name="canDoFunceval">Set to true if ToString() should be called to get better description.</param>
        /// <returns>A string representation of the Value.</returns>
        public string GetStringValue(int expandDepth, bool canDoFunceval)
        {
            return InternalGetValue(0, expandDepth, canDoFunceval);
        }

        /// <summary>
        /// Gets the specified Field.
        /// </summary>
        /// <param name="name">The Name of the Field to get.</param>
        /// <returns>The Value of the specified Field.</returns>
        public MDbgValue GetField(string name)
        {
            MDbgValue ret = null;
            foreach (MDbgValue v in GetFields())
                if (v.Name.Equals(name))
                {
                    ret = v;
                    break;
                }
            if (ret == null)
                throw new MDbgValueException("Field '" + name + "' not found.");
            return ret;
        }

        /// <summary>
        /// Gets all the Fields
        /// </summary>
        /// <returns>An array of all Fields.</returns>
        public MDbgValue[] GetFields()
        {
            //zos; CSScript.Npp related changes
            //if (!IsComplexType)
            //    throw new MDbgValueException("Type is not complex"); //Not complex, so what?

            if (m_cachedFields == null)
                m_cachedFields = InternalGetFields();

            return m_cachedFields;
        }

        /// <summary>
        /// Gets all the properties.
        /// </summary>
        /// <returns>An array of all Properties.</returns>
        public MDbgValue[] GetProperties(string propName = null) //zos; CSScript.Npp related changes
        {
            if (m_cachedProperties == null)
                m_cachedProperties = InternalGetProperties(propName);

            return m_cachedProperties;
        }

        /// <summary>
        /// Gets Array Items.  This function can be called only on one dimensional arrays.
        /// </summary>
        /// <returns>An array of the values for the Array Items.</returns>
        public MDbgValue[] GetArrayItems(int maxCount = int.MaxValue)
        {
            if (!IsArrayType)
                throw new MDbgValueException("Type is not array type");

            //this.InternalGetFields().Where

            CorValue value = Dereference(CorValue, null);
            CorArrayValue av = value.CastToArrayValue();
            int[] dims = av.GetDimensions();
            Debug.Assert(dims != null);

            ArrayList al = new ArrayList();
            Debug.Assert(av.Rank == 1);
            int length = Math.Min(dims[0], maxCount);
            for (int i = 0; i < length; i++)
            {
                MDbgValue v = new MDbgValue(Process, "[" + i + "]", av.GetElementAtPosition(i));
                al.Add(v);
            }
            return (MDbgValue[])al.ToArray(typeof(MDbgValue));
        }

        /// <summary>
        /// Gets List Items.  This function can be called only on one generic list.
        /// </summary>
        /// <returns>An array of the values for the List Items.</returns>
        public MDbgValue[] GetListItems(int maxCount = int.MaxValue) //zos; CSScript.Npp related changes//zos
        {
            var items = GetField("_items");
            var length = Math.Min(this.GetFieldValue<int>("_size"), maxCount);

            CorValue value = Dereference(items.CorValue, null);
            CorArrayValue av = value.CastToArrayValue();
            int[] dims = av.GetDimensions();
            Debug.Assert(dims != null);

            ArrayList al = new ArrayList();
            Debug.Assert(av.Rank == 1);
            for (int i = 0; i < dims[0] && i < length; i++)
            {
                MDbgValue v = new MDbgValue(Process, "[" + i + "]", av.GetElementAtPosition(i));
                al.Add(v);
            }
            return (MDbgValue[])al.ToArray(typeof(MDbgValue));
        }

        /// <summary>
        /// Gets List Items.  This function can be called only on one generic list.
        /// </summary>
        /// <returns>An array of the values for the Dictionary Items.</returns>
        public MDbgValue[] GetDictionaryItems(int maxCount = int.MaxValue) //zos; CSScript.Npp related changes
        {
            var items = GetField("entries");
            var length = Math.Min(this.GetFieldValue<int>("count"), maxCount);

            CorValue value = Dereference(items.CorValue, null);
            CorArrayValue av = value.CastToArrayValue();
            int[] dims = av.GetDimensions();
            Debug.Assert(dims != null);

            ArrayList al = new ArrayList();
            Debug.Assert(av.Rank == 1);
            for (int i = 0; i < dims[0] && i < length; i++)
            {
                MDbgValue v = new MDbgValue(Process, "[" + i + "]", av.GetElementAtPosition(i));
                al.Add(v);
            }
            return (MDbgValue[])al.ToArray(typeof(MDbgValue));
        }

        /// <summary>
        /// Gets the Array Item for the specified indexes
        /// </summary>
        /// <param name="indexes">Which indexes to get the Array Item for.</param>
        /// <returns>The Value for the given indexes.</returns>
        public MDbgValue GetArrayItem(params int[] indexes)
        {
            if (!IsArrayType)
                throw new MDbgValueException("Type is not array type");

            CorValue value = Dereference(CorValue, null);
            CorArrayValue av = value.CastToArrayValue();
            Debug.Assert(av != null);
            if (av.Rank != indexes.Length)
                throw new MDbgValueException("Invalid number of dimensions.");

            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < indexes.Length; ++i)
            {
                if (i != 0)
                    sb.Append(",");
                sb.Append(indexes[i]);
            }
            sb.Append("]");

            MDbgValue v = new MDbgValue(Process, sb.ToString(), av.GetElement(indexes));
            return v;
        }

        /// <summary>
        /// Gets or Sets the Value of the MDbgValue to the given value.
        /// </summary>
        /// <value>This is exposed as an Object but can a primitive type, CorReferenceValue, or CorGenericValue.</value>
        public Object Value
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (value is CorReferenceValue)
                {
                    CorReferenceValue lsValRef = CorValue.CastToReferenceValue();
                    if (lsValRef == null)
                    {
                        throw new MDbgValueWrongTypeException("cannot assign reference value to non-reference value");
                    }
                    lsValRef.Value = ((CorReferenceValue)value).Value;
                }
                else if (value is CorGenericValue)
                {
                    CorGenericValue lsValGen = GetGenericValue();
                    lsValGen.SetValue(((CorGenericValue)value).GetValue());
                }
                else if (value.GetType().IsPrimitive)
                {
                    // trying to set a primitive generic value, let the corapi layer attempt to convert the type                
                    CorGenericValue gv = GetGenericValue();
                    gv.SetValue(value);
                }
                else
                {
                    throw new MDbgValueWrongTypeException("Value is of unsupported type.");
                }
            }
        }

        internal void InternalSetName(string variableName)
        {
            Debug.Assert(variableName != null);
            m_name = variableName;
        }
        //////////////////////////////////////////////////////////////////////////////////
        //
        // Implementation Part
        //
        //////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Small helper used by InternalGetValue to put parens around the
        /// ptrstring generated by Dereference()
        /// </summary>
        /// <param name="ptrStrBuilder">ptrstring generated by Dereference()</param>
        /// <returns>String with parens around the input string</returns>
        private string MakePrefixFromPtrStringBuilder(StringBuilder ptrStrBuilder)
        {
            if (ptrStrBuilder == null)
                return String.Empty;

            string ptrStr = ptrStrBuilder.ToString();
            if (String.IsNullOrEmpty(ptrStr))
                return String.Empty;

            return "(" + ptrStr + ") ";
        }

        private string InternalGetValue(int indentLevel, int expandDepth, bool canDoFunceval)
        {
            Debug.Assert(expandDepth >= 0);

            CorValue value = this.CorValue;
            if (value == null)
            {
                return "<N/A>";
            }

            // Record the memory addresses if displaying them is enabled
            string prefix = String.Empty;
            StringBuilder ptrStrBuilder = null;
            if (m_process.m_engine.Options.ShowAddresses)
            {
                ptrStrBuilder = new StringBuilder();
            }

            try
            {
                value = Dereference(value, ptrStrBuilder);
            }
            catch (COMException ce)
            {
                if (ce.ErrorCode == (int)HResult.CORDBG_E_BAD_REFERENCE_VALUE)
                {
                    return MakePrefixFromPtrStringBuilder(ptrStrBuilder) + "<invalid reference value>";
                }
                throw;
            }

            prefix = MakePrefixFromPtrStringBuilder(ptrStrBuilder);

            if (value == null)
            {
                return prefix + "<null>";
            }

            Unbox(ref value);

            switch (value.Type)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                case CorElementType.ELEMENT_TYPE_I1:
                case CorElementType.ELEMENT_TYPE_U1:
                case CorElementType.ELEMENT_TYPE_I2:
                case CorElementType.ELEMENT_TYPE_U2:
                case CorElementType.ELEMENT_TYPE_I4:
                case CorElementType.ELEMENT_TYPE_U4:
                case CorElementType.ELEMENT_TYPE_I:
                case CorElementType.ELEMENT_TYPE_U:
                case CorElementType.ELEMENT_TYPE_I8:
                case CorElementType.ELEMENT_TYPE_U8:
                case CorElementType.ELEMENT_TYPE_R4:
                case CorElementType.ELEMENT_TYPE_R8:
                case CorElementType.ELEMENT_TYPE_CHAR:
                    {
                        object v = value.CastToGenericValue().GetValue();
                        string result;

                        IFormattable vFormattable = v as IFormattable;
                        if (vFormattable != null)
                            result = vFormattable.ToString(null, System.Globalization.CultureInfo.CurrentUICulture);
                        else
                            result = v.ToString();

                        // let's put quotes around char values
                        if (value.Type == CorElementType.ELEMENT_TYPE_CHAR)
                            result = "'" + result + "'";

                        return prefix + result;
                    }

                case CorElementType.ELEMENT_TYPE_CLASS:
                case CorElementType.ELEMENT_TYPE_VALUETYPE:
                    CorObjectValue ov = value.CastToObjectValue();
                    return prefix + PrintObject(indentLevel, ov, expandDepth, canDoFunceval);

                case CorElementType.ELEMENT_TYPE_STRING:
                    CorStringValue sv = value.CastToStringValue();
                    return prefix + '"' + sv.String + '"';

                case CorElementType.ELEMENT_TYPE_SZARRAY:
                case CorElementType.ELEMENT_TYPE_ARRAY:
                    CorArrayValue av = value.CastToArrayValue();
                    return prefix + PrintArray(indentLevel, av, expandDepth, canDoFunceval);

                case CorElementType.ELEMENT_TYPE_PTR:
                    return prefix + "<non-null pointer>";

                case CorElementType.ELEMENT_TYPE_FNPTR:
                    return prefix + "0x" + value.CastToReferenceValue().Value.ToString("X");

                case CorElementType.ELEMENT_TYPE_BYREF:
                case CorElementType.ELEMENT_TYPE_TYPEDBYREF:
                case CorElementType.ELEMENT_TYPE_OBJECT:
                default:
                    return prefix + "<printing value of type: " + value.Type + " not implemented>";
            }
        }

        private void Unbox(ref CorValue value)
        {
            CorBoxValue boxVal = value.CastToBoxValue();
            if (boxVal != null)
                value = boxVal.GetObject();
        }

        /// <summary>
        /// Recursively dereference the input value until we finally find a non-dereferenceable
        /// value.  Along the way, optionally build up a "ptr string" that shows the addresses
        /// we dereference, separated by "->".
        /// </summary>
        /// <param name="value">Value to dereference</param>
        /// <param name="ptrStringBuilder">StringBuilder if caller wants us to generate
        /// a "ptr string" (in which case we'll stick it there).  If caller doesn't want
        /// a ptr string, this can be null</param>
        /// <returns>CorValue we arrive at after dereferencing as many times as we can</returns>
        private CorValue Dereference(CorValue value, StringBuilder ptrStringBuilder)
        {
            while (true)
            {
                CorReferenceValue rv = value.CastToReferenceValue();
                if (rv == null)
                    break; // not a reference

                if (ptrStringBuilder != null)
                {
                    if (ptrStringBuilder.Length > 0)
                    {
                        ptrStringBuilder.Append("->");
                    }
                    ptrStringBuilder.Append("0x" + rv.Value.ToString("X", System.Globalization.CultureInfo.CurrentUICulture));
                }

                // Bail as soon as we hit a reference to NULL
                if (rv.IsNull)
                    return null;    // reference to null

                CorValue newValue = null;
                try
                {
                    newValue = rv.Dereference();
                }
                catch (COMException ce)
                {
                    // Check for any errors that are expected
                    if (ce.ErrorCode != (int)HResult.CORDBG_E_VALUE_POINTS_TO_FUNCTION)
                    {
                        throw;  // some other error
                    }
                }

                if (newValue == null)
                    break;  // couldn't dereference the reference (eg. void*)

                value = newValue;
            }
            return value;
        }

        // Builds the friendly string for an enum value
        private string InternalGetEnumString(CorObjectValue ov, MetadataType type)
        {
            Debug.Assert(type != null); // Enums should always have a type

            IList<KeyValuePair<string, ulong>> values = type.EnumValues;

            // Get the underlying value
            ulong value = Convert.ToUInt64(ov.CastToGenericValue().UnsafeGetValueAsType(type.EnumUnderlyingType), CultureInfo.InvariantCulture);

            // Find a reasonable value to display
            StringBuilder result = new StringBuilder();
            ulong remainingValue = value;
            bool firstTime = true;
            for (int i = values.Count - 1; i >= 0; i--)
            {
                if ((values[i].Value == value) ||
                         (type.ReallyIsFlagsEnum && (values[i].Value != 0) && ((values[i].Value & value) == values[i].Value)))
                {
                    remainingValue &= ~(values[i].Value);    // Remove the flags from the total needed for flags enums

                    if (!firstTime)
                    {
                        if (type.ReallyIsFlagsEnum)
                        {
                            result.Insert(0, ", ");
                        }
                        else
                        {
                            result.Insert(0, " / ");
                        }
                    }
                    result.Insert(0, values[i].Key);
                    firstTime = false;
                }
            }
            if (remainingValue != 0)
            {
                if (firstTime)
                {
                    // No matches whatsoever
                    result.Insert(0, remainingValue);
                }
                else
                {
                    // Flags enum with leftover bits
                    result.AppendFormat(" (Unnamed bits: {0})", remainingValue);
                }
            }

            return result.ToString();
        }

        bool IsNullableType(CorType ct)
        {
            if (ct.Type != CorElementType.ELEMENT_TYPE_VALUETYPE)
                return false;

            MDbgModule m = m_process.Modules.Lookup(ct.Class.Module);
            String name = m.Importer.GetType(ct.Class.Token).FullName;

            return name.Equals("System.Nullable`1");
        }


        private string PrintObject(int indentLevel, CorObjectValue ov, int expandDepth, bool canDoFunceval)
        {
            Debug.Assert(expandDepth >= 0);

            bool fNeedToResumeThreads = true;

            // Print generics-aware type.
            string name = InternalUtil.PrintCorType(this.m_process, ov.ExactType);

            StringBuilder txt = new StringBuilder();
            txt.Append(name);

            if (expandDepth > 0)
            {
                // we gather the field info of the class before we do
                // funceval since funceval requires running the debugger process
                // and this in turn can cause GC and invalidate our references.
                StringBuilder expandedDescription = new StringBuilder();
                if (IsComplexType)
                {
                    foreach (MDbgValue v in GetFields())
                    {
                        expandedDescription.Append("\n").Append(IndentedString(indentLevel + 1, v.Name)).
                            Append("=").Append(IndentedBlock(indentLevel + 2,
                                   v.GetStringValue(expandDepth - 1, false)));
                    }
                }

                // if the value we're printing is a nullable type that has no value (is null), we can't do a func eval
                // to get its value, since it will be boxed as a null pointer. We already have the information we need, so 
                // we'll just take care of it now. Note that ToString() for null-valued nullable types just prints the 
                // empty string. 

                // bool hasValue = (bool)(GetField("hasValue").CorValue.CastToGenericValue().GetValue());

                if (IsNullableType(ov.ExactType) && !(bool)(GetField("hasValue").CorValue.CastToGenericValue().GetValue()))
                {
                    txt.Append(" < >");
                }

                else if (ov.IsValueClass && canDoFunceval)
                // we could display even values for real Objects, but we will just show 
                // "description" for valueclasses.
                {
                    CorClass cls = ov.ExactType.Class;
                    CorMetadataImport importer = m_process.Modules.Lookup(cls.Module).Importer;
                    MetadataType mdType = importer.GetType(cls.Token) as MetadataType;

                    if (mdType.ReallyIsEnum)
                    {
                        txt.AppendFormat(" <{0}>", InternalGetEnumString(ov, mdType));
                    }
                    else if (m_process.IsRunning)
                        txt.Append(" <N/A during run>");
                    else
                    {
                        MDbgThread activeThread = m_process.Threads.Active;

                        CorValue thisValue;
                        CorHeapValue hv = ov.CastToHeapValue();
                        if (hv != null)
                        {
                            // we need to pass reference value.
                            CorHandleValue handle = hv.CreateHandle(CorDebugHandleType.HANDLE_WEAK_TRACK_RESURRECTION);
                            thisValue = handle;
                        }
                        else
                            thisValue = ov;

                        try
                        {
                            CorEval eval = m_process.Threads.Active.CorThread.CreateEval();
                            m_process.CorProcess.SetAllThreadsDebugState(CorDebugThreadState.THREAD_SUSPEND,
                                                                         activeThread.CorThread);

                            MDbgFunction toStringFunc = m_process.ResolveFunctionName(null, "System.Object", "ToString",
                                                                             thisValue.ExactType.Class.Module.Assembly.AppDomain);

                            Debug.Assert(toStringFunc != null); // we should be always able to resolve ToString function.

                            eval.CallFunction(toStringFunc.CorFunction, new CorValue[] { thisValue });
                            m_process.Go();
                            do
                            {
                                m_process.StopEvent.WaitOne();
                                if (m_process.StopReason is EvalCompleteStopReason)
                                {
                                    CorValue cv = eval.Result;
                                    Debug.Assert(cv != null);
                                    MDbgValue mv = new MDbgValue(m_process, cv);
                                    string valName = mv.GetStringValue(0);

                                    // just purely for esthetical reasons we 'discard' "
                                    if (valName.StartsWith("\"") && valName.EndsWith("\""))
                                        valName = valName.Substring(1, valName.Length - 2);

                                    txt.Append(" <").Append(valName).Append(">");
                                    break;
                                }
                                if ((m_process.StopReason is ProcessExitedStopReason) ||
                                    (m_process.StopReason is EvalExceptionStopReason))
                                {
                                    txt.Append(" <N/A cannot evaluate>");
                                    break;
                                }
                                // hitting bp or whatever should not matter -- we need to ignore it
                                m_process.Go();
                            }
                            while (true);
                        }
                        catch (COMException e)
                        {
                            // Ignore cannot copy a VC class error - Can't copy a VC with object refs in it.
                            if (e.ErrorCode != (int)HResult.CORDBG_E_OBJECT_IS_NOT_COPYABLE_VALUE_CLASS)
                            {
                                throw;
                            }
                        }
                        catch (System.NotImplementedException)
                        {
                            fNeedToResumeThreads = false;
                        }
                        finally
                        {
                            if (fNeedToResumeThreads)
                            {
                                // we need to resume all the threads that we have suspended no matter what.
                                m_process.CorProcess.SetAllThreadsDebugState(CorDebugThreadState.THREAD_RUN,
                                                                             activeThread.CorThread);
                            }
                        }
                    }
                }
                txt.Append(expandedDescription.ToString());
            }
            return txt.ToString();
        }

        //zos; CSScript.Npp related changes
        public string InvokeToString()
        {
            CorObjectValue ov = this.ToInvokableObject();

            var txt = new StringBuilder();

            CorClass cls = ov.ExactType.Class;
            CorMetadataImport importer = m_process.Modules.Lookup(cls.Module).Importer;
            MetadataType mdType = importer.GetType(cls.Token) as MetadataType;

            if (mdType.ReallyIsEnum)
            {
                txt.AppendFormat(" <{0}>", InternalGetEnumString(ov, mdType));
            }
            else if (m_process.IsRunning)
            {
                txt.Append(" <N/A during run>");
            }
            else
            {
                MDbgValue result = this.InvokeMethod("System.Object", "ToString");
                string valName = result.GetStringValue(0);

                // just purely for esthetically reasons we 'discard' "
                if (valName.StartsWith("\"") && valName.EndsWith("\""))
                    valName = valName.Substring(1, valName.Length - 2);

                txt.Append(valName);
            }
            return txt.ToString();
        }

        //zos; CSScript.Npp related changes
        public CorObjectValue ToInvokableObject()
        {
            CorValue value = this.CorValue;
            if (value == null)
                throw new Exception("MDbgValue isn't initialized");

            // Record the memory addresses if displaying them is enabled
            string prefix = String.Empty;
            StringBuilder ptrStrBuilder = null;
            if (m_process.m_engine.Options.ShowAddresses)
            {
                ptrStrBuilder = new StringBuilder();
            }

            try
            {
                value = Dereference(value, ptrStrBuilder);
            }
            catch (COMException ce)
            {
                if (ce.ErrorCode == (int)HResult.CORDBG_E_BAD_REFERENCE_VALUE)
                    throw new Exception(MakePrefixFromPtrStringBuilder(ptrStrBuilder) + "<invalid reference value>");
                throw;
            }

            prefix = MakePrefixFromPtrStringBuilder(ptrStrBuilder);

            if (value == null)
            {
                throw new Exception(prefix + "<null>");
            }

            Unbox(ref value);

            return value.CastToObjectValue();
        }

        //zos; CSScript.Npp related changes
        public MDbgValue InvokeMethod(string name)
        {
            CorClass cls = this.ToInvokableObject().ExactType.Class;
            CorMetadataImport importer = m_process.Modules.Lookup(cls.Module).Importer;
            MetadataType mdType = importer.GetType(cls.Token) as MetadataType;

            if (m_process.IsRunning)
            {
                throw new Exception("<N/A during run>");
            }
            else
            {
                string typeName = mdType.FullName;

                if (!this.IsMethodAvailable(typeName, name))
                {
                    MDbgValue type = this.InvokeMethod("System.Object", "GetType");
                    var baseType = type.InvokeMethod("System.Type", "get_BaseType");
                    typeName = baseType.InvokeToString();

                    while (!this.IsMethodAvailable(typeName, name))
                    {
                        type = baseType;
                        baseType = type.InvokeMethod("System.Type", "get_BaseType");

                        if (baseType.IsNull)
                        {
                            typeName = null;
                            break;
                        }
                        typeName = baseType.InvokeToString();
                    }
                }

                if (typeName != null)
                    return this.InvokeMethod(typeName, name);
                else
                    throw new Exception("Method '" + name + "' not found");
            }
        }

        //zos; CSScript.Npp related changes
        MDbgValue InvokeMethod(string type, string name)
        {
            MDbgValue result = null;

            CorObjectValue ov = this.ToInvokableObject();

            bool fNeedToResumeThreads = true;

            var txt = new StringBuilder();

            CorClass cls = ov.ExactType.Class;

            if (m_process.IsRunning)
            {
                throw new Exception("<N/A during run>");
            }
            else
            {
                MDbgThread activeThread = m_process.Threads.Active;

                CorValue thisValue;
                CorHeapValue hv = ov.CastToHeapValue();
                if (hv != null)
                {
                    // we need to pass reference value.
                    CorHandleValue handle = hv.CreateHandle(CorDebugHandleType.HANDLE_WEAK_TRACK_RESURRECTION);
                    thisValue = handle;
                }
                else
                    thisValue = ov;

                try
                {
                    CorEval eval = m_process.Threads.Active.CorThread.CreateEval();
                    m_process.CorProcess.SetAllThreadsDebugState(CorDebugThreadState.THREAD_SUSPEND,
                                                                 activeThread.CorThread);

                    MDbgFunction func = m_process.ResolveFunctionName(null, type, name,
                                                                     thisValue.ExactType.Class.Module.Assembly.AppDomain);

                    Debug.Assert(func != null); // we should be always able to resolve ToString function.

                    eval.CallFunction(func.CorFunction, new CorValue[] { thisValue });
                    m_process.Go();
                    do
                    {
                        m_process.StopEvent.WaitOne();
                        if (m_process.StopReason is EvalCompleteStopReason)
                        {
                            CorValue cv = eval.Result;
                            if(cv == null) //it was VOID method
                                result = null;
                            else
                                result = new MDbgValue(m_process, cv);
                            break;
                        }
                        if ((m_process.StopReason is ProcessExitedStopReason) ||
                            (m_process.StopReason is EvalExceptionStopReason))
                        {
                            throw new Exception("<N/A cannot evaluate>");
                        }
                        // hitting bp or whatever should not matter -- we need to ignore it
                        m_process.Go();
                    }
                    while (true);
                }
                catch (COMException e)
                {
                    // Ignore cannot copy a VC class error - Can't copy a VC with object refs in it.
                    if (e.ErrorCode != (int)HResult.CORDBG_E_OBJECT_IS_NOT_COPYABLE_VALUE_CLASS)
                    {
                        throw;
                    }
                }
                catch (System.NotImplementedException)
                {
                    fNeedToResumeThreads = false;
                }
                finally
                {
                    if (fNeedToResumeThreads)
                    {
                        // we need to resume all the threads that we have suspended no matter what.
                        m_process.CorProcess.SetAllThreadsDebugState(CorDebugThreadState.THREAD_RUN,
                                                                     activeThread.CorThread);
                    }
                }
            }
            return result;
        }

        //zos; CSScript.Npp related changes
        bool IsMethodAvailable(string type, string name)
        {

            if (m_process.IsRunning)
            {
                throw new Exception("<N/A during run>");
            }
            else
            {
                CorObjectValue ov = this.ToInvokableObject();

                try
                {
                    //m_process.CorProcess.SetAllThreadsDebugState(CorDebugThreadState.THREAD_SUSPEND, m_process.Threads.Active.CorThread);

                    MDbgFunction func = m_process.ResolveFunctionName(null, type, name, ov.ExactType.Class.Module.Assembly.AppDomain);
                    return (func != null);
                }
                catch (COMException e)
                {
                    // Ignore cannot copy a VC class error - Can't copy a VC with object refs in it.
                    if (e.ErrorCode != (int)HResult.CORDBG_E_OBJECT_IS_NOT_COPYABLE_VALUE_CLASS)
                    {
                        throw;
                    }
                }
            }
            return false;
        }

        private string PrintArray(int indentLevel, CorArrayValue av, int expandDepth, bool canDoFunceval)
        {
            Debug.Assert(expandDepth >= 0);

            StringBuilder txt = new StringBuilder();
            txt.Append("array [");
            int[] dims = av.GetDimensions();
            Debug.Assert(dims != null);

            for (int i = 0; i < dims.Length; ++i)
            {
                if (i != 0)
                    txt.Append(",");
                txt.Append(dims[i]);
            }
            txt.Append("]");

            if (expandDepth > 0 && av.Rank == 1 && av.ElementType != CorElementType.ELEMENT_TYPE_VOID)
            {
                for (int i = 0; i < dims[0]; i++)
                {
                    MDbgValue v = new MDbgValue(Process, av.GetElementAtPosition(i));
                    txt.Append("\n").Append(IndentedString(indentLevel + 1, "[" + i + "] = ")).
            Append(IndentedBlock(indentLevel + 2,
                           v.GetStringValue(expandDepth - 1, canDoFunceval)));
                }
            }
            return txt.ToString();
        }

        // Helper to get all the fields, including static fields and base types. 
        private MDbgValue[] InternalGetFields(string fieldName = null)
        {
            List<MDbgValue> al = new List<MDbgValue>();

            //dereference && (unbox);
            CorValue value = Dereference(CorValue, null);
            if (value == null)
            {
                throw new MDbgValueException("null value");
            }
            Unbox(ref value);
            CorObjectValue ov = value.CastToObjectValue();

            CorType cType = ov.ExactType;

            CorFrame cFrame = null;
            if (Process.Threads.HaveActive)
            {
                // we need a current frame to display thread local static values
                if (Process.Threads.Active.HaveCurrentFrame)
                {
                    MDbgFrame temp = Process.Threads.Active.CurrentFrame;
                    while (temp != null && !temp.IsManaged)
                    {
                        temp = temp.NextUp;
                    }
                    if (temp != null)
                    {
                        cFrame = temp.CorFrame;
                    }
                }
            }

            MDbgModule classModule;

            // initialization
            CorClass corClass = ov.Class;
            classModule = Process.Modules.Lookup(corClass.Module);

            // iteration through class hierarchy
            while (true)
            {
                Type classType = classModule.Importer.GetType(corClass.Token);
                foreach (MetadataFieldInfo fi in classType.GetFields())
                {
                    CorValue fieldValue = null;
                    if (fieldName != null && fi.Name != fieldName)
                        continue;

                    try
                    {
                        if (fi.IsLiteral)
                        {
                            fieldValue = null;
                            // for now we just hide the constant fields.
                            continue;
                        }
                        else if (fi.IsStatic)
                        {
                            if (cFrame == null)
                            {
                                // Without a frame, we won't be able to find static values.  So
                                // just skip this guy
                                continue;
                            }

                            fieldValue = cType.GetStaticFieldValue(fi.MetadataToken, cFrame);
                        }
                        else
                        {
                            // we are asuming normal field value
                            fieldValue = ov.GetFieldValue(corClass, fi.MetadataToken);
                        }
                    }
                    catch (COMException)
                    {
                        // we won't report any problems.
                    }
                    al.Add(new MDbgValue(Process, fi.Name, fieldValue) { IsStatic = fi.IsStatic, IsPrivate = !fi.IsPublic });
                    if (fieldName != null)
                        break;
                }
                cType = cType.Base;
                if (cType == null)
                    break;
                corClass = cType.Class;
                classModule = Process.Modules.Lookup(corClass.Module);
            }

            return al.ToArray();
        }

        static Dictionary<string, Tuple<MDbgFunction, CorEval>> propEvalCache = new Dictionary<string, Tuple<MDbgFunction, CorEval>>();

        static public MDbgValue GetStaticPropertyValue(MetadataPropertyInfo pi, MDbgProcess Process)
        {
            try
            {
                bool isExplicitInterfaceMember = (pi.Name.IndexOf(".") != -1);
                if (isExplicitInterfaceMember)
                    return null; //ignore (at least for now) due to the limited practical value

                string propFullName = pi.DeclarintTypeName + ".get_" + pi.Name;

                MDbgFunction func = Process.ResolveFunctionNameFromScope(propFullName, Process.AppDomains[0].CorAppDomain);
                CorEval eval = Process.Threads.Active.CorThread.CreateEval();

                if (pi.IsStatic) //Important to try to read even if pi.CanRead==false as it can be because the getter is private (what is OK for debugger).
                {
                    //func.CorFunction
                    eval.CallFunction(func.CorFunction, new CorValue[0]);
                    Process.Go().WaitOne();
                    if (Process.StopReason is EvalCompleteStopReason && !(Process.StopReason is EvalExceptionStopReason))
                    {
                        CorValue propertyValue = (Process.StopReason as EvalCompleteStopReason).Eval.Result;
                        return new MDbgValue(Process, pi.Name, propertyValue) { IsProperty = true, IsStatic = pi.IsStatic, IsPrivate = !pi.IsPublic };
                    }
                }
            }
            catch (COMException)
            {
                // we won't report any problems.
            }
            return null;
        }

        private MDbgValue[] InternalGetProperties(string propName = null) //zos; CSScript.Npp related changes
        {
            List<MDbgValue> al = new List<MDbgValue>();

            try
            {
                //dereference && (unbox);
                CorValue value = Dereference(CorValue, null);
                if (value == null)
                {
                    throw new MDbgValueException("null value");
                }

                Unbox(ref value);
                CorObjectValue ov = value.CastToObjectValue();

                CorType cType = ov.ExactType;

                CorFrame cFrame = null;
                if (Process.Threads.HaveActive)
                {
                    // we need a current frame to display thread local static values
                    if (Process.Threads.Active.HaveCurrentFrame)
                    {
                        MDbgFrame temp = Process.Threads.Active.CurrentFrame;
                        while (temp != null && !temp.IsManaged)
                        {
                            temp = temp.NextUp;
                        }
                        if (temp != null)
                        {
                            cFrame = temp.CorFrame;
                        }
                    }
                }

                MDbgModule classModule;

                // initialization
                CorClass corClass = ov.Class;
                classModule = Process.Modules.Lookup(corClass.Module);

                bool safe = Process.IsEvalSafe();

                // iteration through class hierarchy
                while (safe)
                {
                    Type classType = classModule.Importer.GetType(corClass.Token);

                    foreach (MetadataPropertyInfo pi in classType.GetProperties())
                    {
                        if (cFrame == null)
                            continue;

                        if (propName != null && pi.Name != propName)
                            continue;


                        CorValue propertyValue = null;
                        try
                        {
                            MDbgFunction func;
                            CorEval eval;

                            string propDisplayName = pi.Name;

                            bool isExplicitInterfaceMember = (propDisplayName.IndexOf(".") != -1);
                            if (isExplicitInterfaceMember)
                                continue; //ignore (at least for now) due to the limited practical value

                            //Debug.Assert(false);

                            string propFullName = classType.FullName + ".get_" + propDisplayName;
                            string propId = propFullName;

                            if (propEvalCache.ContainsKey(propId))
                            {
                                var cahce = propEvalCache[propId];
                                func = cahce.Item1;
                                eval = cahce.Item2;
                            }
                            else
                            {
                                func = Process.ResolveFunctionNameFromScope(propFullName, Process.AppDomains[0].CorAppDomain);
                                eval = Process.Threads.Active.CorThread.CreateEval();
                                propEvalCache[propId] = new Tuple<MDbgFunction, CorEval>(func, eval);
                            }

                            if (pi.CanRead)
                            {
                                if (pi.IsStatic)
                                {
                                    //func.CorFunction
                                    eval.CallFunction(func.CorFunction, new CorValue[0]);
                                    Process.Go().WaitOne();
                                    if (Process.StopReason is EvalCompleteStopReason && !(Process.StopReason is EvalExceptionStopReason))
                                        propertyValue = (Process.StopReason as EvalCompleteStopReason).Eval.Result;
                                }
                                else
                                {
                                    var typeParams = new List<CorType>();
                                    foreach (CorType item in CorValue.ExactType.TypeParameters)
                                        typeParams.Add(item);

                                    if (typeParams.Count == 0)
                                        eval.CallFunction(func.CorFunction, new CorValue[] { CorValue });
                                    else
                                        eval.CallParameterizedFunction(func.CorFunction, typeParams.ToArray(), new CorValue[] { CorValue });

                                    Process.Go().WaitOne();

                                    if (Process.StopReason is EvalCompleteStopReason && !(Process.StopReason is EvalExceptionStopReason))
                                        propertyValue = (Process.StopReason as EvalCompleteStopReason).Eval.Result;
                                }
                            }
                        }
                        catch (COMException)
                        {
                            // we won't report any problems.
                        }

                        //if (propertyValue != null)
                        al.Add(new MDbgValue(Process, pi.Name, propertyValue) { IsProperty = true, IsStatic = pi.IsStatic, IsPrivate = !pi.IsPublic });
                        if (propName != null)
                            break;
                    }

                    try
                    {
                        cType = cType.Base;
                    }
                    catch { cType = null; }  //zos; CSScript.Npp related changes

                    if (cType == null)
                        break;
                    corClass = cType.Class;
                    classModule = Process.Modules.Lookup(corClass.Module);
                }
            }
            catch { }
            return al.ToArray();
        }

        private string IndentedString(int indent, string txt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('\t', indent)
                .Append(txt);
            return sb.ToString();
        }

        private string IndentedBlock(int indent, string text)
        {
            Debug.Assert(text != null);

            string[] lines = text.Split('\n');
            StringBuilder result = new StringBuilder();

            result.Append(lines[0]); // 1 line is always there since text is not null.
            for (int i = 1; i < lines.Length; ++i)
                result.Append('\n').Append(IndentedString(indent, lines[i]));

            return result.ToString();
        }

        private CorGenericValue GetGenericValue()
        {
            CorGenericValue gv = CorValue.CastToGenericValue();
            if (gv == null)
                throw new MDbgValueWrongTypeException();
            return gv;
        }

        private string m_name;
        private CorValue m_corValue;
        private MDbgValue[] m_cachedFields;
        private MDbgValue[] m_cachedProperties;
        private MDbgProcess m_process;
    }
}
