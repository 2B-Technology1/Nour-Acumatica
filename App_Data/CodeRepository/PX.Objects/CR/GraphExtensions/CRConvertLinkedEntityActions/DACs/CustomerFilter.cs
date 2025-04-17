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
using PX.Data.BQL;
using PX.Objects.AR;
using PX.Objects.CR.Extensions.CRCreateActions;
using PX.Objects.CR.MassProcess;

namespace PX.Objects.CR.Extensions.CRConvertLinkedEntityActions
{
	/// <exclude/>
	[Serializable]
	[PXHidden]
	public class CustomerFilter : IBqlTable, IClassIdFilter
	{
		#region AcctCD

		public abstract class acctCD : BqlString.Field<acctCD> { }

		// Acuminator disable once PX1030 PXDefaultIncorrectUse required filter value
		[PXDefault]
		[PXDimensionSelector("BIZACCT",
			typeof(Search<BAccount.acctCD>),
			typeof(BAccount.acctCD),
			DescriptionField = typeof(BAccount.acctName))]
		[PXUIField(DisplayName = "Customer ID", Enabled = false)]
		public virtual string AcctCD { get; set; }

		#endregion

		#region ClassID
		public abstract class classID : BqlString.Field<classID> { }

		// Acuminator disable once PX1030 PXDefaultIncorrectUse required filter value
		[PXString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Customer Class", Required = true)]
		[PXDefault(typeof(Search2<ARSetup.dfltCustomerClassID,
			InnerJoin<CustomerClass, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>,
			Where<CustomerClass.orgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>))]
		[PXSelector(typeof(Search<CustomerClass.customerClassID,
			Where<CustomerClass.orgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>, And<MatchUser>>>),
			CacheGlobal = true,
			DescriptionField = typeof(CustomerClass.descr))]
		public virtual string ClassID { get; set; }
		#endregion

		#region Email

		public abstract class email : BqlString.Field<email> { }

		// Acuminator disable once PX1030 PXDefaultIncorrectUse required filter value
		[PXDBEmail]
		[PXDefault]
		[PXUIField(DisplayName = "Customer Email", Required = false)]
		public virtual string Email { get; set; }

		#endregion

		#region WarningMessage

		public abstract class warningMessage : BqlString.Field<warningMessage> { }

		[PXString]
		[PXUIField(DisplayName = "", IsReadOnly = true)]
		public virtual string WarningMessage { get; set; }

		#endregion
	}
}
