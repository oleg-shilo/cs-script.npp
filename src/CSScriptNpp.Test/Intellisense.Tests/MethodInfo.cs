using CSScriptIntellisense;
using System.Linq;
using Xunit;

namespace Testing
{
    public class MethodInfo
    {
        public MethodInfo()
        {
            RoslynHost.Init();
        }

        [Fact]
        public void CountArgumentsInMemberInfo()
        {
            Assert.Equal(0, NRefactoryExtensions.GetArgumentCount("Method: void Console.WriteLine()"));
            Assert.Equal(1, NRefactoryExtensions.GetArgumentCount("Method: void Console.WriteLine(bool value)"));
            Assert.Equal(2, NRefactoryExtensions.GetArgumentCount("Method: void Console.WriteLine(bool value, object arg0)"));
            Assert.Equal(2, NRefactoryExtensions.GetArgumentCount("Method: void Class.Method(string text, Dictionary<int, string> map)"));
            Assert.Equal(3, NRefactoryExtensions.GetArgumentCount("Method: void Class.Method(string text, Dictionary<int, string> map, int count)"));
        }

        [Fact]
        public void ProcessMethodOverloadsHint()
        {
            SimpleCodeCompletion.ResetProject();

            //Simulate invoking ShowMathodInfo
            //Console.WriteLine(|
            string[] signatures = SimpleCodeCompletion.GetMemberInfo(@"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        Console.WriteLine(args.Length);
    }
}", 131, "test.cs", false);

            Assert.Equal(19, signatures.Count()); // may need to be updated for the new .NET versions

            //Simulate typing...
            //Console.WriteLine("Time {0}", DateTime.|

            var popup = new MemberInfoPanel();

            popup.AddData(signatures);
            Assert.Equal(19, popup.items.Count);

            popup.ProcessMethodOverloadHint(new[] { "Time {0}" });  //'single and more' parameter methods
            Assert.Equal(18, popup.items.Count);

            popup.ProcessMethodOverloadHint(new[] { "\"Time {0}\"", "DateTime." });  //'two and more' parameter methods
            Assert.Equal(6, popup.items.Count);
        }

