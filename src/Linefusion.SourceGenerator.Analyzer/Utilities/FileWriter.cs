using System;
using System.Collections.Generic;
using System.Text;

namespace Linefusion.SourceGenerator.Analyzer.Utilities
{
    public static class FileWriter
    {
        public static (bool, Exception[]) Write(string path, string contents)
        {
            var success = false;
            var exceptions = new List<Exception>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    File.WriteAllText(path, contents);
                    success = true;
                    break;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    Thread.Sleep(100);
                }
            }
            return (success, exceptions.ToArray());
        }
    }
}
