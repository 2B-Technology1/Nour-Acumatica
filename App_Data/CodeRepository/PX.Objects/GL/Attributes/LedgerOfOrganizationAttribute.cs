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

namespace PX.Objects.GL.Attributes
{
	[PXDBInt]
	[PXUIField(DisplayName = Messages.Ledger)]
	public class LedgerOfOrganizationAttribute : PXAggregateAttribute
	{
		public readonly Type OrganizationFieldType;
		public readonly Type BranchFieldType;

		protected static readonly Type _defaultingSelect;
		protected static readonly Type _defaultingSelectDefault;
		
		static LedgerOfOrganizationAttribute()
		{
			_defaultingSelect = typeof(Search5<Ledger.ledgerID,
						LeftJoin<Branch, On<Branch.ledgerID, Equal<Ledger.ledgerID>>>,
						Aggregate<GroupBy<Ledger.ledgerID>>>);

			_defaultingSelectDefault = typeof(Search2<Ledger.ledgerID,
						InnerJoin<Branch, On<Branch.ledgerID, Equal<Ledger.ledgerID>>>,
						Where<Branch.branchID, Equal<Optional2<AccessInfo.branchID>>
						>>);
		}

		public LedgerOfOrganizationAttribute(Type organizationFieldType, Type branchFieldType, Type restrict=null)
			:this(organizationFieldType, branchFieldType, _defaultingSelect, _defaultingSelectDefault, restrict)
		{
		}

		public LedgerOfOrganizationAttribute(Type organizationFieldType, Type branchFieldType, Type select, Type selectDefault, Type restrictSelect)
		{
			OrganizationFieldType = organizationFieldType;
			BranchFieldType = branchFieldType;

			if (select == null)
			{
				select = _defaultingSelect;
			}

			_Attributes.Add(new PXSelectorAttribute(select)
									{
										SubstituteKey = typeof(Ledger.ledgerCD),
										DescriptionField = typeof(Ledger.descr)
									}
								);

			Type selectRestrictor = BqlCommand.Compose(
											typeof(Where2<,>),
												typeof(Where2<,>),
													typeof(Where<,,>),
													typeof(Branch.organizationID), typeof(Equal<>), typeof(Optional2<>), organizationFieldType,
													typeof(Or<,>), typeof(Optional2<>), organizationFieldType, typeof(IsNull),
												typeof(And<>),
													typeof(Where<,,>),
													typeof(Branch.branchID), typeof(Equal<>), typeof(Optional2<>), branchFieldType,
													typeof(Or<,>), typeof(Optional2<>), branchFieldType, typeof(IsNull), 
											typeof(Or<>),
												typeof(Where<Ledger.balanceType, NotEqual<LedgerBalanceType.actual>>));
			
			if (restrictSelect != null)
			{
				selectRestrictor=BqlCommand.Compose(typeof(Where2<,>), selectRestrictor, typeof(And<>), restrictSelect);

				if (selectDefault != null)
				{
					selectDefault= ((BqlCommand.CreateInstance(selectDefault)).WhereAnd(restrictSelect)).GetType();
				}
			}
			_Attributes.Add(new PXRestrictorAttribute(selectRestrictor, Messages.TheSelectedLedgerDoesNotBelongToTheSelectedCompanyOrBranch));
			
			_Attributes.Add(selectDefault != null ? new PXDefaultAttribute(selectDefault) : new PXDefaultAttribute());
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (OrganizationFieldType != null)
			{
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(OrganizationFieldType), OrganizationFieldType.Name,
					(PXCache cache, PXFieldUpdatedEventArgs e) =>
					{
						cache.SetValueExt(e.Row, _FieldName, null);
					}
				);
			}

			if (BranchFieldType != null)
			{
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(BranchFieldType), BranchFieldType.Name,
					(PXCache cache, PXFieldUpdatedEventArgs e) =>
					{
						cache.SetValueExt(e.Row, _FieldName, null);
					}
				);
			}
		}
	}
}
