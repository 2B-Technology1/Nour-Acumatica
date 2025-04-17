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
using PX.Objects.CS;

namespace PX.Objects.AR
{
	/// <summary>
	/// Provides a selector for the <see cref="Terms"> items, which may be put into the <see cref="ARInvoice"> document <br/>
	/// The list is filtered by the visible rights <see cref="TermsVisibleTo.All"> and <see cref="TermsVisibleTo.Customer"> <br/>
	/// and restricted by multiple installement type <see cref="TermsInstallmentType.Multiple"> 
	/// if the AR migration mode is activated <see cref="ARSetup.MigrationMode">. <br/>
	/// <example>
	/// [ARTermsSelector]
	/// </example>
	/// </summary>
	[PXSelector(typeof(Search<Terms.termsID, 
		Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, 
			Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), 
		DescriptionField = typeof(Terms.descr), Filterable = true)]
	[PXRestrictor(typeof(Where<
		Current<ARSetup.migrationMode>, NotEqual<True>,
		Or<Terms.installmentType, NotEqual<TermsInstallmentType.multiple>>>), CS.Messages.CannotBeEmpty)]
	public class ARTermsSelectorAttribute : PXAggregateAttribute
	{
		public ARTermsSelectorAttribute()
			: base()
		{
		}
	}
}
