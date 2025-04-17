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

using PX.Api.ContractBased.Models;
using PX.Commerce.Core;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing.Specific;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.IN;
using PX.Objects.IN.RelatedItems;
using PX.Objects.SO;
using PX.Objects.TX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PX.Commerce.Objects
{
	#region BCItemVisibilityAttribute
	public class BCItemVisibility
	{
		public const string StoreDefault = "X";
		public const string Visible = "V";
		public const string Featured = "F";
		public const string Invisible = "I";

		public class List : PXStringListAttribute
		{
			public List() :
				base(
					new[] {
						StoreDefault,
						Visible,
						Featured,
						Invisible,
					},
					new[]
					{
						BCCaptions.StoreDefault,
						BCCaptions.Visible,
						BCCaptions.Featured,
						BCCaptions.Invisible,
					})
			{ }
		}
		public class ListDef : PXStringListAttribute
		{
			public ListDef() :
				base(
					new[] {
						Visible,
						Featured,
						Invisible,
					},
					new[]
					{
						BCCaptions.Visible,
						BCCaptions.Featured,
						BCCaptions.Invisible,
					})
			{ }
		}

		public static string Convert(String val)
		{
			switch (val)
			{
				case StoreDefault: return BCCaptions.StoreDefault;
				case Visible: return BCCaptions.Visible;
				case Featured: return BCCaptions.Featured;
				case Invisible: return BCCaptions.Invisible;
				default: return null;
			}
		}
	}
	#endregion
	#region BCPostDiscountAttribute
	public class BCPostDiscountAttribute : PXStringListAttribute
	{
		public const string LineDiscount = "L";
		public const string DocumentDiscount = "D";

		public BCPostDiscountAttribute() :
			base(
				new[] {
					LineDiscount,
					DocumentDiscount,
				},
				new[]
				{
					BCObjectsMessages.LineDiscount,
					BCObjectsMessages.DocumentDiscount,
				})
		{
		}
	}
	#endregion

	#region BCRiskStatusAttribute
	public class BCRiskStatusAttribute : PXStringListAttribute
	{
		public const string HighRisk = "H";
		public const string MediumRisk = "M";
		public const string LowRisk = "L";

		public BCRiskStatusAttribute() :
			base(
				new[] {
					HighRisk,
					MediumRisk,
				},
				new[]
				{
					BCObjectsMessages.HighRisk,
					BCObjectsMessages.MediumOrHighRisk,
				})
		{
		}

		public BCRiskStatusAttribute(bool isForOrder) :
			base(
				new[] {
					HighRisk,
					MediumRisk,
					LowRisk
				},
				new[]
				{
					BCCaptions.High,
					BCCaptions.Medium,
					BCCaptions.Low
				})
		{
		}

		public sealed class high : PX.Data.BQL.BqlString.Constant<high>
		{
			public high() : base(BCCaptions.High)
			{
			}
		}
		public sealed class medium : PX.Data.BQL.BqlString.Constant<medium>
		{
			public medium() : base(BCCaptions.Medium)
			{
			}
		}

		public sealed class low : PX.Data.BQL.BqlString.Constant<low>
		{
			public low() : base(BCCaptions.Low)
			{
			}
		}

		public sealed class none : PX.Data.BQL.BqlString.Constant<none>
		{
			public none() : base(BCCaptions.None)
			{
			}
		}
	}
	#endregion

	#region BCItemFileTypeAttribute
	public class BCFileTypeAttribute : PXStringListAttribute
	{
		public const string Image = "I";
		public const string Video = "V";

		public BCFileTypeAttribute() :
			base(
				new[] {
					Image,
					Video,
				},
				new[]
				{
					BCCaptions.Image,
					BCCaptions.Video,
				})
		{
		}

	}
	#endregion

	#region BCAvailabilityLevelsAttribute
	public class BCAvailabilityLevelsAttribute : PXStringListAttribute
	{
		public const string Available = "A";
		public const string AvailableForShipping = "S";
		public const string OnHand = "H";

		public BCAvailabilityLevelsAttribute() :
			base(
				new[] {
					Available,
					AvailableForShipping,
					OnHand,
				},
				new[]
				{
					BCCaptions.Available,
					BCCaptions.AvailableForShipping,
					BCCaptions.OnHand,
				})
		{

		}

		public sealed class available : PX.Data.BQL.BqlString.Constant<available>
		{
			public available() : base(Available)
			{
			}
		}
		public sealed class availableForShipping : PX.Data.BQL.BqlString.Constant<availableForShipping>
		{
			public availableForShipping() : base(AvailableForShipping)
			{
			}
		}
		public sealed class onHand : PX.Data.BQL.BqlString.Constant<onHand>
		{
			public onHand() : base(OnHand)
			{
			}
		}
	}
	#endregion

	#region BCWarehouseModeAttribute
	public class BCWarehouseModeAttribute : PXStringListAttribute
	{
		public const string AllWarehouse = "A";
		public const string SpecificWarehouse = "S";

		public BCWarehouseModeAttribute() :
				base(
					new[]
					{
						AllWarehouse,
						SpecificWarehouse},
					new[]
					{
						BCCaptions.AllWarehouse,
						BCCaptions.SpecificWarehouse
					})
		{ }
		public sealed class allWarehouse : PX.Data.BQL.BqlString.Constant<allWarehouse>
		{
			public allWarehouse() : base(AllWarehouse)
			{
			}
		}
		public sealed class specificWarehouse : PX.Data.BQL.BqlString.Constant<specificWarehouse>
		{
			public specificWarehouse() : base(SpecificWarehouse)
			{
			}
		}
	}
	#endregion

	#region BCSalesCategoriesExportAttribute
	public class BCSalesCategoriesExportAttribute : PXStringListAttribute
	{
		public const string DoNotSync = "N";
		public const string SyncToProductTags = "E";

		public BCSalesCategoriesExportAttribute() :
				base(
					new[]
					{
						DoNotSync,
						SyncToProductTags},
					new[]
					{
						BCCaptions.DoNotExport,
						BCCaptions.ExportAsProductTags
					})
		{ }
	}
	#endregion

	#region BCShopifyStorePlanAttribute
	public class BCShopifyStorePlanAttribute : PXStringListAttribute
	{
		public const string LitePlan = "LP";
		public const string BasicPlan = "BP";
		public const string NormalPlan = "NP";
		public const string AdvancedPlan = "AP";
		public const string PlusPlan = "PP";

		public BCShopifyStorePlanAttribute() :
				base(
					new[]
					{
						LitePlan,
						BasicPlan,
						NormalPlan,
						AdvancedPlan,
						PlusPlan},
					new[]
					{
						BCCaptions.ShopifyLitePlan,
						BCCaptions.ShopifyBasicPlan,
						BCCaptions.ShopifyNormalPlan,
						BCCaptions.ShopifyAdvancedPlan,
						BCCaptions.ShopifyPlusPlan
					})
		{ }
	}
	#endregion

	#region BCItemAvailabilityAttribute
	public class BCItemAvailabilities
	{
		public const string StoreDefault = "X";
		public const string AvailableTrack = "T";
		public const string AvailableSkip = "S";
		public const string DoNotUpdate = "N";
		public const string PreOrder = "P";
		public const string Disabled = "D";

		public class List : PXStringListAttribute
		{
			public List() :
				base(
					new[] {
						AvailableTrack,
						AvailableSkip,
						PreOrder,
						DoNotUpdate,
						Disabled
					},
					new[]
					{
						BCCaptions.AvailableTrack,
						BCCaptions.AvailableSkip,
						BCCaptions.PreOrder,
						BCCaptions.DoNotUpdate,
						BCCaptions.Disabled,
					})
			{
			}
		}
		public class ListDef : PXStringListAttribute, IPXRowSelectedSubscriber
		{
			public ListDef() :
				base(
					new[] {
						StoreDefault,
						AvailableTrack,
						AvailableSkip,
						PreOrder,
						DoNotUpdate,
						Disabled,
					},
					new[]
					{
						BCCaptions.StoreDefault,
						BCCaptions.AvailableTrack,
						BCCaptions.AvailableSkip,
						BCCaptions.PreOrder,
						BCCaptions.DoNotUpdate,
						BCCaptions.Disabled,
					})
			{
			}

			public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				InventoryItem row = e.Row as InventoryItem;

				if (row != null)
				{
					if (row.StkItem == false)
					{
						var list = new BCItemAvailabilities.NonStockAvailability().ValueLabelDic;

						PXStringListAttribute.SetList<BCInventoryItem.availability>(sender, row, list.Keys.ToArray(), list.Values.ToArray());
						sender.Adjust<PXUIFieldAttribute>(row).For<BCInventoryItem.notAvailMode>(fa => fa.Visible = false);


					}
					else
					{
						var list = new BCItemAvailabilities.ListDef().ValueLabelDic;

						PXStringListAttribute.SetList<BCInventoryItem.availability>(sender, row, list.Keys.ToArray(), list.Values.ToArray());
						sender.Adjust<PXUIFieldAttribute>(row).For<BCInventoryItem.notAvailMode>(fa => fa.Visible = true);
					}
				}
			}
		}
		public class NonStockAvailability : PXStringListAttribute
		{
			public NonStockAvailability() :
				base(
					new[] {
						StoreDefault,
						AvailableSkip,
						PreOrder,
						DoNotUpdate,
						Disabled,
					},
					new[]
					{
						BCCaptions.StoreDefault,
						BCCaptions.AvailableSkip,
						BCCaptions.PreOrder,
						BCCaptions.DoNotUpdate,
						BCCaptions.Disabled,
					})
			{
			}
		}

		public static string Convert(String val)
		{
			switch (val)
			{
				case StoreDefault: return BCCaptions.StoreDefault;
				case AvailableTrack: return BCCaptions.AvailableTrack;
				case AvailableSkip: return BCCaptions.AvailableSkip;
				case PreOrder: return BCCaptions.PreOrder;
				case DoNotUpdate: return BCCaptions.DoNotUpdate;
				case Disabled: return BCCaptions.Disabled;
				default: return null;
			}
		}
		public static string Resolve(String itemValue, String storeValue)
		{
			string availability = itemValue;
			if (availability == null || availability == BCCaptions.StoreDefault)
				availability = BCItemAvailabilities.Convert(storeValue);
			return availability;
		}

		public sealed class storeDefault : PX.Data.BQL.BqlString.Constant<storeDefault>
		{
			public storeDefault() : base(StoreDefault)
			{
			}
		}
		public sealed class availableTrack : PX.Data.BQL.BqlString.Constant<availableTrack>
		{
			public availableTrack() : base(AvailableTrack)
			{
			}
		}
		public sealed class availableSkip : PX.Data.BQL.BqlString.Constant<availableSkip>
		{
			public availableSkip() : base(AvailableSkip)
			{
			}
		}
		public sealed class preOrder : PX.Data.BQL.BqlString.Constant<preOrder>
		{
			public preOrder() : base(PreOrder)
			{
			}
		}
		public sealed class disabled : PX.Data.BQL.BqlString.Constant<disabled>
		{
			public disabled() : base(Disabled)
			{
			}
		}
	}
	#endregion
	#region BCItemNotAvailModeAttribute
	public class BCItemNotAvailModes
	{
		public const string StoreDefault = "X";
		public const string DoNothing = "N";
		public const string DisableItem = "D";
		public const string PreOrderItem = "P";

		public class List : PXStringListAttribute
		{
			public List() :
			base(
				new[] {
					DoNothing,
					DisableItem,
					PreOrderItem,
				},
				new[]
				{
					BCCaptions.DoNothing,
					BCCaptions.DisableItem,
					BCCaptions.PreOrderItem,
				})
			{
			}
		}
		public class ListDef : PXStringListAttribute
		{
			public ListDef() :
			base(
				new[] {
					StoreDefault,
					DoNothing,
					DisableItem,
					PreOrderItem,
				},
				new[]
				{
					BCCaptions.StoreDefault,
					BCCaptions.DoNothing,
					BCCaptions.DisableItem,
					BCCaptions.EnableSellingItem,
				})
			{
			}
		}

		public static string Convert(String val)
		{
			switch (val)
			{
				case StoreDefault: return BCCaptions.StoreDefault;
				case DoNothing: return BCCaptions.DoNothing;
				case DisableItem: return BCCaptions.DisableItem;
				case PreOrderItem: return BCCaptions.EnableSellingItem;
				default: return null;
			}
		}
	}
	#endregion

	#region BCDimensionMaskAttribute
	public class BCDimensionMaskAttribute : BCDimensionAttribute, IPXRowSelectedSubscriber, IPXRowSelectingSubscriber, IPXFieldDefaultingSubscriber, IPXFieldUpdatedSubscriber
	{
		protected Type NewNumbering;
		protected Type BranchField;

		public BCDimensionMaskAttribute(String dimension, Type numbering, Type branch)
			: base(dimension)
		{
			if (numbering == null) throw new ArgumentException("numbering");

			NewNumbering = numbering;
			BranchField = branch;
		}
		public override void CacheAttached(PXCache sender)
		{
			SetSegmentDelegate(new PXSelectDelegate<short?, string>(BCSegmentSelect));

			base.CacheAttached(sender);

			sender.Graph.FieldVerifying.AddHandler(_BqlTable, NewNumbering.Name, NumberingFieldVerifying);
		}

		public System.Collections.IEnumerable BCSegmentSelect([PXShort] short? segment, [PXString] string value)
		{
			if (segment == 0)
			{
				yield return new SegmentValue(new String('#', _Definition.Dimensions[_Dimension].Sum(s => s.Length)), "Auto Numbering", false);
			}
			if (segment > 0)
			{
				PXSegment seg = segment != null ? _Definition.Dimensions[_Dimension][segment.Value - 1] : _Definition.Dimensions[_Dimension].FirstOrDefault();
				if (!seg.Validate) yield return new SegmentValue(new String('#', seg.Length), "Auto Numbering", false);
			}

			foreach (SegmentValue segmentValue in base.SegmentSelect(segment, value))
				yield return segmentValue;
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			//Suppress Auto-Numbering
			//base.RowPersisting(sender, e);
		}
		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			Boolean enabled = GetSegments(_Dimension).Count() > 1;
			if (!enabled) sender.SetValue(e.Row, _FieldOrdinal, null);
		}
		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Boolean enabled = GetSegments(_Dimension).Count() > 1;

			PXUIFieldAttribute.SetEnabled(sender, e.Row, _FieldName, enabled);
		}
		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			base.FieldVerifying(sender, e);

			//Validate Mask
			string mask = e.NewValue as String;
			if (mask == null) return;

			Int32 index = 0, count = 0;
			Int32 autoSegment = -1;
			foreach (PXSegment seg in GetSegments(_Dimension))
			{
				//Replace after merge
				short segmentID = (short)seg.GetType().InvokeMember("SegmentID", BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance, null, seg, new object[0]);

				if (mask.Length < index + seg.Length) throw new PXSetPropertyException(BCMessages.InvalidMaskLength);
				String part = mask.Substring(index, seg.Length);

				if (part.StartsWith("#"))
				{
					if (autoSegment >= 0) throw new PXSetPropertyException(BCMessages.MultipleAutoNumberSegments);
					autoSegment = segmentID;
				}
				else if (seg.Validate)
				{
					Dictionary<String, ValueDescr> dict = PXDatabaseGetSlot().Values[_Dimension][segmentID];
					if (!dict.ContainsKey(part)) throw new PXSetPropertyException(BCMessages.InvalidSegmentValue);
				}

				index += seg.Length;
				count++;
			}
			if (count > 1 && autoSegment < 0) throw new PXSetPropertyException(BCMessages.InvalidAutoNumberSegment);
		}
		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			base.FieldDefaulting(sender, e);

			string mask = String.Empty;

			foreach (PXSegment seg in GetSegments(_Dimension))
			{
				//Replace after merge
				short segmentID = (short)seg.GetType().InvokeMember("SegmentID", BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance, null, seg, new object[0]);
				bool autonumber = (bool)seg.GetType().InvokeMember("AutoNumber", BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance, null, seg, new object[0]);

				if (autonumber) mask += new String('#', seg.Length);
				else if (seg.Validate)
				{
					Dictionary<String, ValueDescr> dict = PXDatabaseGetSlot().Values[_Dimension][segmentID];
					mask += dict.FirstOrDefault().Key;
				}
				else mask += new String(' ', seg.Length);
			}

			e.NewValue = mask;
		}
		public virtual void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Object val = sender.GetValue(e.Row, NewNumbering.Name);
			sender.RaiseFieldVerifying(NewNumbering.Name, e.Row, ref val);
		}
		public virtual void NumberingFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			base.FieldVerifying(sender, e);
			string numb = (string)e.NewValue;
			string mask = (string)sender.GetValue(e.Row, _FieldOrdinal);
			if (numb == null)
				return;

			int? branch = sender.Graph.Caches[BqlCommand.GetItemType(BranchField).Name]
				.GetValue(sender.Graph.Caches[BqlCommand.GetItemType(BranchField).Name].Current, BranchField.Name) as int?;

			VerifyNumberSequenceLength(numb, mask, _Dimension, branch, sender.Graph.Accessinfo.BusinessDate);
		}

		public static void VerifyNumberSequenceLength(string numb, string mask, string dimension, int? branch, DateTime? businessDate)
		{

			Int32 index = 0;
			Int32 autoSegmentLength = -1;
			foreach (PXSegment seg in GetSegments(dimension))
			{
				//bool autonumber = (bool)seg.GetType().InvokeMember("AutoNumber", BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance, null, seg, new object[0]);

				if ((mask != null && mask.Substring(index, seg.Length).StartsWith("#")))
				{
					autoSegmentLength = seg.Length;
				}

				index += seg.Length;
			}
			PX.Objects.CS.NumberingSequence seq = null;
			if (branch != null) seq = PX.Objects.CS.AutoNumberAttribute.GetNumberingSequence(numb, branch, businessDate);
			if (seq == null) seq = PX.Objects.CS.AutoNumberAttribute.GetNumberingSequence(numb, null, businessDate);

			if (autoSegmentLength > 0 && seq == null)
				throw new PXSetPropertyException(BCMessages.InvalidAutoNumberSegment);
			if (autoSegmentLength > 0 && seq?.LastNbr?.Length != autoSegmentLength || seq?.LastNbr?.Length > index)
				throw new PXSetPropertyException(BCMessages.InvalidNumberingLength, numb, dimension);
		}
	}
	#endregion
	#region BCCustomNumberingAttribute
	public class BCCustomNumberingAttribute : PXEventSubscriberAttribute
	{
		protected String Dimension;
		protected Type Mask;
		protected Type Numbering;
		protected Type NumberingSelect;

		public BCCustomNumberingAttribute(String dimension, Type mask, Type numbering, Type select)
		{
			Dimension = dimension ?? throw new ArgumentException("dimension");
			Mask = mask ?? throw new ArgumentException("mask");
			Numbering = numbering ?? throw new ArgumentException("numbering");
			NumberingSelect = select ?? throw new ArgumentException("select");
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			PXCache bindingCache = sender.Graph.Caches[BqlCommand.GetItemType(Mask)]; // Initialize cache in advance to allow DimensionSelector from GuesCustomer fire events on persisting
			sender.Graph.RowPersisting.AddHandler(_BqlTable, PrioritizedRowPersisting);
		}

		public void PrioritizedRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Insert)
				return;

			BCAPISyncScope.BCSyncScopeContext context = BCAPISyncScope.GetScoped();
			if (context == null) return;

			PXView view = new PXView(sender.Graph, true, BqlCommand.CreateInstance(NumberingSelect));
			Object store = view.SelectSingle(context.ConnectorType, context.Binding);

			String mask = (String)sender.Graph.Caches[BqlCommand.GetItemType(Mask)]
				.GetValue(PXResult.Unwrap(store, BqlCommand.GetItemType(Mask)), Mask.Name);
			String numbering = (String)sender.Graph.Caches[BqlCommand.GetItemType(Numbering)]
				.GetValue(PXResult.Unwrap(store, BqlCommand.GetItemType(Numbering)), Numbering.Name);

			Int32 index = 0;
			Int32 segment = -1;
			for (int i = 0; i < BCDimensionMaskAttribute.GetSegments(Dimension).Count(); i++)
			{
				PXSegment seg = BCDimensionMaskAttribute.GetSegments(Dimension).ElementAt(i);

				if (mask == null || mask.Substring(index, seg.Length).StartsWith("#"))
				{
					segment = i + 1;
					break;
				}

				index += seg.Length;
			}

			if (mask != null) sender.SetValue(e.Row, _FieldOrdinal, mask);
			if (numbering != null)
			{
				String newSymbol = null;
				AutoNumberAttribute.Numberings allNumberings = PXDatabase.GetSlot<AutoNumberAttribute.Numberings>(typeof(AutoNumberAttribute.Numberings).Name, typeof(Numbering));
				if (!allNumberings.GetNumberings().TryGetValue(numbering, out newSymbol) || String.IsNullOrEmpty(newSymbol))
					newSymbol = "<NEW>";

				PXDimensionAttribute.SetCustomNumbering(sender, _FieldName, numbering, segment < 0 ? (int?)null : (int?)segment, newSymbol);
			}
		}
	}
	#endregion

	#region BCAutoNumberAttribute
	public class BCAutoNumberAttribute : AutoNumberAttribute
	{
		public BCAutoNumberAttribute(Type setupField, Type dateField)
			: base(null, dateField, new string[] { }, new Type[] { setupField })
		{
		}

		public static void CheckAutoNumbering(PXGraph graph, string numberingID)
		{
			Numbering numbering = null;

			if (numberingID != null)
			{
				numbering = (Numbering)PXSelect<Numbering,
								Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>>
								.Select(graph, numberingID);
			}

			if (numbering == null)
			{
				throw new PXSetPropertyException(PX.Objects.CS.Messages.NumberingIDNull);
			}

			if (numbering.UserNumbering == true)
			{
				throw new PXSetPropertyException(PX.Objects.CS.Messages.CantManualNumber, numbering.NumberingID);
			}
		}
	}
	#endregion
	#region SalesCategoriesAttribute
	public class SalesCategoriesAttribute : PXStringListAttribute
	{
		public SalesCategoriesAttribute() : base(new string[] { }, new string[] { })
		{
			MultiSelect = true;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			Tuple<String, String>[] _Values = BCCategorySlot.GetCachedCategories().Categories;

			_AllowedValues = _Values.Select(t => t.Item1).ToArray();
			_AllowedLabels = _Values.Select(t => t.Item2).ToArray();
		}

		public class BCCategorySlot : IPrefetchable
		{
			public const string SLOT = "BCCategorySlot";

			public Tuple<String, String>[] Categories = new Tuple<string, string>[0];

			public void Prefetch()
			{
				Categories = new Tuple<string, string>[0];
				Categories = PXDatabase.SelectMulti<INCategory>(new PXDataField<INCategory.categoryID>(), new PXDataField<INCategory.description>()).Select(i =>
				{
					return Tuple.Create(i.GetInt32(0).ToString(), i.GetString(1));
				}).ToArray();
			}
			public static BCCategorySlot GetCachedCategories()
			{
				return PXDatabase.GetSlot<BCCategorySlot>(BCCategorySlot.SLOT, typeof(INCategory));
			}
		}
	}
	#endregion
	#region MultipleOrderTypeAttribute
	public class MultipleOrderTypeAttribute : PXStringListAttribute
	{
		protected Tuple<String, String>[] _Values = new Tuple<string, string>[0];

		public MultipleOrderTypeAttribute() : base(new string[] { }, new string[] { })
		{
			MultiSelect = true;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_Values = BCOrderTypeSlot.GetCachedOrderTypes().OrderTypes;

			_AllowedValues = _Values.Select(t => t.Item1).ToArray();
			_AllowedLabels = _Values.Select(t => t.Item1 + " - " + t.Item2).ToArray();
		}
		public class BCOrderTypeSlot : IPrefetchable
		{
			public const string SLOT = "BCOrderTypeSlot";

			public Tuple<String, String>[] OrderTypes = new Tuple<string, string>[0];

			public void Prefetch()
			{
				OrderTypes = new Tuple<string, string>[0];
				OrderTypes = PXDatabase.SelectMulti<SOOrderType>(new PXDataFieldValue<SOOrderType.active>(true),
					new PXDataField<SOOrderType.orderType>(), new PXDataField<SOOrderType.behavior>(), new PXDataField<SOOrderType.descr>(), new PXDataField<SOOrderType.aRDocType>())
					.Select(r =>
					(
						OrderType: r.GetString(0),
						Behavior: r.GetString(1),
						Descr: r.GetString(2),
						ARDocType: r.GetString(3)
					))
					.Where(b => b.Behavior.IsIn(SOBehavior.IN, SOBehavior.QT) || (b.Behavior.IsIn(SOBehavior.SO, SOBehavior.TR) && b.ARDocType == ARDocType.Invoice))
					.Select(i => Tuple.Create(i.OrderType, i.Descr))
					.OrderBy(x => x.Item1)
					.ToArray();
			}
			public static BCOrderTypeSlot GetCachedOrderTypes()
			{
				return PXDatabase.GetSlot<BCOrderTypeSlot>(BCOrderTypeSlot.SLOT, typeof(SOOrderType));
			}
		}
	}
	#endregion
	#region BCSettingsCheckerAttribute
	public class BCSettingsCheckerAttribute : PXEventSubscriberAttribute
	{
		protected bool _IsMandatory = false;
		protected string[] _AppliedEntities = new string[0];

		public BCSettingsCheckerAttribute(string[] appliedEntities)
		{
			_AppliedEntities = appliedEntities;
		}
		public static bool CheckEntity(PXCache cache, string field, object data, string entity)
		{
			if (data == null)
			{
				cache.SetAltered(field, true);
			}
			foreach (BCSettingsCheckerAttribute attr in cache.GetAttributesOfType<BCSettingsCheckerAttribute>(data, field))
			{
				return attr._AppliedEntities.Contains(entity);
			}

			return false;
		}

		public static bool GetMandatory(PXCache cache, string field, object data)
		{
			if (data == null)
			{
				cache.SetAltered(field, true);
			}
			foreach (BCSettingsCheckerAttribute attr in cache.GetAttributesOfType<BCSettingsCheckerAttribute>(data, field))
			{
				return attr._IsMandatory;
			}

			return false;
		}
		public static void SetMandatory(PXCache cache, string field, object data, bool? value)
		{
			if (data == null)
			{
				cache.SetAltered(field, true);
			}
			foreach (BCSettingsCheckerAttribute attr in cache.GetAttributesOfType<BCSettingsCheckerAttribute>(data, field))
			{
				//if once required, should be required.
				if(!attr._IsMandatory) attr._IsMandatory = value ?? false;
			}
		}
	}
	#endregion

	#region BCMappingDirectionAttribute
	public class BCMappingDirectionAttribute : PXStringListAttribute
	{
		public const string Export = "E";
		public const string Import = "I";

		public BCMappingDirectionAttribute() :
				base(
					new[]
					{
						Export,
						Import
					},
					new[]
					{
						BCCaptions.SyncDirectionExport,
						BCCaptions.SyncDirectionImport
					})
		{ }

		public sealed class export : PX.Data.BQL.BqlString.Constant<export>
		{
			public export() : base(Export)
			{
			}
		}
		public sealed class import : PX.Data.BQL.BqlString.Constant<import>
		{
			public import() : base(Import)
			{
			}
		}
	}
	#endregion

	#region PIIMasterEntityAttribute
	public class PIIMasterEntityAttribute : PXStringListAttribute
	{
		public const string SalesOrder = "SO";
		public const string SalesInvoice = "IN";

		public PIIMasterEntityAttribute() :
				base(
					new[]
					{
						SalesOrder,
						SalesInvoice
					},
					new[]
					{
						BCCaptions.SalesOrder,
						BCCaptions.SalesInvoice
					})
		{ }

		public sealed class salesOrder : PX.Data.BQL.BqlString.Constant<salesOrder>
		{
			public salesOrder() : base(SalesOrder)
			{
			}
		}
		public sealed class salesInvoice : PX.Data.BQL.BqlString.Constant<salesInvoice>
		{
			public salesInvoice() : base(SalesInvoice)
			{
			}
		}
	}
	#endregion

	#region PIIActionAttribute
	public class PIIActionAttribute : PXStringListAttribute
	{
		public const string Pseudonymize = "P";
		public const string Erase = "E";
		public const string Restore = "R";

		public PIIActionAttribute() :
				base(
					new[]
					{
						Pseudonymize,
						Erase,
						Restore
					},
					new[]
					{
						BCCaptions.Pseudonymize,
						BCCaptions.Erase,
						BCCaptions.Restore
					})
		{ }

		public sealed class pseudonymize : PX.Data.BQL.BqlString.Constant<pseudonymize>
		{
			public pseudonymize() : base(Pseudonymize)
			{
			}
		}
		public sealed class erase : PX.Data.BQL.BqlString.Constant<erase>
		{
			public erase() : base(Erase)
			{
			}
		}
		public sealed class restore : PX.Data.BQL.BqlString.Constant<restore>
		{
			public restore() : base(Restore)
			{
			}
		}
	}
	#endregion

	#region BCEncryptPersonalDataAttribute
	public class BCEncryptPersonalDataAttribute : PXRSACryptStringAttribute
	{
		Type _encryptionRequired;
		Type _pseudonymizationStatus;
		public BCEncryptPersonalDataAttribute(Type encryptionRequired, Type pseudonymizationStatus) : base()
		{
			_encryptionRequired = encryptionRequired;
			_pseudonymizationStatus = pseudonymizationStatus;
			IsViewDecrypted = true;
			IsEncryptionRequired = false;
			IsUnicode = true;
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			isEncryptionRequired = false;
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update || sender.IsInsertedUpdatedDeleted)
			{
				isEncryptionRequired = CheckEncryptionRequired(sender, e.Row, _encryptionRequired.Name);
				if (!isEncryptionRequired) sender.BypassAuditFields.Clear();
			}
			base.CommandPreparing(sender, e);
		}
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);
			IsViewDecrypted = true;
			if (e.Row != null && CheckEncryptionRequired(sender, e.Row, _encryptionRequired.Name) == true && e.ReturnValue != null)
			{
				int? pseudonymizationStatus = (sender.GetValue(e.Row, _pseudonymizationStatus.Name) as int?);
				IsViewDecrypted = pseudonymizationStatus == PXPseudonymizationStatusListAttribute.Pseudonymized ? false : true;
			}

		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			IsEncryptionRequired = CheckEncryptionRequired(sender, e.Row, _encryptionRequired.Name);
			base.RowSelecting(sender, e);
			//need to change empty back to null so that while printing empty lines are not displayed
			if (e.Row != null)
			{
				var result = ((string)sender.GetValue(e.Row, _FieldOrdinal));
				if (result == string.Empty)
					sender.SetValue(e.Row, _FieldOrdinal, null);
			}

		}

		public override bool EncryptOnCertificateReplacement(PXCache cache, object row)
		{
			return CheckEncryptionRequired(cache, row, _encryptionRequired.Name);
		}

		protected virtual bool CheckEncryptionRequired(PXCache sender, object dacObject, string fieldName)
		{
			if (sender == null || dacObject == null || string.IsNullOrEmpty(fieldName)) return false;

			return ((bool?)sender.GetValue(dacObject, fieldName) == true);
		}
	}
	#endregion

	#region BCInventoryItemStatus
	public class BCINItemStatus : PX.Objects.IN.INItemStatus
	{
		/// <summary>
		/// Convert the Status Code to Status Description
		/// </summary>
		/// <param name="val">Status Code</param>
		/// <returns>Status Description</returns>
		public static string Convert(String val)
		{
			switch (val)
			{
				case Active: return PX.Objects.IN.Messages.Active;
				case Inactive: return PX.Objects.IN.Messages.Inactive;
				case NoSales: return PX.Objects.IN.Messages.NoSales;
				case NoPurchases: return PX.Objects.IN.Messages.NoPurchases;
				case ToDelete: return PX.Objects.IN.Messages.ToDelete;
				default: return null;
			}
		}
	}
	#endregion

	#region BCCustomerCategoryAttribute
	/// <summary>
	/// Sets a drop down list containing all valid <see cref="Customer.CustomerCategory"/> values as an input field for a DAC field.
	/// </summary>
	public class BCCustomerCategoryAttribute : PXStringListAttribute
	{
		/// <summary>
		/// Code for an individual customer.
		/// </summary>
		public const string IndividualValue = "I";

		/// <summary>
		/// Label for an individual customer.
		/// </summary>
		public const string IndividualLabel = "Individual";

		/// <summary>
		/// Code for a company customer.
		/// </summary>
		public const string OrganizationValue = "O";

		/// <summary>
		/// Label for a company customer.
		/// </summary>
		public const string OrganizationLabel = "Organization";

		/// <summary>
		/// Initializes a new instance with values default values.
		/// </summary>
		public BCCustomerCategoryAttribute() : base(
			new[] { IndividualValue, OrganizationValue },
			new[] { IndividualLabel, OrganizationLabel })
		{ }

		public class organizationCategory : PX.Data.BQL.BqlString.Constant<organizationCategory>
		{
			public organizationCategory() : base(OrganizationValue) {; }
		}

		public class individualCategory : PX.Data.BQL.BqlString.Constant<individualCategory>
		{
			public individualCategory() : base(IndividualValue) {; }
		}
		public static bool IsOrganization(string customerCategory)
		{
			if (string.IsNullOrEmpty(customerCategory))
				return false;

			if (customerCategory == OrganizationLabel || customerCategory == OrganizationValue)
				return true;

			return false;
		}

		public static bool IsIndividual(string customerCategory)
		{
			if (string.IsNullOrEmpty(customerCategory))
				return true;

			if (customerCategory == IndividualLabel || customerCategory == IndividualValue)
				return true;

			return false;
		}
	}
	#endregion

	#region BCRoleListAttribute
	/// <summary>
	/// Sets a drop down list containing all valid Shopify Roles as an input field for a DAC field.
	/// </summary>
	public class BCRoleListAttribute : PXStringListAttribute
	{
		/// <summary>
		/// Code for a Location admin role.
		/// </summary>
		public const string LocationAdminValue = "L";

		/// <summary>
		/// Label for a Location admin role.
		/// </summary>
		public const string LocationAdminLabel = "Admin";

		/// <summary>
		/// Value for a Order Only role.
		/// </summary>
		public const string OrderOnlyValue = "O";

		/// <summary>
		/// Label for a Order Only role.
		/// </summary>
		public const string OrderOnlyLabel = "User";

		/// <summary>
		/// Label for a Shopify location admin role.
		/// </summary>
		public const string ShopifyAdminLabel = "Location admin";

		/// <summary>
		/// Label for a Shopify ordering only role.
		/// </summary>
		public const string ShopifyOrderOnlyLabel = "Ordering only";

		/// <summary>
		/// Initializes a new instance with values default values.
		/// </summary>
		public BCRoleListAttribute() : base(
			new[] { LocationAdminValue, OrderOnlyValue },
			new[] { LocationAdminLabel, OrderOnlyLabel })
		{ }

		public class orderOnlyValue : PX.Data.BQL.BqlString.Constant<orderOnlyValue>
		{
			public orderOnlyValue() : base(OrderOnlyValue) {; }
		}

		public class locationAdminValue : PX.Data.BQL.BqlString.Constant<locationAdminValue>
		{
			public locationAdminValue() : base(LocationAdminValue) {; }
		}

		/// <summary>
		/// Convert the external role name to local location role
		/// </summary>
		/// <param name="externRoleName">The external role name</param>
		/// <param name="convertToLabel">If true, convert to local role label; otherwise convert to local role value</param>
		/// <returns>The local role value or role label</returns>
		/// <exception cref="PXException"></exception>
		public static string ConvertExternRoleToLocal(string externRoleName, bool convertToLabel = true)
		{
			switch (externRoleName?.Trim())
			{
				case ShopifyAdminLabel: return convertToLabel ? LocationAdminLabel : LocationAdminValue;
				case ShopifyOrderOnlyLabel: return convertToLabel ? OrderOnlyLabel : OrderOnlyValue;
				default:
					throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.CompanyRoleAssignmentNotSupported, $"{externRoleName}", $"{ShopifyAdminLabel}, {ShopifyOrderOnlyLabel}"));
			}
		}

		/// <summary>
		/// Convert the local location role name to external role
		/// </summary>
		/// <param name="localRoleName">The local location role name</param>
		/// <returns>The external role label</returns>
		/// <exception cref="PXException"></exception>
		public static string ConvertLocalRoleToExtern(string localRoleName)
		{
			switch (localRoleName?.Trim())
			{
				case LocationAdminValue:
				case LocationAdminLabel: return ShopifyAdminLabel;
				case OrderOnlyValue:
				case OrderOnlyLabel: return ShopifyOrderOnlyLabel;
				default:
					throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.CompanyRoleAssignmentNotSupported, $"{localRoleName}", $"{LocationAdminLabel}, {OrderOnlyLabel}"));
			}
		}
	}
	#endregion

}
