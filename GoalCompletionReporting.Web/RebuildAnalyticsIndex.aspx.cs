using System;
using System.Linq;
using Sitecore.ContentSearch;
using Sitecore.Configuration;
using Sitecore.Analytics.Processing.ProcessingPool;
using Sitecore.Analytics.Data.DataAccess.MongoDb;
using Sitecore.Analytics.Model;

namespace GoalCompletionReporting.Web
{
    public partial class RebuildAnalyticsIndex : System.Web.UI.Page
    {
        protected void btnRebuild_OnClick(object sender, EventArgs e)
        {
            ContentSearchManager.GetIndex("sitecore_analytics_index").Reset();
            var poolPath = "aggregationProcessing/processingPools/live";
            var pool = Factory.CreateObject(poolPath, true) as ProcessingPool;
            var driver = MongoDbDriver.FromConnectionString("analytics");
            var visitorData = driver.Interactions.FindAllAs<VisitData>();
            var keys = visitorData.Select(data => new InteractionKey(data.ContactId, data.InteractionId));
            foreach (var key in keys)
            {
                var poolItem = new ProcessingPoolItem(key.ToByteArray());
                pool?.Add(poolItem);
            }
        }
    }
}