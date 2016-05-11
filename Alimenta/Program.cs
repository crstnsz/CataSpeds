//using SevenZip;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Alimenta
{
    class Program
    {
        const int COPIAR = 40;
        const int COMPLETAR = 5;

        private static bool IniciarCopia(string caminho)
        {
            int lenght = Directory.GetFiles(caminho, "*.*").Length;
            if (lenght < COMPLETAR)
                return true;

            ///foreach (var item in Directory.GetDirectories(caminho))
            ///{
            //     if (IniciarCopia(item))
            //        return true;
            //}
            return false;
        }

        //private static string ValidateDirectory(int index, string path)
        //{
        //    string ch = "_" + index.ToString();
        //    index++;
        //    string newPath = path.Contains(ch) ? path.Replace(ch, "") : path;
        //    if (Directory.Exists(path))
        //        path = ValidateDirectory(index, newPath + "_" + index.ToString());

        //    return path;
        //}

        //private static string NomeNaoDuplicado(string arquivoOriginal)
        //{
        //    string nomeArquivo = arquivoOriginal;
        //    int contador = 0;
        //    while (File.Exists(nomeArquivo))
        //        nomeArquivo = Path.Combine(Path.GetDirectoryName(nomeArquivo), Path.GetFileNameWithoutExtension(arquivoOriginal) + "_" + ++contador + Path.GetExtension(nomeArquivo));
        //    return nomeArquivo;
        //}

        //public static object bloqueiaRodando = new object();
        //public static bool rodando = false;

        //static void IgnorandoPDF(string caminho, bool primeiro)
        //{
        //    if (primeiro)
        //    {
        //        lock (bloqueiaRodando)
        //        {
        //            rodando = true;
        //        }
        //    }


        //    foreach (var item in Directory.GetFiles(caminho, "*.zip"))
        //    {
        //        try
        //        {
        //            //descompactar e copiar arquivos
        //            SevenZip.SevenZipBase.SetLibraryPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7z.dll"));
        //            SevenZipExtractor zipExtractor = new SevenZipExtractor(item);
        //            string pathZip = Path.Combine(Path.GetDirectoryName(item), Path.GetFileNameWithoutExtension(item) + "_" + Path.GetExtension(item).Substring(1));
        //            pathZip = ValidateDirectory(0, pathZip);
        //            Console.WriteLine("DECOMPACTANDO " + pathZip);
        //            zipExtractor.ExtractArchive(pathZip);

        //            File.Move(item, NomeNaoDuplicado(Path.Combine(ConfigurationManager.AppSettings["PASTA_COMPACTADOS"], item)));
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("Erro no arquivo " + item);
        //            Console.WriteLine(ex.Message);
        //        }
        //    }

        //    string[] pdfs = Directory.GetFiles(caminho, "*.pdf");
        //    string fileName = string.Empty;
        //    foreach (var item in pdfs)
        //    {
        //        try
        //        {

        //            string nomeArquivo = Path.Combine(ConfigurationManager.AppSettings["PASTA_PDF"], Path.GetFileName(item));
                    

        //            File.Move(item, NomeNaoDuplicado(nomeArquivo));

        //        }
        //        catch (Exception exp)
        //        {
        //            Console.WriteLine("FN " + fileName);
        //            Console.WriteLine("Falha ao mover pdf " + item + " " + exp.Message);
        //        }
        //    }

        //    string[] dirs = Directory.GetDirectories(caminho);
        //    foreach (var item in dirs)
        //    {
        //        IgnorandoPDF(item, false);
        //    }

        //    if (primeiro)
        //    {
        //        lock (bloqueiaRodando)
        //        {
        //            rodando = true;
        //        }
        //    }
        //}

        static bool BuscarEMover(string caminho, int arquivosMovidos)
        {
            Console.WriteLine("Buscando");
            string[] files2 = Directory.GetFiles(caminho, "*.xml");
            Console.WriteLine("Achei " + files2.Length);

            foreach (var item in files2)
            {
                Console.WriteLine("Abrindo..." + item);
                try
                {
                    XmlDocument doc = new XmlDocument();

                    doc.Load(item);

                    if (doc.SelectSingleNode("procEventoNFe") != null)
                    {
                        File.Move(item, Path.Combine(PastaCce, Path.GetFileName(item)));
                    }
                    else if (doc.SelectSingleNode("nfeProc") != null)
                        File.Move(item, Path.Combine(PastaNfe, Path.GetFileName(item)));


                    //if (Path.GetExtension(item).ToLower() == ".zip" || Path.GetExtension(item).ToLower() == ".pdf")
                    //    continue;

                    //File.Move(item, Path.Combine(ConfigurationManager.AppSettings["PASTA_DESTINO"], Path.GetFileName(item)));
                }
                catch (Exception exp)
                {
                    Console.WriteLine("Falha: " + exp.Message);
                }

                arquivosMovidos++;
                if (arquivosMovidos == COPIAR || Path.GetExtension(item) == ".zip")
                    return true;
            }

            string[] dirs = Directory.GetDirectories(caminho);
            foreach (var item in dirs)
            {
                if (BuscarEMover(item, arquivosMovidos))
                    return true;
            }

            return false;
        }

        private static string PastaNfe;
        private static string PastaCce;

        static void Main(string[] args)
        {
            Console.WriteLine("Alimenta 1.8.0.0");

            //Task pdf = new Task(() => 
            // IgnorandoPDF(ConfigurationManager.AppSettings["PASTA_ORIGEM"], true);
            // IgnorandoDescompactandoZip(ConfigurationManager.AppSettings["PASTA_ORIGEM"], true);
            //    )
             ;

            if (!Directory.Exists(ConfigurationManager.AppSettings["PASTA_DESTINO"]))
            {
                PastaNfe = Path.Combine(ConfigurationManager.AppSettings["PASTA_DESTINO"], "NFE");
                PastaCce = Path.Combine(ConfigurationManager.AppSettings["PASTA_DESTINO"], "CCE");
                Directory.CreateDirectory(PastaNfe);
                Directory.CreateDirectory(PastaCce);
            }


            DateTime ultimaExecucao = DateTime.Now;
            //System.Threading.Thread.Sleep(3000);
            for (;;)
            {
                if (IniciarCopia(ConfigurationManager.AppSettings["PASTA_PENDENTE"]))
                    //&& !TemArquivo(ConfigurationManager.AppSettings["PASTA_PENDENTE"]))
                {
                    BuscarEMover(ConfigurationManager.AppSettings["PASTA_ORIGEM"], 0);
                    ultimaExecucao = DateTime.Now;
                }
                else
                    Console.WriteLine("Descansando..." + DateTime.Now.Subtract(ultimaExecucao).TotalMinutes + " minuto(s)");

                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
