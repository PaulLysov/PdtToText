using ConvertDocsAPI;
using System;

namespace PdtToText
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();

            if (args is null || args.Length < 2)
            {

                Console.WriteLine("Start work");

                var defaultPath = $"{System.IO.Directory.GetCurrentDirectory()}\\convert";
                int convertCount = new PdfToTextConvert().ConvertFromFolder(defaultPath, false);

                Console.WriteLine($"Work complete, convert files = [{convertCount}]");
            }

            if (args.Length > 1)
            {
                try
                {
                    var service = new PdfToTextConvert();
                    var result = service.GetTextFromFileByPath(args[0]);
                    service.SaveTextToFile(result, args[1]).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return;
        }
    }
}
