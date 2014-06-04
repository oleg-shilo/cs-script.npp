using System;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Diagnostics;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorMetadata.NativeApi;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;

namespace Microsoft.Samples.Debugging.CorMetadata
{
    public sealed class MetadataPropertyInfo : PropertyInfo //zos; CSScript.Npp related changes
    {
        MetadataMethodInfo setterInfo;
        MetadataMethodInfo getterInfo;
        public bool IsStatic { get; set; }
        public bool IsPublic { get; set; }
        public bool canWrite;
        public bool canRead;

        bool ContainsAttribute<T>(T collection, T attr) where T : struct
        {
            int c = (int)(object)collection;
            int a = (int)(object)attr;
            return (c | a) == c;
        }

        internal MetadataPropertyInfo(IMetadataImport importer, int fieldToken, MetadataType declaringType)
        {
            m_importer = importer;
            m_fieldToken = fieldToken;
            m_declaringType = declaringType;

            // Initialize
            int mdTypeDef;
            int pchProp, pcbSigBlob, pdwCPlusTypeFlab, pcchValue, pdwAttr, pmdSetter, pmdGetter;
            IntPtr ppvSigBlob;
            IntPtr ppvRawValue;

            int rmdOtherMethod;
            int pcOtherMethod;

            m_importer.GetPropertyProps(m_fieldToken,
                                     out mdTypeDef,
                                     null,
                                     0,
                                     out pchProp,
                                     out pdwAttr,
                                     out ppvSigBlob,
                                     out pcbSigBlob,
                                     out pdwCPlusTypeFlab,
                                     out ppvRawValue,
                                     out pcchValue,
                                     out pmdSetter,
                                     out pmdGetter,
                                     out rmdOtherMethod,
                                     0,
                                     out pcOtherMethod
                                     );

            StringBuilder szField = new StringBuilder(pchProp);
            m_importer.GetPropertyProps(m_fieldToken,
                                     out mdTypeDef,
                                     szField,
                                     szField.Capacity,
                                     out pchProp,
                                     out pdwAttr,
                                     out ppvSigBlob,
                                     out pcbSigBlob,
                                     out pdwCPlusTypeFlab,
                                     out ppvRawValue,
                                     out pcchValue,
                                     out pmdSetter,
                                     out pmdGetter,
                                     out rmdOtherMethod,
                                     0,
                                     out pcOtherMethod
                                     );

            if (m_importer.IsValidToken((uint)pmdGetter))
            {
                getterInfo = new MetadataMethodInfo(m_importer, pmdGetter);
                if (ContainsAttribute(getterInfo.Attributes, MethodAttributes.Static))
                    IsStatic = true;
                if (ContainsAttribute(getterInfo.Attributes, MethodAttributes.Public))
                    IsPublic = true;
                if (ContainsAttribute(getterInfo.Attributes, MethodAttributes.Public) && !ContainsAttribute(getterInfo.Attributes, MethodAttributes.Abstract))
                    canRead = true;

            }

            if (m_importer.IsValidToken((uint)pmdSetter))
            {
                setterInfo = new MetadataMethodInfo(m_importer, pmdSetter);
                if (getterInfo == null && (setterInfo.Attributes & MethodAttributes.Static) == MethodAttributes.Static)
                    IsStatic = true;

                if (ContainsAttribute(setterInfo.Attributes, MethodAttributes.Public) && !ContainsAttribute(setterInfo.Attributes, MethodAttributes.Abstract))
                    canWrite = true;
            }

            m_propertyAttributes = (PropertyAttributes)pdwAttr;

            m_name = szField.ToString();
        }

        private static object ParseDefaultValue(MetadataType declaringType, IntPtr ppvSigBlob, IntPtr ppvRawValue)
        {
            IntPtr ppvSigTemp = ppvSigBlob;
            CorCallingConvention callingConv = MetadataHelperFunctions.CorSigUncompressCallingConv(ref ppvSigTemp);
            Debug.Assert(callingConv == CorCallingConvention.Field);

            CorElementType elementType = MetadataHelperFunctions.CorSigUncompressElementType(ref ppvSigTemp);
            if (elementType == CorElementType.ELEMENT_TYPE_VALUETYPE)
            {
                uint token = MetadataHelperFunctions.CorSigUncompressToken(ref ppvSigTemp);

                if (token == declaringType.MetadataToken)
                {
                    // Static literal field of the same type as the enclosing type
                    // may be one of the value fields of an enum
                    if (declaringType.ReallyIsEnum)
                    {
                        // If so, the value will be of the enum's underlying type,
                        // so we change it from VALUETYPE to be that type so that
                        // the following code will get the value
                        elementType = declaringType.EnumUnderlyingType;
                    }
                }
            }

            switch (elementType)
            {
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return (char)Marshal.ReadByte(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_I1:
                    return (sbyte)Marshal.ReadByte(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_U1:
                    return Marshal.ReadByte(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_I2:
                    return Marshal.ReadInt16(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_U2:
                    return (ushort)Marshal.ReadInt16(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_I4:
                    return Marshal.ReadInt32(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_U4:
                    return (uint)Marshal.ReadInt32(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_I8:
                    return Marshal.ReadInt64(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_U8:
                    return (ulong)Marshal.ReadInt64(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_I:
                    return Marshal.ReadIntPtr(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_U:
                case CorElementType.ELEMENT_TYPE_R4:
                case CorElementType.ELEMENT_TYPE_R8:
                // Technically U and the floating-point ones are options in the CLI, but not in the CLS or C#, so these are NYI
                default:
                    return null;
            }
        }

        public override Object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public override Object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override PropertyAttributes Attributes
        {
            get
            {
                return m_propertyAttributes;
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override String Name
        {
            get
            {
                return m_name;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Type ReflectedType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int MetadataToken
        {
            get
            {
                return m_fieldToken;
            }
        }

        private IMetadataImport m_importer;
        private int m_fieldToken;
        private MetadataType m_declaringType;

        private string m_name;
        private PropertyAttributes m_propertyAttributes;
        private Object m_value;

        public override bool CanRead
        {
            get { return canRead; }
        }

        public override bool CanWrite
        {
            get { return canWrite; }
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            throw new NotImplementedException();
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
            PropertyAttributes staticLiteralField = PropertyAttributes.HasDefault;
            if ((m_propertyAttributes & staticLiteralField) != staticLiteralField)
            {
                throw new InvalidOperationException("Field is not a static literal field.");
            }
            if (m_value == null)
            {
                throw new NotImplementedException("GetValue not implemented for the given field type.");
            }
            else
            {
                return m_value;
            }
        }

        public override Type PropertyType
        {
            get { throw new NotImplementedException(); }
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
