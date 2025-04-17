using System;
using Nour20240918;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace Nour20241217
{
  public class LoyaltyRulesEntry : PXGraph<LoyaltyRulesEntry, LoyaltyRules>
  {

        public SelectFrom<LoyaltyRules>.View LoyaltyRules;

  }
}