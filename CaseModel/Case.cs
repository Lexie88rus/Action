using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseModel
{
    public class Case : ICase
    {
        /// <summary>
        /// Фичи
        /// </summary>
        private List<IFeature> features = new List<IFeature>();

        /// <summary>
        /// Фичи
        /// </summary>
        public IEnumerable<IFeature> Features {get{ return features; } }

   
        public Case (IEnumerable<IFeature> features): base()
        {
            this.features = features.ToList();
        }
    }
}
