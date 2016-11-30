﻿using System;
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
        private const string GoalsViewName = "goals";
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

            var viewParams = new ViewParameters
            {
                ContactId = contactId,
                ViewName = GoalsViewName,
                ViewEntityId = null
            };
            var resultSet = CustomerIntelligenceManager.ViewProvider.GenerateContactView(viewParams);

            return resultSet.Data.Dataset[GoalsViewName].Rows
                .Cast<DataRow>()
                .Select(GetGoalIdFromDataRow())
                .Distinct();
        }

        private static Func<DataRow, object> GetGoalIdFromDataRow()
        {
            return dataRow => dataRow[2];
        }
    }
}
