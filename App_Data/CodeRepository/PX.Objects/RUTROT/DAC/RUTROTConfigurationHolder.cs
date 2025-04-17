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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.RUTROT.DAC
{
	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
	public class RUTROTConfigurationHolder: PXMappedCacheExtension, IRUTROTConfigurationHolder
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.rutRotDeduction>();
		}
		#region AllowsRUTROT
		public abstract class allowsRUTROT : PX.Data.BQL.BqlBool.Field<allowsRUTROT> { }

		public virtual bool? AllowsRUTROT
		{
			get;
			set;
		}
		#endregion
		#region RUTDeductionPct
		public abstract class rUTDeductionPct : PX.Data.BQL.BqlDecimal.Field<rUTDeductionPct> { }

		public decimal? RUTDeductionPct
		{
			get;
			set;
		}
		#endregion
		#region RUTPersonalAllowanceLimit
		public abstract class rUTPersonalAllowanceLimit : PX.Data.BQL.BqlDecimal.Field<rUTPersonalAllowanceLimit> { }

		public virtual decimal? RUTPersonalAllowanceLimit
		{
			get;
			set;
		}
		#endregion
		#region RUTExtraAllowanceLimit
		public abstract class rUTExtraAllowanceLimit : PX.Data.BQL.BqlDecimal.Field<rUTExtraAllowanceLimit> { }

		public virtual decimal? RUTExtraAllowanceLimit
		{
			get;
			set;
		}
		#endregion
		#region ROTDeductionPct
		public abstract class rOTDeductionPct : PX.Data.BQL.BqlDecimal.Field<rOTDeductionPct> { }

		public decimal? ROTDeductionPct
		{
			get;
			set;
		}
		#endregion
		#region ROTPersonalAllowanceLimit
		public abstract class rOTPersonalAllowanceLimit : PX.Data.BQL.BqlDecimal.Field<rOTPersonalAllowanceLimit> { }

		public virtual decimal? ROTPersonalAllowanceLimit
		{
			get;
			set;
		}
		#endregion
		#region ROTExtraAllowanceLimit
		public abstract class rOTExtraAllowanceLimit : PX.Data.BQL.BqlDecimal.Field<rOTExtraAllowanceLimit> { }

		public virtual decimal? ROTExtraAllowanceLimit
		{
			get;
			set;
		}
		#endregion
		#region RUTROTCuryID
		public abstract class rUTROTCuryID : PX.Data.BQL.BqlString.Field<rUTROTCuryID> { }

		public virtual string RUTROTCuryID
		{
			get;
			set;
		}
		#endregion
		#region RUTROTClaimNextRefNbr
		public abstract class rUTROTClaimNextRefNbr : PX.Data.BQL.BqlInt.Field<rUTROTClaimNextRefNbr> { }

		public virtual int? RUTROTClaimNextRefNbr
		{
			get;
			set;
		}
		#endregion
		#region RUTROTOrgNbrValidRegEx
		public abstract class rUTROTOrgNbrValidRegEx : PX.Data.BQL.BqlString.Field<rUTROTOrgNbrValidRegEx> { }

		public virtual string RUTROTOrgNbrValidRegEx
		{
			get;
			set;
		}
		#endregion
		#region Default Type
		public abstract class defaultRUTROTType : PX.Data.BQL.BqlString.Field<defaultRUTROTType> { }

		public virtual string DefaultRUTROTType
		{
			get;
			set;
		}

		#endregion
		#region TaxAgencyAccountID
		public abstract class taxAgencyAccountID : PX.Data.BQL.BqlInt.Field<taxAgencyAccountID> { }

		public virtual int? TaxAgencyAccountID
		{
			get;
			set;
		}
		#endregion
		#region BalanceOnProcess
		public abstract class balanceOnProcess : PX.Data.BQL.BqlString.Field<balanceOnProcess> { }

		public virtual string BalanceOnProcess { get; set; }
		#endregion
	}
}
