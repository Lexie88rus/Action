using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseModel
{
    /// <summary>
    /// Фича для выгрузки в вектор данных
    /// </summary>
    public interface IFeature
    {
        /// <summary>
        /// Значение фичи
        /// </summary>
        int [] Value { get; }

        /// <summary>
        /// Название фичи
        /// </summary>
        String [] Name { get; }

        /// <summary>
        /// Выгружать в вектор
        /// </summary>
        bool ToBeExported { get; set; }
    }
}
