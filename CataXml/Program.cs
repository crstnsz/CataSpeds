using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CataXml
{
    class Program
    {
        HashSet<string> diretoriosEncontrados = new HashSet<string>();
        static long contador = 0;
        static DateTime check = DateTime.Now;

        static void Main(string[] args)
        {
            long fileSize = 0;
            fileSize += ProcuraEmSubdir(@"C:\", true);
            fileSize += ProcuraEmSubdir(@"E:\", true);
            Print(fileSize);
            Console.ReadLine();


        }

        private static void Print(long fileSize)
        {
            Console.WriteLine("Total em Bytes? {0}", fileSize);
            Console.WriteLine("Total em Mega Bytes? {0}", fileSize / (1024 * 1024));
            Console.WriteLine("Total em Giba Bytes? {0}", fileSize / (1024 * 1024 * 1024));
        }

        static long ProcuraEmSubdir(string diretorioRaiz, bool first)
        {
            long fileSize = 0;
            try
            {
                string[] arquivos = Directory.GetFiles(diretorioRaiz, "*.dmp");
                if (arquivos.Length > 0)
                    File.AppendAllText("E:/Tmp/lista_dmp.txt", diretorioRaiz + Environment.NewLine);

                foreach (var item in arquivos)
                    fileSize += new FileInfo(item).Length;

                string[] dir = Directory.GetDirectories(diretorioRaiz);

                foreach (var item in dir)
                {
                    if (first)
                        Console.WriteLine(item);
                    fileSize += ProcuraEmSubdir(item, false);

                    if (first)
                        Print(fileSize);

                    if (DateTime.Now.Subtract(check).TotalMinutes > 1)
                    {
                        check = DateTime.Now;
                        Console.WriteLine(item);
                    }
                }
            }
            catch (Exception exp)
            {

            }

            return fileSize;

        }
    }
}
