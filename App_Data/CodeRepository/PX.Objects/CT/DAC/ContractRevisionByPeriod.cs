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

using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.CT
{
	[Serializable]
	[PXProjection(typeof(Select5<MasterFinPeriod,
						LeftJoin<ContractRenewalHistory,
							On<MasterFinPeriod.startDate, NotEqual<MasterFinPeriod.endDate>,
								And<MasterFinPeriod.finDate, GreaterEqual<ContractRenewalHistory.effectiveFrom>,
								And<MasterFinPeriod.finDate, GreaterEqual<ContractRenewalHistory.activationDate>,
								And<ContractRenewalHistory.status, NotEqual<Contract.status.draft>,
								And<ContractRenewalHistory.status, NotEqual<Contract.status.inApproval>,
								And<ContractRenewalHistory.status, NotEqual<Contract.status.inUpgrade>,
								And<ContractRenewalHistory.status, NotEqual<Contract.status.pendingActivation>,
								And<ContractRenewalHistory.effectiveFrom, IsNotNull,
								And<Where<MasterFinPeriod.startDate, LessEqual<ContractRenewalHistory.expireDate>,
									Or<ContractRenewalHistory.expireDate, IsNull>>>>>>>>>>>,
						LeftJoin<Contract,
							On<Where<ContractRenewalHistory.contractID, Equal<Contract.contractID>>>>>,
						Where<MasterFinPeriod.startDate, LessEqual<Contract.terminationDate>,
							Or<Contract.terminationDate, IsNull>>,
						Aggregate<GroupBy<MasterFinPeriod.finDate,
									GroupBy<ContractRenewalHistory.contractID,
									Max<ContractRenewalHistory.revID>>>>>)
		)]
	[PXCacheName("Contract revision by period")]
	public class ContractRevisionByPeriod : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<ContractRevisionByPeriod>.By<contractID, revID>
		{
			public static ContractRevisionByPeriod Find(PXGraph graph, int? contractID, Int32? revID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, contractID, revID, options);
		}
		public static class FK
		{
			public class Contract : CT.Contract.PK.ForeignKeyOf<ContractRevisionByPeriod>.By<contractID> { }
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[FinPeriodID(IsKey = true, BqlField = typeof(MasterFinPeriod.finPeriodID))]
		public virtual String FinPeriodID
		{
			get;
			set;
		}
		#endregion
		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		[PXDBIdentity(IsKey = true, BqlField = typeof(ContractRenewalHistory.contractID))]
		[PXUIField(DisplayName = "Contract ID")]
		public virtual Int32? ContractID
		{
			get;
			set;
		}
		#endregion
		#region RevID
		public abstract class revID : PX.Data.BQL.BqlInt.Field<revID> { }
		[PXDBInt(BqlField = typeof(ContractRenewalHistory.revID))]
		[PXUIField(DisplayName = "Revision Number")]
		public virtual int? RevID
		{
			get;
			set;
		}
		#endregion
		#region ActivationDate
		public abstract class activationDate : PX.Data.BQL.BqlDateTime.Field<activationDate> { }

		[PXDBDate(BqlField = typeof(ContractRenewalHistory.activationDate))]
		public virtual DateTime? ActivationDate
		{
			get;
			set;
		}
		#endregion
		#region EffectiveFrom
		public abstract class effectiveFrom : PX.Data.BQL.BqlDateTime.Field<effectiveFrom> { }

		[PXDBDate(BqlField = typeof(ContractRenewalHistory.effectiveFrom))]
		[PXUIField(DisplayName = "Effective From")]
		public virtual DateTime? EffectiveFrom
		{
			get;
			set;
		}
		#endregion
		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

		[PXDBDate(BqlField = typeof(ContractRenewalHistory.expireDate))]
		[PXUIField(DisplayName = "Expiration Date")]
		public virtual DateTime? ExpireDate
		{
			get;
			set;
		}
		#endregion
		#region TerminationDate
		public abstract class terminationDate : PX.Data.BQL.BqlDateTime.Field<terminationDate> { }

		[PXDBDate(BqlField = typeof(Contract.terminationDate))]
		public virtual DateTime? TerminationDate
		{
			get;
			set;
		}
		#endregion
		#region StartFinPeriod
		public abstract class startFinPeriod : PX.Data.BQL.BqlDateTime.Field<startFinPeriod> { }

		[PXDBDate(BqlField = typeof(MasterFinPeriod.startDate))]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartFinPeriod { get; set; }
		#endregion
		#region EndFinPeriod
		public abstract class endFinPeriod : PX.Data.BQL.BqlDateTime.Field<endFinPeriod> { }

		[PXDBDate(BqlField = typeof(MasterFinPeriod.endDate))]
		public virtual DateTime? EndFinPeriod { get; set; }
		#endregion
		#region New
		public abstract class newCount : PX.Data.BQL.BqlInt.Field<newCount> { }
		[PXInt]
		public virtual int? NewCount
		{
			[PXDependsOnFields(typeof(effectiveFrom),
								typeof(startFinPeriod),
								typeof(endFinPeriod))]
			get
			{
				if (EffectiveFrom >= StartFinPeriod &&
					EffectiveFrom < EndFinPeriod)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
			set
			{

			}
		}
		#endregion
		#region Expired
		public abstract class expiredCount : PX.Data.BQL.BqlInt.Field<expiredCount> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Expired")]
		public virtual int? ExpiredCount
		{
			[PXDependsOnFields(typeof(terminationDate),
								typeof(expireDate),
								typeof(startFinPeriod),
								typeof(endFinPeriod))]
			get
			{
				if (ExpireDate >= StartFinPeriod &&	ExpireDate < EndFinPeriod ||
					TerminationDate >= StartFinPeriod && TerminationDate < EndFinPeriod)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
			set
			{

			}
		}
		#endregion
	}
}
