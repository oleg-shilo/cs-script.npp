using System;
using System.Linq;

namespace CSScriptNpp.Dialogs
{
    public class DbgObject
    {
        DbgObject[] children;

        public bool IsDescendantOfAny(params DbgObject[] ancestors)
        {
            var parent = this.Parent;
            do
            {
                if (ancestors.Contains(parent))
                    return true;
                if (parent != null)
                    parent = parent.Parent;
            }
            while (parent != null);
            return false;
        }

        public DbgObject[] Children
        {
            get { return children; }
            set
            {
                children = value;
                if (Children != null)
                    Array.ForEach(Children, x => x.Parent = this);
            }
        }
        public DbgObject Parent { get; set; }
        public bool HasChildren { get; set; }
        public bool IsExpanded { get; set; }
        public bool IsStatic { get; set; }
        public bool IsArray { get; set; }
        public bool IsField { get; set; }
        public bool IsEditPlaceholder { get; set; }
        public string DbgId { get; set; }
        public bool IsPublic { get; set; }
        public bool IsSeparator { get; set; }
        public bool IsExpression { get; set; }
        public bool IsModified { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        string _value;

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;

                Tooltip = null;
                if (_value != null && _value.Length > TrancationSize)
                {
                    Tooltip = "Display value has been truncated";
                    DispayValue = _value.Substring(0, TrancationSize).Replace("\r\n", "") + "...";
                }
                else
                    DispayValue = _value;
            }
        }

        public string Tooltip { get; set; }
        public string DispayValue { get; set; }

        public const int TrancationSize = 40;

        public bool IsUnresolved
        {
            get
            {
                return Value == "<N/A>" && Type == "<N/A>";
            }
        }

        public bool IsVisualizable
        {
            get
            {
                return !string.IsNullOrEmpty(Value) && Value != "<null>" && !IsUnresolved;
            }
        }

        public void CopyDbgDataFrom(DbgObject source)
        {
            this.DbgId = source.DbgId;
            this.Value = source.Value;
            this.DispayValue = source.DispayValue;
            this.Type = source.Type;
            this.IsStatic = source.IsStatic;
            this.IsArray = source.IsArray;
            this.IsField = source.IsField;
            this.IsPublic = source.IsPublic;
            this.Tooltip = source.Tooltip;
            this.IsModified = source.IsModified;
            this.HasChildren = source.HasChildren;
        }

        public int IndentationLevel
        {
            get
            {
                int level = 0;
                DbgObject parent = this.Parent;
                while (parent != null)
                {
                    level++;
                    parent = parent.Parent;
                }
                return level;
            }
        }
    }
}