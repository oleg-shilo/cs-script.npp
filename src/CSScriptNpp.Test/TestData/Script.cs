using CSScriptIntellisense.Test;
using System;
using System.Windows.Forms;

#pragma warning disable 168

class Script
{
	[STAThread]
	static public void Main(string[] args)
	{
        TestBaseClassDefC t;
        //TestBaseGenericClass2<TestBaseClass> tt;
        //TestBaseGenericClass3<Script, TestBaseClass> ttt;
        TestDelgt<int> ttttt;
        TestClass1 ss;
        TestFieldsClass gg;
	}
}

