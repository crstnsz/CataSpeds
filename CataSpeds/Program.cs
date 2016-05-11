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
            try
            {
                string[] baseDir = {
                   @"\\srv-web\e$\Ambientes\TP",
                   @"\\srv-web2\Ambientes\TP"};

                long fileSize = 0;

                foreach (var item in baseDir)
                {
                    fileSize += ProcuraEmSubdir(item, true);
                }
                Print(fileSize);

                Console.ReadLine();
            }
            catch (Exception exp)
            {
                File.AppendAllText("erro" + DateTime.Now.Day + " " + DateTime.Now.Hour+  ".txt", exp.Message);
            }


        }

        static void Main_(string[] args)
        {
            //long fileSize = ProcuraEmSubdir(@"\\srv-ts2\REPOSITORIO4\REINTEGRA\CLIENTES\Aker\SPED Fiscal");

            long fileSize = CadeMeuSped(@"\\drb-web2.becomex.corp\E$\", true, "17469701002897");
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

        private static bool IN(string value, params string[] list)
        {
            foreach (var item in list)
            {
                if (item == value)
                    return true;
            }

            return false;
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

                string[] arquivos = Directory.GetFiles(diretorioRaiz, "*.*");

                foreach (var arquivo in arquivos)
                {
                    try
                    {
                        string extension = Path.GetExtension(arquivo).ToLower();
                        if (!IN(extension, ".xml", ".zip", ".rar", ".db", ".dat"))
                        {
                            bool igual = true;
                            using (StreamReader sr = new StreamReader(arquivo))
                            {
                                char[] vec = new char[] { '|', '0', '0', '0', '0', '|' };

                                int i = 0;
                                for (; i < vec.Length; i++)
                                    if (sr.Read() != vec[i])
                                    {
                                        igual = false;
                                        break;
                                    }

                                if (i != vec.Length)
                                    igual = false;
                            }


                            if (igual)
                            {
                                string destino = Path.GetFileNameWithoutExtension(arquivo) + "_" + (++contador).ToString() + Path.GetExtension(arquivo);

                                try
                                {
                                    File.Copy(arquivo, Path.Combine(@"E:\ENTRADA\CRISTIANO", destino));
                                }
                                catch (Exception exp)
                                {
                                    try
                                    {
                                        File.Copy(arquivo, Path.Combine(@"E:\ENTRADA\CRISTIANO", destino));
                                        Console.WriteLine("Copiado");
                                        try
                                        {
                                            File.Delete(arquivo);
                                            Console.WriteLine("Deletado");
                                        }
                                        catch
                                        {
                                            Console.WriteLine("Realmente não apaga");
                                            File.AppendAllText(Path.GetFileNameWithoutExtension(destino) + "_erro.txt", arquivo + exp.Message + Environment.NewLine);
                                        }
                                    }
                                    catch
                                    {
                                        File.AppendAllText(Path.GetFileNameWithoutExtension(destino) + "_erro.txt", arquivo + exp.Message + Environment.NewLine);
                                    }


                                }
                                fileSize += new FileInfo(arquivo).Length;
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        if (exp.Message.StartsWith("The process cannot access the file"))
                        {
                            File.AppendAllText("PENDURA.txt", arquivo + "=>" + exp.Message + Environment.NewLine);
                        }
                    }
                }

                //foreach (var arquivo in Directory.GetFiles(diretorioRaiz, "*.ZIP"))
                //{
                //    // File.AppendAllText("relatorio_marina.txt", arquivo + Environment.NewLine);
                //    File.Copy(arquivo, Path.Combine(@"E:\ENTRADA\ZIP_REINTEGRA", Path.GetFileNameWithoutExtension(arquivo) + (++contador).ToString() + Path.GetExtension(arquivo)));
                //}

                //foreach (var arquivo in Directory.GetFiles(diretorioRaiz, "*.RAR"))
                //{
                //    //File.AppendAllText("relatorio_marina.txt", arquivo + Environment.NewLine);
                //    File.Copy(arquivo, Path.Combine(@"E:\ENTRADA\ZIP_REINTEGRA", Path.GetFileNameWithoutExtension(arquivo) + (++contador).ToString() + Path.GetExtension(arquivo)));
                //}

                //foreach (var arquivo in Directory.GetFiles(diretorioRaiz, "*.7z"))
                //{
                //    //File.AppendAllText("relatorio_marina.txt", arquivo + Environment.NewLine);
                //    File.Copy(arquivo, Path.Combine(@"E:\ENTRADA\ZIP_REINTEGRA", Path.GetFileNameWithoutExtension(arquivo) + (++contador).ToString() + Path.GetExtension(arquivo)));
                //}


            }
            catch (UnauthorizedAccessException uexp)
            {
                File.AppendAllText("erro.txt", diretorioRaiz + uexp.Message + Environment.NewLine);
            }
            catch (PathTooLongException exp)
            {
                File.AppendAllText("erro.txt", diretorioRaiz + exp.Message + Environment.NewLine);
            }

            return fileSize;

        }


        static long CadeMeuSped(string diretorioRaiz, bool first, string cnpj)
        {
            long fileSize = 0;
            try
            {
                string[] dir = Directory.GetDirectories(diretorioRaiz);

                foreach (var item in dir)
                {
                    if (first)
                        Console.WriteLine(item);
                    fileSize += CadeMeuSped(item, false, cnpj);

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
                        string line = sr.ReadLine();
                        if (line.StartsWith("|"))
                        {
                            if (line.Contains(cnpj))
                                Console.WriteLine(arquivo);
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
