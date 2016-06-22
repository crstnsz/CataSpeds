using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SepararErros
{
    class Program
    {
        static void Main(string[] args)
        {
            var textos = File.ReadAllLines("erros.txt");
            Console.WriteLine("Procurando todos os arquivos de erro");
            var arqs = Directory.GetFiles(@"\\drb-web2\ENTRADA\ERRO\NFE\VERIFICAR ERROS\XMLs usuários", "*_ERR.csv", SearchOption.AllDirectories);
            Console.WriteLine("iniciando processamento");
            var count = 0;
            foreach (var arq in arqs)
            {
                if (args.Contains("\\VERIFICAR ERROS\\"))
                    continue;

                try
                {
                    var conteudo = File.ReadAllText(arq);
                    foreach (var texto in textos)
                    {
                        var pastaDestino = Path.Combine(@"\\drb-web2\ENTRADA\ERRO\NFE", "VERIFICAR ERROS", texto.Replace(":", "_").Replace("\\", "_").Replace("/", "_").Replace("?", "_").Replace("*", "_").Replace("\"", "_").Replace(">", "_").Replace("<", "_").Replace("|", "_"));

                        if (!Directory.Exists(pastaDestino))
                            Directory.CreateDirectory(pastaDestino);

                        if (conteudo.Contains(texto))
                        {
                            if (!File.Exists(Path.Combine(pastaDestino, Path.GetFileName(arq))))
                                File.Move(arq, Path.Combine(pastaDestino, Path.GetFileName(arq)));
                            var arqxml = arq.Replace("_ERR.csv", ".xml");
                            if (File.Exists(arqxml))
                                File.Move(arqxml, Path.Combine(pastaDestino, Path.GetFileName(arqxml)));
                            break;
                        }
                    }

                    if (++count % 1000 == 0)
                        Console.WriteLine(arq);
                }
                catch(Exception exp )
                {
                    Console.WriteLine(exp.Message + exp.StackTrace);
                    Console.ReadLine();
                }
            }

        }
    }
}
