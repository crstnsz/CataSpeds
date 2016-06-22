using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CataXml
{
    class Program
    {
        static int reindex = 0;

        private static void MoverSemSubstituir(string de, string para, bool copiar)
        {
            string original = para;
            try
            {
                while (File.Exists(para))
                    para = Path.Combine(Path.GetDirectoryName(original), Path.GetFileNameWithoutExtension(original) + (++reindex).ToString() + Path.GetExtension(original));
                if (copiar)
                    File.Copy(de, para);
                else
                    File.Move(de, para);
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
                Console.WriteLine("DE:" + de);
                Console.WriteLine("PARA:" + para);
                Console.ReadLine();
            }

            while (Directory.GetFiles(Path.GetDirectoryName(para)).Length > 1000)
            {
                Console.WriteLine("Aguardando processar");
                System.Threading.Thread.Sleep(30000);
            }

        }

        enum PraOnde
        {
            Nfe,
            Evento,
            FicaAi
        }

        private static PraOnde Vai(XmlNodeList list)
        {
            foreach (XmlNode no in list)
//              if (no.LocalName == "procEventoNFe")
//                    return PraOnde.Evento;
//                else 
                if (no.LocalName == "nfeProc")
                    return PraOnde.Nfe;
            return PraOnde.FicaAi;

        }
        HashSet<string> diretoriosEncontrados = new HashSet<string>();
        static DateTime check = DateTime.Now;
        static bool BuscarEMover(string caminho, bool copiar)
        {
            Console.WriteLine("Buscando em " + caminho);
            string[] files2 = null;
            try
            {
                files2 = Directory.GetFiles(caminho, "*.xml");
            }
            catch(Exception exp)
            {
                Console.WriteLine("Falha: " + exp.Message);
                return true;
            }
            Console.WriteLine("Achei " + files2.Length);

            DateTime aliviaAi = DateTime.Now;
            int proc = 0;
            foreach (var item in files2)
            {
                Console.WriteLine("Abrindo..." + item);
                try
                {

                    if (DateTime.Now.Hour > 14 && DateTime.Now.Hour < 19)
                    {
                        if (proc > 0 && DateTime.Now.Subtract(aliviaAi).TotalMilliseconds > 1000)
                        {
                            System.Threading.Thread.Sleep(500);
                            Console.WriteLine("Aliviando o lado da galera");
                            aliviaAi = DateTime.Now;
                        }
                    }

                    XmlDocument doc = new XmlDocument();

                    doc.Load(item);


                    switch (Vai(doc.ChildNodes))
                    {
                        case PraOnde.Evento:
                            MoverSemSubstituir(item, Path.Combine(PastaCce, Path.GetFileName(item)), copiar);
                            break;
                        case PraOnde.Nfe:
                            proc++;
                            MoverSemSubstituir(item, Path.Combine(PastaNfe, Path.GetFileName(item)), copiar);
                            break;
                    }
                }
                catch (Exception exp)
                {
                    Console.WriteLine("Falha: " + exp.Message);
                }

            }



            string[] dirs = null;

            for (;;)
            {
                try
                {
                    dirs = Directory.GetDirectories(caminho);
                    break;
                }
                // Em caso de queda na rede espera voltar e tenta novamente
                catch (DirectoryNotFoundException dexp)
                {
                    Console.WriteLine("EXP: " + dexp.Message);
                    System.Threading.Thread.Sleep(60000);
                }
                catch (UnauthorizedAccessException uaexp)
                {
                    File.AppendAllText("ErrosAcesso.txt", @"
===================================================================================
" + caminho + @"
" + uaexp.Message + @"
" + uaexp.StackTrace + @"
===================================================================================");
                }
                catch (PathTooLongException ptlexp)
                {
                    File.AppendAllText("ErrosAcesso.txt", @"
===================================================================================
" + caminho + @"
" + ptlexp.Message + @"
" + ptlexp.StackTrace + @"
===================================================================================");
                }
            }

            foreach (var item in dirs)
            {
                if (Path.GetFileName(item) == "System Volume Information")
                    continue;
                if (BuscarEMover(item, copiar))
                    return true;
            }

            return false;
        }

        private static string PastaNfe;
        private static string PastaCce;

        static void Main(string[] args)
        {
            Console.WriteLine("Procura XML 2.0.0.0");

            try
            {

                //Task pdf = new Task(() => 
                // IgnorandoPDF(ConfigurationManager.AppSettings["PASTA_ORIGEM"], true);
                // IgnorandoDescompactandoZip(ConfigurationManager.AppSettings["PASTA_ORIGEM"], true);
                //    )
                ;

                PastaNfe = Path.Combine(ConfigurationManager.AppSettings["PASTA_DESTINO"], "NFE");
                PastaCce = Path.Combine(ConfigurationManager.AppSettings["PASTA_DESTINO"], "CCE");
                Directory.CreateDirectory(PastaNfe);
                Directory.CreateDirectory(PastaCce);
                Directory.CreateDirectory(PastaCce);
                BuscarEMover(@"\\becomex.corp\", false);
                //BuscarEMover(@"\\becomex.corp\e$", false);
                //BuscarEMover(@"\\srv-web\e$\Ambientes\TP", true);
                //BuscarEMover(@"\\srv-web2\Ambientes\TP", true);
                BuscarEMover(@"\\srv-ts2.becomex.corp\c$", false);
                BuscarEMover(@"\\becomex-superhp\Repositório", true);
                Console.WriteLine("Terminou");
                Console.ReadLine();

            }
            catch(Exception exp)
            {
                Console.WriteLine(exp.Message);
                Console.WriteLine(exp.StackTrace);
                Console.ReadLine();
            }


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
                string[] arquivos = Directory.GetFiles(diretorioRaiz, "*.xml");
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
