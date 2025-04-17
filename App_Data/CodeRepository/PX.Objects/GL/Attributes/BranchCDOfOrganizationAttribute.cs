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

namespace PX.Objects.GL.Attributes {

	[PXDBString(255, IsUnicode = true)]
	[PXString(255, IsUnicode = true)]
	[PXUIField(DisplayName = "Branch", FieldClass = _FieldClass)]
	public class BranchCDOfOrganizationAttribute : PXEntityAttribute 
	{
		public const string _FieldClass = "BRANCH";
		public const string _DimensionName = "BRANCH";

		public readonly Type OrganizationFieldType;

		public virtual PXSelectorMode SelectorMode { get; set; } = PXSelectorMode.DisplayModeValue;

		public BranchCDOfOrganizationAttribute(Type organizationFieldType, bool onlyActive = true, Type searchType = null)
		{
			Initialize();

			OrganizationFieldType = organizationFieldType;

			Type selectorSource = typeof(Search<Branch.branchCD, Where<MatchWithBranch<Branch.branchID>>>);

			PXSelectorAttribute attr =
				new PXSelectorAttribute(selectorSource)
				{
					DescriptionField = typeof(Branch.acctName),
					SelectorMode= SelectorMode
				};

			_Attributes.Add(attr);

			_Attributes.Add(new PXRestrictorAttribute(BqlCommand.Compose(
															typeof(Where<,,>),
															typeof(Branch.organizationID), typeof(Equal<>), typeof(Optional2<>), OrganizationFieldType,
															typeof(Or<,>), typeof(Optional2<>), OrganizationFieldType, typeof(IsNull)),
														Messages.TheSpecifiedBranchDoesNotBelongToTheSelectedCompany));
			if (onlyActive)
			{
				_Attributes.Add(new PXRestrictorAttribute(typeof(Where<Branch.active, Equal<True>>), Messages.BranchInactive));
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (OrganizationFieldType != null)
			{
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(OrganizationFieldType), OrganizationFieldType.Name, OrganizationFieldUpdated);
			}
		}

		private void OrganizationFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			cache.SetValueExt(e.Row, _FieldName, null);
		}
	}
}
