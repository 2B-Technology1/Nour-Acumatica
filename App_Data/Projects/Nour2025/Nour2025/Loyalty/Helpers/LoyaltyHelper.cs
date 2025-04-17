using System;
using System.Linq;
using Maintenance;
using Nour20240918;
using Nour20241217;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.SO;

namespace Nour2025.Loyalty.Helpers
{
    public static class LoyaltyHelper
    {
        #region Constants
        public static class CalculationFields
        {
            public const string Profit = "Profit";
            public const string TotalSales = "TotalSales";
        }

        public static class LoyaltyStatus
        {
            public const string Active = "Active";
            public const string Redeemed = "Redeemed";
            public const string InActive = "InActive";
            public const string Expired = "Expired";
        }
        #endregion

        public static decimal CalculateEarnedLoyaltyPoints(Customer customer, SOOrder order) 
        {
            // Get the LoyaltyTier of the current Customer.
            var customerExt = customer.GetExtension<BAccountExt>();
            if(customerExt == null || customerExt.UsrLoyaltyTier == null) 
                return 0;

            int loyaltyId = customerExt.UsrLoyaltyTier.Value;

            var graph = PXGraph.CreateInstance<SOOrderEntry>();
            var loyaltyTier =  SelectFrom<LoyaltyTiers>.Where<LoyaltyTiers.loyaltyTierID.IsEqual<@P.AsInt>>.View
                .Select(PXGraph.CreateInstance<SOOrderEntry>(), loyaltyId).RowCast<LoyaltyTiers>()
                .FirstOrDefault();

            if (loyaltyTier != null)
            {
                // Fetch LoyaltyTierRules related to the LoyaltyTier
                var tierRules = SelectFrom<LoyaltyTierRule>
                    .Where<LoyaltyTierRule.loyaltyTierID.IsEqual<@P.AsInt>>
                    .OrderBy<Asc<LoyaltyTierRule.lineNbr>>
                    .View.Select(graph, loyaltyTier.LoyaltyTierID)
                    .RowCast<LoyaltyTierRule>()
                    .ToList();

                // Fetch LoyaltyTierCalculations related to the LoyaltyTier
                var tierCalculations = SelectFrom<LoyaltyTierCalculation>
                    .Where<LoyaltyTierCalculation.loyaltyTierID.IsEqual<@P.AsInt>>
                    .OrderBy<Asc<LoyaltyTierCalculation.lineNbr>>
                    .View.Select(graph, loyaltyTier.LoyaltyTierID)
                    .RowCast<LoyaltyTierCalculation>()
                    .ToList();

                var tierCalculationList = tierCalculations
                    .OrderBy(tc => tc.LineNbr).ToList();
                var FirstCalculation = tierCalculationList.FirstOrDefault();
                var FirstOperand = GetCalculationFieldValueOutOfSOOrder(order, FirstCalculation);
                decimal result = FirstOperand * FirstCalculation.PointsFactor.Value;

                for(int i = 1; i < tierCalculationList.Count; i++)
                {
                    var previosCalculationRow = tierCalculationList[i-1];
                    var calculation = tierCalculationList[i];
                    var calculationFieldValue = GetCalculationFieldValueOutOfSOOrder(order, calculation);
                    var currentRowResult = calculationFieldValue * calculation.PointsFactor.Value;

                    result = EvaluateArithmatic(result, currentRowResult, previosCalculationRow.ArithmaticOperator);
                }
                
                return result;
            }
                return 0;
        }


        private static decimal GetCalculationFieldValueOutOfSOOrder(SOOrder sOOrder, LoyaltyTierCalculation calculation)
        {
            if (sOOrder == null)
                throw new ArgumentNullException(nameof(sOOrder));

            switch (calculation.CalculationField)
            {
                case CalculationFields.TotalSales:
                    // Example: Use the OrderTotal field
                    return sOOrder.OrderTotal ?? 0m;

                case CalculationFields.Profit:
                    // Example: Calculate profit (assuming you have relevant fields like COGS and OrderTotal)
                    decimal orderTotal = sOOrder.OrderTotal ?? 0m;
                    decimal costOfGoodsSold = sOOrder.CuryLineTotal ?? 0m; // Replace with actual COGS field if different
                    return orderTotal - costOfGoodsSold;

                default:
                    throw new NotSupportedException($"Calculation field {calculation.CalculationField} is not supported.");
            }
        }

        

        public static decimal EvaluateArithmatic(decimal operand1, decimal operand2, string operatorSymbol)
        {
            if (string.IsNullOrWhiteSpace(operatorSymbol))
                throw new ArgumentException("Operator cannot be null or empty.");

            switch (operatorSymbol)
            {
                case "+":
                    return operand1 + operand2;
                case "-":
                    return operand1 - operand2;
                case "*":
                    return operand1 * operand2;
                case "/":
                    if (operand2 == 0)
                        throw new DivideByZeroException("Cannot divide by zero.");
                    return operand1 / operand2;
                case "%":
                    if (operand2 == 0)
                        throw new DivideByZeroException("Cannot calculate modulus with zero.");
                    return operand1 % operand2;
                default:
                    throw new NotSupportedException($"Arithmetic operator '{operatorSymbol}' is not supported.");
            }
        }
        public static bool EvaluateLogical(bool operand1, bool operand2, string operatorSymbol)
        {
            if (string.IsNullOrWhiteSpace(operatorSymbol))
                throw new ArgumentException("Operator cannot be null or empty.");

            switch (operatorSymbol)
            {
                case "&&":
                    return operand1 && operand2;
                case "||":
                    return operand1 || operand2;
                case "==":
                    return operand1 == operand2;
                case "!=":
                    return operand1 != operand2;
                default:
                    throw new NotSupportedException($"Logical operator '{operatorSymbol}' is not supported.");
            }
        }



        public static void StoreEarnedLoyaltyPoints(Customer customer, SOOrder order, decimal earnedPoints)
        {
            if (earnedPoints <= 0)
            {
                PXTrace.WriteInformation("No points to store as the earned points are zero or negative.");
                return;
            }

            if (customer == null)
            {
                throw new PXException("Customer cannot be null.");
            }

            if (order == null)
            {
                throw new PXException("Sales Order cannot be null.");
            }

            using (PXTransactionScope transaction = new PXTransactionScope())
            {
                var graph = PXGraph.CreateInstance<PXGraph>();
                LoyaltyActivity loyaltyActivity = new LoyaltyActivity
                {
                    CustomerID = customer.BAccountID,
                    TransactionID = order.OrderNbr,
                    TransactionDate = PXTimeZoneInfo.Now,
                    TransactionType = "SOOrder",
                    OrderInvoiceReference = null,
                    PointsEarned = (int)earnedPoints,
                    ConsumedPoints = 0,
                    RemainingPoints = (int)earnedPoints,
                    Type = "Earned Points",
                    Status = "Active",
                    ActivationDate = PXTimeZoneInfo.Now,
                    ExpirationDate = PXTimeZoneInfo.Now.AddMonths(12) // Assuming points expire in 12 months
                };

                PXCache<LoyaltyActivity> cache = graph.Caches<LoyaltyActivity>();
                cache.Insert(loyaltyActivity);

                graph.Actions.PressSave();
                transaction.Complete();

                PXTrace.WriteInformation($"Loyalty points stored successfully for CustomerID: {customer.BAccountID}, Order: {order.OrderNbr}.");
            }
        }

    }
}
