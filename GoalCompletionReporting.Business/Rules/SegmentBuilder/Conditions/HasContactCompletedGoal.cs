using System;
using System.Linq.Expressions;
using Sitecore.Analytics.Rules.SegmentBuilder;
using Sitecore.ContentSearch.Analytics.Models;
using Sitecore.ContentSearch.Rules.Conditions;
using Sitecore.Data;

namespace GoalCompletionReporting.Business.Rules.SegmentBuilder.Conditions
{
    public class HasContactCompletedGoal<T> : TypedQueryableStringOperatorCondition<T, IndexedContact> where T : VisitorRuleContext<IndexedContact>
    {
        private const string IndexField = "Contact.CompletedGoals";
        private const string ContainsOperatorId = "{2E67477C-440C-4BCA-A358-3D29AED89F47}";

        public HasContactCompletedGoal()
        {
            OperatorId = ContainsOperatorId;
        }

        protected override Expression<Func<IndexedContact, bool>> GetResultPredicate(T ruleContext)
        {
            ID valueAsId;
            var parsedValueToId = ID.TryParse(Value, out valueAsId);
            ShortID valueAsShortId;
            var parsedValueToShortId = ShortID.TryParse(Value, out valueAsShortId);

            if (!parsedValueToId && !parsedValueToShortId)
            {
                return c => false;
            }

            if (parsedValueToShortId && valueAsId == (ID)null)
            {
                valueAsId = valueAsShortId.ToID();
            }

            var idConvertedToString = valueAsId.Guid.ToString("N").ToLowerInvariant();
            return GetCompareExpression(c => c[IndexField], idConvertedToString);
        }
    }
}
