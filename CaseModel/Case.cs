using DataScrapper;
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

        private static int ToInt(DateTime date)
        {
            return date.Day + date.Month * 100 + date.Year * 10000;
        }

        public Case (CaseDb rawCase, IDictionary<int, string> doctypes, int decision)
        {
            var RID = new Feature(rawCase.RID, "RID");
            features.Add(RID);

            var amount = new Feature(rawCase.Amount, "Amount");
            features.Add(amount);

            var date = new Feature(Case.ToInt(rawCase.Date), "Date");
            features.Add(date);

            var court = new Feature(rawCase.CourtId, "Court");
            features.Add(court);

            var finalInstance = new Feature(rawCase.FinalInstanceId, "FinalInstance");
            features.Add(finalInstance);

            var reconsideration = new Feature(rawCase.Reconsideration, "Reconsideration");
            features.Add(reconsideration);

            var claimantNum = new Feature(rawCase.Claimant == null ? 0 : rawCase.Claimant.Count(), "ClaimantNum");
            features.Add(claimantNum);

            var docNum = new Feature(rawCase.Documents == null ? 0 : rawCase.Documents.Count(), "DocumentNum");
            features.Add(docNum);

            var docs = new DocumentsFeature(rawCase.Documents, doctypes);
            features.Add(docs);

            var category = new Feature(rawCase.CategoryId, "Category");
            features.Add(category);

            var thirdParty = new Feature(rawCase.ThirdParty == null || rawCase.ThirdParty.Count() == 0 ? 0 : 1, "ThirdParty");
            features.Add(thirdParty);

            var petition = new Feature(rawCase.Petition, "Petition");
            features.Add(petition);

            var counterclaim = new Feature(rawCase.Counterclaim, "Counterclaim");
            features.Add(counterclaim);

            var dec = new Feature(decision, "Decision");
            features.Add(dec);
        }
    }
}
