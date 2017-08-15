using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScrapper
{
    /// <suУmmary>
    /// Участники дела
    /// </summary>
    public enum Participants
    {
        Respondent = 1,
        Claimant = 2,
        ThirdParty = 7
    }

    /// <summary>
    /// Инстанции
    /// </summary>
    public enum Instances
    {
        FirstInstance = 1
    }
    /// <summary>
    /// Типы решений суда
    /// </summary>
    public enum Decisions
    {
        Refuse = 0,
        Partial = 1,
        Fully =2
    }

    public enum FormsOwnership
    {
        Unknown = -1,
        OAO = 1,
        ZAO = 2,
        Gov = 3,
        OtherComp = 4,
        IndividualEntrepreneur = 5,
        Person =6
    }
}
