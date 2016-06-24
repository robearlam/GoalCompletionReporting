using System;
using System.Data;
using System.Linq;
using Sitecore.Cintel;
using Sitecore.Cintel.Reporting;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Analytics.Models;
using Sitecore.ContentSearch.ComputedFields;

namespace GoalCompletionReporting.Business.Search
{
    public class GoalsCompletedField : IComputedIndexField
    {
        public string FieldName { get; set; }

        public string ReturnType { get; set; }

        public object ComputeFieldValue(IIndexable indexable)
        {
            var contactIndexable = indexable as IContactIndexable;
            if (contactIndexable == null)
                return null;

            var contactId = (Guid)contactIndexable.Id.Value;
            if (contactId == Guid.Empty)
                return null;

            var viewParams = new ViewParameters();
            viewParams.ContactId = contactId;
            viewParams.ViewName = "goals";
            viewParams.ViewEntityId = null;
            var resultSet = CustomerIntelligenceManager.ViewProvider.GenerateContactView(viewParams);

            return resultSet.Data.Dataset["goals"].Rows
                .Cast<DataRow>()
                .Select(dataRow => dataRow[2]) //column 2 is the goal id
                .Distinct();
        }
    }
}
