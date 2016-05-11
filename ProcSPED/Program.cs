using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OracleClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcSPED
{
    class Program
    {
        //FISCAL
        public const int DAT_INICIO_FISCAL = 4;
        public const int DAT_FIN_FISCAL = 5;
        public const int NOME_FISCAL = 6;
        public const int CNPJ_FISCAL = 7;

        //PIS
        public const int DAT_INICIO_PIS = 6;
        public const int DAT_FIN_PIS = 7;
        public const int NOME_PIS = 8;
        public const int CNPJ_PIS = 9;

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("USAR: ProcSPED [DIRETORIO]");
                return;
            }

            using (OracleConnection conexao = new OracleConnection("Data Source=ORA_EXTRAT;User ID=REPFIS;Password=allu19ypoq;Unicode=True"))
            {
                conexao.Open();
                using (var writer = File.CreateText("relatorio.csv"))
                {
                    Procurar(@"I:\FIAT", writer, conexao);
                }
            }
            Console.ReadLine();
        }

        enum Tipo
        {
            Fiscal = 1,
            EFD = 2
        }


        private static int ExisteNaBase(OracleConnection conexao, string cnpj, string dataInicio, string dataFim, Tipo tipo)
        {

            OracleCommand existe = new OracleCommand("SELECT 1 FROM SPED_" + (tipo == Tipo.EFD ? "PIS" : "FISCAL") + "_REG0000 "
                + "WHERE CNPJ = :Cnpj AND DT_INI = :DtIni AND DT_FIN = :DtFin", conexao);

            var cnpjPar = existe.CreateParameter();
            cnpjPar.ParameterName = "Cnpj";
            cnpjPar.Size = 14;
            cnpjPar.DbType = System.Data.DbType.String;
            cnpjPar.Value = cnpj;


            var dtIni = existe.CreateParameter();
            dtIni.ParameterName = "DtIni";
            dtIni.Size = 10;
            dtIni.DbType = System.Data.DbType.String;
            dtIni.Value = dataInicio;

            var dtFin = existe.CreateParameter();
            dtFin.ParameterName = "DtFin";
            dtFin.Size = 10;
            dtFin.DbType = System.Data.DbType.String;
            dtFin.Value = dataFim;


            existe.Parameters.Add(cnpjPar);
            existe.Parameters.Add(dtIni);
            existe.Parameters.Add(dtFin);

            OracleNumber retorno = (OracleNumber)existe.ExecuteOracleScalar();

            return Convert.ToInt32(retorno.IsNull?0:1);
        }




        private static void Relatorio(OracleConnection conexao, StreamWriter writer, string arquivo, string cnpj, string dataInicio, string dataFim, Tipo tipo)
        {

            DateTime periodo = new DateTime(
                Convert.ToInt32(dataInicio.Substring(4, 4)), 
                Convert.ToInt32(dataInicio.Substring(2, 2)), 
                Convert.ToInt32(dataInicio.Substring(0, 2)));


            //                ARQUIVO, MES, ANO, TIPO, TEM NO BANCO, TEM NO REPOSITORIO 
            writer.Write(arquivo);
            writer.Write(";");
            writer.Write(periodo.Month);
            writer.Write(";");
            writer.Write(periodo.Year);
            writer.Write(";");
            writer.Write(tipo == Tipo.EFD ? "EFD" : "FISCAL");
            writer.Write(";");
            int existeNaBase = ExisteNaBase(conexao, cnpj, dataInicio, dataFim, tipo);
            writer.Write(existeNaBase);
            writer.Write(";");
            int existeNoRepositorio = ExisteNoRepositorio(cnpj, periodo, tipo);
            writer.Write(existeNoRepositorio);

            InsereParaPesquisar(conexao, arquivo, cnpj, dataInicio, dataFim, tipo, existeNaBase, existeNoRepositorio);

        }

        private static void InsereParaPesquisar(OracleConnection conexao, string arquivo, string cnpj, string dataInicio, string dataFim, Tipo tipo, int existeNaBase, int existeNoRepositorio)
        {
            OracleCommand insert = new OracleCommand("INSERT INTO SPED_VERIFICAO (ARQUIVO, CNPJ, DT_INI, DT_FIM, TIPO, EXISTE_BASE, EXISTE_REPOS) VALUES (:Arquivo, :Cnpj, :DtIni, :DtFin, :Tipo, :Base, :Repos) ", conexao);
            insert.Parameters.AddWithValue("Arquivo", arquivo);
            insert.Parameters.AddWithValue("Cnpj", cnpj);
            insert.Parameters.AddWithValue("DtIni", dataInicio);
            insert.Parameters.AddWithValue("DtFin", dataFim);
            insert.Parameters.AddWithValue("Tipo", (int)tipo);
            insert.Parameters.AddWithValue("Base", (int)existeNaBase);
            insert.Parameters.AddWithValue("Repos", (int)existeNoRepositorio);
            insert.ExecuteNonQuery();
        }


        private static int ExisteNoRepositorio(string cnpj, DateTime periodo, Tipo tipo)
        {
            string dirRaiz = @"\\drb-web2\REPOSITORIO_FISCAL\";
            return Convert.ToInt32(File.Exists(Path.Combine(dirRaiz, (tipo == Tipo.EFD?"EFD_CONTRIBUICOES":"SPED_FISCAL"), cnpj.Substring(0, 8), cnpj, string.Format("{0}-{1:00}-{2:00}.TXT", cnpj, periodo.Year, periodo.Month))));
        }

        static void Procurar(string diretorio, StreamWriter writer, OracleConnection conexao)
        {
            string[] arquivos = Directory.GetFiles(diretorio, "*.*");
            foreach (var arquivo in arquivos)
            {

                using (var reader = File.OpenText(arquivo))
                {
                    string line = String.Empty;
                    if ((line = reader.ReadLine()) != null)
                    {
                        if (!line.StartsWith("|"))
                            continue;
                        if (line.Length < 6)
                            continue;
                        if (line[5] != '|')
                            continue;
                        if (string.IsNullOrEmpty(line))
                            continue;

                        string[] fields = line.Split('|');
                        if (fields.Length > 2)
                        {
                            //SE FOR A PRIMEIRA LINHA IDENTIFICA SE É ARQUIVO DE PIS OU FISCAL
                            if (fields[1] == "0000")
                            {

                                if (fields[DAT_FIN_FISCAL].Length == 8 && fields[DAT_FIN_FISCAL].Length == 8)
                                {
                                    var dataIni = fields[DAT_INICIO_FISCAL];
                                    var dataFim = fields[DAT_FIN_FISCAL];
                                    writer.WriteLine("{0};{1};{2};FISCAL;{3}", fields[CNPJ_FISCAL], dataIni, dataFim, arquivo);
                                    Console.WriteLine("{0};{1};{2};FISCAL;{3}", fields[CNPJ_FISCAL], dataIni, dataFim, arquivo);
                                    Relatorio(conexao, writer, arquivo, fields[CNPJ_FISCAL], dataIni, dataFim, Tipo.Fiscal);
                                }
                                else
                                    if (fields[DAT_INICIO_PIS].Length == 8 && fields[DAT_FIN_PIS].Length == 8)
                                    {
                                        var dataIni = fields[DAT_INICIO_PIS];
                                        var dataFim = fields[DAT_FIN_PIS];
                                        writer.WriteLine("{0};{1};{2};PIS;{3}", fields[CNPJ_PIS], dataIni, dataFim, arquivo);
                                        Console.WriteLine("{0};{1};{2};PIS;{3}", fields[CNPJ_PIS], dataIni, dataFim, arquivo);

                                        Relatorio(conexao, writer, arquivo, fields[CNPJ_PIS], dataIni, dataFim, Tipo.EFD);
                                    }

                            }
                        }
                    }
                }
            }


            var dirs = Directory.GetDirectories(diretorio);
            foreach (var dir in dirs)
            {
                Procurar(dir, writer, conexao);
            }

        }
    }
}
