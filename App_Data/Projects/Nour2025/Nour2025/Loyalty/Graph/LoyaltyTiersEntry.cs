using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace Nour20240918
{
  public class LoyaltyTiersEntry : PXGraph<LoyaltyTiersEntry,LoyaltyTiers >
  {
    public SelectFrom<LoyaltyTiers>.View LoyalityTiers;

    public SelectFrom<LoyaltyTierReward>
            .InnerJoin<LoyaltyTiers>
                .On<LoyaltyTierReward.loyaltyTierID.IsEqual<LoyaltyTiers.loyaltyTierID>>
            .Where<LoyaltyTierReward.loyaltyTierID.IsEqual<LoyaltyTiers.loyaltyTierID.FromCurrent>>
            .View LoyaltyRewards;


    public SelectFrom<LoyaltyTierRule>
        .InnerJoin<LoyaltyTiers>
            .On<LoyaltyTierRule.loyaltyTierID.IsEqual<LoyaltyTiers.loyaltyTierID>>
        .Where<LoyaltyTierRule.loyaltyTierID.IsEqual<LoyaltyTiers.loyaltyTierID.FromCurrent>>
                .OrderBy<Asc<LoyaltyTierRule.lineNbr>> // Sort by LineNbr in ascending order
        .View LoyaltyTierRules;

        public SelectFrom<LoyaltyTierCalculation>
                .InnerJoin<LoyaltyTiers>
                    .On<LoyaltyTierCalculation.loyaltyTierID.IsEqual<LoyaltyTiers.loyaltyTierID>>
                .Where<LoyaltyTierCalculation.loyaltyTierID.IsEqual<LoyaltyTiers.loyaltyTierID.FromCurrent>>
                        .OrderBy<Asc<LoyaltyTierCalculation.lineNbr>> // Sort by LineNbr in ascending order
                .View LoyaltyTierCalculations;



        //protected void _(Events.RowInserting<LoyaltyTierReward> e)
        //{
        //    var Row = e.Row;
        //    if (Row == null) return;

        //    // Check for duplicates in the current list
        //    bool exists = LoyaltyRewards.Select()
        //        .RowCast<LoyaltyTierReward>()
        //        .Any(reward => reward.LoyaltyTierRewardID == Row.LoyaltyTierRewardID);

        //    if (exists)
        //    {
        //        // Throw an exception or cancel the insert operation
        //        e.Cache.RaiseExceptionHandling<LoyaltyTierReward.loyaltyTierRewardID>(
        //            Row,
        //            Row.LoyaltyTierRewardID,
        //            new PXSetPropertyException("This reward already exists in the list.", PXErrorLevel.Error)
        //        );

        //        e.Cancel = true; // Cancel the insert operation
        //    }
        //}


        //protected void _(Events.RowInserting<LoyaltyTierRule> e)
        //{
        //    var Row = e.Row;
        //    if (Row == null) return;

        //    // Check for duplicates in the current list
        //    bool exists = LoyaltyTierRules.Select()
        //        .RowCast<LoyaltyTierRule>()
        //        .Any(reward => reward.LoyaltyRuleID== Row.LoyaltyRuleID);

        //    if (exists)
        //    {
        //        // Throw an exception or cancel the insert operation
        //        e.Cache.RaiseExceptionHandling<LoyaltyTierReward.loyaltyTierRewardID>(
        //            Row,
        //            Row.LoyaltyRuleID,
        //            new PXSetPropertyException("This rule already exists in the list.", PXErrorLevel.Error)
        //        );

        //        e.Cancel = true; // Cancel the insert operation
        //    }
        //}



        //#region LastOperatorIsNA
        //protected virtual void _(Events.RowSelected<LoyaltyTierRule> e)
        //{
        //    if (e.Row == null)
        //        return;

        //    LoyaltyTierRule loyaltyTierRule = e.Row;
        //    IEnumerable<LoyaltyTierRule> loyaltyTierRulesList = LoyaltyTierRules.Select().RowCast<LoyaltyTierRule>()
        //        .OrderBy(r => r.LineNbr);
        //    LoyaltyTierRule lastItem = LoyaltyTierRules.Select().RowCast<LoyaltyTierRule>().LastOrDefault();

        //    foreach (var item in loyaltyTierRulesList)
        //    {
        //        if (item == lastItem)
        //        {
        //            e.Cache.SetValueExt<LoyaltyTierRule.logicalOperator>(e.Row, "-");
        //            PXUIFieldAttribute.SetEnabled<LoyaltyTierRule.logicalOperator>(LoyaltyTierRules.Cache, item, false);
        //        }
        //        else
        //        {
        //            PXUIFieldAttribute.SetEnabled<LoyaltyTierRule.logicalOperator>(LoyaltyTierRules.Cache, item, true);

        //        }
        //    }
        //    LoyaltyTierRules.View.RequestRefresh();
        //}
        //protected virtual void _(Events.RowSelected<LoyaltyTierCalculation> e)
        //{
        //    if (e.Row == null)
        //        return;

        //    LoyaltyTierCalculation loyaltyCalculation = e.Row;
        //    IEnumerable<LoyaltyTierCalculation> loyaltyCalculationsList = LoyaltyTierCalculations.Select().RowCast<LoyaltyTierCalculation>()
        //            .OrderBy(r => r.LineNbr);
        //    LoyaltyTierCalculation lastItem = loyaltyCalculationsList.LastOrDefault();

        //    foreach (var item in loyaltyCalculationsList)
        //    {
        //        if (item == lastItem)
        //        {
        //            // Set the ArithmaticOperator field to "N/A" and disable it for the last row
        //            e.Cache.SetValueExt<LoyaltyTierCalculation.arithmaticOperator>(item, "N/A");
        //            PXUIFieldAttribute.SetEnabled<LoyaltyTierCalculation.arithmaticOperator>(LoyaltyTierCalculations.Cache, item, false);
        //        }
        //        else
        //        {
        //            // Enable the ArithmaticOperator field for all other rows
        //            PXUIFieldAttribute.SetEnabled<LoyaltyTierCalculation.arithmaticOperator>(LoyaltyTierCalculations.Cache, item, true);
        //        }
        //    }
        //    // Refresh the LoyaltyCalculations view to reflect changes in the UI
        //    LoyaltyTierCalculations.View.RequestRefresh();
        //}


        //protected virtual void _(Events.RowPersisting<LoyaltyTierRule> e)
        //{
        //    if (e.Row == null)
        //        return;

        //    LoyaltyTierRule loyaltyTierRule = e.Row;
        //    LoyaltyTierRule lastItem = LoyaltyTierRules.Select().RowCast<LoyaltyTierRule>().LastOrDefault();

        //    if (lastItem != null && lastItem == loyaltyTierRule)
        //    {
        //        loyaltyTierRule.LogicalOperator = "-";
        //    }
        //}
        //#endregion





        //protected virtual void _(Events.RowPersisting<LoyaltyTierRule> e)
        //{
        //    if (e.Row == null)
        //        return;

        //    LoyaltyTierRule loyaltyTierRule = e.Row;
        //    IEnumerable<LoyaltyTierRule> loyaltyTierRulesList = LoyaltyTierRules.Select().RowCast<LoyaltyTierRule>();
        //    LoyaltyTierRule lastItem = loyaltyTierRulesList.LastOrDefault();

        //    if (loyaltyTierRule != lastItem && (string.IsNullOrEmpty(loyaltyTierRule.LogicalOperator)) || loyaltyTierRule.LogicalOperator == "N/A")
        //    {
        //        // Throw an error if a non-last row does not have a Logical Operator
        //        PXUIFieldAttribute.SetError<LoyaltyTierRule.logicalOperator>(
        //            LoyaltyTierRules.Cache,
        //            loyaltyTierRule,
        //            "Logical Operator is required for all rules except the last one."
        //        );

        //        throw new PXRowPersistingException(
        //            typeof(LoyaltyTierRule.logicalOperator).Name,
        //            null,
        //            "Logical Operator is required for all rules except the last one."
        //        );
        //    }
        //}

        //protected virtual void _(Events.RowPersisting<LoyaltyTierCalculation> e)
        //{
        //    if (e.Row == null)
        //        return;

        //    LoyaltyTierCalculation loyaltyCalculation = e.Row;
        //    IEnumerable<LoyaltyTierCalculation> loyaltyCalculationsList = LoyaltyTierCalculations.Select().RowCast<LoyaltyTierCalculation>();
        //    LoyaltyTierCalculation lastItem = loyaltyCalculationsList.LastOrDefault();

        //    if (loyaltyCalculation != lastItem && (string.IsNullOrEmpty(loyaltyCalculation.ArithmaticOperator)) || loyaltyCalculation.ArithmaticOperator == "N/A")
        //    {
        //        // Throw an error if a non-last row does not have an Arithmetic Operator
        //        PXUIFieldAttribute.SetError<LoyaltyTierCalculation.arithmaticOperator>(
        //            LoyaltyTierCalculations.Cache,
        //            loyaltyCalculation,
        //            "Arithmetic Operator is required for all calculations except the last one."
        //        );

        //        throw new PXRowPersistingException(
        //            typeof(LoyaltyTierCalculation.arithmaticOperator).Name,
        //            null,
        //            "Arithmetic Operator is required for all calculations except the last one."
        //        );
        //    }
        //}




    }
}