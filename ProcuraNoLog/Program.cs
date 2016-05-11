using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProcuraNoLog
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] arquivo = Directory.GetFiles(@"E:\ENTRADA\LOG");


            Console.Write("Informe o nome do arquivo procurado ou padrão procurado:");
            string padrao = Console.ReadLine();

            Console.WriteLine("Iniciando Pesquisa:");
            //Console.WriteLine("0%======20%=======40%=======60%=======80%=====100%");

            
            //double pbs = arquivo.Length / 50;
            //double pbl = pbs;
            //double pbc = pbs;

            List<string> arquivos = new List<string>();
            foreach (var item in arquivo)
            {
                //if (pbs == (pbc += pbl))
                //{
                //    pbs += pbl;
                //    for (int i = 0; i < pbl; i++)
                //        Console.Write("=");
                //}

                Regex reg = new Regex(padrao);
                
                using (TextReader tr = File.OpenText(item))
                {
                    string linha;
                    while ((linha = tr.ReadLine()) != null)
                    {
                        if (reg.IsMatch(linha))
                            arquivos.Add(item);
                    }

                }
            }

            Console.WriteLine("Achou");
            foreach (var item in arquivos)
                Console.WriteLine(item);

            Console.ReadLine();

        }
    }
}
