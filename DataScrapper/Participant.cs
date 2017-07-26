using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScrapper
{
    /// <summary>
    /// Представление для участника дела
    /// </summary>
    public class Participant
    {
        /// <summary>
        /// Имя/название
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ИНН
        /// </summary>
        public string Inn { get; set; }

        /// <summary>
        /// ОГРН
        /// </summary>
        public string Ogrn { get; set; }

        /// <summary>
        /// Адрес стороны
        /// </summary>
        public string Address { get; set; }
    }
}
