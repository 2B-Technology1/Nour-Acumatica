using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace Nour20240918
{
  public class LoyaltyTiersEntry : PXGraph<LoyaltyTiersEntry, LoyaltyTiers>
  {
    public SelectFrom<LoyaltyTiers>.View LoyalityTiers;

    public SelectFrom<LoyaltyRewards>
            .InnerJoin<LoyaltyTiers>
                .On<LoyaltyRewards.loyaltyTierID.IsEqual<LoyaltyTiers.loyaltyTierID>>
            .Where<LoyaltyRewards.loyaltyTierID.IsEqual<LoyaltyTiers.loyaltyTierID.FromCurrent>>
            .View LoyaltyRewards;


    public SelectFrom<LoyaltyTierRule>
        .InnerJoin<LoyaltyTiers>
            .On<LoyaltyTierRule.loyaltyTierID.IsEqual<LoyaltyTiers.loyaltyTierID>>
        .Where<LoyaltyTierRule.loyaltyTierID.IsEqual<LoyaltyTiers.loyaltyTierID.FromCurrent>>
                .OrderBy<Asc<LoyaltyTierRule.lineNbr>> // Sort by LineNbr in ascending order
        .View LoyaltyTierRules;

        protected void _(Events.RowInserting<LoyaltyRewards> e)
        {
            var Row = e.Row;
            if (Row == null) return;

            // Check for duplicates in the current list
            bool exists = LoyaltyRewards.Select()
                .RowCast<LoyaltyRewards>()
                .Any(reward => reward.LoyaltyRewardID == Row.LoyaltyRewardID);

            if (exists)
            {
                // Throw an exception or cancel the insert operation
                e.Cache.RaiseExceptionHandling<LoyaltyRewards.loyaltyRewardID>(
                    Row,
                    Row.LoyaltyRewardID,
                    new PXSetPropertyException("This reward already exists in the list.", PXErrorLevel.Error)
                );

                e.Cancel = true; // Cancel the insert operation
            }
        }


        protected void _(Events.RowInserting<LoyaltyTierRule> e)
        {
            var Row = e.Row;
            if (Row == null) return;

            // Check for duplicates in the current list
            bool exists = LoyaltyTierRules.Select()
                .RowCast<LoyaltyTierRule>()
                .Any(reward => reward.LoyaltyRuleID== Row.LoyaltyRuleID);

            if (exists)
            {
                // Throw an exception or cancel the insert operation
                e.Cache.RaiseExceptionHandling<LoyaltyRewards.loyaltyRewardID>(
                    Row,
                    Row.LoyaltyRuleID,
                    new PXSetPropertyException("This rule already exists in the list.", PXErrorLevel.Error)
                );

                e.Cancel = true; // Cancel the insert operation
            }
        }



        #region Rules Tab Events
        protected virtual void _(Events.RowSelected<LoyaltyTierRule> e)
        {
            if (e.Row == null)
                return;

            LoyaltyTierRule loyaltyTierRule = e.Row;
            IEnumerable<LoyaltyTierRule> loyaltyTierRulesList = LoyaltyTierRules.Select().RowCast<LoyaltyTierRule>();
            LoyaltyTierRule lastItem = LoyaltyTierRules.Select().RowCast<LoyaltyTierRule>().LastOrDefault();

            foreach (var item in loyaltyTierRulesList)
            {
                if (item == lastItem)
                {
                    item.LogicalOperator = "-";
                    PXUIFieldAttribute.SetEnabled<LoyaltyTierRule.logicalOperator>(LoyaltyTierRules.Cache, item, false);
                }
                else
                {
                    PXUIFieldAttribute.SetEnabled<LoyaltyTierRule.logicalOperator>(LoyaltyTierRules.Cache, item, true);
                }
            }
        }
        
        protected virtual void _(Events.RowPersisting<LoyaltyTierRule> e)
        {
            if (e.Row == null)
                return;

            LoyaltyTierRule loyaltyTierRule = e.Row;
            LoyaltyTierRule lastItem = LoyaltyTierRules.Select().RowCast<LoyaltyTierRule>().LastOrDefault();

            if (lastItem != null && lastItem == loyaltyTierRule)
            {
                loyaltyTierRule.LogicalOperator = "-";
            }
        }

        #endregion




    }
}