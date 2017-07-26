using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseModel
{
    /// <summary>
    /// Дело
    /// </summary>
    public interface ICase
    {
        /// <summary>
        /// Фичи
        /// </summary>
         IEnumerable<IFeature> Features { get; }
    }
}
