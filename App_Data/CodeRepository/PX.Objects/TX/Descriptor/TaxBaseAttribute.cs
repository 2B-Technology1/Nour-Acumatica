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

using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM.TemporaryHelpers;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PX.Objects.Common;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.TX
{
	public abstract partial class TaxBaseAttribute : PXAggregateAttribute,
													 IPXRowInsertedSubscriber,
													 IPXRowUpdatedSubscriber,
													 IPXRowDeletedSubscriber,
													 IPXRowPersistedSubscriber,
													 IComparable
	{
		protected string _LineNbr = "LineNbr";
		protected string _CuryOrigTaxableAmt = "CuryOrigTaxableAmt";
		protected string _CuryTaxAmt = "CuryTaxAmt";
		protected string _CuryTaxAmtSumm = "CuryTaxAmtSumm";
		protected string _CuryTaxDiscountAmt = "CuryTaxDiscountAmt";
		protected string _CuryTaxableAmt = "CuryTaxableAmt";
		protected string _CuryExemptedAmt = "CuryExemptedAmt";
		protected string _CuryTaxableDiscountAmt = "CuryTaxableDiscountAmt";
		protected string _CuryExpenseAmt = "CuryExpenseAmt";
		protected string _CuryRateTypeID = "CuryRateTypeID";
		protected string _CuryEffDate = "CuryEffDate";
		protected string _CuryRate = "SampleCuryRate";
		protected string _RecipRate = "SampleRecipRate";
		protected string _IsTaxSaved = "IsTaxSaved";
		protected string _RecordID = "RecordID";
		protected string _ExternalTaxesImportInProgress = "ExternalTaxesImportInProgress";
		protected string _IsDirectTaxLine = "IsDirectTaxLine";

		protected Type _ParentType;
		protected Type _ChildType;
		protected Type _TaxType;
		protected Type _TaxSumType;
		protected Type _CuryKeyField = null;

		protected Dictionary<object, object> inserted = null;
		protected Dictionary<object, object> updated = null;

		protected bool _IncludeDirectTaxLine = false;

		#region TaxID
		protected string _TaxID = "TaxID";
		public Type TaxID
		{
			set
			{
				_TaxID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region TaxCategoryID
		protected string _TaxCategoryID = "TaxCategoryID";
		public Type TaxCategoryID
		{
			set
			{
				_TaxCategoryID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region TaxZoneID
		protected string _TaxZoneID = "TaxZoneID";
		public Type TaxZoneID
		{
			set
			{
				_TaxZoneID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region DocDate
		protected string _DocDate = "DocDate";
		public Type DocDate
		{
			set
			{
				_DocDate = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		public Type ParentBranchIDField { get; set; }
		#region FinPeriodID
		protected string _FinPeriodID = "FinPeriodID";
		public Type FinPeriodID
		{
			set
			{
				_FinPeriodID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion

		#region CuryTranAmt
		protected abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }
		protected Type CuryTranAmt = typeof(curyTranAmt);
		protected string _CuryTranAmt
		{
			get
			{
				return CuryTranAmt.Name;
			}
		}
		#endregion

		#region OrigGroupDiscountRate
		protected abstract class origGroupDiscountRate : PX.Data.BQL.BqlDecimal.Field<origGroupDiscountRate> { }
		protected Type OrigGroupDiscountRate = typeof(origGroupDiscountRate);
		protected string _OrigGroupDiscountRate
		{
			get
			{
				return OrigGroupDiscountRate.Name;
			}
		}
		#endregion
		#region OrigDocumentDiscountRate
		protected abstract class origDocumentDiscountRate : PX.Data.BQL.BqlDecimal.Field<origDocumentDiscountRate> { }
		protected Type OrigDocumentDiscountRate = typeof(origDocumentDiscountRate);
		protected string _OrigDocumentDiscountRate
		{
			get
			{
				return OrigDocumentDiscountRate.Name;
			}
		}
		#endregion
		#region GroupDiscountRate
		protected abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate> { }
		protected Type GroupDiscountRate = typeof(groupDiscountRate);
		protected string _GroupDiscountRate
		{
			get
			{
				return GroupDiscountRate.Name;
			}
		}
		#endregion
		#region DocumentDiscountRate
		protected abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate> { }
		protected Type DocumentDiscountRate = typeof(documentDiscountRate);
		protected string _DocumentDiscountRate
		{
			get
			{
				return DocumentDiscountRate.Name;
			}
		}
		#endregion
		#region TermsID
		protected string _TermsID = "TermsID";
		public Type TermsID
		{
			set
			{
				_TermsID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryID
		protected string _CuryID = "CuryID";
		public Type CuryID
		{
			set
			{
				_CuryID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryDocBal
		protected string _CuryDocBal = "CuryDocBal";
		public Type CuryDocBal
		{
			set
			{
				_CuryDocBal = (value != null) ? value.Name : null;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryTaxDiscountTotal
		protected string _CuryTaxDiscountTotal = "CuryOrigTaxDiscAmt";
		public Type CuryDocBalUndiscounted
		{
			set
			{
				_CuryTaxDiscountTotal = (value != null) ? value.Name : null;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryTaxTotal
		protected string _CuryTaxTotal = "CuryTaxTotal";
		public Type CuryTaxTotal
		{
			set
			{
				_CuryTaxTotal = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryTaxInclTotal
		protected string _CuryTaxInclTotal = null;
		public Type CuryTaxInclTotal
		{
			set
			{
				_CuryTaxInclTotal = value?.Name;
			}
		}
		#endregion
		#region CuryOrigDiscAmt
		protected string _CuryOrigDiscAmt = "CuryOrigDiscAmt";
		public Type CuryOrigDiscAmt
		{
			set
			{
				_CuryOrigDiscAmt = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryWhTaxTotal
		protected string _CuryWhTaxTotal = "CuryOrigWhTaxAmt";
		public Type CuryWhTaxTotal
		{
			set
			{
				_CuryWhTaxTotal = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryLineTotal
		protected abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		public Type CuryLineTotal = typeof(curyLineTotal);
		protected string _CuryLineTotal
		{
			get
			{
				return CuryLineTotal.Name;
			}
		}
		#endregion
		#region CuryDiscTot
		protected abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }
		protected Type CuryDiscTot = typeof(curyDiscTot);
		protected string _CuryDiscTot
		{
			get
			{
				return CuryDiscTot.Name;
			}
		}
		#endregion
		#region TaxCalc
		protected TaxCalc _TaxCalc = TaxCalc.Calc;
		public TaxCalc TaxCalc
		{
			set
			{
				_TaxCalc = value;
			}
			get
			{
				return _TaxCalc;
			}
		}
		#endregion
		#region TaxFlags
		protected TaxCalc _TaxFlags = TaxCalc.NoCalc;
		public TaxCalc TaxFlags
		{
			set
			{
				_TaxFlags = value;
			}
			get
			{
				return _TaxFlags;
			}
		}
		#endregion
		#region TaxCalcMode
		protected string _TaxCalcMode = null;
		public Type TaxCalcMode
		{
			set
			{
				_TaxCalcMode = value.Name;
			}
			get
			{
				return null;
			}
		}
		protected virtual bool _isTaxCalcModeEnabled
		{
			get { return !String.IsNullOrEmpty(_TaxCalcMode) && _NetGrossEntryModeEnabled; }
		}

		protected virtual bool _NetGrossEntryModeEnabled
		{
			get
			{
				if (netGrossEntryModeEnable == null)
					netGrossEntryModeEnable = PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>();
				return netGrossEntryModeEnable == true;
			}
		}
		private bool? netGrossEntryModeEnable = null;
		

		#endregion
		#region Precision
		public int? Precision { get; set; }
		#endregion

		#region RetainageApplyFieldName
		protected virtual string RetainageApplyFieldName => nameof(APInvoice.RetainageApply);
		#endregion
		protected virtual bool CalcGrossOnDocumentLevel { get; set; }
		protected virtual bool AskRecalculationOnCalcModeChange { get; set; }
		protected virtual string _PreviousTaxCalcMode { get; set; }

		public Type ChildBranchIDField { get; set; }
		public Type ChildFinPeriodIDField { get; set; }

		protected bool _NoSumTaxable = false;

		public static List<PXEventSubscriberAttribute> GetAttributes<Field, Target>(PXCache sender, object data)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			bool exactfind = false;

			var res = new List<PXEventSubscriberAttribute>();
			var q = sender.GetAttributes<Field>(data).Where(
				(attr) => (!exactfind || data == null) && ((exactfind = attr.GetType() == typeof(Target))
				|| attr is TaxAttribute && typeof(Target) == typeof(TaxAttribute)));

			foreach (var a in q)
			{
				a.IsDirty = true;
				res.Add(a);
			}


			res.Sort((a, b) => ((IComparable)a).CompareTo(b));

			return res;
		}

		public static void SetTaxCalc<Field>(PXCache cache, object data, TaxCalc isTaxCalc)
			where Field : IBqlField
		{
			SetTaxCalc<Field, TaxAttribute>(cache, data, isTaxCalc);
		}

		public static void SetTaxCalc<Field, Target>(PXCache cache, object data, TaxCalc isTaxCalc)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(cache, data))
			{
				((TaxAttribute)attr).TaxCalc = (TaxCalc) ((short)isTaxCalc & (short)TaxCalc.ManualLineCalc);
				((TaxAttribute)attr).TaxFlags = (TaxCalc)((short)isTaxCalc & (short)TaxCalc.Flags);
			}
		}

		public static TaxCalc GetTaxCalc<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			return GetTaxCalc<Field, TaxAttribute>(cache, data);
		}

		public static TaxCalc GetTaxCalc<Field, Target>(PXCache cache, object data)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(cache, data))
			{
				if (((TaxAttribute)attr).TaxCalc != TaxCalc.NoCalc)
				{
					return TaxCalc.Calc;
				}
			}
			return TaxCalc.NoCalc;
		}

		public static void IncludeDirectTaxLine<Field>(PXCache cache, object data, bool includeDirectTaxLine)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is TaxBaseAttribute)
				{
					((TaxBaseAttribute)attr)._IncludeDirectTaxLine = includeDirectTaxLine;
				}
			}
		}

		public virtual object Insert(PXCache cache, object item)
		{
			return cache.Insert(item);
		}

		public virtual object Update(PXCache cache, object item)
		{
			return cache.Update(item);
		}

		public virtual object Delete(PXCache cache, object item)
		{
			return cache.Delete(item);
		}

		public static void Calculate<Field>(PXCache sender, PXRowInsertedEventArgs e)
			where Field : IBqlField
		{
			Calculate<Field, TaxAttribute>(sender, e);
		}

		public static bool IsDirectTaxLine<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is TaxBaseAttribute)
				{
					if (((TaxBaseAttribute)attr).IsDirectTaxLine(cache, data))
					{
						return true;
					}
				}
			}

			return false;
		}

		public static void Calculate<Field, Target>(PXCache sender, PXRowInsertedEventArgs e)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			bool isCalcedByAttribute = false;
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(sender, e.Row))
			{
				isCalcedByAttribute = true;

				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualLineCalc)
				{
					((TaxAttribute)attr).TaxCalc = TaxCalc.Calc;

					try
					{
						((IPXRowInsertedSubscriber)attr).RowInserted(sender, e);
					}
					finally
					{
						((TaxAttribute)attr).TaxCalc = TaxCalc.ManualLineCalc;
					}
				}

				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualCalc)
				{
					object copy;
					if (((TaxAttribute)attr).inserted.TryGetValue(e.Row, out copy))
					{
						((IPXRowUpdatedSubscriber)attr).RowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, copy, false));
						((TaxAttribute)attr).inserted.Remove(e.Row);

						if (((TaxAttribute)attr).updated.TryGetValue(e.Row, out copy))
						{
							((TaxAttribute)attr).updated.Remove(e.Row);
						}
					}
				}
			}

			if (!isCalcedByAttribute)
			{
				InvokeRecalcTaxes(sender);
			}
		}

		public static void Calculate<Field>(PXCache sender, PXRowUpdatedEventArgs e)
			where Field : IBqlField
		{
			Calculate<Field, TaxAttribute>(sender, e);
		}

		public static void Calculate<Field, Target>(PXCache sender, PXRowUpdatedEventArgs e)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			bool isCalcedByAttribute = false;
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(sender, e.Row))
			{
				isCalcedByAttribute = true;

				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualLineCalc)
				{
					((TaxAttribute)attr).TaxCalc = TaxCalc.Calc;

					try
					{
						((IPXRowUpdatedSubscriber)attr).RowUpdated(sender, e);
					}
					finally
					{
						((TaxAttribute)attr).TaxCalc = TaxCalc.ManualLineCalc;
					}
				}

				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualCalc)
				{
					object copy;
					if (((TaxAttribute)attr).updated.TryGetValue(e.Row, out copy))
					{
						((IPXRowUpdatedSubscriber)attr).RowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, copy, false));
						((TaxAttribute)attr).updated.Remove(e.Row);
					}
				}
			}
		}

		internal static void InvokeRecalcTaxes(PXCache sender)
        {
			var tgraph = sender.Graph.GetType();
			var extensions = tgraph.GetField("Extensions", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(sender.Graph) as PXGraphExtension[];
			var ext = extensions?.FirstOrDefault(extension => IsInstanceOfGenericType(typeof(Extensions.SalesTax.TaxBaseGraph<,>), extension));

			if (ext == null) return;

			var method = ext.GetType().GetMethod("RecalcTaxes", BindingFlags.NonPublic | BindingFlags.Instance);
			method?.Invoke(ext, null);
		}

		private static bool IsInstanceOfGenericType(Type genericType, object instance)
		{
			Type type = instance.GetType();
			while (type != null)
			{
				if (type.IsGenericType &&
					type.GetGenericTypeDefinition() == genericType)
				{
					return true;
				}
				type = type.BaseType;
			}
			return false;
		}

		protected virtual string GetTaxZone(PXCache sender, object row)
		{
			return (string)ParentGetValue(sender.Graph, _TaxZoneID);
		}

		protected virtual DateTime? GetDocDate(PXCache sender, object row)
		{
			return (DateTime?)ParentGetValue(sender.Graph, _DocDate);
		}

		protected virtual string GetTaxCategory(PXCache sender, object row)
		{
			return (string)sender.GetValue(row, _TaxCategoryID);
		}

		protected virtual bool? GetIsDirectTaxLine(PXCache sender, object row)
		{
			return (bool?)sender.GetValue(row, _IsDirectTaxLine);
		}

		protected virtual decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType = "I")
		{
			return (decimal?)sender.GetValue(row, _CuryTranAmt);
		}

		protected virtual decimal? GetDocLineFinalAmtNoRounding(PXCache sender, object row, string TaxCalcType = "I")
		{
			return (decimal?)sender.GetValue(row, _CuryTranAmt);
		}

		protected virtual string GetTaxID(PXCache sender, object row)
		{
			return (string)sender.GetValue(row, _TaxID);
		}

		protected virtual object InitializeTaxDet(object data)
		{
			return data;
		}

		public virtual bool IsExternalTax(PXGraph graph, string taxZoneID)
		{
			TaxZone taxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(graph, taxZoneID);
			if (taxZone != null)
				return taxZone.IsExternal.GetValueOrDefault(false) && !String.IsNullOrEmpty(taxZone.TaxPluginID);
			else
				return false;
		}

		protected virtual void AddOneTax(PXCache cache, object detrow, ITaxDetail taxitem)
		{
			if (taxitem != null)
			{
				object newdet;
				TaxParentAttribute.NewChild(cache, detrow, _ChildType, out newdet);
				((ITaxDetail)newdet).TaxID = taxitem.TaxID;
				newdet = InitializeTaxDet(newdet);
				object insdet = Insert(cache, newdet);

				if (insdet != null) PXParentAttribute.SetParent(cache, insdet, _ChildType, detrow);
			}
		}

		public virtual ITaxDetail MatchesCategory(PXCache sender, object row, ITaxDetail zoneitem)
		{
			string taxcat = GetTaxCategory(sender, row);
			string taxid = GetTaxID(sender, row);
			DateTime? docdate = GetDocDate(sender, row);

			TaxRev rev = PXSelect<TaxRev, Where<TaxRev.taxID, Equal<Required<TaxRev.taxID>>, And<Required<TaxRev.startDate>, Between<TaxRev.startDate, TaxRev.endDate>, And<TaxRev.outdated, Equal<False>>>>>.Select(sender.Graph, zoneitem.TaxID, docdate);

			if (rev == null)
			{
				return null;
			}

			if (string.Equals(taxid, zoneitem.TaxID))
			{
				return zoneitem;
			}

			TaxCategory cat = (TaxCategory)PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(sender.Graph, taxcat);

			if (cat == null)
			{
				return null;
			}
			else
			{
				return MatchesCategory(sender, row, new ITaxDetail[] { zoneitem }).FirstOrDefault();
			}

		}

		public virtual IEnumerable<ITaxDetail> MatchesCategory(PXCache sender, object row, IEnumerable<ITaxDetail> zonetaxlist)
		{
			string taxcat = GetTaxCategory(sender, row);

			List<ITaxDetail> ret = new List<ITaxDetail>();

			TaxCategory cat = (TaxCategory)PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(sender.Graph, taxcat);

			if (cat == null)
			{
				return ret;
			}

			HashSet<string> cattaxlist = new HashSet<string>();
			foreach (TaxCategoryDet detail in PXSelect<TaxCategoryDet, Where<TaxCategoryDet.taxCategoryID, Equal<Required<TaxCategoryDet.taxCategoryID>>>>.Select(sender.Graph, taxcat))
			{
				cattaxlist.Add(detail.TaxID);
			}

			foreach (ITaxDetail zoneitem in zonetaxlist)
			{
				bool zonematchestaxcat = cattaxlist.Contains(zoneitem.TaxID);
				if (cat.TaxCatFlag == false && zonematchestaxcat || cat.TaxCatFlag == true && !zonematchestaxcat)
				{
					ret.Add(zoneitem);
				}
			}

			return ret;
		}


		protected abstract IEnumerable<ITaxDetail> ManualTaxes(PXCache sender, object row);

		protected virtual void DefaultTaxes(PXCache sender, object row, bool DefaultExisting)
		{
			PXCache cache = sender.Graph.Caches[_TaxType];
			string taxzone = GetTaxZone(sender, row);
			string taxcat = GetTaxCategory(sender, row);
			DateTime? docdate = GetDocDate(sender, row);

			var applicableTaxes = new HashSet<string>();
			bool isDirectTaxLine = IsDirectTaxLine(sender, row);
			string applicableDirectTaxId = null;

			foreach (PXResult<TaxZoneDet, TaxCategory, TaxRev, TaxCategoryDet, Tax> r in PXSelectJoin<TaxZoneDet,
				CrossJoin<TaxCategory,
				InnerJoin<TaxRev, On<TaxRev.taxID, Equal<TaxZoneDet.taxID>>,
				LeftJoin<TaxCategoryDet, On<TaxCategoryDet.taxID, Equal<TaxZoneDet.taxID>,
					And<TaxCategoryDet.taxCategoryID, Equal<TaxCategory.taxCategoryID>>>,
				LeftJoin<Tax,On<Tax.taxID,Equal<TaxZoneDet.taxID>>>>>>,
				Where<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>,
					And<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>,
					And<Required<TaxRev.startDate>, Between<TaxRev.startDate, TaxRev.endDate>, And<TaxRev.outdated, Equal<False>,
					And<Where<TaxCategory.taxCatFlag, Equal<False>, And<TaxCategoryDet.taxCategoryID, IsNotNull,
						Or<TaxCategory.taxCatFlag, Equal<True>, And<TaxCategoryDet.taxCategoryID, IsNull>>>>>>>>>>.Select(sender.Graph, taxzone, taxcat, docdate))
			{
				Tax tax = (Tax)r;
				TaxZoneDet taxZoneDet = (TaxZoneDet)r;

				if (!isDirectTaxLine && tax.DirectTax != true)
				{
					AddOneTax(cache, row, taxZoneDet);
					applicableTaxes.Add(taxZoneDet.TaxID);
				}
				else if (isDirectTaxLine && tax.DirectTax == true)
				{
					AddOneTax(cache, row, taxZoneDet);
					applicableTaxes.Add(taxZoneDet.TaxID);
					applicableDirectTaxId = taxZoneDet.TaxID;
				}
			}

			string taxID;
			if ((taxID = GetTaxID(sender, row)) != null)
			{
				AddOneTax(cache, row, new TaxZoneDet() { TaxID = taxID });
				applicableTaxes.Add(taxID);
			}

			foreach (ITaxDetail r in ManualTaxes(sender, row))
			{
				if (applicableTaxes.Contains(r.TaxID))
					applicableTaxes.Remove(r.TaxID);
			}

			foreach (string applicableTax in applicableTaxes)
			{
				AddTaxTotals(cache, applicableTax, row);
			}

			if (DefaultExisting)
			{
				foreach (ITaxDetail r in MatchesCategory(sender, row, ManualTaxes(sender, row)))
				{
					Tax tax = Tax.PK.Find(sender.Graph, r.TaxID);

					if (!isDirectTaxLine && tax.DirectTax != true)
					{
						AddOneTax(cache, row, r);
					}
					else if(isDirectTaxLine && applicableDirectTaxId == tax.TaxID)
					{
						AddOneTax(cache, row, r);
					}
				}
			}

			// update IsDirectTax field with new value if field exists.
			if (_IncludeDirectTaxLine && GetFieldType(sender,_IsDirectTaxLine) != null)
			{
				UpdateIsDirectTaxLineFeildValue(sender, row, isDirectTaxLine, GetIsDirectTaxLine(sender, row) == true);
			}
		}

		protected virtual void UpdateIsDirectTaxLineFeildValue(PXCache cache, object row, bool newValue, bool oldValue)
		{
			if (newValue != oldValue)
			{
				SetValueOptional(cache, row, newValue, _IsDirectTaxLine);
			}
		}

		protected virtual bool IsDirectTaxLine(PXCache sender, object row)
		{
			if (!_IncludeDirectTaxLine) return false;

			string taxzone = GetTaxZone(sender, row);
			string taxcat = GetTaxCategory(sender, row);
			DateTime? docdate = GetDocDate(sender, row);

			if (string.IsNullOrEmpty(taxzone) || string.IsNullOrEmpty(taxcat)
				|| TaxCategory.PK.Find(sender.Graph, taxcat)?.TaxCatFlag == true) return false;

			HashSet<string> applicableDirectTaxes = new HashSet<string>();
			HashSet<string> applicableIndirectTaxes = new HashSet<string>();

			foreach (PXResult<TaxZoneDet, TaxCategory, TaxRev, TaxCategoryDet, Tax> r in SelectFrom<TaxZoneDet>
				.CrossJoin<TaxCategory>
				.InnerJoin<TaxRev>
					.On<TaxRev.taxID.IsEqual<TaxZoneDet.taxID>>
				.LeftJoin<TaxCategoryDet>
					.On<TaxCategoryDet.taxID.IsEqual<TaxZoneDet.taxID>.And<TaxCategoryDet.taxCategoryID.IsEqual<TaxCategory.taxCategoryID>>>
				.LeftJoin<Tax>
					.On<Tax.taxID.IsEqual<TaxZoneDet.taxID>>
				.Where<TaxZoneDet.taxZoneID.IsEqual<@P.AsString>
					.And<TaxCategory.taxCategoryID.IsEqual<@P.AsString>>
					.And<@P.AsDateTime.IsBetween<TaxRev.startDate, TaxRev.endDate>>
					.And<TaxRev.outdated.IsEqual<False>>
					.And<TaxCategory.taxCatFlag.IsEqual<False>
					.And<TaxCategoryDet.taxCategoryID.IsNotNull>>>
					.View.Select(sender.Graph, taxzone, taxcat, docdate))
			{
				Tax tax = (Tax)r;
				if (tax.DirectTax == true)
				{
					applicableDirectTaxes.Add(tax.TaxID);
				}
				else
				{
					applicableIndirectTaxes.Add(tax.TaxID);
				}
			}

			//  direct tax can be applicable only if one direct tax is there an no other indirect tax is appllicable
			if (applicableIndirectTaxes.Count == 0 && applicableDirectTaxes.Count == 1 && !SkipDirectTax(sender, row, applicableDirectTaxes.First()))
			{
				return true;
			}

			ShowInvalidDirectTaxCombinationWarnings(sender, row, taxcat, applicableDirectTaxes, applicableIndirectTaxes);

			return false;
		}

		protected virtual void ShowInvalidDirectTaxCombinationWarnings(PXCache sender, object row, string taxcat, HashSet<string> applicableDirectTaxes, HashSet<string> applicableIndirectTaxes)
		{
			if (applicableIndirectTaxes.Count > 0 && applicableDirectTaxes.Count > 0)
			{
				sender.RaiseExceptionHandling(_TaxCategoryID, row, taxcat,
					new PXSetPropertyException(Messages.ImportTaxeAndNonImportTaxesCannotBeAppliedTogether, PXErrorLevel.Warning,
					applicableDirectTaxes.First(), applicableIndirectTaxes.First()));
			}
			else if (applicableIndirectTaxes.Count == 0 && applicableDirectTaxes.Count > 1)
			{
				sender.RaiseExceptionHandling(_TaxCategoryID, row, taxcat,
					new PXSetPropertyException(Messages.SeveralImportTaxesCanNotBeApplied, PXErrorLevel.Warning, taxcat));
			}
		}

		protected virtual void DefaultTaxes(PXCache sender, object row)
		{
			DefaultTaxes(sender, row, true);
		}

		private Type GetFieldType(PXCache cache, string FieldName)
		{
			List<Type> fields = cache.BqlFields;
			for (int i = 0; i < fields.Count; i++)
			{
				if (String.Compare(fields[i].Name, FieldName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return fields[i];
				}
			}
			return null;
		}

		private Type GetTaxIDType(PXCache cache)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXSelectorAttribute)
				{
					if (((PXSelectorAttribute)attr).Field == typeof(Tax.taxID))
					{
						return GetFieldType(cache, ((PXSelectorAttribute)attr).FieldName);
					}
				}
			}
			return null;
		}

		private Type AddWhere(Type command, Type where)
		{
			if (command.IsGenericType)
			{
				Type[] args = command.GetGenericArguments();
				Type[] pars = new Type[args.Length + 1];
				pars[0] = command.GetGenericTypeDefinition();
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].IsGenericType && (
						args[i].GetGenericTypeDefinition() == typeof(Where<,>) ||
						args[i].GetGenericTypeDefinition() == typeof(Where2<,>) ||
						args[i].GetGenericTypeDefinition() == typeof(Where<,,>)))
					{
						pars[i + 1] = typeof(Where2<,>).MakeGenericType(args[i], typeof(And<>).MakeGenericType(where));
					}
					else
					{
						pars[i + 1] = args[i];
					}
				}
				return BqlCommand.Compose(pars);
			}
			return null;
		}

		protected List<object> SelectTaxes(PXCache sender, object row, PXTaxCheck taxchk)
		{
			return SelectTaxes<Where<True, Equal<True>>>(sender.Graph, row, taxchk);
		}

		protected abstract List<Object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
			where Where : IBqlWhere, new();

		protected abstract List<Object> SelectDocumentLines(PXGraph graph, object row);

		protected Tax AdjustTaxLevel(PXGraph graph, Tax taxToAdjust)
		{
			if (_isTaxCalcModeEnabled && taxToAdjust.TaxCalcLevel != CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt && taxToAdjust.DirectTax != true)
			{
				string TaxCalcMode = GetTaxCalcMode(graph);
				if (!String.IsNullOrEmpty(TaxCalcMode))
				{
					Tax adjdTax = (Tax)graph.Caches[typeof(Tax)].CreateCopy(taxToAdjust);
					switch (TaxCalcMode)
					{
						case TaxCalculationMode.Gross:
							adjdTax.TaxCalcLevel = CSTaxCalcLevel.Inclusive;
							break;
						case TaxCalculationMode.Net:
							adjdTax.TaxCalcLevel = CSTaxCalcLevel.CalcOnItemAmt;
							break;
						case TaxCalculationMode.TaxSetting:
							break;
					}
					return adjdTax;
				}
			}
			return taxToAdjust;
		}

		protected virtual void ClearTaxes(PXCache sender, object row)
		{
			PXCache cache = sender.Graph.Caches[_TaxType];
			foreach (object taxrow in SelectTaxes(sender, row, PXTaxCheck.Line))
			{
				Delete(cache, ((PXResult)taxrow)[0]);
			}
		}

		private decimal Sum(PXGraph graph, List<Object> list, Type field)
		{
			if (field == null) return 0;
			else
			{
				Type itemType = BqlCommand.GetItemType(field);
				return list
					.Cast<PXResult>()
					.Select(a => (decimal?)graph.Caches[itemType].GetValue(a[itemType], field.Name) ?? 0m)
					.Sum();
			}
		}

		protected virtual void AddTaxTotals(PXCache sender, string taxID, object row)
		{
			PXCache cache = sender.Graph.Caches[_TaxSumType];

			object newdet = Activator.CreateInstance(_TaxSumType);
			((TaxDetail)newdet).TaxID = taxID;
			newdet = InitializeTaxDet(newdet);
			object insdet = Insert(cache, newdet);
		}

		protected Terms SelectTerms(PXGraph graph)
		{
			string TermsID = (string)ParentGetValue(graph, _TermsID);
			Terms ret = TermsAttribute.SelectTerms(graph, TermsID);
			ret = ret ?? new Terms();

			return ret;
		}

		protected virtual void SetTaxableAmt(PXCache sender, object row, decimal? value)
		{
		}

		protected virtual void SetTaxAmt(PXCache sender, object row, decimal? value)
		{
		}

		protected virtual bool IsDeductibleVATTax(Tax tax)
		{
			return tax?.DeductibleVAT == true;
		}

		protected virtual bool IsExemptTaxCategory(PXGraph graph, object row)
		{
			PXCache sender = graph.Caches[_ChildType];
			return IsExemptTaxCategory(sender, row);
		}

		protected virtual bool IsExemptTaxCategory(PXCache sender, object row)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.exemptedTaxReporting>() != true)
				return false;

			bool isExemptTaxCategory = false;
			string taxCategory = GetTaxCategory(sender, row);

			if (!string.IsNullOrEmpty(taxCategory))
			{
				TaxCategory category = (TaxCategory)PXSelect<
					TaxCategory,
					Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>
					.Select(sender.Graph, taxCategory);

				isExemptTaxCategory = category?.Exempt == true;
			}

			return isExemptTaxCategory;
		}

		protected abstract decimal? GetTaxableAmt(PXCache sender, object row);

		protected abstract decimal? GetTaxAmt(PXCache sender, object row);

		protected List<object> SelectInclusiveTaxes(PXGraph graph, object row)
		{
			List<object> res = new List<object>();

			if (IsExemptTaxCategory(graph, row))
			{
				return res;
			}

			string calcMode = TaxCalculationMode.TaxSetting;
			if (_isTaxCalcModeEnabled)
			{
				string taxCalcMode = GetTaxCalcMode(graph);
				if (!string.IsNullOrEmpty(taxCalcMode))
				{
					calcMode = taxCalcMode;
				}
			}

			if (calcMode == TaxCalculationMode.TaxSetting)
			{
				res = SelectTaxes<Where<
						Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
						And<Tax.taxType, NotEqual<CSTaxType.withholding>,
						And<Tax.directTax, Equal<False>>>>>(graph, row, PXTaxCheck.Line);
			}
			else if (calcMode == TaxCalculationMode.Gross)
			{
				res = SelectTaxes<Where<
						Tax.taxCalcLevel, NotEqual<CSTaxCalcLevel.calcOnItemAmtPlusTaxAmt>,
						And<Tax.taxType, NotEqual<CSTaxType.withholding>,
						And<Tax.directTax, Equal<False>>>>>(graph, row, PXTaxCheck.Line);
			}

			return res;
		}

		protected List<object> SelectLvl1Taxes(PXGraph graph, object row)
		{
			return
				IsExemptTaxCategory(graph, row)
					? new List<object>()
					: SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.calcOnItemAmt>,
				And<Tax.taxCalcLevel2Exclude, Equal<False>>>>(graph, row, PXTaxCheck.Line);
		}

		protected virtual void TaxSetLineDefault(PXCache sender, object taxrow, object row)
		{
			if (taxrow == null)
			{
				throw new PXArgumentException(nameof(taxrow), ErrorMessages.ArgumentNullException);
			}

			TaxDetail taxDetail = (TaxDetail)((PXResult)taxrow)[0];
			Tax tax = PXResult.Unwrap<Tax>(taxrow);
			TaxRev taxRev = PXResult.Unwrap<TaxRev>(taxrow);

			if (taxRev.TaxID == null)
			{
				taxRev.TaxableMin = 0m;
				taxRev.TaxableMax = 0m;
				taxRev.TaxRate = 0m;
			}

			if (IsPerUnitTax(tax))
			{
				TaxSetLineDefaultForPerUnitTaxes(sender, row, tax, taxRev, taxDetail);
				return;
			}

			PXCache cache = sender.Graph.Caches[_TaxType];
			decimal curyTranAmt = (decimal)GetCuryTranAmt(sender, row, tax.TaxCalcType);
			Terms terms = SelectTerms(sender.Graph);

			List<object> inclusiveTaxes = SelectInclusiveTaxes(sender.Graph, row);
			decimal curyInclTaxAmt = SumWithReverseAdjustment(sender.Graph, inclusiveTaxes, GetFieldType(cache, _CuryTaxAmt));
			decimal curyInclTaxDiscountAmt = 0m;
			Type curyTaxDiscountAmtField = GetFieldType(cache, _CuryTaxDiscountAmt);

			if (curyTaxDiscountAmtField != null)
			{
				curyInclTaxDiscountAmt = SumWithReverseAdjustment(sender.Graph, inclusiveTaxes, curyTaxDiscountAmtField);
			}

			decimal curyTaxableAmt = 0.0m;
			decimal curyTaxableDiscountAmt = 0.0m;
			decimal taxableAmt = 0.0m;
			decimal curyTaxAmt = 0.0m;
			decimal curyTaxDiscountAmt = 0.0m;

			DiscPercentsDict.TryGetValue(ParentRow(sender.Graph), out decimal? discPercent);

			decimal calculatedTaxRate = (decimal)taxRev.TaxRate / 100;
			decimal undiscountedPercent = 1 - (discPercent ?? terms.DiscPercent ?? 0m) / 100;

			switch (tax.TaxCalcLevel)
			{
				case CSTaxCalcLevel.Inclusive:
					(curyTaxableAmt, curyTaxAmt) = CalculateInclusiveTaxAmounts(sender, row, cache, taxDetail, inclusiveTaxes,
																				calculatedTaxRate, curyTranAmt);
					break;
				case CSTaxCalcLevel.CalcOnItemAmt:
					{
						decimal curyPerUnitTaxAmount = GetPerUnitTaxAmountForTaxableAdjustmentCalculation(tax, taxDetail, cache, row, sender);
						curyTaxableAmt = curyTranAmt - curyInclTaxAmt - curyInclTaxDiscountAmt + curyPerUnitTaxAmount;
						break;
					}
				case CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt:
					{
						decimal curyPerUnitTaxAmount = GetPerUnitTaxAmountForTaxableAdjustmentCalculation(tax, taxDetail, cache, row, sender);
						List<object> lvl1Taxes = SelectLvl1Taxes(sender.Graph, row);

						decimal curyLevel1TaxAmt = SumWithReverseAdjustment(sender.Graph, lvl1Taxes, GetFieldType(cache, _CuryTaxAmt));

						curyTaxableAmt = curyTranAmt - curyInclTaxAmt + curyLevel1TaxAmt - curyInclTaxDiscountAmt + curyPerUnitTaxAmount;
						break;
					}
			}

			ApplyDiscounts(tax, sender, row, undiscountedPercent, calculatedTaxRate,
						   ref curyTaxableAmt, ref curyTaxableDiscountAmt, ref curyTaxDiscountAmt, ref curyTaxAmt);

			if (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt
					|| tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt)
			{
				if (cache.Fields.Contains(_CuryOrigTaxableAmt))
				{
					cache.SetValue(taxDetail, _CuryOrigTaxableAmt, MultiCurrencyCalculator.RoundCury(cache, taxDetail, curyTaxableAmt, Precision));
				}

				AdjustMinMaxTaxableAmt(cache, taxDetail, taxRev, ref curyTaxableAmt, ref taxableAmt);

				curyTaxAmt = curyTaxableAmt * calculatedTaxRate;
				curyTaxDiscountAmt = curyTaxableDiscountAmt * calculatedTaxRate;

				if (tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxAmount)
				{
					curyTaxAmt *= undiscountedPercent;
				}
			}

			taxDetail.TaxRate = taxRev.TaxRate;
			taxDetail.NonDeductibleTaxRate = taxRev.NonDeductibleTaxRate;
			SetValueOptional(cache, taxDetail, MultiCurrencyCalculator.RoundCury(cache, taxDetail, curyTaxableDiscountAmt), _CuryTaxableDiscountAmt);
			SetValueOptional(cache, taxDetail, MultiCurrencyCalculator.RoundCury(cache, taxDetail, curyTaxDiscountAmt), _CuryTaxDiscountAmt);

			decimal roundedCuryTaxableAmt = MultiCurrencyCalculator.RoundCury(cache, taxDetail, curyTaxableAmt, Precision);

			bool isExemptTaxCategory = IsExemptTaxCategory(sender, row);
			if (isExemptTaxCategory)
			{
				SetTaxDetailExemptedAmount(cache, taxDetail, roundedCuryTaxableAmt);
			}
			else
			{
				SetTaxDetailTaxableAmount(cache, taxDetail, roundedCuryTaxableAmt);
			}

			decimal roundedCuryTaxAmt = MultiCurrencyCalculator.RoundCury(cache, taxDetail, curyTaxAmt, Precision);

			if (IsDeductibleVATTax(tax))
			{
				taxDetail.CuryExpenseAmt = MultiCurrencyCalculator.RoundCury(cache, taxDetail, curyTaxAmt * (1 - (taxRev.NonDeductibleTaxRate ?? 0m) / 100), Precision);
				curyTaxAmt = roundedCuryTaxAmt - (decimal)taxDetail.CuryExpenseAmt;

				decimal expenseAmt;
				MultiCurrencyCalculator.CuryConvBase(cache, taxDetail, taxDetail.CuryExpenseAmt.Value, out expenseAmt);
				taxDetail.ExpenseAmt = expenseAmt;
			}
			else
			{
				curyTaxAmt = roundedCuryTaxAmt;
			}

			if (!isExemptTaxCategory)
			{
				SetTaxDetailTaxAmount(cache, taxDetail, curyTaxAmt);
			}

			if (taxRev.TaxID != null && tax.DirectTax != true)
			{
				Update(cache, taxDetail);
				if (tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive)
				{
					sender.MarkUpdated(row);
				}
			}
			else if (_IncludeDirectTaxLine && taxRev.TaxID != null && tax.DirectTax == true && !SkipDirectTax(sender, row, taxRev.TaxID))
			{
				SetTaxDetailTaxableAmount(cache, taxDetail, 0.0m);
				SetTaxDetailTaxAmount(cache, taxDetail, curyTranAmt);
				Update(cache, taxDetail);
			}
			else
			{
				Delete(cache, taxDetail);
			}
		}

		protected virtual bool SkipDirectTax(PXCache sender, object row, string applicableDirectTaxId)
		{
			return false;
		}

		private (decimal InclTaxTaxable, decimal InclTaxAmount) CalculateInclusiveTaxAmounts(PXCache sender, object row, PXCache cache,
																							 TaxDetail nonPerUnitTaxDetail, List<object> inclusiveTaxes,
																							 in decimal calculatedTaxRate, in decimal curyTranAmt)
		{
			var (inclusivePerUnitTaxesIncludedInTaxOnTaxCalc,
				 inclusivePerUnitTaxesExcludedFromTaxOnTaxCalc, inclusiveNonPerUnitTaxes) = SegregateInclusiveTaxes(inclusiveTaxes);

			decimal curyInclusivePerUnitTaxAmountIncludedInTaxOnTaxCalc = 0m;
			decimal curyPerUnitTaxAmountExcludedFromTaxOnTaxCalc = 0m;
			decimal totalInclusiveNonPerUnitTaxRate = SumWithReverseAdjustment(sender.Graph, inclusiveNonPerUnitTaxes, typeof(TaxRev.taxRate)) / 100;
			Type taxDetailsCuryTaxAmountFieldType = cache.GetBqlField(_CuryTaxAmt);

			if (taxDetailsCuryTaxAmountFieldType != null)
			{
				curyInclusivePerUnitTaxAmountIncludedInTaxOnTaxCalc =
					SumWithReverseAdjustment(sender.Graph, inclusivePerUnitTaxesIncludedInTaxOnTaxCalc, taxDetailsCuryTaxAmountFieldType);

				curyPerUnitTaxAmountExcludedFromTaxOnTaxCalc =
					SumWithReverseAdjustment(sender.Graph, inclusivePerUnitTaxesExcludedFromTaxOnTaxCalc, taxDetailsCuryTaxAmountFieldType);
			}

			//The general formula for line Taxable Amount with the consideration for Per-Unit taxes:
			// Taxable = (LineAmount - PerUnitTaxesAmountExcluded from Tax On Tax Calculation) / (1 + Total Rate) - PerUnitTaxesAmountIncluded in Tax On Tax Calculation
			decimal curyRealTaxableAmt =
				((curyTranAmt - curyPerUnitTaxAmountExcludedFromTaxOnTaxCalc) / (1 + totalInclusiveNonPerUnitTaxRate)) - curyInclusivePerUnitTaxAmountIncludedInTaxOnTaxCalc;

			//Taxable for inclusive non per-unit taxes should be 
			decimal curyTaxableForNonPerUnitInclusiveTax = curyRealTaxableAmt + curyInclusivePerUnitTaxAmountIncludedInTaxOnTaxCalc;

			//Calculate tax amount for non per-unit inclusive tax by multiplying taxable on its tax rate
			decimal nonPerUnitTaxAmount = curyTaxableForNonPerUnitInclusiveTax * calculatedTaxRate;
			decimal curyNonPerUnitTaxAmount = MultiCurrencyCalculator.RoundCury(cache, nonPerUnitTaxDetail, nonPerUnitTaxAmount, Precision);

			//Recalculate total inclusive amount 
			decimal curyTotalInclusiveTaxAmt = curyInclusivePerUnitTaxAmountIncludedInTaxOnTaxCalc + curyPerUnitTaxAmountExcludedFromTaxOnTaxCalc;
			var taxRevCache = sender.Graph.Caches[typeof(TaxRev)];

			foreach (PXResult inclusiveNonPerUnitTaxRow in inclusiveNonPerUnitTaxes)
			{
				object inclusiveTaxRevision = inclusiveNonPerUnitTaxRow[typeof(TaxRev)];
				Tax currentInclusiveTax = (Tax)inclusiveNonPerUnitTaxRow[typeof(Tax)];
				decimal? taxRate = taxRevCache.GetValue<TaxRev.taxRate>(inclusiveTaxRevision) as decimal?;
				decimal multiplier = currentInclusiveTax.ReverseTax == true
					? Decimal.MinusOne
					: Decimal.One;

				decimal curyCurrentInclusiveTaxAmount = (curyTaxableForNonPerUnitInclusiveTax * taxRate / 100m) ?? 0m;
				curyCurrentInclusiveTaxAmount = MultiCurrencyCalculator.RoundCury(cache, nonPerUnitTaxDetail, curyCurrentInclusiveTaxAmount, Precision) * multiplier;

				curyTotalInclusiveTaxAmt += curyCurrentInclusiveTaxAmount;
			}

			curyRealTaxableAmt = curyTranAmt - curyTotalInclusiveTaxAmt;
			curyTaxableForNonPerUnitInclusiveTax = curyRealTaxableAmt + curyInclusivePerUnitTaxAmountIncludedInTaxOnTaxCalc;

			SetTaxableAmt(sender, row, curyRealTaxableAmt);
			SetTaxAmt(sender, row, curyTotalInclusiveTaxAmt);

			return (curyTaxableForNonPerUnitInclusiveTax, curyNonPerUnitTaxAmount);
		}

		private (List<object> PerUnitTaxesIncludedInTaxOnTaxCalc, List<object> PerUnitTaxesExcludedFromTaxOnTaxCalc, List<object> NonPerUnitTaxes) SegregateInclusiveTaxes(List<object> inclusiveTaxes)
		{
			List<object> perUnitTaxesIncludedInTaxOnTaxCalc = new List<object>();
			List<object> perUnitTaxesExcludedFromTaxOnTaxCalc = new List<object>();
			List<object> inclusiveNonPerUnitTaxes = new List<object>(capacity: inclusiveTaxes.Count);

			foreach (PXResult inclusiveTaxRow in inclusiveTaxes)
			{
				Tax inclusiveTax = inclusiveTaxRow.GetItem<Tax>();

				if (inclusiveTax == null)
					continue;

				if (!IsPerUnitTax(inclusiveTax))
				{
					inclusiveNonPerUnitTaxes.Add(inclusiveTaxRow);
				}
				else if (inclusiveTax.TaxCalcLevel2Exclude == true)
				{
					perUnitTaxesExcludedFromTaxOnTaxCalc.Add(inclusiveTaxRow);
				}
				else
				{
					perUnitTaxesIncludedInTaxOnTaxCalc.Add(inclusiveTaxRow);
				}
			}

			return (perUnitTaxesIncludedInTaxOnTaxCalc, perUnitTaxesExcludedFromTaxOnTaxCalc, inclusiveNonPerUnitTaxes);
		}

		private void ApplyDiscounts(Tax tax, PXCache sender, object row, decimal undiscountedPercent, decimal calculatedTaxRate,
									ref decimal curyTaxableAmt, ref decimal curyTaxableDiscountAmt, ref decimal curyTaxDiscountAmt, ref decimal curyTaxAmt)
		{
			if (ConsiderDiscount(tax))
			{
				curyTaxableAmt *= undiscountedPercent;
			}
			else if (ConsiderEarlyPaymentDiscountDetail(sender, row, tax))
			{
				curyTaxableDiscountAmt = curyTaxableAmt * (1m - undiscountedPercent);
				curyTaxableAmt *= undiscountedPercent;
				curyTaxDiscountAmt = curyTaxableDiscountAmt * calculatedTaxRate;
			}
			else if (ConsiderInclusiveDiscountDetail(sender, row, tax))
			{
				curyTaxableDiscountAmt = curyTaxableAmt * (1m - undiscountedPercent);
				curyTaxDiscountAmt = curyTaxableDiscountAmt * calculatedTaxRate;
				curyTaxableAmt *= undiscountedPercent;
				curyTaxAmt *= undiscountedPercent;
			}
		}

		protected virtual bool ConsiderDiscount(Tax tax)
		{
			return (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt
								|| tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt)
							&& tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxableAmount;
		}
		private bool ConsiderEarlyPaymentDiscountDetail(PXCache sender, object detail, Tax tax)
		{
			object parent = PXParentAttribute.SelectParent(sender, detail, _ParentType);
			return ConsiderEarlyPaymentDiscount(sender, parent, tax);
		}
		private bool ConsiderInclusiveDiscountDetail(PXCache sender, object detail, Tax tax)
		{
			object parent = PXParentAttribute.SelectParent(sender, detail, _ParentType);
			return ConsiderInclusiveDiscount(sender, parent, tax);
		}
		protected virtual bool ConsiderEarlyPaymentDiscount(PXCache sender, object parent, Tax tax)
		{
			return false;
		}
		protected virtual bool ConsiderInclusiveDiscount(PXCache sender, object parent, Tax tax)
		{
			return false;
		}

		protected virtual void SetTaxDetailTaxableAmount(PXCache cache, TaxDetail taxdet, decimal? curyTaxableAmt)
		{
			cache.SetValue(taxdet, _CuryTaxableAmt, curyTaxableAmt);
		}

		protected virtual void SetTaxDetailExemptedAmount(PXCache cache, TaxDetail taxdet, decimal? curyExemptedAmt)
		{
			if (!string.IsNullOrEmpty(_CuryExemptedAmt))
			{
				cache.SetValue(taxdet, _CuryExemptedAmt, curyExemptedAmt);
			}
		}

		protected virtual void SetTaxDetailTaxAmount(PXCache cache, TaxDetail taxdet, decimal? curyTaxAmt)
		{
			cache.SetValue(taxdet, _CuryTaxAmt, curyTaxAmt);
		}

		protected virtual void SetTaxDetailCuryExpenseAmt(PXCache cache, TaxDetail taxdet, decimal CuryExpenseAmt)
		{
			taxdet.CuryExpenseAmt = MultiCurrencyCalculator.RoundCury(cache, taxdet, CuryExpenseAmt, Precision);
		}

		#region CuryOrigDiscAmt TaxRecalculation
		[Obsolete("This method is obsolete and will be removed in future versions of Acumatica. Use PX.Objects.Common.SquareEquationSolver instead")]
		public static Pair<double, double> SolveQuadraticEquation(double a, double b, double c)
		{
			var roots = Common.SquareEquationSolver.SolveQuadraticEquation(a, b, c);
			return roots.HasValue
				? new Pair<double, double>(roots.Value.X1, roots.Value.X2)
				: null;
		}

		private Dictionary<object, bool> OrigDiscAmtExtCallDict = new Dictionary<object, bool>();
		private Dictionary<object, decimal?> DiscPercentsDict = new Dictionary<object, decimal?>();

		protected virtual void CuryOrigDiscAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row == null)
				return;

			OrigDiscAmtExtCallDict[e.Row] = e.ExternalCall;
		}

		protected virtual bool ShouldUpdateFinPeriodID(PXCache sender, object oldRow, object newRow)
		{
			return (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
				   && (string)sender.GetValue(oldRow, _FinPeriodID) != (string)sender.GetValue(newRow, _FinPeriodID);
		}

		protected virtual void ParentRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (e.Row == null)
				return;

			int? oldBranchID = null;
			int? newBranchID = null;

			if (ParentBranchIDField != null)
			{
				oldBranchID = (int?)sender.GetValue(e.OldRow, ParentBranchIDField.Name);
				newBranchID = (int?)sender.GetValue(e.Row, ParentBranchIDField.Name);
			}

			if (oldBranchID != newBranchID
				|| ShouldUpdateFinPeriodID(sender, e.OldRow, e.Row))
			{
				PXCache cache = sender.Graph.Caches[_TaxSumType];
				List<object> details = TaxParentAttribute.ChildSelect(cache, e.Row, _ParentType);
				foreach (object det in details)
				{
					if (oldBranchID != newBranchID)
					{
						cache.SetDefaultExt(det, ChildBranchIDField.Name);
					}

					if (ShouldUpdateFinPeriodID(sender, e.OldRow, e.Row))
					{
						cache.SetDefaultExt(det, ChildFinPeriodIDField.Name);
					}

					cache.MarkUpdated(det);
				}
			}

			bool externallCall = false;
			OrigDiscAmtExtCallDict.TryGetValue(e.Row, out externallCall);
			if (!externallCall)
				return;

			decimal newDiscAmt = ((decimal?)sender.GetValue(e.Row, _CuryOrigDiscAmt)).GetValueOrDefault();
			decimal oldDiscAmt = ((decimal?)sender.GetValue(e.OldRow, _CuryOrigDiscAmt)).GetValueOrDefault();

			if (newDiscAmt != oldDiscAmt && !DiscPercentsDict.ContainsKey(e.Row))
			{
				DiscPercentsDict.Add(e.Row, 0m);
				PXFieldUpdatedEventArgs args = new PXFieldUpdatedEventArgs(e.Row, oldDiscAmt, false);

				using (new TermsAttribute.UnsubscribeCalcDiscScope(sender))
				{
					try
					{
						if (newDiscAmt == 0m)
							return;

						ParentFieldUpdated(sender, args);
						DiscPercentsDict[e.Row] = null;

						bool considerEarlyPaymentDiscount = false;
						decimal reducedTaxAmt = 0m;
						PXCache cache = sender.Graph.Caches[_TaxSumType];

						foreach (object taxitem in SelectTaxes(sender, e.Row, PXTaxCheck.RecalcTotals))
						{
							object taxsum = ((PXResult)taxitem)[0];
							Tax tax = PXResult.Unwrap<Tax>(taxitem);

							if (RecalcTaxableRequired(tax))
							{
								reducedTaxAmt += (tax.ReverseTax == true ? -1m : 1m) * (((decimal?)cache.GetValue(taxsum, _CuryTaxAmt)).GetValueOrDefault() +
									(IsDeductibleVATTax(tax) ? ((decimal?)cache.GetValue(taxsum, _CuryExpenseAmt)).GetValueOrDefault() : 0m));
							}
							else if (ConsiderEarlyPaymentDiscount(sender, e.Row, tax) || ConsiderInclusiveDiscount(sender, e.Row, tax))
							{
								considerEarlyPaymentDiscount = true;
								break; //as combination of reduce taxable and reduce taxable on early payment is forbidden, we can skip further calculation
							}
						}
						if (considerEarlyPaymentDiscount)
						{
							decimal curyDocBal = ((decimal?)sender.GetValue(e.Row, _CuryDocBal)).GetValueOrDefault();
							DiscPercentsDict[e.Row] = 100 * newDiscAmt / curyDocBal;
						}
						else if (reducedTaxAmt != 0m)
						{
							decimal curyDocBal = ((decimal?)sender.GetValue(e.Row, _CuryDocBal)).GetValueOrDefault();
							DiscPercentsDict[e.Row] = CalculateCashDiscountPercent(curyDocBal, reducedTaxAmt, newDiscAmt);
						}
					}
					catch
					{
						DiscPercentsDict[e.Row] = null;
					}
					finally
					{
						ParentFieldUpdated(sender, args);
						sender.RaiseRowUpdated(e.Row, e.OldRow);

						OrigDiscAmtExtCallDict.Remove(e.Row);
						DiscPercentsDict.Remove(e.Row);
					}
				}
			}
		}

		private decimal? CalculateCashDiscountPercent(decimal curyDocBalanceOld, decimal reducableTaxAmountOld, decimal newCashDiscountAmount)
		{
			var roots = Common.SquareEquationSolver.SolveQuadraticEquation(reducableTaxAmountOld, -curyDocBalanceOld, newCashDiscountAmount);

			if (roots == null)
				return null;

			var (x1, x2) = roots.Value;
			return x1 >= 0 && x1 <= 1
					? x1 * 100
					: x2 >= 0 && x2 <= 1
						? x2 * 100
						: (decimal?)null;
		}

		protected virtual bool RecalcTaxableRequired(Tax tax)
		{
			return tax?.TaxCalcLevel != CSTaxCalcLevel.Inclusive &&
											tax?.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxableAmount;
		}

		#endregion

		protected virtual void AdjustTaxableAmount(PXCache cache, object row, List<object> taxitems, ref decimal CuryTaxableAmt, string TaxCalcType)
		{
		}

		protected virtual void AdjustExemptedAmount(PXCache cache, object row, List<object> taxitems, ref decimal CuryExemptedAmt, string TaxCalcType)
		{
		}

		protected virtual TaxDetail CalculateTaxSum(PXCache sender, object taxrow, object row)
		{
			if (taxrow == null)
			{
				throw new PXArgumentException("taxrow", ErrorMessages.ArgumentNullException);
			}

			PXCache cache = sender.Graph.Caches[_TaxType];
			PXCache sumcache = sender.Graph.Caches[_TaxSumType];

			TaxDetail taxdet = (TaxDetail)((PXResult)taxrow)[0];
			Tax tax = PXResult.Unwrap<Tax>(taxrow);
			TaxRev taxrev = PXResult.Unwrap<TaxRev>(taxrow);

			if (taxrev.TaxID == null)
			{
				taxrev.TaxableMin = 0m;
				taxrev.TaxableMax = 0m;
				taxrev.TaxRate = 0m;
			}

			decimal curyOrigTaxableAmt = 0m;
			decimal CuryTaxAmtSumm = 0m;
			decimal CuryTaxableAmt = 0.0m;
			decimal CuryTaxableDiscountAmt = 0.0m;
			decimal TaxableAmt = 0.0m;
			decimal CuryTaxAmt = 0.0m;
			decimal CuryTaxDiscountAmt = 0.0m;
			decimal CuryExpenseAmt = 0.0m;
			decimal CuryExemptedAmt = 0.0m;

			List<object> taxitems = SelectTaxesToCalculateTaxSum(sender, row, taxdet);

			if (taxitems.Count == 0 || taxrev.TaxID == null)
			{
				return null;
			}

			if (tax.DirectTax == true && _IncludeDirectTaxLine)
			{
				taxdet.TaxRate = taxrev.TaxRate;
				taxdet.NonDeductibleTaxRate = taxrev.NonDeductibleTaxRate;
				CuryTaxAmtSumm = Sum(sender.Graph, taxitems, GetFieldType(cache, _CuryTaxAmt));
				sumcache.SetValue(taxdet, _CuryTaxableAmt, MultiCurrencyCalculator.RoundCury(sumcache, taxdet, 0.0m, Precision));
				sumcache.SetValue(taxdet, _CuryTaxAmt, MultiCurrencyCalculator.RoundCury(sumcache, taxdet, CuryTaxAmtSumm, Precision));
				sumcache.SetValue(taxdet, _CuryTaxAmtSumm, MultiCurrencyCalculator.RoundCury(sumcache, taxdet, CuryTaxAmtSumm, Precision));
				return taxdet;
			}

			if (tax.TaxCalcType == CSTaxCalcType.Item)
			{
				if (cache.Fields.Contains(_CuryOrigTaxableAmt))
				{
					curyOrigTaxableAmt = Sum(sender.Graph,
						taxitems,
						GetFieldType(cache, _CuryOrigTaxableAmt));
				}

				CuryTaxableAmt = Sum(sender.Graph,
					taxitems,
					GetFieldType(cache, _CuryTaxableAmt));

				Type curyTaxableDiscountAmtField = GetFieldType(cache, _CuryTaxableDiscountAmt);
				if (curyTaxableDiscountAmtField != null)
				{
					CuryTaxableDiscountAmt = Sum(sender.Graph,
					taxitems,
					curyTaxableDiscountAmtField);
				}

				AdjustTaxableAmount(sender, row, taxitems, ref CuryTaxableAmt, tax.TaxCalcType);

				if (tax.TaxType == CSTaxType.PerUnit || tax.ZeroTaxable == true)
				{
					CuryTaxAmt = CuryTaxAmtSumm = Sum(sender.Graph,
						taxitems,
						GetFieldType(cache, _CuryTaxAmt));
				}
				else
				{
					CuryTaxAmt = CuryTaxAmtSumm = (CuryTaxableAmt == 0m) ? 0m : Sum(sender.Graph,
						taxitems,
						GetFieldType(cache, _CuryTaxAmt));
				}

				Type curyTaxDiscountAmtField = GetFieldType(cache, _CuryTaxDiscountAmt);
				if (curyTaxDiscountAmtField != null)
				{
					CuryTaxDiscountAmt = Sum(sender.Graph, taxitems, curyTaxDiscountAmtField);
				}

				CuryExpenseAmt = Sum(sender.Graph,
					taxitems,
					GetFieldType(cache, _CuryExpenseAmt));
			}
			else if (
				tax.TaxType != CSTaxType.Withholding && (
				CalcGrossOnDocumentLevel && _isTaxCalcModeEnabled && GetTaxCalcMode(sender.Graph) == TaxCalculationMode.Gross ||
				tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive && (!_isTaxCalcModeEnabled || GetTaxCalcMode(sender.Graph) != TaxCalculationMode.Net)))
			{
				CuryTaxableAmt = Sum(sender.Graph, taxitems, GetFieldType(cache, _CuryTaxableAmt));
				CuryTaxAmt = CuryTaxAmtSumm = Sum(sender.Graph, taxitems, GetFieldType(cache, _CuryTaxAmt));

				Type curyTaxableDiscountAmtField = GetFieldType(cache, _CuryTaxableDiscountAmt);
				if (curyTaxableDiscountAmtField != null)
				{
					CuryTaxableDiscountAmt = Sum(sender.Graph,
					taxitems,
					curyTaxableDiscountAmtField);
				}

				Type curyTaxDiscountAmtField = GetFieldType(cache, _CuryTaxDiscountAmt);
				if (curyTaxDiscountAmtField != null)
				{
					CuryTaxDiscountAmt = Sum(sender.Graph, taxitems, curyTaxDiscountAmtField);
				}

				var docLines = SelectDocumentLines(sender.Graph, row);
				if (docLines.Any())
				{
					var docLineCache = sender.Graph.Caches[docLines[0].GetType()];
					var realLineAmounts = docLines.ToDictionary(
						_ => (int)docLineCache.GetValue(_, _LineNbr),
						_ => GetDocLineFinalAmtNoRounding(docLineCache, _, tax.TaxCalcType) ?? 0.0m);

					List<object> alltaxitems = SelectTaxes(sender, row, PXTaxCheck.RecalcLine);
					List<object> inclusiveTaxes = alltaxitems.Where(_ => PXResult.Unwrap<Tax>(_).TaxCalcLevel == CSTaxCalcLevel.Inclusive).ToList();
					var taxLines = inclusiveTaxes.Select(_ => new
					{
						LineNbr = (int)cache.GetValue((TaxDetail)((PXResult)_)[0], _LineNbr),
						TaxID = PXResult.Unwrap<Tax>(_).TaxID,
						TaxRate = PXResult.Unwrap<TaxRev>(_).TaxRate ?? 0,
						TaxRateMultiplier = PXResult.Unwrap<Tax>(_).ReverseTax == true ? -1.0M : 1.0M,
						CuryTaxableAmt = (decimal?)cache.GetValue((TaxDetail)((PXResult)_)[0], _CuryTaxableAmt),
						CuryTaxAmt = (decimal?)cache.GetValue((TaxDetail)((PXResult)_)[0], _CuryTaxAmt)
					});

					var currentTaxRate = ((taxdet.TaxRate != 0.0m) ? taxdet.TaxRate : taxLines.FirstOrDefault(_ => _.TaxID == taxdet.TaxID)?.TaxRate) ?? (decimal?)0.0m;
					var currentTaxLines = taxLines.Where(_ => _.TaxID == taxdet.TaxID).Select(_ => _.LineNbr).ToList();

					var groups = new List<InclusiveTaxGroup>();
					decimal notlLineCuryTaxAmt = 0M;
					decimal notLineCuryTaxableAmt = 0M;
					foreach (var lineNbr in currentTaxLines)
					{
						var lineTaxes = taxLines.Where(_ => _.LineNbr == lineNbr).OrderBy(_ => _.TaxID).ToList();
						var groupKey = string.Join("::", lineTaxes.Select(_ => _.TaxID));
						var sumTaxRate = lineTaxes.Sum(_ => _.TaxRate * _.TaxRateMultiplier);
						decimal lineAmt = 0;
						if (!realLineAmounts.TryGetValue(lineNbr, out lineAmt))
						{
							notlLineCuryTaxAmt += lineTaxes.Sum(_ => _.CuryTaxAmt) ?? 0M;
							notLineCuryTaxableAmt += lineTaxes.Sum(_ => _.CuryTaxableAmt) ?? 0M;
						}
						if (groups.Any(g => g.Key == groupKey))
						{
							groups.Single(g => g.Key == groupKey).TotalAmount += lineAmt;
						}
						else
						{
							groups.Add(new InclusiveTaxGroup() { Key = groupKey, Rate = sumTaxRate, TotalAmount = lineAmt });
						}
					}

					CuryTaxAmt = groups.Sum(g => MultiCurrencyCalculator.RoundCury(sender, taxdet,
						(g.TotalAmount / (1 + g.Rate / 100.0m) * currentTaxRate / 100.0m) ?? 0.0m, Precision))
						- CuryTaxDiscountAmt + notlLineCuryTaxAmt;

					CuryTaxableAmt = MultiCurrencyCalculator.RoundCury(sender, taxdet,
						groups.Sum(g => g.TotalAmount / (1 + g.Rate / 100.0m)), Precision)
						- CuryTaxableDiscountAmt + notLineCuryTaxableAmt;
				}

				if (tax.DeductibleVAT == true)
					CuryExpenseAmt = CuryTaxAmt * (1.0M - (taxrev.NonDeductibleTaxRate ?? 0.0M) / 100.0M);
			}
			else
			{
				List<object> lvl1Taxes = SelectLvl1Taxes(sender.Graph, row);

				if (_NoSumTaxable && (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt || lvl1Taxes.Count == 0))
				{
					// When changing doc date will 
					// not recalculate taxable amount
					//
					CuryTaxableAmt = (decimal)sumcache.GetValue(taxdet, _CuryTaxableAmt);
					CuryTaxableDiscountAmt = GetOptionalDecimalValue(sumcache, taxdet, _CuryTaxableDiscountAmt);
				}
				else
				{
					CuryTaxableAmt = Sum(sender.Graph,
						taxitems,
						GetFieldType(cache, _CuryTaxableAmt));

					CuryTaxAmtSumm = Sum(sender.Graph, taxitems, GetFieldType(cache, _CuryTaxAmt));
					CuryTaxAmtSumm = MultiCurrencyCalculator.RoundCury(sender, taxdet, CuryTaxAmtSumm, Precision);

					Type curyTaxableDiscountAmtField = GetFieldType(cache, _CuryTaxableDiscountAmt);
					if (curyTaxableDiscountAmtField != null)
					{
						CuryTaxableDiscountAmt = Sum(sender.Graph, taxitems, curyTaxableDiscountAmtField);
					}

					AdjustTaxableAmount(sender, row, taxitems, ref CuryTaxableAmt, tax.TaxCalcType);
				}

				curyOrigTaxableAmt = MultiCurrencyCalculator.RoundCury(sumcache, taxdet, CuryTaxableAmt, Precision);

				AdjustMinMaxTaxableAmt(sumcache, taxdet, taxrev, ref CuryTaxableAmt, ref TaxableAmt);

				CuryTaxAmt = CuryTaxableAmt * (decimal)taxrev.TaxRate / 100;
				CuryTaxAmt = MultiCurrencyCalculator.RoundCury(sumcache, taxdet, CuryTaxAmt, Precision);

				CuryTaxDiscountAmt = CuryTaxableDiscountAmt * (decimal)taxrev.TaxRate / 100;

				AdjustExpenseAmt(tax, taxrev, CuryTaxAmt, ref CuryExpenseAmt);
				AdjustTaxAmtOnDiscount(sender, tax, ref CuryTaxAmt);
			}

			taxdet = (TaxDetail)sumcache.CreateCopy(taxdet);

			if (sumcache.Fields.Contains(_CuryOrigTaxableAmt))
			{
				sumcache.SetValue(taxdet, _CuryOrigTaxableAmt, curyOrigTaxableAmt);
			}

			CuryExemptedAmt = Sum(sender.Graph,
				taxitems,
				GetFieldType(cache, _CuryExemptedAmt));

			AdjustExemptedAmount(sender, row, taxitems, ref CuryExemptedAmt, tax.TaxCalcType);

			taxdet.TaxRate = taxrev.TaxRate;
			taxdet.NonDeductibleTaxRate = taxrev.NonDeductibleTaxRate;
			sumcache.SetValue(taxdet, _CuryTaxableAmt, MultiCurrencyCalculator.RoundCury(sumcache, taxdet, CuryTaxableAmt, Precision));
			sumcache.SetValue(taxdet, _CuryExemptedAmt, MultiCurrencyCalculator.RoundCury(sumcache, taxdet, CuryExemptedAmt, Precision));
			sumcache.SetValue(taxdet, _CuryTaxAmt, MultiCurrencyCalculator.RoundCury(sumcache, taxdet, CuryTaxAmt, Precision));
			sumcache.SetValue(taxdet, _CuryTaxAmtSumm, MultiCurrencyCalculator.RoundCury(sumcache, taxdet, CuryTaxAmtSumm, Precision));
			SetValueOptional(sumcache, taxdet, MultiCurrencyCalculator.RoundCury(sumcache, taxdet, CuryTaxableDiscountAmt), _CuryTaxableDiscountAmt);
			SetTaxDetailCuryExpenseAmt(sumcache, taxdet, CuryExpenseAmt);
			SetValueOptional(sumcache, taxdet, MultiCurrencyCalculator.RoundCury(sumcache, taxdet, CuryTaxDiscountAmt), _CuryTaxDiscountAmt);

			if (IsDeductibleVATTax(tax) && tax.TaxCalcType != CSTaxCalcType.Item)
			{
				sumcache.SetValue(taxdet, _CuryTaxAmt,
					(decimal)(sumcache.GetValue(taxdet, _CuryTaxAmt) ?? 0m) -
					(decimal)(sumcache.GetValue(taxdet, _CuryExpenseAmt) ?? 0m));
			}

			if (IsPerUnitTax(tax))
			{
				taxdet = FillAggregatedTaxDetailForPerUnitTax(sender, row, tax, taxrev, taxdet, taxitems);
			}

			return taxdet;
		}

		protected virtual List<object> SelectTaxesToCalculateTaxSum(PXCache sender, object row, TaxDetail taxdet)
		{
			return SelectTaxes<Where<Tax.taxID, Equal<Required<Tax.taxID>>>>(sender.Graph, row, PXTaxCheck.RecalcLine, taxdet.TaxID);
		}

		protected class InclusiveTaxGroup
		{
			public string Key { get; set; }
			public decimal Rate { get; set; }
			public decimal TotalAmount { get; set; }
		}


		protected virtual void CalculateTaxSumTaxAmt(
			PXCache sender,
			TaxDetail taxdet,
			Tax tax,
			TaxRev taxrev)
		{
			if (tax.TaxType == CSTaxType.PerUnit)
			{
				PXTrace.WriteError(Messages.PerUnitTaxesNotSupportedOperation);
				throw new PXException(Messages.PerUnitTaxesNotSupportedOperation);
			}

			PXCache sumcache = sender.Graph.Caches[_TaxSumType];

			decimal taxableAmt = 0.0m;
			decimal curyExpenseAmt = 0.0m;

			decimal curyTaxableAmt = GetOptionalDecimalValue(sender, taxdet, _CuryTaxableAmt);
			decimal curyTaxableDiscountAmt = GetOptionalDecimalValue(sender, taxdet, _CuryTaxableDiscountAmt);
			decimal curyOrigTaxableAmt = MultiCurrencyCalculator.RoundCury(sumcache, taxdet, curyTaxableAmt, Precision);

			decimal taxRate = taxrev.TaxRate ?? 0.0m;

			AdjustMinMaxTaxableAmt(sender, taxdet, taxrev, ref curyTaxableAmt, ref taxableAmt);

			decimal curyTaxAmt = curyTaxableAmt * taxRate / 100;
			decimal curyTaxDiscountAmt = curyTaxableDiscountAmt * taxRate / 100;

			AdjustExpenseAmt(tax, taxrev, curyTaxAmt, ref curyExpenseAmt);
			AdjustTaxAmtOnDiscount(sender, tax, ref curyTaxAmt);

			if (sumcache.Fields.Contains(_CuryOrigTaxableAmt))
			{
				sumcache.SetValue(taxdet, _CuryOrigTaxableAmt, curyOrigTaxableAmt);
			}

			taxdet.TaxRate = taxRate;
			taxdet.NonDeductibleTaxRate = taxrev.NonDeductibleTaxRate ?? 0.0m;
			sumcache.SetValue(taxdet, _CuryTaxableAmt, MultiCurrencyCalculator.RoundCury(sumcache, taxdet, curyTaxableAmt, Precision));
			sumcache.SetValue(taxdet, _CuryTaxAmt, MultiCurrencyCalculator.RoundCury(sumcache, taxdet, curyTaxAmt, Precision));
			SetValueOptional(sumcache, taxdet, MultiCurrencyCalculator.RoundCury(sumcache, taxdet, curyTaxableDiscountAmt), _CuryTaxableDiscountAmt);
			SetTaxDetailCuryExpenseAmt(sumcache, taxdet, curyExpenseAmt);
			SetValueOptional(sumcache, taxdet, MultiCurrencyCalculator.RoundCury(sumcache, taxdet, curyTaxDiscountAmt), _CuryTaxDiscountAmt);

			if (IsDeductibleVATTax(tax) && tax.TaxCalcType != CSTaxCalcType.Item)
			{
				sumcache.SetValue(taxdet, _CuryTaxAmt,
					(decimal)(sumcache.GetValue(taxdet, _CuryTaxAmt) ?? 0m) -
					(decimal)(sumcache.GetValue(taxdet, _CuryExpenseAmt) ?? 0m));
			}
		}

		private void AdjustExpenseAmt(
			Tax tax,
			TaxRev taxrev,
			decimal curyTaxAmt,
			ref decimal curyExpenseAmt)
		{
			if (IsDeductibleVATTax(tax))
			{
				curyExpenseAmt = curyTaxAmt * (1 - (taxrev.NonDeductibleTaxRate ?? 0m) / 100);
			}
		}

		private void AdjustTaxAmtOnDiscount(
			PXCache sender,
			Tax tax,
			ref decimal curyTaxAmt)
		{
			if ((tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt || tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt) &&
				tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxAmount)
			{
				decimal? DiscPercent = null;
				DiscPercentsDict.TryGetValue(ParentRow(sender.Graph), out DiscPercent);

				Terms terms = SelectTerms(sender.Graph);

				curyTaxAmt = curyTaxAmt * (1 - (DiscPercent ?? terms.DiscPercent ?? 0m) / 100);
			}
		}

		private void AdjustMinMaxTaxableAmt(
			PXCache sumcache,
			TaxDetail taxdet,
			TaxRev taxrev,
			ref decimal curyTaxableAmt,
			ref decimal taxableAmt)
		{
			try
			{
			MultiCurrencyCalculator.CuryConvBase(sumcache, taxdet, curyTaxableAmt, out taxableAmt);
			}
			catch (Exception){ }

			if (taxrev.TaxableMin != 0.0m)
			{
				if (taxableAmt < taxrev.TaxableMin)
				{
					curyTaxableAmt = 0.0m;
					taxableAmt = 0.0m;
				}
			}

			if (taxrev.TaxableMax != 0.0m)
			{
				if (taxableAmt > taxrev.TaxableMax)
				{
					MultiCurrencyCalculator.CuryConvCury(sumcache, taxdet, (decimal)taxrev.TaxableMax, out curyTaxableAmt);
					taxableAmt = (decimal)taxrev.TaxableMax;
				}
			}
		}

		private static void SetValueOptional(PXCache cache, object data, object value, string field)
		{
			int ordinal = cache.GetFieldOrdinal(field);
			if (ordinal >= 0)
			{
				cache.SetValue(data, ordinal, value);
			}
		}

		private TaxDetail TaxSummarize(PXCache sender, object taxrow, object row)
		{
			if (taxrow == null)
			{
				throw new PXArgumentException("taxrow", ErrorMessages.ArgumentNullException);
			}

			PXCache sumcache = sender.Graph.Caches[_TaxSumType];
            TaxDetail taxSum = CalculateTaxSum(sender, taxrow, row);

			if (taxSum != null)
			{
				return (TaxDetail)sumcache.Update(taxSum);
			}
			else
			{
				if (row != null && !IsTaxCalculationNeeded(sender, row))
					return null;

				TaxDetail taxdet = (TaxDetail)((PXResult)taxrow)[0];
				Delete(sumcache, taxdet);
				return null;
			}
		}

		protected virtual void CalcTaxes(PXCache sender, object row)
		{
			CalcTaxes(sender, row, PXTaxCheck.RecalcLine);
		}

		/// <summary>
		/// This method is intended to select document line for given tax row.
		/// Do not use it to select parent document foir given line.
		/// </summary>
		/// <param name="cache">Cache of the tax row.</param>
		/// <param name="row">Tax row for which line will be returned.</param>
		/// <returns>Document line object.</returns>
		protected virtual object SelectParent(PXCache cache, object row)
		{
			return PXParentAttribute.SelectParent(cache, row, _ChildType);
		}

		protected virtual void CalcTaxes(PXCache sender, object row, PXTaxCheck taxchk)
		{
			CalcTaxes(sender, row, taxchk, true);
		}

		protected virtual void CalcTaxes(PXCache sender, object row, PXTaxCheck taxchk, bool calcTaxes)
		{
			PXCache cache = sender.Graph.Caches[_TaxType];

			object detrow = row;

			foreach (object taxrow in SelectTaxes(sender, row, taxchk))
			{
				if (row == null)
				{
					detrow = SelectParent(cache, ((PXResult)taxrow)[0]);
				}

				if (detrow != null)
				{
					TaxSetLineDefault(sender, taxrow, detrow);
				}
			}
			CalcTotals(sender, row, calcTaxes && IsTaxCalculationNeeded(sender, row));
		}

		/// <summary>
		/// This method can be overridden to disable allow to disable automatic tax recalculation
		/// </summary>
		public virtual bool IsTaxCalculationNeeded(PXCache sender, object row)
		{
			return true;
		}

		public virtual IEnumerable<T> DistributeTaxDiscrepancy<T, CuryTaxField, BaseTaxField>(PXGraph graph, IEnumerable<T> taxDetList, decimal CuryTaxAmt, bool updateCache)
			where T : TaxDetail, CM.ITranTax
			where CuryTaxField : IBqlField
			where BaseTaxField : IBqlField
		{
			decimal curyTaxSum = 0m;
			decimal curyTaxableSum = 0m;

			T maxDetail = null;
			PXCache taxDetCache = graph.Caches[_TaxType];

			foreach (var taxLine in taxDetList)
			{
				decimal curyTaxAmt = (decimal)taxDetCache.GetValue<CuryTaxField>(taxLine);
				decimal curyTaxableAmt = (decimal)(taxDetCache.GetValue(taxLine, _CuryTaxableAmt) ?? 0m);

				curyTaxSum += curyTaxAmt;
				curyTaxableSum += curyTaxableAmt;

				if (maxDetail == null)
				{
					maxDetail = taxLine;
				}
				else
				{
					decimal curyTaxableAmtMax = (decimal)(taxDetCache.GetValue(maxDetail, _CuryTaxableAmt) ?? 0m);
					if (Math.Abs(curyTaxableAmtMax) < Math.Abs(curyTaxableAmt))
					{
						maxDetail = taxLine;
					}
				}
			}

			decimal discrepancy = CuryTaxAmt - curyTaxSum;
			if (Math.Abs(discrepancy) > 0m)
			{
				decimal discrSum = 0m;
				foreach (T taxLine in taxDetList)
				{
					decimal partDiscr = MultiCurrencyCalculator.RoundCury(taxDetCache, taxLine,
						discrepancy * (curyTaxableSum != 0 ? (decimal)(taxDetCache.GetValue(taxLine, _CuryTaxableAmt) ?? 0m) / curyTaxableSum : (1m / taxDetList.Count())));
					decimal curyTaxAmt = (decimal)taxDetCache.GetValue<CuryTaxField>(taxLine) + partDiscr;
					taxDetCache.SetValue<CuryTaxField>(taxLine, curyTaxAmt);
					discrSum += partDiscr;
					decimal taxAmt;
					MultiCurrencyCalculator.CuryConvBase(taxDetCache, taxLine, curyTaxAmt, out taxAmt);
					taxDetCache.SetValue<BaseTaxField>(taxLine, taxAmt);

					if (updateCache)
					{
						Update(taxDetCache, taxLine);
					}
				}

				if (discrSum != discrepancy && maxDetail != null)
				{
					decimal curyTaxAmt = (decimal)taxDetCache.GetValue<CuryTaxField>(maxDetail) + discrepancy - discrSum;
					taxDetCache.SetValue<CuryTaxField>(maxDetail, curyTaxAmt);
					decimal taxAmt;
					MultiCurrencyCalculator.CuryConvBase(taxDetCache, maxDetail, curyTaxAmt, out taxAmt);
					taxDetCache.SetValue<BaseTaxField>(maxDetail, taxAmt);

					if (updateCache)
					{
						Update(taxDetCache, maxDetail);
					}
				}
			}

			return taxDetList;
		}

		protected virtual void CalcDocTotals(
			PXCache sender,
			object row,
			decimal CuryTaxTotal,
			decimal CuryInclTaxTotal,
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			_CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);
		}

		protected virtual decimal CalcLineTotal(PXCache sender, object row)
		{
			decimal CuryLineTotal = 0m;

			object[] details = PXParentAttribute.SelectSiblings(sender, null);

			if (details != null)
			{
				foreach (object detrow in details)
				{
					CuryLineTotal += GetCuryTranAmt(sender, sender.ObjectsEqual(detrow, row) ? row : detrow) ?? 0m;
				}
			}
			return CuryLineTotal;
		}

		protected virtual void _CalcDocTotals(
			PXCache sender,
			object row,
			decimal CuryTaxTotal,
			decimal CuryInclTaxTotal,
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			decimal CuryLineTotal = CalcLineTotal(sender, row);

			decimal CuryDocTotal = CuryLineTotal + CuryTaxTotal - CuryInclTaxTotal;

			decimal doc_CuryLineTotal = (decimal)(ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m);
			decimal doc_CuryTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryTaxTotal) ?? 0m);

			if (!Equals(CuryLineTotal, doc_CuryLineTotal) ||
				!Equals(CuryTaxTotal, doc_CuryTaxTotal))
			{
				ParentSetValue(sender.Graph, _CuryLineTotal, CuryLineTotal);
				ParentSetValue(sender.Graph, _CuryTaxTotal, CuryTaxTotal);

				if (!string.IsNullOrEmpty(_CuryTaxInclTotal))
				{
					ParentSetValue(sender.Graph, _CuryTaxInclTotal, CuryInclTaxTotal);
				}

				if (!string.IsNullOrEmpty(_CuryDocBal))
				{
					ParentSetValue(sender.Graph, _CuryDocBal, CuryDocTotal);
					return;
				}
			}

			if (!string.IsNullOrEmpty(_CuryDocBal))
			{
				decimal doc_CuryDocBal = (decimal)(ParentGetValue(sender.Graph, _CuryDocBal) ?? 0m);

				if (!Equals(CuryDocTotal, doc_CuryDocBal))
				{
					ParentSetValue(sender.Graph, _CuryDocBal, CuryDocTotal);
				}
			}
		}

		protected virtual TaxDetail GetTaxDetail(PXCache sender, object taxrow, object row, out bool NeedUpdate)
		{
			if (taxrow == null)
			{
				throw new PXArgumentException("taxrow", ErrorMessages.ArgumentNullException);
			}

			NeedUpdate = false;

			return (TaxDetail)((PXResult)taxrow)[0];
		}

		protected virtual void CalcTotals(PXCache sender, object row, bool CalcTaxes)
		{
			bool IsUseTax = false;

			decimal CuryTaxTotal = 0m;
			decimal CuryTaxDiscountTotal = 0m;
			decimal CuryInclTaxTotal = 0m;
			decimal CuryInclTaxDiscountTotal = 0m;
			decimal CuryWhTaxTotal = 0m;

			foreach (object taxrow in SelectTaxes(sender, row, PXTaxCheck.RecalcTotals))
			{
				TaxDetail taxdet = null;
				if (CalcTaxes)
				{
					taxdet = TaxSummarize(sender, taxrow, row);
				}
				else
				{
					taxdet = GetTaxDetail(sender, taxrow, row, out bool needUpdate);
					if (needUpdate)
					{
						taxdet = (TaxDetail)sender.Graph.Caches[_TaxSumType].Update(taxdet);
					}
				}

				if (taxdet != null && PXResult.Unwrap<Tax>(taxrow).TaxType == CSTaxType.Use)
				{
					IsUseTax = true;
				}
				else if (taxdet != null)
				{
					PXCache taxDetCache = sender.Graph.Caches[taxdet.GetType()];
					decimal CuryTaxAmt = (decimal)taxDetCache.GetValue(taxdet, _CuryTaxAmt);
					decimal CuryTaxDiscountAmt = GetOptionalDecimalValue(taxDetCache, taxdet, _CuryTaxDiscountAmt);

					//assuming that tax cannot be withholding and reverse at the same time
					Decimal multiplier = PXResult.Unwrap<Tax>(taxrow).ReverseTax == true ? Decimal.MinusOne : Decimal.One;

					if (PXResult.Unwrap<Tax>(taxrow).TaxType == CSTaxType.Withholding)
					{
						CuryWhTaxTotal += multiplier * CuryTaxAmt;
					}


					if (PXResult.Unwrap<Tax>(taxrow).TaxCalcLevel == "0")
					{
						CuryInclTaxTotal += multiplier * CuryTaxAmt;
						CuryInclTaxDiscountTotal += multiplier * CuryTaxDiscountAmt;
					}

					CuryTaxTotal += multiplier * CuryTaxAmt;
					CuryTaxDiscountTotal += multiplier * CuryTaxDiscountAmt;

					if (IsDeductibleVATTax(PXResult.Unwrap<Tax>(taxrow)))
					{
						CuryTaxTotal += multiplier * (decimal)taxdet.CuryExpenseAmt;

						if (PXResult.Unwrap<Tax>(taxrow).TaxCalcLevel == "0")
						{
							CuryInclTaxTotal += multiplier * (decimal)taxdet.CuryExpenseAmt;
						}
					}
				}
			}

			if (ParentGetStatus(sender.Graph) != PXEntryStatus.Deleted && ParentGetStatus(sender.Graph) != PXEntryStatus.InsertedDeleted)
			{
				CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal + CuryInclTaxDiscountTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);
			}

			if (IsUseTax && !sender.Graph.UnattendedMode)
			{
				ParentCache(sender.Graph).RaiseExceptionHandling(_CuryTaxTotal, ParentRow(sender.Graph), CuryTaxTotal,
					new PXSetPropertyException(Messages.UseTaxExcludedFromTotals, PXErrorLevel.Warning));
			}
		}

		private decimal GetOptionalDecimalValue(PXCache cache, object data, string field)
		{
			decimal value = 0m;
			int fieldOrdinal = cache.GetFieldOrdinal(field);
			if (fieldOrdinal >= 0)
			{
				value = (decimal)(cache.GetValue(data, fieldOrdinal) ?? 0m);
			}
			return value;
		}

		protected virtual PXCache ParentCache(PXGraph graph)
		{
			return graph.Caches[_ParentType];
		}

		protected virtual object ParentRow(PXGraph graph)
		{
			if (_ParentRow == null)
			{
				return ParentCache(graph).Current;
			}
			else
			{
				return _ParentRow;
			}
		}

		protected virtual PXEntryStatus ParentGetStatus(PXGraph graph)
		{
			PXCache cache = ParentCache(graph);
			if (_ParentRow == null)
			{
				return cache.GetStatus(cache.Current);
			}
			else
			{
				return cache.GetStatus(_ParentRow);
			}
		}

		protected virtual void ParentSetValue(PXGraph graph, string fieldname, object value)
		{
			PXCache cache = ParentCache(graph);

			if (_ParentRow == null)
			{
				object copy = cache.CreateCopy(cache.Current);
				cache.SetValueExt(cache.Current, fieldname, value);
				cache.MarkUpdated(cache.Current);
				cache.RaiseRowUpdated(cache.Current, copy);
			}
			else
			{
				cache.SetValueExt(_ParentRow, fieldname, value);
			}
		}

		protected virtual object ParentGetValue(PXGraph graph, string fieldname)
		{
			PXCache cache = ParentCache(graph);
			if (_ParentRow == null)
			{
				return cache.GetValue(cache.Current, fieldname);
			}
			else
			{
				return cache.GetValue(_ParentRow, fieldname);
			}
		}

		protected object ParentGetValue<Field>(PXGraph graph)
			where Field : IBqlField
		{
			return ParentGetValue(graph, typeof(Field).Name.ToLower());
		}

		protected void ParentSetValue<Field>(PXGraph graph, object value)
			where Field : IBqlField
		{
			ParentSetValue(graph, typeof(Field).Name.ToLower(), value);
		}

		protected virtual bool CompareZone(PXGraph graph, string zoneA, string zoneB)
		{
			if (!string.Equals(zoneA, zoneB, StringComparison.OrdinalIgnoreCase))
			{
				if (IsExternalTax(graph, zoneA) != IsExternalTax(graph, zoneB))
				{
					return false;
				}
				foreach (PXResult<TaxZoneDet> r in PXSelectGroupBy<TaxZoneDet, Where<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>, Or<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>>>, Aggregate<GroupBy<TaxZoneDet.taxID, Count>>>.Select(graph, zoneA, zoneB))
				{
					if (r.RowCount == 1)
					{
						return false;
					}
				}
			}
			return true;
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXRowInsertedSubscriber) ||
				typeof(ISubscriber) == typeof(IPXRowUpdatedSubscriber) ||
				typeof(ISubscriber) == typeof(IPXRowDeletedSubscriber))
			{
				subscribers.Add(this as ISubscriber);
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			object oldRow = null;
			bool cachedToInserted = false;

			if (_TaxCalc != TaxCalc.NoCalc && _TaxCalc != TaxCalc.ManualLineCalc)
			{
				RaiseAttributesRowInserted(sender, e);


				oldRow = sender.CreateCopy(e.Row);
				if (!inserted.ContainsKey(e.Row))
				{
					inserted[e.Row] = oldRow;
					cachedToInserted = true;
				}
			}

			decimal? val;
			if (GetTaxCategory(sender, e.Row) == null && ((val = GetCuryTranAmt(sender, e.Row)) == null || val == 0m))
			{
				return;
			}

			if (_TaxCalc == TaxCalc.Calc)
			{
				Preload(sender);

				DefaultTaxes(sender, e.Row);
				CalcTaxes(sender, e.Row, PXTaxCheck.Line);
			}
			else if (_TaxCalc == TaxCalc.ManualCalc)
			{
				CalcTotals(sender, e.Row, false);
			}

			if (_TaxCalc != TaxCalc.NoCalc && _TaxCalc != TaxCalc.ManualLineCalc)
			{
				RaiseAttributesRowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, oldRow, false));

				if (cachedToInserted)
				{
					oldRow = sender.CreateCopy(e.Row);
					inserted[e.Row] = oldRow;
				}
			}
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			object oldRow = null;
			bool cachedToUpdated = false;
			if (_TaxCalc != TaxCalc.NoCalc && _TaxCalc != TaxCalc.ManualLineCalc)
			{
				RaiseAttributesRowUpdated(sender, e);

				oldRow = sender.CreateCopy(e.Row);
				if (!updated.ContainsKey(e.Row))
				{
					updated[e.Row] = oldRow;
					cachedToUpdated = true;
				}
			}

			if (_TaxCalc == TaxCalc.Calc)
			{
				var OldTaxZoneID= GetTaxZone(sender, e.OldRow);
				var NewTaxZoneID = GetTaxZone(sender, e.Row);

				if (!CompareZone(sender.Graph, OldTaxZoneID, NewTaxZoneID))
				{
					Preload(sender);
					ReDefaultTaxes(sender, e.OldRow, e.Row, false);
				}
				else if (!object.Equals(GetTaxCategory(sender, e.OldRow), GetTaxCategory(sender, e.Row)))
				{
					Preload(sender);
					ReDefaultTaxes(sender, e.OldRow, e.Row);
				}
				else if (!object.Equals(GetTaxID(sender, e.OldRow), GetTaxID(sender, e.Row)))
				{
					PXCache cache = sender.Graph.Caches[_TaxType];
					TaxDetail taxDetail = (TaxDetail)cache.CreateInstance();
					taxDetail.TaxID = GetTaxID(sender, e.OldRow);
					DelOneTax(cache, e.Row, taxDetail);
					AddOneTax(cache, e.Row, new TaxZoneDet() { TaxID = GetTaxID(sender, e.Row) });
				}

				bool calculated = false;

				if (ShouldRecalculateTaxesOnRowUpdate(sender, e.Row, e.OldRow))
				{
					CalcTaxes(sender, e.Row, PXTaxCheck.Line);
					calculated = true;
				}

				if (!calculated)
				{
					CalcTotals(sender, e.Row, false);
				}
			}
			else if (_TaxCalc == TaxCalc.ManualCalc)
			{
				CalcTotals(sender, e.Row, false);
			}

			if (_TaxCalc != TaxCalc.NoCalc && _TaxCalc != TaxCalc.ManualLineCalc)
			{
				RaiseAttributesRowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, oldRow, false));

				if (cachedToUpdated)
				{
					oldRow = sender.CreateCopy(e.Row);
					updated[e.Row] = oldRow;
				}
			}
		}

		protected virtual bool ShouldRecalculateTaxesOnRowUpdate(PXCache rowCache, object newRow, object oldRow)
		{
			string oldTaxZone = GetTaxZone(rowCache, oldRow);
			string newTaxZone = GetTaxZone(rowCache, newRow);

			if (oldTaxZone != newTaxZone)
				return true;

			string oldTaxCategory = GetTaxCategory(rowCache, oldRow);
			string newTaxCategory = GetTaxCategory(rowCache, newRow);

			if (oldTaxCategory != newTaxCategory)
				return true;

			decimal? oldCuryTranAmount = GetCuryTranAmt(rowCache, oldRow);
			decimal? newCuryTranAmount = GetCuryTranAmt(rowCache, newRow);

			if (oldCuryTranAmount != newCuryTranAmount)
				return true;

			string oldTaxID = GetTaxID(rowCache, oldRow);
			string newTaxID = GetTaxID(rowCache, newRow);

			if (oldTaxID != newTaxID)
				return true;

			if (PXAccess.FeatureInstalled<FeaturesSet.perUnitTaxSupport>())
			{
				decimal oldQuantity = GetLineQty(rowCache, oldRow) ?? 0m;
				decimal newQuantity = GetLineQty(rowCache, newRow) ?? 0m;

				if (oldQuantity != newQuantity)
					return true;

				string oldUOM = GetUOM(rowCache, oldRow);
				string newUOM = GetUOM(rowCache, newRow);

				if (oldUOM != newUOM)
					return true;
			}

			return false;
		}

		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (_TaxCalc != TaxCalc.NoCalc)
			{
				RaiseAttributesRowDeleted(sender, e);
			}

			PXEntryStatus parentStatus = ParentGetStatus(sender.Graph);
			if (parentStatus == PXEntryStatus.Deleted || parentStatus == PXEntryStatus.InsertedDeleted) return;

			decimal? val;
			if (GetTaxCategory(sender, e.Row) == null && ((val = GetCuryTranAmt(sender, e.Row)) == null || val == 0m))
			{
				return;
			}

			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				ClearTaxes(sender, e.Row);
				CalcTaxes(sender, null, PXTaxCheck.Line, IsTaxCalculationNeeded(sender, e.Row));
			}
			else if (_TaxCalc == TaxCalc.ManualCalc)
			{
				CalcTotals(sender, e.Row, false);
			}
		}

		protected virtual void RaiseAttributesRowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			foreach (IPXRowInsertedSubscriber attribute in _Attributes.OfType<IPXRowInsertedSubscriber>().ToArray())
			{
				attribute.RowInserted(sender, e);
			}
		}

		protected virtual void RaiseAttributesRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			foreach (IPXRowUpdatedSubscriber attributte in _Attributes.OfType<IPXRowUpdatedSubscriber>().ToArray())
			{
				attributte.RowUpdated(sender, e);
			}
		}

		protected virtual void RaiseAttributesRowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			foreach (IPXRowDeletedSubscriber attributte in _Attributes.OfType<IPXRowDeletedSubscriber>().ToArray())
			{
				attributte.RowDeleted(sender, e);
			}
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Completed)
			{
				if (inserted != null)
					inserted.Clear();
				if (updated != null)
					updated.Clear();
			}
		}


		protected object _ParentRow;

		protected virtual void CurrencyInfo_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				if (e.Row != null && ((CM.CurrencyInfo)e.Row).CuryRate != null && (e.OldRow == null || !sender.ObjectsEqual<CM.CurrencyInfo.curyRate, CM.CurrencyInfo.curyMultDiv>(e.Row, e.OldRow)))
				{
					PXView siblings = CM.CurrencyInfoAttribute.GetView(sender.Graph, _ChildType, _CuryKeyField);
					if (siblings != null && siblings.SelectSingle() != null)
					{
						CalcTaxes(siblings.Cache, null);
					}
				}
			}
		}

		protected virtual void ParentFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				if (e.Row.GetType() == _ParentType)
				{
					_ParentRow = e.Row;
				}
				CalcTaxes(sender.Graph.Caches[_ChildType], null);
				_ParentRow = null;
			}
		}

		protected virtual void IsTaxSavedFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			decimal? curyTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
			decimal? curyWhTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryWhTaxTotal);

			CalcDocTotals(sender, e.Row, curyTaxTotal.GetValueOrDefault(), 0, curyWhTaxTotal.GetValueOrDefault(), 0m);
		}

		protected virtual List<object> ChildSelect(PXCache cache, object data)
		{
			return TaxParentAttribute.ChildSelect(cache, data, this._ParentType);
		}

		protected virtual void ZoneUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var originalTaxCalc = TaxCalc;
			try
			{
				//old Tax Zone
			if (IsExternalTax(sender.Graph, (string)e.OldValue)) 
				{
					TaxCalc = TaxCalc.Calc;
				}
				//new Tax Zone
				if (IsExternalTax(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID)) || (bool?)sender.GetValue(e.Row, _ExternalTaxesImportInProgress) == true || !IsTaxCalculationNeeded(sender, e.Row))
				{
					TaxCalc = TaxCalc.ManualCalc;
				}


				if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
				{
					PXCache cache = sender.Graph.Caches[_ChildType];
					if (!CompareZone(sender.Graph, (string)e.OldValue, (string)sender.GetValue(e.Row, _TaxZoneID)) || sender.GetValue(e.Row, _TaxZoneID) == null)
					{
						Preload(sender);

						List<object> details = this.ChildSelect(cache, e.Row);

						var args = new ZoneUpdatedArgs { Cache = cache, Details = details, OldValue = (string)e.OldValue, NewValue = (string)sender.GetValue(e.Row, _TaxZoneID) };
						OnZoneUpdated(args);

						_ParentRow = e.Row;
						CalcTaxes(cache, null);
						_ParentRow = null;
					}
				}
			}
			finally
			{
				TaxCalc = originalTaxCalc;
			}
		}

		public class ZoneUpdatedArgs
		{
			public PXCache Cache;
			public List<object> Details;
			public string OldValue;
			public string NewValue;
		}
		protected virtual void OnZoneUpdated(ZoneUpdatedArgs e)
		{
			ReDefaultTaxes(e.Cache, e.Details);
		}

		protected virtual void ReDefaultTaxes(PXCache cache, List<object> details)
		{
			foreach (object det in details)
			{
				ClearTaxes(cache, det);
				ClearChildTaxAmts(cache, det);
			}

			foreach (object det in details)
			{
				DefaultTaxes(cache, det, false);
			}
		}

		protected virtual void ClearChildTaxAmts(PXCache cache, object childRow)
		{
			PXCache childCache = cache.Graph.Caches[_ChildType];
			SetTaxableAmt(childCache, childRow, 0);
			SetTaxAmt(childCache, childRow, 0);
			if (childCache.Locate(childRow) != null) //if record is not in cache then it is just being inserted - no need for manual update
			{
				childCache.MarkUpdated(childRow, assertError: true);
			}
		}

		protected virtual void ReDefaultTaxes(PXCache cache, object clearDet, object defaultDet, bool defaultExisting = true)
		{
			ClearTaxes(cache, clearDet);
			ClearChildTaxAmts(cache, defaultDet);
			DefaultTaxes(cache, defaultDet, defaultExisting);
		}

		protected virtual void DateUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				Preload(sender);

				PXCache cache = sender.Graph.Caches[_ChildType];
				List<object> details = this.ChildSelect(cache, e.Row);
				foreach (object det in details)
				{
					ReDefaultTaxes(cache, det, det, true);
				}
				_ParentRow = e.Row;
				_NoSumTaxable = true;
				try
				{
					CalcTaxes(cache, null);
				}
				finally
				{
					_ParentRow = null;
					_NoSumTaxable = false;
				}
			}
		}

		protected abstract void SetExtCostExt(PXCache sender, object child, decimal? value);

		protected abstract string GetExtCostLabel(PXCache sender, object row);

		protected string GetTaxCalcMode(PXGraph graph)
		{
			if (!_isTaxCalcModeEnabled)
			{
				throw new PXException(Messages.DocumentTaxCalculationModeNotEnabled);
			}
			var mode = (string)ParentGetValue(graph, _TaxCalcMode);
			if (string.IsNullOrWhiteSpace(mode))
			{
				mode = TX.TaxCalculationMode.TaxSetting;
			}
			return mode;
		}
		protected string GetOriginalTaxCalcMode(PXGraph graph)
		{
			if (!_isTaxCalcModeEnabled)
			{
				throw new PXException(Messages.DocumentTaxCalculationModeNotEnabled);
			}
			return string.IsNullOrEmpty(_PreviousTaxCalcMode) ? (string)ParentGetValue(graph, _TaxCalcMode) : _PreviousTaxCalcMode;
		}
		protected virtual bool AskRecalculate(PXCache sender, PXCache detailCache, object detail)
		{
			PXView view = sender.Graph.Views[sender.Graph.PrimaryView];
			string askMessage = PXLocalizer.LocalizeFormat(Messages.RecalculateExtCost, GetExtCostLabel(detailCache, detail));
			return view.Ask(askMessage, MessageButtons.YesNo) == WebDialogResult.Yes;
		}

		protected virtual void TaxCalcModeUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			string newValue = sender.GetValue(e.Row, _TaxCalcMode) as string;
			_PreviousTaxCalcMode = e.OldValue as string;
			if (newValue != (string)e.OldValue)
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				List<object> details = this.ChildSelect(cache, e.Row);

				decimal? taxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
				if (details != null && details.Count != 0 && AskRecalculationOnCalcModeChange)
				{
					if (taxTotal.HasValue && taxTotal.Value != 0 && AskRecalculate(sender, cache, details[0]))
					{
						PXCache taxDetCache = cache.Graph.Caches[_TaxType];
						foreach (object det in details)
						{
							TaxDetail taxSum = TaxSummarizeOneLine(cache, det, SummType.All);
							if (taxSum == null) continue;
							decimal? taxableAmount;
							decimal? taxAmount;
							switch (newValue)
							{
								case TaxCalculationMode.Net:
									taxableAmount = (decimal?)taxDetCache.GetValue(taxSum, _CuryTaxableAmt);
									SetExtCostExt(cache, det, MultiCurrencyCalculator.RoundCury(cache, det, taxableAmount.Value, Precision));
									break;
								case TaxCalculationMode.Gross:
									taxableAmount = (decimal?)taxDetCache.GetValue(taxSum, _CuryTaxableAmt);
									taxAmount = (decimal?)taxDetCache.GetValue(taxSum, _CuryTaxAmt);
									SetExtCostExt(cache, det, MultiCurrencyCalculator.RoundCury(cache, det, taxableAmount.Value + taxAmount.Value, Precision));
									break;
								case TaxCalculationMode.TaxSetting:
									TaxDetail taxSumInclusive = TaxSummarizeOneLine(cache, det, SummType.Inclusive);
									decimal? ExtCost;
									if (taxSumInclusive != null)
									{
										ExtCost = (decimal?)taxDetCache.GetValue(taxSumInclusive, _CuryTaxableAmt) + (decimal?)taxDetCache.GetValue(taxSumInclusive, _CuryTaxAmt);
									}
									else
									{
										ExtCost = (decimal?)taxDetCache.GetValue(taxSum, _CuryTaxableAmt);
									}
									SetExtCostExt(cache, det, MultiCurrencyCalculator.RoundCury(cache, det, ExtCost.Value, Precision));
									break;
							}
						}
					}
				}

				Preload(sender);
				if (details != null)
				{
					foreach (object det in details)
					{
						ReDefaultTaxes(cache, det, det, false);
					}
				}
				_ParentRow = e.Row;
				CalcTaxes(cache, null);
				_ParentRow = null;
			}
		}

		private enum SummType
		{
			Inclusive, All
		}

		private TaxDetail TaxSummarizeOneLine(PXCache cache, object row, SummType summType)
		{
			List<object> taxitems = new List<object>();
			switch (summType)
			{
				case SummType.All:
					if (CalcGrossOnDocumentLevel && _isTaxCalcModeEnabled)
					{
						taxitems = SelectTaxes<Where<Tax.taxCalcLevel, NotEqual<CSTaxCalcLevel.calcOnItemAmtPlusTaxAmt>,
							And<Tax.taxType, NotEqual<CSTaxType.withholding>,
							And<Tax.directTax, Equal<False>>>>>(cache.Graph, row, PXTaxCheck.Line);
					}
					else
					{
						taxitems = SelectTaxes<Where<Tax.taxCalcLevel, NotEqual<CSTaxCalcLevel.calcOnItemAmtPlusTaxAmt>,
							And<Tax.taxCalcType, Equal<CSTaxCalcType.item>,
							And<Tax.taxType, NotEqual<CSTaxType.withholding>,
							And<Tax.directTax, Equal<False>>>>>>(cache.Graph, row, PXTaxCheck.Line);
					}
					break;
				case SummType.Inclusive:
					if (CalcGrossOnDocumentLevel && _isTaxCalcModeEnabled)
					{
						taxitems = SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
							And<Tax.taxType, NotEqual<CSTaxType.withholding>,
							And<Tax.directTax, Equal<False>>>>>(cache.Graph, row, PXTaxCheck.Line);
					}
					else
					{
						taxitems = SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
							And<Tax.taxCalcType, Equal<CSTaxCalcType.item>,
							And<Tax.taxType, NotEqual<CSTaxType.withholding>,
							And<Tax.directTax, Equal<False>>>>>>(cache.Graph, row, PXTaxCheck.Line);
					}
					break;
			}

			if (taxitems.Count == 0) return null;

			PXCache taxDetCache = cache.Graph.Caches[_TaxType];
			TaxDetail taxLineSumDet = (TaxDetail)taxDetCache.CreateInstance();
			decimal? CuryTaxableAmt = (decimal?)taxDetCache.GetValue(((PXResult)taxitems[0])[0], _CuryTaxableAmt);

			//AdjustTaxableAmount(sender, row, taxitems, ref CuryTaxableAmt, tax.TaxCalcType);

			decimal? CuryTaxAmt = SumWithReverseAdjustment(cache.Graph,
				taxitems,
				GetFieldType(taxDetCache, _CuryTaxAmt));

			if (CalcGrossOnDocumentLevel && _isTaxCalcModeEnabled)
			{
				var oldCalcMode = this.GetOriginalTaxCalcMode(cache.Graph);
				var newCalcMode = this.GetTaxCalcMode(cache.Graph);

				if (newCalcMode == TaxCalculationMode.Gross && oldCalcMode != TaxCalculationMode.Gross)
				{
					foreach (var taxitem in taxitems)
					{
						var tax = (Tax)((PXResult)taxitem)[typeof(Tax)];
						var taxRev = (TaxRev)((PXResult)taxitem)[typeof(TaxRev)];
						if (tax?.TaxCalcType == CSTaxCalcType.Doc)
						{
							var origTaxAmt = (decimal?)taxDetCache.GetValue(((PXResult)taxitem)[0], _CuryTaxAmt);
							var calculatedTaxAmt = CuryTaxableAmt * taxRev.TaxRate / 100.0M;
							CuryTaxAmt += calculatedTaxAmt - origTaxAmt;
						}
					}
				}
			}

			decimal? CuryExpenseAmt = SumWithReverseAdjustment(cache.Graph,
				taxitems,
				GetFieldType(taxDetCache, _CuryExpenseAmt));

			taxDetCache.SetValue(taxLineSumDet, _CuryTaxableAmt, CuryTaxableAmt);
			taxDetCache.SetValue(taxLineSumDet, _CuryTaxAmt, CuryTaxAmt + CuryExpenseAmt);

			return taxLineSumDet;
		}

		private decimal SumWithReverseAdjustment(PXGraph graph, List<Object> list, Type field)
		{
			decimal ret = 0.0m;
			list.ForEach(a =>
			{
				decimal? val = (decimal?)graph.Caches[BqlCommand.GetItemType(field)].GetValue(((PXResult)a)[BqlCommand.GetItemType(field)], field.Name);
				Tax tax = (Tax)((PXResult)a)[typeof(Tax)];
				decimal multiplier = tax.ReverseTax == true ? Decimal.MinusOne : Decimal.One;
				ret += (val ?? 0m) * multiplier;
			}
			);
			return ret;
		}

		protected virtual void TaxSum_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			object newdet = e.Row;

			if (newdet == null)
				return;

			Dictionary<string, object> newdetKeys = GetKeyFieldValues(cache, newdet);
			bool insertNewTaxTran = true;

			if (ExternalTax.IsExternalTax(cache.Graph, (string)cache.GetValue(newdet, _TaxZoneID)) != true)
			{
				if (e.ExternalCall && e.Row is TaxDetail taxDetail && CheckIfTaxDetailHasPerUnitTaxType(cache.Graph, taxDetail.TaxID))  //Forbid to insert per-unit taxes manually from the UI
				{
					e.Cancel = true;
					throw new PXSetPropertyException(Messages.PerUnitTaxCannotBeInsertedManuallyErrorMsg);
				}

				if (_IncludeDirectTaxLine && e.ExternalCall && GetTax(cache.Graph, ((TaxDetail)newdet).TaxID)?.DirectTax == true)  //Forbid to insertion of direct manually from the UI
				{
					e.Cancel = true;
					throw new PXSetPropertyException(Messages.DirectTaxCanNotBeInsertedManually);
				}

				foreach (object cacheddet in cache.Cached)
				{
					Dictionary<string, object> cacheddetKeys = new Dictionary<string, object>();
					cacheddetKeys = GetKeyFieldValues(cache, cacheddet);
					bool recordsEqual = true;
					PXEntryStatus status = cache.GetStatus(cacheddet);

					if (status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
					{
						foreach (KeyValuePair<string, object> keyValue in newdetKeys)
						{
							if (cacheddetKeys.ContainsKey(keyValue.Key) && !Object.Equals(cacheddetKeys[keyValue.Key], keyValue.Value))
							{
								recordsEqual = false;
								break;
							}
						}
						if (recordsEqual)
						{
							if (cache.Graph.IsMobile) // if inserting from mobile - override old detail
							{
								cache.Delete(cacheddet);
							}
							else
							{
								insertNewTaxTran = false;
								break;
							}
						}
					}
				}
				if (!insertNewTaxTran)
					e.Cancel = true;
			}
		}

		protected virtual void TaxSum_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			if (e.ExternalCall && _IncludeDirectTaxLine && e.Row is TaxDetail taxDetail && GetTax(cache.Graph, taxDetail.TaxID)?.DirectTax == true) //Forbid to delete direct tax from UI
			{
				string taxzone = GetTaxZone(cache, e.Row);
				string taxCategory = null;
				PXCache childCache = cache.Graph.Caches[_ChildType];
				List<object> details = ChildSelect(childCache, e.Row);
				HashSet<string> transactionCategories = new HashSet<string>();
				
				foreach (object det in details)
				{
					string category = GetTaxCategory(childCache, det);
					if (!string.IsNullOrWhiteSpace(category))
					{
						transactionCategories.Add(category);
					}
				}

				foreach (PXResult<TaxZoneDet, TaxCategory, TaxCategoryDet, Tax> r in SelectFrom<TaxZoneDet>
			   .CrossJoin<TaxCategory>
			   .LeftJoin<TaxCategoryDet>
				   .On<TaxCategoryDet.taxID.IsEqual<TaxZoneDet.taxID>
				   .And<TaxCategoryDet.taxCategoryID.IsEqual<TaxCategory.taxCategoryID>>>
			   .LeftJoin<Tax>
				   .On<Tax.taxID.IsEqual<TaxZoneDet.taxID>>
			   .Where<TaxZoneDet.taxZoneID.IsEqual<@P.AsString>
				   .And<Tax.taxID.IsEqual<@P.AsString>>
				   .And<TaxCategory.taxCatFlag.IsEqual<False>
				   .And<TaxCategoryDet.taxCategoryID.IsNotNull>>>
				   .View.Select(cache.Graph, taxzone, taxDetail?.TaxID))
				{
					taxCategory = ((TaxCategory)r)?.TaxCategoryID;
					if (!string.IsNullOrWhiteSpace(taxCategory) && transactionCategories.Contains(taxCategory))
					{
						break;
					}
				}
				e.Cancel = true;
				throw new PXException(Messages.DirectTaxCanNotBeDeletedManually, taxCategory);
			}
		}

		private Dictionary<string, object> GetKeyFieldValues(PXCache cache, object row)
		{
			Dictionary<string, object> keyValues = new Dictionary<string, object>();
			foreach (string key in cache.Keys)
			{
				if (key != _RecordID)
					keyValues.Add(key, cache.GetValue(row, key));
			}
			return keyValues;
		}

		protected virtual void DelOneTax(PXCache sender, object detrow, object taxrow)
		{
			PXCache cache = sender.Graph.Caches[_ChildType];
			bool hasInclusiveTax = false;
			foreach (object taxdet in SelectTaxes(cache, detrow, PXTaxCheck.Line))
			{
				if (object.Equals(((TaxDetail)((PXResult)taxdet)[0]).TaxID, ((TaxDetail)taxrow).TaxID))
				{
					sender.Delete(((PXResult)taxdet)[0]);

					if (((Tax)((PXResult)taxdet)[1]).TaxCalcLevel == CSTaxCalcLevel.Inclusive)
					{
						hasInclusiveTax = true;
					}
				}
			}
			if (hasInclusiveTax)
			{
				SetTaxableAmt(cache, detrow, 0);
				SetTaxAmt(cache, detrow, 0);
				cache.MarkUpdated(detrow);
			}
		}

		protected virtual void Preload(PXCache sender)
		{
			SelectTaxes(sender, null, PXTaxCheck.RecalcTotals);
		}

		/// <summary>
		/// During the import process, some fields may not have a default value.
		/// </summary>
		private static void InvokeExceptForExcelImport(PXCache cache, Action action)
		{
			if (!cache.Graph.IsImportFromExcel && !cache.Graph.IsCopyPasteContext)
			{
				action.Invoke();
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_ChildType = sender.GetItemType();

			inserted = new Dictionary<object, object>();
			updated = new Dictionary<object, object>();

			PXCache cache = sender.Graph.Caches[_TaxType];

			sender.Graph.FieldUpdated.AddHandler(_ParentType, _DocDate, (s, e) => InvokeExceptForExcelImport(s, () => DateUpdated(s, e)));
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _TaxZoneID, (s, e) => InvokeExceptForExcelImport(s, () => ZoneUpdated(s, e)));
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _IsTaxSaved, (s, e) => InvokeExceptForExcelImport(s, () => IsTaxSavedFieldUpdated(s, e)));
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _TermsID, ParentFieldUpdated);
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _CuryID, ParentFieldUpdated);
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _CuryOrigDiscAmt, CuryOrigDiscAmt_FieldUpdated);

			sender.Graph.RowUpdated.AddHandler(_ParentType, ParentRowUpdated);

			sender.Graph.RowInserting.AddHandler(_TaxSumType, TaxSum_RowInserting);
			sender.Graph.RowDeleting.AddHandler(_TaxSumType, TaxSum_RowDeleting);

			if (PXAccess.FeatureInstalled<FeaturesSet.perUnitTaxSupport>())
			{
				sender.Graph.RowPersisting.AddHandler(_ChildType, DocumentLineCheckPerUnitTaxesOnRowPersisting);
				sender.Graph.RowPersisting.AddHandler(_ParentType, DocumentCheckPerUnitTaxesOnRowPersisting);
				sender.Graph.RowSelected.AddHandler(_ParentType, CheckCurrencyAndRetainageOnDocumentRowSelected);

				sender.Graph.RowDeleting.AddHandler(_TaxSumType, CheckForPerUnitTaxesOnAggregatedTaxRowDeleting);
				sender.Graph.RowSelected.AddHandler(_TaxSumType, DisablePerUnitTaxesOnAggregatedTaxDetailRowSelected);
			}

			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(null))
			{
				if (attr is CM.CurrencyInfoAttribute)
				{
					_CuryKeyField = sender.GetBqlField(attr.FieldName);
					break;
				}
			}

			if (_CuryKeyField != null)
			{
				sender.Graph.RowUpdated.AddHandler<CM.CurrencyInfo>(CurrencyInfo_RowUpdated);
			}

			sender.Graph.Caches.SubscribeCacheCreated<Tax>(delegate
			{
				PXUIFieldAttribute.SetVisible<Tax.exemptTax>(sender.Graph.Caches[typeof(Tax)], null, false);
				PXUIFieldAttribute.SetVisible<Tax.statisticalTax>(sender.Graph.Caches[typeof(Tax)], null, false);
				PXUIFieldAttribute.SetVisible<Tax.reverseTax>(sender.Graph.Caches[typeof(Tax)], null, false);
				PXUIFieldAttribute.SetVisible<Tax.pendingTax>(sender.Graph.Caches[typeof(Tax)], null, false);
				PXUIFieldAttribute.SetVisible<Tax.taxType>(sender.Graph.Caches[typeof(Tax)], null, false);
			});

			if (_isTaxCalcModeEnabled)
			{
				sender.Graph.FieldUpdated.AddHandler(_ParentType, _TaxCalcMode, (s, e) => InvokeExceptForExcelImport(s, () => TaxCalcModeUpdated(s, e)));
			}
		}

		public TaxBaseAttribute(Type ParentType, Type TaxType, Type TaxSumType, Type CalcMode = null, Type parentBranchIDField = null)
		{
			ParentBranchIDField = parentBranchIDField;

			ChildFinPeriodIDField = typeof(TaxTran.finPeriodID);
			ChildBranchIDField = typeof(TaxTran.branchID);

			_ParentType = ParentType;
			_TaxType = TaxType;
			_TaxSumType = TaxSumType;

			if (CalcMode != null)
			{
				if (!typeof(IBqlField).IsAssignableFrom(CalcMode))
				{
					throw new PXArgumentException("CalcMode", ErrorMessages.ArgumentException);
				}
				TaxCalcMode = CalcMode;
			}
		}

		public virtual int CompareTo(object other)
		{
			return 0;
		}

		protected virtual IComparer<Tax> GetTaxByCalculationLevelComparer() => TaxByCalculationLevelComparer.Instance;

		protected virtual IDictionary<string, PXResult<Tax, TaxRev>> CollectInvolvedTaxes<TWhere>(
			PXGraph graph,
			IEnumerable<ITaxDetail> details,
			BqlCommand select,
			object[] currents,
			object[] whereParameters,
			object[] selectParameters = null)
			where TWhere : IBqlWhere, new()
		{
			IDictionary<string, PXResult<Tax, TaxRev>> involvedTaxes = new Dictionary<string, PXResult<Tax, TaxRev>>();

			if (select == null) return involvedTaxes;

			HashSet<string> taxes = new HashSet<string>(details.Select(d => d.TaxID));

			object[] parameters = whereParameters.Prepend((object)taxes.ToArray());
			if(selectParameters != null && selectParameters.Any())
			{
				parameters = parameters.Prepend(selectParameters);
			}

			select = select.WhereAnd<Where2<Where<Tax.taxID, In<Required<Tax.taxID>>>, And<TWhere>>>();

			foreach (PXResult<Tax, TaxRev> record in select.CreateView(graph).SelectMultiBound(currents, parameters))
			{
				Tax adjdTax = AdjustTaxLevel(graph, record);
				involvedTaxes[((Tax)record).TaxID] = new PXResult<Tax, TaxRev>(adjdTax, record);
			}

			return involvedTaxes;
		}

		protected virtual bool InnerLineNbrCondition<TTaxDetail>(PXTaxCheck taxchk, TTaxDetail record, int index, List<object> taxList)
			where TTaxDetail : class, ITaxDetail, IBqlTable, new()
		{
			return taxchk != PXTaxCheck.RecalcLine
				|| !(record is ITaxDetailWithLineNbr recordWitLineNbr)
				|| !((TTaxDetail)(PXResult<TTaxDetail, Tax, TaxRev>)taxList[index] is ITaxDetailWithLineNbr itemWitLineNbr)
				|| itemWitLineNbr.LineNbr == recordWitLineNbr.LineNbr;
		}

		protected virtual void InsertTax<TTaxDetail>(
			PXGraph graph,
			PXTaxCheck taxchk,
			TTaxDetail record,
			IDictionary<string, PXResult<Tax, TaxRev>> tails,
			List<object> taxList)
			where TTaxDetail : class, ITaxDetail, IBqlTable, new()
		{
			IComparer<Tax> taxByCalculationLevelComparer = GetTaxByCalculationLevelComparer();
			taxByCalculationLevelComparer.ThrowOnNull(nameof(taxByCalculationLevelComparer));

			if (record.TaxID != null 
				&& tails.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
			{
				int index;
				for (index = taxList.Count;
					(index > 0) 
						&& InnerLineNbrCondition(taxchk, record, index - 1, taxList)
						&& taxByCalculationLevelComparer.Compare((PXResult<TTaxDetail, Tax, TaxRev>)taxList[index - 1], line) > 0;
					index--);
				taxList.Insert(
					index, 
					new PXResult<TTaxDetail, Tax, TaxRev>(
						record, 
						AdjustTaxLevel(graph, line), 
						line));
			}
		}
	}
}