        [Fact]
        public void DocumentationForReflectedDocumentationOfDocGhost()
        {
            var xmlText = @"<summary>
Gets or sets my property.
</summary>
<value>
My property.
</value>";
            string plainText = xmlText.XmlToPlainText(true);

            Assert.Equal(@"Gets or sets my property.
--------------------------
My property.", plainText);
        }

        [Fact]
        public void DocumentationForReflectedClassOnlyDocumentation()
        {
            var xmlText = @"<summary>
Simple class for testing Reflector
</summary>";
            string plainText = xmlText.XmlToPlainText(true);

            Assert.Equal(@"Simple class for testing Reflector", plainText);
        }

        [Fact]
        public void DocumentationForReflectedDocumentation()
        {
            var xmlText = @"<summary>Creates all directories and subdirectories as specified by <paramref name=""path""/>.
                            </summary>
                            <returns>A <see cref=""T:System.IO.DirectoryInfo""/> as specified by <paramref name=""path""/>.
                            </returns>
                            <param name=""path"">The directory path to create.</param>
                            <param name=""path2"">Fake parameter for testing.</param>
                            <exception cref=""T:System.IO.IOException"">The directory specified by <paramref name=""path""/>
                            is read-only.</exception>
                            <exception cref=""T:System.UnauthorizedAccessException"">The caller does not have
                            the required permission.</exception>
                            <exception cref=""T:System.ArgumentException"">
                            <paramref name=""path""/> is a zero-length string, contains only white space, or
                            contains one or more invalid characters as defined by <see cref=""F:System.IO.Path.InvalidPathChars""/>.-or-
                            <paramref name=""path""/> is prefixed with, or contains only a colon character
                            (:).</exception>
                            <exception cref=""T:System.ArgumentNullException"">
                            <paramref name=""path""/> is null. </exception>
                            <exception cref=""T:System.IO.PathTooLongException"">The specified path, file name,
                            or both exceed the system-defined maximum length. For example, on Windows-based
                            platforms, paths must be less than 248 characters and file names must be less
                            than 260 characters. </exception>
                            <exception cref=""T:System.IO.DirectoryNotFoundException"">The specified path is
                            invalid (for example, it is on an unmapped drive). </exception>
                            <exception cref=""T:System.NotSupportedException"">
                            <paramref name=""path""/> contains a colon character (:) that is not part of a
                            drive label (""C:\"").</exception>
                            <filterpriority>1</filterpriority>
                            <PermissionSet>
                            <IPermission class=""System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"" version=""1"" Unrestricted=""true""/>
                            </PermissionSet>";

            string plainText = xmlText.XmlToPlainText(true);

            Assert.Equal(@"Creates all directories and subdirectories as specified by path.
--------------------------
Returns: A System.IO.DirectoryInfo as specified by path.
--------------------------
path: The directory path to create.
path2: Fake parameter for testing.
--------------------------
Exceptions: " /*otherwise CodeMade swallows space at the end of line*/+ @"
  System.IO.IOException
  System.UnauthorizedAccessException
  System.ArgumentException
  System.ArgumentNullException
  System.IO.PathTooLongException
  System.IO.DirectoryNotFoundException
  System.NotSupportedException", plainText);
        }

        [Fact]
        public void DocumentationForTooltip()
        {
            string apiDoc = @"<summary>Deletes the specified file.<para>The parameter <paramref name=""path""/> must not be NULL.</para></summary>
<param name=""path"">The name of the file to be deleted. Wildcard characters are
not supported.</param>
<param name=""recursively"">Delete files in subdirectories.</param>
<exception cref=""T:System.ArgumentException"">
<paramref name=""path""/> is a zero-length string, contains only white space, or
contains one or more invalid characters as defined by <see cref=""F:System.IO.Path.InvalidPathChars""/>.
</exception>
<exception cref=""T:System.ArgumentNullException"">
<paramref name=""path""/> is null. </exception>
<exception cref=""T:System.IO.DirectoryNotFoundException"">The specified path is
invalid (for example, it is on an unmapped drive). </exception>
<exception cref=""T:System.IO.IOException"">The specified file is in use. -or-There
is an open handle on the file, and the operating system is Windows XP or earlier.
This open handle can result from enumerating directories and files. For more
information, see How to: Enumerate Directories and Files.</exception>
<exception cref=""T:System.NotSupportedException"">
<paramref name=""path""/> is in an invalid format. </exception>
<exception cref=""T:System.IO.PathTooLongException"">The specified path, file name,
or both exceed the system-defined maximum length. For example, on Windows-based
platforms, paths must be less than 248 characters, and file names must be less
than 260 characters. </exception>
<exception cref=""T:System.UnauthorizedAccessException"">The caller does not have
the required permission.-or- <paramref name=""path""/> is a directory.-or- <paramref name=""path""/>
specified a read-only file. </exception>
<filterpriority>1</filterpriority>
<PermissionSet>
<IPermission class=""System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"" version=""1"" Unrestricted=""true""/>
</PermissionSet>"
                              .XmlToPlainText();

            var expected = @"Deletes the specified file.
The parameter path must not be NULL.

path: The name of the file to be deleted. Wildcard characters are not supported.
recursively: Delete files in subdirectories.

Exceptions: " /*otherwise CodeMade swallows space at the end of line*/+ @"
  System.ArgumentException
  System.ArgumentNullException
  System.IO.DirectoryNotFoundException
  System.IO.IOException
  System.NotSupportedException
  System.IO.PathTooLongException
  System.UnauthorizedAccessException";

            Assert.Equal(expected, apiDoc);
        }
    }
}