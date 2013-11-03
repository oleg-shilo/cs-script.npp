//css_args /ac
//css_ref lib\taglib-sharp.dll
using System.IO;
using System;

void main()
{
    foreach(string file in Directory.GetFiles(".", "*.mp3"))
    {
        var mp3 = TagLib.File.Create(file);
        string dir = Path.GetDirectoryName(file);
        string fileName = string.Format(" { 00 }. { 1 }.mp3", mp3.Tag.Track, mp3.Tag.Title);
        File.Move(file, Path.Combine(dir, fileName));
    }

    Console.WriteLine("________________________");
    Console.WriteLine("Done...");
}