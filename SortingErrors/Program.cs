﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace SortingErrors
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var directorypath = @"E:\tmp\Lingvistic-1\Lingvistic-1";
                var files = new ConcurrentBag<Tuple<string, string, string>>();
                Directory.EnumerateFiles(directorypath).AsParallel().ForAll(p =>
                {
                    var text = File.ReadAllText(p);
                    var name = Path.GetFileName(p);
                    var stackTraceIndex = text.IndexOf("Server stack trace:");
                    if (stackTraceIndex == -1)
                    {
                        stackTraceIndex = text.IndexOf("System.Threading.ThreadAbortException:");
                    }
                    if (stackTraceIndex == -1)
                    {
                        stackTraceIndex = text.IndexOf("System.ServiceModel.FaultException");
                    }
                    var docIdIndex = text.IndexOf("DocID");
                    var textIndex = text.IndexOf("Text=", text.Length / 2);

                    string stackTrace;
                    if (stackTraceIndex == -1 || docIdIndex == -1)
                    {
                        stackTrace = string.Empty;
                    }
                    else
                    {
                        stackTrace = text.Substring(stackTraceIndex, docIdIndex - stackTraceIndex);
                    }
                    if (textIndex != -1)
                    {
                        text = text.Substring(textIndex + "Text=".Length);
                    }
                    files.Add(Tuple.Create(name, text, stackTrace));
                });
                var groupId = 0;
                foreach (var group in files.GroupBy(t => t.Item3))
                {
                    var directoryPath = Path.Combine(directorypath, groupId.ToString());
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    foreach (var tuple in group)
                    {
                        var text = tuple.Item2;
                        text += "\n\n\n<<<<<<<<<<<<<<<<<>>>>>>>>>>>>>>>>>>\n\n\n";
                        text += tuple.Item3;
                        File.WriteAllText(Path.Combine(directoryPath, tuple.Item1), text);
                    }
                    groupId++;
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
                throw;
            }
        }
    }
}
