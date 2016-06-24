using System;
using System.Linq.Expressions;
using Sitecore.Analytics.Rules.SegmentBuilder;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Rules;
using Sitecore.Diagnostics;
using Sitecore.Rules.Conditions;

namespace GoalCompletionReporting.Business.Rules.SegmentBuilder.Conditions
{
    public abstract class TypedQueryableStringOperatorCondition<T, TItem> : StringOperatorCondition<T>, IQueryableRule<TItem> where T : QueryableRuleContext<TItem> where TItem : IObjectIndexers
    {
        public Expression<Func<TItem, bool>> InitPredicate { protected get; set; }

        public Expression<Func<TItem, bool>> ResultPredicate { get; protected set; }

        public string Value { get; set; }

        protected override bool Execute(T ruleContext)
        {
            Assert.ArgumentNotNull(ruleContext, "ruleContext");
            string operatorId = OperatorId;
            if (string.IsNullOrEmpty(operatorId) || Value == null)
            {
                ApplyFilter(ruleContext, c => false);
                return false;
            }
            if (GetOperator() == StringConditionOperator.Unknown)
            {
                Log.Warn("Cannot evaluate condition. String condition operator is not defined: " + operatorId, this);
                ApplyFilter(ruleContext, c => false);
                return false;
            }
            ApplyFilter(ruleContext, GetResultPredicate(ruleContext));
            return true;
        }

        protected abstract Expression<Func<TItem, bool>> GetResultPredicate(T ruleContext);

        protected Expression<Func<TItem, bool>> GetCompareExpression(Expression<Func<TItem, string>> leftExpression, string value)
        {
            return GetOperator().Compare(leftExpression, value);
        }

        private void ApplyFilter(T ruleContext, Expression<Func<TItem, bool>> expression)
        {
            ResultPredicate = (InitPredicate ?? ruleContext.Where).And(expression);
            if (InitPredicate != null)
                return;
            ruleContext.Where = ResultPredicate;
        }
    }
}

