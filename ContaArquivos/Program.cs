using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaArquivos
{
    class Program
    {
        public class Valor
        {
            public long Contar;
            public long Tamanho;
        }

        static DateTime check = DateTime.Now;
        static SortedDictionary<string, Valor> armazen = new SortedDictionary<string, Valor>();

        static void Main(string[] args)
        {
            //ProcuraEmSubdir("C:/");
            //ProcuraEmSubdir("E:/");

            ProcuraLog(@"\\srv-web\e$\Ambientes\TP");
            ProcuraLog(@"\\srv-web2\Ambientes\TP");


            //string[] linhas = new string[armazen.Count];
            //int i = 0;
            //foreach (var item in armazen)
            //    linhas[i++] = string.Format("Tipo {0} Quantidade {1} Tamanho {2}", item.Key, item.Value.Contar, item.Value.Tamanho);

            //File.AppendAllLines("resumo.txt", linhas);

            Console.WriteLine("Acabou....");
            Console.ReadLine();

        }
        static void ProcuraLog(string diretorioRaiz)
        {
            Console.Write(".");

            string[] arquivos = new string[] { };
            try
            {
                arquivos = Directory.GetFiles(diretorioRaiz, "*.log");

                string[] arquivosLog = new string[arquivos.Length];
                int i = 0;
                foreach (var arq in arquivos)
                    arquivosLog[i++] = string.Format("{0}; {1}" + Environment.NewLine, arq, new FileInfo(arq).Length);

                File.AppendAllLines("arquivosLog.csv", arquivosLog);

                string[] diretorios = new string[] { };

                try
                {
                    diretorios = Directory.GetDirectories(diretorioRaiz);

                    foreach (var dir in diretorios)
                        ProcuraLog(dir);
                }
                catch
                {

                }
            }
            catch
            {

            }
        }

        static void ProcuraEmSubdir(string diretorioRaiz)
        {

            long fileSize = 0;
            try
            {
                string[] arquivos = Directory.GetFiles(diretorioRaiz, "*.*");
                foreach (var arq in arquivos)
                {
                    string ext = Path.GetExtension(arq).ToUpper();
                    if (!armazen.ContainsKey(ext))
                        armazen.Add(ext, new Valor() { Contar = 1, Tamanho = new FileInfo(arq).Length });
                    else
                    {
                        armazen[ext].Contar++;
                        armazen[ext].Tamanho += new FileInfo(arq).Length;
                    }

                    if (DateTime.Now.Subtract(check).TotalMinutes > 1)
                    {
                        check = DateTime.Now;
                        Console.Clear();
                        foreach (var item in armazen)
                            Console.WriteLine("Tipo {0} Quantidade {1} Tamanho {2}", item.Key, item.Value.Contar, item.Value.Tamanho);
                    }

                }
            }
            catch (Exception exp)
            {

            }

            try
            {

                string[] dir = Directory.GetDirectories(diretorioRaiz);

                foreach (var item in dir)
                {
                    ProcuraEmSubdir(item);

                    if (DateTime.Now.Subtract(check).TotalMinutes > 1)
                    {
                        check = DateTime.Now;
                        Console.Clear();
                        foreach (var linha in armazen)
                            Console.WriteLine("Tipo {0} Quantidade {1} Tamanho {2}", linha.Key, linha.Value.Contar, linha.Value.Tamanho);
                    }
                }
            }
            catch (Exception exp)
            {

            }
        }
    }
}
