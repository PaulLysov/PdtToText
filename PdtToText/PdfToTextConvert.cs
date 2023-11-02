using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace ConvertDocsAPI
{
    public class PdfToTextConvert
    {
        private readonly bool _hasDelete;
        private readonly bool _enableDebugLog;
        private readonly string _defaultPath;

        public int ConvertFromFolder(string path, bool hasDelete)
        {
            if (string.IsNullOrEmpty(path))
                path = _defaultPath;

            hasDelete = _hasDelete || hasDelete;

            List<string> files = !string.IsNullOrEmpty(Path.GetExtension(path))
                ? new List<string> { path }
                : Directory.GetFiles(path).Where(x => x.EndsWith(".pdf")).ToList();


            var tasks = new List<Task>();
            Parallel.ForEach(files, file =>
            {
                tasks.Append(ConvertFile(file, hasDelete));
            });
            Task.WaitAll(tasks.ToArray());
            return files.Count;
        }

        public async Task ConvertFile(string path, bool hasDelete)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return;

                string filePath = Path.GetDirectoryName(path);
                string fileName = Path.GetFileName(path);
                string fileExtension = Path.GetExtension(fileName);
                string changeExtension = fileName.Replace(fileExtension, ".txt");

                string resultFilePath = filePath + "\\" + changeExtension;

                if (fileExtension == ".pdf")
                    await SaveTextToFile(GetTextFromFileByPath(path), resultFilePath);

                if (hasDelete)
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string EncodeString(string unicodeString, Encoding from, Encoding to)
        {
            // Convert the string into a byte array.
            byte[] unicodeBytes = from.GetBytes(unicodeString);

            // Perform the conversion from one encoding to the other.
            byte[] asciiBytes = Encoding.Convert(from, to, unicodeBytes);

            // Convert the new byte[] into a char[] and then into a string.
            char[] asciiChars = new char[to.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            to.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            string asciiString = new string(asciiChars);
            return asciiString;
        }

        public string[] GetTextFromFileByPath(string filePath)
        {
            using (PdfReader reader = new PdfReader(filePath))
            {
                StringBuilder sb = new StringBuilder();
                ITextExtractionStrategy its = new LocationTextExtractionStrategy();
                //ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    var str = PdfTextExtractor.GetTextFromPage(reader, i, its);
                    //var test = EncodeString(str,Encoding.Unicode, Encoding.UTF8);
                    sb.Append(str);
                }
                return sb.ToString().Split(Environment.NewLine.ToCharArray());
            }
        }

        public async Task SaveTextToFile(IEnumerable<string> data, string filePath)
        {
            await File.WriteAllLinesAsync(filePath, data);
        }
    }
}
