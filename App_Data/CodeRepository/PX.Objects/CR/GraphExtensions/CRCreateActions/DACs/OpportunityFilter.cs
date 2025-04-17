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

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	[Serializable]
	[PXHidden]
	public class OpportunityFilter : IBqlTable, IClassIdFilter
	{
		#region CloseDate

		public abstract class closeDate : PX.Data.BQL.BqlDateTime.Field<closeDate> { }

		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXDBDateAndTime]
		[PXUIField(DisplayName = "Estimation", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual DateTime? CloseDate { get; set; }

		#endregion

		#region Subject

		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }

		[PXDefault]
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual String Subject { get; set; }

		#endregion

		#region OpportunityClass

		public abstract class opportunityClass : PX.Data.BQL.BqlString.Field<opportunityClass> { }

		[PXDefault]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Opportunity Class")]
		[PXSelector(typeof(CROpportunityClass.cROpportunityClassID),
			DescriptionField = typeof(CROpportunityClass.description))]
		public virtual string OpportunityClass { get; set; }

		string IClassIdFilter.ClassID => OpportunityClass;

		#endregion
	}
}