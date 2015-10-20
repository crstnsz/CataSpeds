using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CataSpeds
{
    class Program
    {
        static long contador = 0;
        static DateTime check = DateTime.Now;

        static void Main(string[] args)
        {
            //long fileSize = ProcuraEmSubdir(@"\\srv-ts2\REPOSITORIO4\REINTEGRA\CLIENTES\Aker\SPED Fiscal");
            
            long fileSize = ProcuraEmSubdir(@"\\srv-ts2\REPOSITORIO4\REINTEGRA\CLIENTES", true);
            //fileSize += ProcuraEmSubdir(@"\\srv-ts2\REPOSITORIO4", true);

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

                string[] arquivos = Directory.GetFiles(diretorioRaiz, "*.txt");

                foreach (var arquivo in arquivos)
                {
                    using (StreamReader sr = new StreamReader(arquivo))
                    {
                        if (sr.Read() == (int)'|')
                        {
                            fileSize += new FileInfo(arquivo).Length;

                            File.Copy(arquivo, Path.Combine(@"E:\ENTRADA\_PENDENTES", Path.GetFileNameWithoutExtension(arquivo) + (++contador).ToString() + Path.GetExtension(arquivo)));
                        }
                    }
                }

                foreach (var arquivo in Directory.GetFiles(diretorioRaiz, "*SPED*.ZIP"))
                {
                    File.Copy(arquivo, Path.Combine(@"E:\SPED", Path.GetFileNameWithoutExtension(arquivo) + (++contador).ToString() + Path.GetExtension(arquivo)));
                }

                foreach (var arquivo in Directory.GetFiles(diretorioRaiz, "*SPED*.RAR"))
                {
                    File.Copy(arquivo, Path.Combine(@"E:\SPED", Path.GetFileNameWithoutExtension(arquivo) + (++contador).ToString() + Path.GetExtension(arquivo)));
                }


            }
            catch (PathTooLongException exp)
            {

            }

            return fileSize;

        }
    }
}
