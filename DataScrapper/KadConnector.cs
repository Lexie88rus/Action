using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScrapper
{
    /// <summary>
    /// Класс для получения данных из БД Access
    /// </summary>
    public class KadConnector
    {
        /// <summary>
        /// Коннект к БД Access
        /// </summary>
        private OleDbConnection conn;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="path">путь к файлу</param>
        public KadConnector(String path)
        {
            if (String.IsNullOrEmpty(path)) throw new ArgumentNullException("path");

            try
            {
                conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source="+ path + ";Persist Security Info=False;");
                conn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось создать подключение к БД Access", ex);
            }
        }


        /// <summary>
        /// Выполнение запроса
        /// </summary>
        /// <param name="query">запрос</param>
        /// <returns>данные для чтения из БД</returns>
        public OleDbDataReader ExecuteQuery(String query)
        {
            if (String.IsNullOrEmpty(query)) throw new ArgumentNullException("query");

            OleDbDataReader reader = null;

            try
            {
                OleDbCommand oleQuery = new OleDbCommand(query, conn);
                reader = oleQuery.ExecuteReader();
                return reader;
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось выполнить запрос", ex);
            }
        }

        public void CloseConnection()
        {
            this.conn.Close();
        }
    }
}
