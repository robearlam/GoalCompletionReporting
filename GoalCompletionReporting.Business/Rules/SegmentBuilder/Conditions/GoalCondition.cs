using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Sitecore.Analytics;
using Sitecore.Analytics.Core;
using Sitecore.Analytics.Model;
using Sitecore.Analytics.Rules.SegmentBuilder;
using Sitecore.Analytics.Tracking;
using Sitecore.Common;
using Sitecore.ContentSearch.Analytics.Models;
using Sitecore.ContentSearch.Rules.Conditions;
using Sitecore.Diagnostics;

namespace GoalCompletionReporting.Business.Rules.SegmentBuilder.Conditions
{
    public class GoalCondition<T> : TypedQueryableStringOperatorCondition<T, IndexedContact> where T : VisitorRuleContext<IndexedContact>
    {
        private readonly bool filterByCustomData;

        public string CustomData { get; set; }

        public string CustomDataOperatorId { get; set; }

        public int NumberOfElapsedDays { get; set; }

        public string NumberOfElapsedDaysOperatorId { get; set; }

        public int NumberOfPastInteractions { get; set; }

        public string NumberOfPastInteractionsOperatorId { get; set; }

        private Guid? goalGuid;
        private bool goalGuidInitialized;

        public string GoalId { get; set; }

        private Guid? GoalGuid
        {
            get
            {
                if (goalGuidInitialized)
                    return goalGuid;
                try
                {
                    goalGuid = new Guid(GoalId);
                }
                catch
                {
                    Log.Warn($"Could not convert value to guid: {GoalId}", GetType());
                }
                goalGuidInitialized = true;
                return goalGuid;
            }
        }

        public GoalCondition()
        {
            this.OperatorId = "{2E67477C-440C-4BCA-A358-3D29AED89F47}";

        }

        protected override Expression<Func<IndexedContact, bool>> GetResultPredicate(T ruleContext)
        {
            Assert.ArgumentNotNull(ruleContext, "ruleContext");
            if (!GoalGuid.HasValue)
                return c => false;

            var bob = FilterKeyBehaviorCacheEntries(Tracker.Current.Contact.GetKeyBehaviorCache()).Any(entry =>
            {
                Guid id = entry.Id;
                Guid? goalGuid = this.GoalGuid;
                if (goalGuid.HasValue)
                    return id == goalGuid.GetValueOrDefault();
                return false;
            });

            return c => false;
        }

        protected virtual IEnumerable<KeyBehaviorCacheEntry> FilterKeyBehaviorCacheEntries(KeyBehaviorCache keyBehaviorCache)
        {
            Assert.ArgumentNotNull((object)keyBehaviorCache, "keyBehaviorCache");
            IEnumerable<KeyBehaviorCacheEntry> enumerable = this.FilterKeyBehaviorCacheEntriesByInteractionConditions(Enumerable.Concat<KeyBehaviorCacheEntry>(Enumerable.Concat<KeyBehaviorCacheEntry>(Enumerable.Concat<KeyBehaviorCacheEntry>(Enumerable.Concat<KeyBehaviorCacheEntry>(Enumerable.Concat<KeyBehaviorCacheEntry>(Enumerable.Concat<KeyBehaviorCacheEntry>((IEnumerable<KeyBehaviorCacheEntry>)keyBehaviorCache.Campaigns, (IEnumerable<KeyBehaviorCacheEntry>)keyBehaviorCache.Channels), (IEnumerable<KeyBehaviorCacheEntry>)keyBehaviorCache.CustomValues), (IEnumerable<KeyBehaviorCacheEntry>)keyBehaviorCache.Goals), (IEnumerable<KeyBehaviorCacheEntry>)keyBehaviorCache.Outcomes), (IEnumerable<KeyBehaviorCacheEntry>)keyBehaviorCache.PageEvents), (IEnumerable<KeyBehaviorCacheEntry>)keyBehaviorCache.Venues));
            if (this.filterByCustomData)
            {
                if (this.CustomData == null)
                {
                    Log.Warn("CustomData can not be null", (object)this.GetType());
                    return Enumerable.Empty<KeyBehaviorCacheEntry>();
                }
                enumerable = Enumerable.Where<KeyBehaviorCacheEntry>(enumerable, (Func<KeyBehaviorCacheEntry, bool>)(entry =>
                {
                    if (entry.Data != null)
                        return ConditionsUtility.CompareStrings(entry.Data, this.CustomData, this.CustomDataOperatorId);
                    return false;
                }));
            }
            return Assert.ResultNotNull<IEnumerable<KeyBehaviorCacheEntry>>(Enumerable.Intersect<KeyBehaviorCacheEntry>(this.GetKeyBehaviorCacheEntries(keyBehaviorCache), enumerable, (IEqualityComparer<KeyBehaviorCacheEntry>)new KeyBehaviorCacheEntry.KeyBehaviorCacheEntryEqualityComparer()));
        }

        protected virtual IEnumerable<KeyBehaviorCacheEntry> FilterKeyBehaviorCacheEntriesByInteractionConditions(IEnumerable<KeyBehaviorCacheEntry> keyBehaviorCacheEntries)
        {
            Assert.ArgumentNotNull((object)keyBehaviorCacheEntries, "keyBehaviorCacheEntries");
            if (ConditionsUtility.GetInt32Comparer(this.NumberOfElapsedDaysOperatorId) == null)
                return Enumerable.Empty<KeyBehaviorCacheEntry>();
            Func<int, int, bool> numberOfPastInteractionsComparer = ConditionsUtility.GetInt32Comparer(this.NumberOfPastInteractionsOperatorId);
            if (numberOfPastInteractionsComparer == null)
                return Enumerable.Empty<KeyBehaviorCacheEntry>();
            return Assert.ResultNotNull(keyBehaviorCacheEntries.GroupBy(entry => new
            {
                InteractionId = entry.InteractionId,
                InteractionStartDateTime = entry.InteractionStartDateTime
            }).OrderByDescending(entries => entries.Key.InteractionStartDateTime).Where((entries, i) =>
            {
                if (numberOfPastInteractionsComparer((DateTime.UtcNow - entries.Key.InteractionStartDateTime).Days, this.NumberOfElapsedDays))
                    return numberOfPastInteractionsComparer(i + 2, this.NumberOfPastInteractions);
                return false;
            }).SelectMany(entries => entries));
        }

        protected IEnumerable<KeyBehaviorCacheEntry> GetKeyBehaviorCacheEntries(KeyBehaviorCache keyBehaviorCache)
        {
            Assert.ArgumentNotNull(keyBehaviorCache, "keyBehaviorCache");
            return keyBehaviorCache.Goals;
        }

        protected bool HasEventOccurredInInteraction(IInteractionData interaction)
        {
            Assert.ArgumentNotNull(interaction, "interaction");
            Assert.IsNotNull(interaction.Pages, "interaction.Pages is not initialized.");
            return interaction.Pages.SelectMany(page => page.PageEvents).Any(pageEvent =>
            {
                if (!pageEvent.IsGoal)
                    return false;
                Guid eventDefinitionId = pageEvent.PageEventDefinitionId;
                Guid? goalGuid = this.GoalGuid;
                if (goalGuid.HasValue)
                    return eventDefinitionId == goalGuid.GetValueOrDefault();
                return false;
            });
        }
    }
}
