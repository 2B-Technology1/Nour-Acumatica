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
using System.Collections;
using System.Linq;

using PX.Common;
using PX.Data;
using PX.Objects.Common.Extensions;

using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public class ARPriceCodeSelectorAttribute : PXCustomSelectorAttribute
	{
		[PXHidden]
		public class ARPriceCode : IBqlTable
		{
			#region PriceCode
			public abstract class priceCode : Data.BQL.BqlString.Field<priceCode> { }
			[PXString(30, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", IsKey = true)]
			[PXUIField(DisplayName = "Price Code", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string PriceCode
			{
				get;
				set;
			}
			#endregion

			#region Description
			public abstract class description : Data.BQL.BqlString.Field<description> { }
			[PXString(256, IsUnicode = true, IsKey = true)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual string Description
			{
				get;
				set;
			}
			#endregion
		}

		private Type _priceTypeField;

		public virtual bool SuppressReadDeletedSupport { get; set; } = false;

		public ARPriceCodeSelectorAttribute(Type priceTypeField)
			: base(typeof(ARPriceCode.priceCode), typeof(ARPriceCode.priceCode), typeof(ARPriceCode.description))
		{
			_priceTypeField = priceTypeField;
			SuppressUnconditionalSelect = true;
			DescriptionField = typeof(ARPriceCode.description);
		}

		protected virtual PXView CustomerCodeView(PXGraph graph)
			=> graph.TypedViews.GetView(new Select<BAccount,
					Where2<Where<BAccount.type, Equal<BAccountType.customerType>, Or<BAccount.type, Equal<BAccountType.combinedType>>>,
						And2<Where<BAccount.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
							Or<Not<FeatureInstalled<FeaturesSet.visibilityRestriction>>>>,
						And<Match<Current<AccessInfo.userName>>>>>>(), true);

		protected virtual PXView PriceClassCodeView(PXGraph graph)
			=> graph.TypedViews.GetView(new Select<ARPriceClass>(), true);

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var priceCodeType = sender.GetValue(e.Row, _priceTypeField.Name) as string;
			if (string.IsNullOrEmpty(priceCodeType) || priceCodeType == PriceTypeList.BasePrice)
				return;

			base.FieldVerifying(sender, e);
		}

		protected virtual void ModifyFilters(PXFilterRow[] filters, string originalFieldName, string newFieldName)
		{
			if (filters != null)
				foreach (PXFilterRow filter in filters)
				{
					if (string.Compare(filter.DataField, originalFieldName, true) == 0)
						filter.DataField = newFieldName;
				}
		}

		protected virtual void ModifyColumns(string[] filters, string originalFieldName, string newFieldName)
		{
			if (filters == null || filters.Length == 0)
				return;
			var index = filters.FindIndex(x => x != null && x.Equals(originalFieldName, StringComparison.InvariantCultureIgnoreCase));
			if (index >= 0)
				filters[index] = newFieldName;
		}

		protected virtual IEnumerable GetRecords()
		{
			var graph = PXView.CurrentGraph;
			var priceFilterCache = graph?.Caches[_BqlTable];

			var priceFilter = PXView.Currents.FirstOrDefault(c => _BqlTable.IsAssignableFrom(c.GetType()));
			priceFilter ??= priceFilterCache?.Current;
			if (priceFilter == null)
				return Array<ARPriceCode>.Empty;

			var priceCodeType = priceFilterCache.GetValue(priceFilter, _priceTypeField.Name) as string;
			if (string.IsNullOrEmpty(priceCodeType) || priceCodeType == PriceTypeList.BasePrice)
				return Array<ARPriceCode>.Empty;

			void ModifyFields(string[] columns, PXFilterRow[] filters, string priceCode, string desc)
			{
				ModifyColumns(columns, typeof(ARPriceCode.priceCode).Name, priceCode);
				ModifyColumns(columns, typeof(ARPriceCode.description).Name, desc);

				ModifyFilters(filters, typeof(ARPriceCode.priceCode).Name, priceCode);
				ModifyFilters(filters, typeof(ARPriceCode.description).Name, desc);
			};

			string[] viewColumns = PXView.SortColumns;
			PXFilterRow[] viewFilters = PXView.Filters;
			int startRow = PXView.StartRow;
			int totalRows = 0;
			ARPriceCode[] rows;

			if (priceCodeType == PriceTypeList.Customer)
			{
				ModifyFields(viewColumns, viewFilters, typeof(BAccount.acctCD).Name, typeof(BAccount.acctName).Name);
				rows = CustomerCodeView(graph)
					.Select(PXView.Currents, PXView.Parameters, PXView.Searches, viewColumns, PXView.Descendings, viewFilters, ref startRow, PXView.MaximumRows, ref totalRows)
					.RowCast<BAccount>()
					.Select(x => new ARPriceCode { PriceCode = x.AcctCD.Trim(), Description = x.AcctName })
					.ToArray();
			}
			else
			{
				ModifyFields(viewColumns, viewFilters, typeof(ARPriceClass.priceClassID).Name, typeof(ARPriceClass.description).Name);
				rows = PriceClassCodeView(graph)
					.Select(PXView.Currents, PXView.Parameters, PXView.Searches, viewColumns, PXView.Descendings, viewFilters, ref startRow, PXView.MaximumRows, ref totalRows)
					.RowCast<ARPriceClass>()
					.Select(x => new ARPriceCode { PriceCode = x.PriceClassID.Trim(), Description = x.Description })
					.ToArray();
			}

			PXView.StartRow = 0;

			var result = new PXDelegateResult
			{
				IsResultSorted = true,
				IsResultFiltered = true,
			};

			result.AddRange(rows);

			return result;
		}

		protected override bool IsReadDeletedSupported
		{
			get
			{
				if (SuppressReadDeletedSupport) return false;

				return base.IsReadDeletedSupported;
			}
		}
	}
}
