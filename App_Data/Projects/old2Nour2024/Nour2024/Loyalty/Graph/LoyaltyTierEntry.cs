using System;
using Nour20240918;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace Nour2024
{
  public class LoyaltyTierEntry : PXGraph<LoyaltyTierEntry, LoyaltyTiers>
  {

    public SelectFrom<LoyaltyTiers>.View LoyalityTiers;

    

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