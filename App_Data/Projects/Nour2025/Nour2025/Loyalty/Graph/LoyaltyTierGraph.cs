using System;
using Nour20240918;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace Nour20241219
{
  public class LoyaltyTierGraph : PXGraph<LoyaltyTierGraph, LoyaltyTiers>
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



        public PXFilter<MasterTable> MasterView;
    public PXFilter<DetailsTable> DetailsView;

    [Serializable]
    public class MasterTable : IBqlTable
    {

    }

    [Serializable]
    public class DetailsTable : IBqlTable
    {

    }


  }
}