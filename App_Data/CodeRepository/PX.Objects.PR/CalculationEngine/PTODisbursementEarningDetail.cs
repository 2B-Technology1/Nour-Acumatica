/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using PX.Data;
using PX.Data.BQL;
using PX.Objects.EP;
using System;
using System.Collections.Generic;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		[PXHidden]
		public class PTODisbursementEarningDetail : IBqlTable
		{
			#region PTOBankID
			[PXString]
			[PXSelector(typeof(PRPTOBank.bankID))]
			public string PTOBankID { get; set; }
			public abstract class ptoBankID : BqlString.Field<ptoBankID> { }
			#endregion
			#region EmployeeID
			[PXInt]
			[PXSelector(typeof(PREmployee.bAccountID))]
			public int? EmployeeID { get; set; }
			public abstract class employeeID : BqlInt.Field<employeeID> { }
			#endregion
			#region LiabilityAccountID
			[PTOLiabilityAccount(typeof(ptoBankID), typeof(PRPayment.employeeID), typeof(PRPayment.payGroupID))]
			public int? LiabilityAccountID { get; set; }
			public abstract class liabilityAccountID : BqlInt.Field<liabilityAccountID> { }
			#endregion
			#region LiabilitySubID
			[PTOEarningDetailLiabilitySubAccount(typeof(liabilityAccountID))]
			public int? LiabilitySubID { get; set; }
			public abstract class liabilitySubID : BqlInt.Field<liabilitySubID> { }
			#endregion
			#region AssetAccountID
			[PTOAssetAccount(typeof(ptoBankID), typeof(PRPayment.employeeID), typeof(PRPayment.payGroupID))]
			public int? AssetAccountID { get; set; }
			public abstract class assetAccountID : BqlInt.Field<assetAccountID> { }
			#endregion
			#region AssetSubID
			[PTOEarningDetailAssetSubAccount(typeof(assetAccountID))]
			public int? AssetSubID { get; set; }
			public abstract class assetSubID : BqlInt.Field<assetSubID> { }
			#endregion
		}

		private class PTOEarningDetailLiabilitySubAccountAttribute : PTOLiabilitySubAccountAttribute
		{
			protected override List<Dependency> DefaultingDependencies { get; set; } = new List<Dependency>()
			{
				new Dependency(typeof(PTODisbursementEarningDetail.ptoBankID).Name, PRPTOAcctSubDefault.MaskPTOBank ),
			};

			public PTOEarningDetailLiabilitySubAccountAttribute(Type accountField) : base(accountField) { }

			protected override IEnumerable<object> GetMaskSources(PXCache cache, object row)
			{
				var line = row as PTODisbursementEarningDetail;
				var employee = (PREmployee)Employee.Select(cache.Graph, (int?)CacheHelper.GetCurrentValue(cache.Graph, EmployeeIDField));
				var payGroup = (PRPayGroup)PayGroup.Select(cache.Graph, (string)CacheHelper.GetCurrentValue(cache.Graph, PayGroupIDField));
				object ptoBank = PXSelectorAttribute.Select<PTODisbursementEarningDetail.ptoBankID>(cache, line);

				object employeeSubID = GetValue<PREmployee.ptoLiabilitySubID>(cache.Graph, employee);
				object payGroupSubID = GetValue<PRPayGroup.ptoLiabilitySubID>(cache.Graph, payGroup);
				object ptoSubID = GetValue<PRPTOBank.ptoLiabilitySubID>(cache.Graph, ptoBank);

				// Be careful, this array order needs to match with PRPTOAcctSubDefault.SubListAttribute (used in PRPTOSubAccountMaskAttribute)
				return new object[] { employeeSubID, payGroupSubID, ptoSubID };
			}
		}

		private class PTOEarningDetailAssetSubAccountAttribute : PTOAssetSubAccountAttribute
		{
			protected override List<Dependency> DefaultingDependencies { get; set; } = new List<Dependency>()
			{
				new Dependency( typeof(PTODisbursementEarningDetail.ptoBankID).Name, PRPTOAcctSubDefault.MaskPTOBank ),
			};

			public PTOEarningDetailAssetSubAccountAttribute(Type accountField) : base(accountField) { }

			protected override IEnumerable<object> GetMaskSources(PXCache cache, object row)
			{
				var line = row as PTODisbursementEarningDetail;
				var employee = (PREmployee)Employee.Select(cache.Graph, (int?)CacheHelper.GetCurrentValue(cache.Graph, EmployeeIDField));
				var payGroup = (PRPayGroup)PayGroup.Select(cache.Graph, (string)CacheHelper.GetCurrentValue(cache.Graph, PayGroupIDField));
				object ptoBank = PXSelectorAttribute.Select<PTODisbursementEarningDetail.ptoBankID>(cache, line);

				object employeeSubID = GetValue<PREmployee.ptoAssetSubID>(cache.Graph, employee);
				object payGroupSubID = GetValue<PRPayGroup.ptoAssetSubID>(cache.Graph, payGroup);
				object ptoSubID = GetValue<PRPTOBank.ptoAssetSubID>(cache.Graph, ptoBank);

				// Be careful, this array order needs to match with PRPTOAcctSubDefault.SubListAttribute (used in PRPTOSubAccountMaskAttribute)
				return new object[] { employeeSubID, payGroupSubID, ptoSubID };
			}
		}
	}
}
