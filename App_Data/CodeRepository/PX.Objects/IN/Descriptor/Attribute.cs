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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using PX.Api;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.SQLTree;

using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.Common.Bql;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.SO;

namespace PX.Objects.IN
{
	public class CommonSetupDecPl : IPrefetchable
	{
		protected int _Qty = CommonSetup.decPlQty.Default;
		protected int _PrcCst = CommonSetup.decPlPrcCst.Default;
		void IPrefetchable.Prefetch()
		{
			using (PXDataRecord record = PXDatabase.SelectSingle<CommonSetup>(
				new PXDataField(typeof(CommonSetup.decPlQty).Name),
				new PXDataField(typeof(CommonSetup.decPlPrcCst).Name)))
			{
				if (record != null)
				{
					_Qty = (int)record.GetInt16(0);
					_PrcCst = (int)record.GetInt16(1);
				}
			}
		}

		public static int Qty
		{
			get
			{
				CommonSetupDecPl definition = PXDatabase.GetSlotWithContextCache<CommonSetupDecPl>(typeof(CommonSetupDecPl).Name, typeof(CommonSetup));
				return definition?._Qty ?? CommonSetup.decPlQty.Default;
			}
		}

		public static int PrcCst
		{
			get
			{
				CommonSetupDecPl definition = PXDatabase.GetSlot<CommonSetupDecPl>(typeof(CommonSetupDecPl).Name, typeof(CommonSetup));
				if (definition != null)
				{
					return definition._PrcCst;
				}
				return CommonSetup.decPlPrcCst.Default;
			}
		}
	}

	public class PXUnitConversionException : PXSetPropertyException
	{
		public PXUnitConversionException()
			: base(Messages.MissingUnitConversion)
		{
		}

		public PXUnitConversionException(string UOM)
			: base(Messages.MissingUnitConversionVerbose, UOM)
		{
		}

		public PXUnitConversionException(SerializationInfo info, StreamingContext context)
			: base(info, context){}

		public PXUnitConversionException(string from, string to)
			: base(Messages.ConversionNotFound, from, to)
		{
		}
	}

	/// <summary>
	/// This exception type raised when Unit of Inventory Item is indivisible(<see cref="InventoryItem.DecimalBaseUnit"/>, <see cref="InventoryItem.DecimalSelesUnit"/> or <see cref="InventoryItem.DecimalPurchaseUnit"/> is set <c>false</c>)
	/// and the entered value is non integer
	/// </summary>
	public class PXNotDecimalUnitException: PXSetPropertyException
	{
		private static string GetMessageFormat(InventoryUnitType unitType)
		{
			switch(unitType)
			{
				case InventoryUnitType.BaseUnit:
					return Messages.NotDecimalBaseUnit;
				case InventoryUnitType.SalesUnit:
					return Messages.NotDecimalSalesUnit;
				case InventoryUnitType.PurchaseUnit:
					return Messages.NotDecimalPurchaseUnit;
				default:
					throw new ArgumentOutOfRangeException(nameof(unitType));
			}
		}

		public bool IsLazyThrow { get; set; }

		public PXNotDecimalUnitException(InventoryUnitType unitType, string inventoryCD, string unitID, PXErrorLevel errorLevel)
			: base(GetMessageFormat(unitType), errorLevel, unitID, inventoryCD)
		{
		}

		public PXNotDecimalUnitException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }
	}

	public interface IQtyPlanned
	{
		bool? Reverse { get; set; }
		decimal? PlanQty { get; set; }
	}

	#region INPrecision

	public enum INPrecision
	{
		NOROUND = 0,
		QUANTITY = 1,
		UNITCOST = 2
	}

	#endregion

	#region INMidpointRounding

	public enum INMidpointRounding
	{
		ROUND = 0,
		FLOOR = 1
	}

	#endregion

    #region INPlanLevel

    public class INPlanLevel
    {
        public const int Site = 0;
        public const int Location = 1;
        public const int LotSerial = 2;
        public const int LocationLotSerial = Location | LotSerial;

		public const int ExcludeSite = 1 << 16;
		public const int ExcludeLocation = 1 << 17;
		public const int ExcludeLotSerial = 1 << 18;
		public const int ExcludeSiteLotSerial = ExcludeSite | ExcludeLotSerial;
		public const int ExcludeLocationLotSerial = ExcludeLocation | ExcludeLotSerial;
    }
    #endregion

	#region PXDBCostScalarAttribute
	public class PXDBCostScalarAttribute : PXDBScalarAttribute
	{
		public PXDBCostScalarAttribute(Type search)
			: base(search)
		{
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			base.RowSelecting(sender, e);

			if (sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, 0m);
			}
		}
	}
	#endregion

	#region PXExistance

	public class PXExistance : PXBoolAttribute, IPXRowSelectingSubscriber
	{
		#region Implementation
		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			foreach (string key in sender.Keys)
			{
				if (sender.GetValue(e.Row, key) == null)
				{
					return;
				}
			}
			sender.SetValue(e.Row, _FieldOrdinal, true);
		}
		#endregion
	}

	#endregion

    #region PXNonExistence

    public class PXNonExistence : PXBoolAttribute, IPXRowSelectingSubscriber
    {
        #region Implementation
        public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            foreach (string key in sender.Keys)
            {
                if (sender.GetValue(e.Row, key) != null)
                {
                    return;
                }
            }
            sender.SetValue(e.Row, _FieldOrdinal, true);
        }
        #endregion
    }

    #endregion

    #region INUnitType

    public class INUnitType
	{
		public const short Global = 3;
		public const short ItemClass = 2;
		public const short InventoryItem = 1;

		public class global : PX.Data.BQL.BqlShort.Constant<global>
		{
			public global() : base(Global) { ;}
		}
		public class itemClass : PX.Data.BQL.BqlShort.Constant<itemClass>
		{
			public itemClass() : base(ItemClass) { ;}
		}
		public class inventoryItem : PX.Data.BQL.BqlShort.Constant<inventoryItem>
		{
			public inventoryItem() : base(InventoryItem) { ;}
		}
	}

	#endregion

	#region Conversion info

	public class ConversionInfo
	{
		public INUnit Conversion { get; }

		public short Type
		{
			get { return Conversion.UnitType ?? 0; }
		}

		public InventoryItem Inventory { get; }
		
		public ConversionInfo(INUnit conversion)
		{
			Conversion = conversion;
		}

		public ConversionInfo(INUnit conversion, InventoryItem inventory):this(conversion)
		{
			Inventory = inventory;
		}
	}

	/// <summary>
	/// Type of unit for <see cref="InventoryItem">InventoryItem<see/>.
	/// Corresponds to type(purpose) of units for InventoryItem as <see cref="InventoryItem.BaseUnit"/>, <see cref="InventoryItem.SalesUnit"/> or <see cref="InventoryItem.PurchaseUnit"/>
	/// </summary>
	[Flags]
	public enum InventoryUnitType : byte
	{
		/// <summary>
		/// Default(unknown) type of unit
		/// </summary>
		None = 0,
		/// <summary>
		/// Corresponds to unit which set as <see cref="InventoryItem.BaseUnit"/>
		/// </summary>
		BaseUnit = 1,
		/// <summary>
		/// Corresponds to unit which set as <see cref="InventoryItem.SalesUnit"/>
		/// </summary>
		SalesUnit = 2,
		/// <summary>
		/// Corresponds to unit which set as <see cref="InventoryItem.PurchaseUnit"/>
		/// </summary>
		PurchaseUnit = 4
	}

	#endregion

	#region PXEntityAttribute
	[Obsolete("Prefer to use the PX.Data.PXEntityAttribute instead")]
	public class AcctSub2Attribute : PXEntityAttribute { }
	#endregion

	#region INAcctSubDefault

	public class INAcctSubDefault
	{
		public class CustomListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues => _AllowedValues;
			public string[] AllowedLabels => _AllowedLabels;

			public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels) : base(AllowedValues, AllowedLabels) {}
			public CustomListAttribute(Tuple<string, string>[] valuesToLabels) : base(valuesToLabels) {}
			}

		public class ClassListAttribute : CustomListAttribute
		{
			public ClassListAttribute() : base(
				new[]
			{
					Pair(MaskItem, Messages.MaskItem),
					Pair(MaskSite, Messages.MaskSite),
					Pair(MaskClass, Messages.MaskClass),
				}) {}
		}

		public class ReasonCodeListAttribute : CustomListAttribute
		{
			public ReasonCodeListAttribute() : base(
				new[]
			{
					Pair(MaskReasonCode, Messages.MaskReasonCode),
					Pair(MaskItem, Messages.MaskItem),
					Pair(MaskSite, Messages.MaskSite),
					Pair(MaskClass, Messages.MaskClass),
				}) {}
		}

        public class POAccrualListAttribute : CustomListAttribute
        {
			public POAccrualListAttribute() : base(
				new[]
            {
					Pair(MaskItem, Messages.MaskItem),
					Pair(MaskSite, Messages.MaskSite),
					Pair(MaskClass, Messages.MaskClass),
					Pair(MaskVendor, Messages.MaskVendor),
				}) {}
        }

		public const string MaskReasonCode = "0";
		public const string MaskItem = "I";
		public const string MaskSite = "W";
		public const string MaskClass = "P";
        public const string MaskVendor = "V";

		public static void Required(PXCache sender, PXRowSelectedEventArgs e)
		{
			AcctSubRequired required = new AcctSubRequired(sender, e);
		}

		public static void Required(PXCache sender, PXRowPersistingEventArgs e)
		{
			AcctSubRequired required = new AcctSubRequired(sender, e);
		}

		public class AcctSubRequired
		{
			protected enum AcctSubDefaultClass { FromItem = 0, FromSite = 1, FromClass = 2 }
			protected enum AcctSubDefaultReasonCode { FromReasonCode = 0, FromItem = 1, FromSite = 2, FromClass = 3 }

			#region State
			public bool InvtAcct = false;
			public bool InvtSub = false;
			public bool SalesAcct = false;
			public bool SalesSub = false;
			public bool COGSAcct = false;
			public bool COGSSub = false;
			public bool ReasonCodeSub = false;
			public bool StdCstVarAcct = false;
			public bool StdCstVarSub = false;
			public bool StdCstRevAcct = false;
			public bool StdCstRevSub = false;
			public bool POAccrualAcct = false;
			public bool POAccrualSub = false;

			protected string[] _sources = new ClassListAttribute().AllowedValues;
			protected string[] _rcsources = new ReasonCodeListAttribute().AllowedValues;
			#endregion

			#region Initialization
			protected virtual void Populate(INPostClass postclass, AcctSubDefaultClass option)
			{
				if (postclass != null)
				{
					InvtAcct = InvtAcct || (postclass.InvtAcctDefault == _sources[(int)option]);
					InvtSub = InvtSub || (string.IsNullOrEmpty(postclass.InvtSubMask) == false && postclass.InvtSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);

					SalesAcct = SalesAcct || (postclass.SalesAcctDefault == _sources[(int)option]);
					SalesSub = SalesSub || (string.IsNullOrEmpty(postclass.SalesSubMask) == false && postclass.SalesSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);

					COGSAcct = COGSAcct || (postclass.COGSAcctDefault == _sources[(int)option]);
					COGSSub = COGSSub || (postclass.COGSSubFromSales == false && string.IsNullOrEmpty(postclass.COGSSubMask) == false && postclass.COGSSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);

					StdCstVarAcct = StdCstVarAcct || (postclass.StdCstVarAcctDefault == _sources[(int)option]);
					StdCstVarSub = StdCstVarSub || (string.IsNullOrEmpty(postclass.StdCstVarSubMask) == false && postclass.StdCstVarSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);

					StdCstRevAcct = StdCstRevAcct || (postclass.StdCstRevAcctDefault == _sources[(int)option]);
					StdCstRevSub = StdCstRevSub || (string.IsNullOrEmpty(postclass.StdCstRevSubMask) == false && postclass.StdCstRevSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);

					POAccrualAcct = POAccrualAcct || (postclass.POAccrualAcctDefault == _sources[(int)option]);
					POAccrualSub = POAccrualSub || (string.IsNullOrEmpty(postclass.POAccrualSubMask) == false && postclass.POAccrualSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);
				}
			}

			protected virtual void Populate(ReasonCode reasoncode, AcctSubDefaultReasonCode option)
			{
				if (reasoncode != null)
				{
					ReasonCodeSub = ReasonCodeSub || (string.IsNullOrEmpty(reasoncode.SubMask) == false && reasoncode.SubMask.IndexOf(char.Parse(_rcsources[(int)option])) > -1);
				}
			}
			#endregion

			#region Ctor
			public AcctSubRequired(PXCache sender, object data)
			{
				if (sender.GetItemType() == typeof(InventoryItem))
				{
					PXCache cache = sender.Graph.Caches[typeof(INPostClass)];
					Populate((INPostClass)cache.Current, AcctSubDefaultClass.FromItem);

					StdCstVarAcct = StdCstVarAcct && (data != null && ((InventoryItem)data).ValMethod == INValMethod.Standard);
                    StdCstVarSub = StdCstVarSub && (data != null && ((InventoryItem)data).ValMethod == INValMethod.Standard);
                    StdCstRevAcct = StdCstRevAcct && (data != null && ((InventoryItem)data).ValMethod == INValMethod.Standard);
                    StdCstRevSub = StdCstRevSub && (data != null && ((InventoryItem)data).ValMethod == INValMethod.Standard);

					foreach (ReasonCode reasoncode in PXSelectReadonly<ReasonCode, Where<ReasonCode.usage, NotEqual<ReasonCodeUsages.sales>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.creditWriteOff>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.balanceWriteOff>>>>>.Select(sender.Graph))
					{
						Populate(reasoncode, AcctSubDefaultReasonCode.FromItem);
					}
				}

				else if (sender.GetItemType() == typeof(INPostClass))
				{
					//class accounts are used for defaulting, combine requirements.
					Populate((INPostClass)data, AcctSubDefaultClass.FromClass);

					foreach (ReasonCode reasoncode in PXSelectReadonly<ReasonCode, Where<ReasonCode.usage, NotEqual<ReasonCodeUsages.sales>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.creditWriteOff>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.balanceWriteOff>>>>>.Select(sender.Graph))
					{
						Populate(reasoncode, AcctSubDefaultReasonCode.FromClass);
					}
				}

				else if (sender.GetItemType() == typeof(INSite) && PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
				{
					foreach (INPostClass postclass in PXSelectReadonly<INPostClass>.Select(sender.Graph))
					{
						Populate(postclass, AcctSubDefaultClass.FromSite);
					}

					foreach (ReasonCode reasoncode in PXSelectReadonly<ReasonCode, Where<ReasonCode.usage, NotEqual<ReasonCodeUsages.sales>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.creditWriteOff>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.balanceWriteOff>>>>>.Select(sender.Graph))
					{
						Populate(reasoncode, AcctSubDefaultReasonCode.FromSite);
					}
				}
			}

			public AcctSubRequired(PXCache sender, PXRowSelectedEventArgs e)
				: this(sender, e.Row)
			{
				OnRowSelected(sender, e);
			}

			public AcctSubRequired(PXCache sender, PXRowPersistingEventArgs e)
				: this(sender, e.Row)
			{
				OnRowPersisting(sender, e);
			}
			#endregion

			#region Implementation
			public virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				PXUIFieldAttribute.SetRequired<INPostClass.invtAcctID>(sender, this.InvtAcct || this.InvtSub);
				PXUIFieldAttribute.SetRequired<INPostClass.invtSubID>(sender, this.InvtSub);
				PXUIFieldAttribute.SetRequired<INPostClass.salesAcctID>(sender, this.SalesAcct || this.SalesSub);
				PXUIFieldAttribute.SetRequired<INPostClass.salesSubID>(sender, this.SalesSub);
				PXUIFieldAttribute.SetRequired<INPostClass.cOGSAcctID>(sender, this.COGSAcct || this.COGSSub);
				PXUIFieldAttribute.SetRequired<INPostClass.cOGSSubID>(sender, this.COGSSub);
				PXUIFieldAttribute.SetRequired<INPostClass.stdCstVarAcctID>(sender, this.StdCstVarAcct || this.StdCstVarSub);
				PXUIFieldAttribute.SetRequired<INPostClass.stdCstVarSubID>(sender, this.StdCstVarSub);
				PXUIFieldAttribute.SetRequired<INPostClass.stdCstRevAcctID>(sender, this.StdCstRevAcct || this.StdCstRevSub);
				PXUIFieldAttribute.SetRequired<INPostClass.stdCstRevSubID>(sender, this.StdCstRevSub);
				PXUIFieldAttribute.SetRequired<INPostClass.pOAccrualAcctID>(sender, this.POAccrualAcct || this.POAccrualSub);
				PXUIFieldAttribute.SetRequired<INPostClass.pOAccrualSubID>(sender, this.POAccrualSub);
				PXUIFieldAttribute.SetRequired<INPostClass.reasonCodeSubID>(sender, this.ReasonCodeSub);
			}

			public void ThrowFieldIsEmpty<Field>(PXCache sender, object data)
				where Field : IBqlField
			{
				if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{typeof(Field).Name}]")))
				{
					throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, typeof(Field).Name);
				}
			}

			public virtual void OnRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
			{
				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
				{
					if (this.InvtAcct && sender.GetValue<INPostClass.invtAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.invtAcctID>(sender, e.Row);
					}
					if (this.InvtSub && sender.GetValue<INPostClass.invtSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.invtSubID>(sender, e.Row);
					}
					if (this.SalesAcct && sender.GetValue<INPostClass.salesAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.salesAcctID>(sender, e.Row);
					}
					if (this.SalesSub && sender.GetValue<INPostClass.salesSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.salesSubID>(sender, e.Row);
					}
					if (this.COGSAcct && sender.GetValue<INPostClass.cOGSAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.cOGSAcctID>(sender, e.Row);
					}
					if (this.COGSSub && sender.GetValue<INPostClass.cOGSSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.cOGSSubID>(sender, e.Row);
					}
					if (this.StdCstVarAcct && sender.GetValue<INPostClass.stdCstVarAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.stdCstVarAcctID>(sender, e.Row);
					}
					if (this.StdCstVarSub && sender.GetValue<INPostClass.stdCstVarSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.stdCstVarSubID>(sender, e.Row);
					}
					if (this.StdCstRevAcct && sender.GetValue<INPostClass.stdCstRevAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.stdCstRevAcctID>(sender, e.Row);
					}
					if (this.StdCstRevSub && sender.GetValue<INPostClass.stdCstRevSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.stdCstRevSubID>(sender, e.Row);
					}
					if (this.POAccrualAcct && sender.GetValue<INPostClass.pOAccrualAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.pOAccrualAcctID>(sender, e.Row);
					}
					if (this.POAccrualSub && sender.GetValue<INPostClass.pOAccrualSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.pOAccrualSubID>(sender, e.Row);
					}
					if (this.ReasonCodeSub && sender.GetValue<INPostClass.reasonCodeSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.reasonCodeSubID>(sender, e.Row);
					}
				}
			}
			#endregion
		}
	}

	#endregion

	#region SubAccountMaskAttribute

	[PXDBString(30, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class SubAccountMaskAttribute : PXEntityAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ZZZZZZZZZZ";

		public SubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, INAcctSubDefault.MaskItem, new INAcctSubDefault.ClassListAttribute().AllowedValues, new INAcctSubDefault.ClassListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, params object[] sources)
			where Field : IBqlField
		{
			return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new INAcctSubDefault.ClassListAttribute().AllowedValues, sources);
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			if (string.IsNullOrEmpty(mask))
			{
				object newval;
				graph.Caches[BqlCommand.GetItemType(typeof(Field))].RaiseFieldDefaulting<Field>(null, out newval);
				mask = (string)newval;
			}

			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new INAcctSubDefault.ClassListAttribute().AllowedValues, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(ex, new INAcctSubDefault.ClassListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, bool? stkItem, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			if (string.IsNullOrEmpty(mask))
			{
				object newval;
				graph.Caches[BqlCommand.GetItemType(typeof(Field))].RaiseFieldDefaulting<Field>(null, out newval);
				mask = (string)newval;
			}

			try
			{
					return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, stkItem, new INAcctSubDefault.ClassListAttribute().AllowedValues, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(ex, new INAcctSubDefault.ClassListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	#endregion

    #region POAccrualSubAccountMaskAttribute

    [PXDBString(30, InputMask = "")]
    [PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
    public sealed class POAccrualSubAccountMaskAttribute : PXEntityAttribute
    {
        private const string _DimensionName = "SUBACCOUNT";
				private const string _MaskName = "ZZZZZZZZZX";
        public POAccrualSubAccountMaskAttribute()
            : base()
        {
            PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, INAcctSubDefault.MaskItem, new INAcctSubDefault.POAccrualListAttribute().AllowedValues, new INAcctSubDefault.POAccrualListAttribute().AllowedLabels);
            attr.ValidComboRequired = false;
            _Attributes.Add(attr);
            _SelAttrIndex = _Attributes.Count - 1;
        }

        public static string MakeSub<Field>(PXGraph graph, string mask, params object[] sources)
            where Field : IBqlField
        {
            return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new INAcctSubDefault.POAccrualListAttribute().AllowedValues, sources);
        }

        public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
            where Field : IBqlField
        {
            if (string.IsNullOrEmpty(mask))
            {
                object newval;
                graph.Caches[typeof(Field).DeclaringType].RaiseFieldDefaulting<Field>(null, out newval);
                mask = (string)newval;
            }

            try
            {
                return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new INAcctSubDefault.POAccrualListAttribute().AllowedValues, sources);
            }
            catch (PXMaskArgumentException ex)
            {
                PXCache cache = graph.Caches[fields[ex.SourceIdx].DeclaringType];
                string fieldName = fields[ex.SourceIdx].Name;
                throw new PXMaskArgumentException(ex, new INAcctSubDefault.POAccrualListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
            }
        }
    }

    #endregion

	#region ReasonCodeSubAccountMaskAttribute

	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class ReasonCodeSubAccountMaskAttribute : PXEntityAttribute
	{
		public const string ReasonCode = "R";
		public const string InventoryItem = "I";
		public const string PostingClass = "P";
		public const string Warehouse = "W";

		private static readonly string[] writeOffValues = new string[] { ReasonCode, InventoryItem, Warehouse, PostingClass };
		private static readonly string[] writeOffLabels = new string[] { CS.Messages.ReasonCode, Messages.InventoryItem, Messages.Warehouse, Messages.PostingClass };

		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ReasonCodeIN";
		public ReasonCodeSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, ReasonCode, writeOffValues, writeOffLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, writeOffValues, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(writeOffLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	#endregion

	#region ILSDetail

	public interface ILSDetail : ILSMaster
	{
		Int32? SplitLineNbr
		{
			get;
			set;
		}
		string AssignedNbr
		{
			get;
			set;
		}
		string LotSerClassID
		{
			get;
			set;
		}
		bool? IsStockItem
		{
			get;
			set;
		}
	}

	public interface ILSGeneratedDetail
	{
		bool? HasGeneratedLotSerialNbr
		{
			get;
			set;
		}
	}

	#endregion

	#region ILSPrimary
	public interface ILSPrimary : ILSMaster
	{
		decimal? UnassignedQty
		{
			get;
			set;
		}
		bool? HasMixedProjectTasks { get; set;  }
		bool? IsStockItem { set; }
		int? CostCenterID { get; }
	}

	public interface ILSTransferPrimary : ILSPrimary
	{
		string OrigRefNbr
		{
			get;
		}
		Int32? OrigLineNbr
		{
			get;
		}
		decimal? MaxTransferBaseQty
		{
			get;
		}
	}
	#endregion

	#region ILSMaster

	public interface ILSMaster : IItemPlanMaster
	{
		string TranType
		{
			get;
		}
		DateTime? TranDate 
		{ 
			get; 
		}
		Int16? InvtMult
		{
			get;
			set;
		}
	    new Int32? InventoryID
		{
			get;
			set;
		}
	    new Int32? SiteID
		{
			get;
			set;
		}
		Int32? LocationID
		{
			get;
			set;
		}
		Int32? SubItemID
		{
			get;
			set;
		}
		string LotSerialNbr
		{
			get;
			set;
		}
		DateTime? ExpireDate
		{
			get;
			set;
		}
		string UOM
		{
			get;
			set;
		}
		Decimal? Qty
		{
			get;
			set;
		}
		decimal? BaseQty
		{
			get;
			set;
		}
		int? ProjectID { get; set; }
		int? TaskID { get; set; }

		bool? IsIntercompany { get; }
	}

	#endregion

	#region INExpireDateAttribute

	[PXDBDate(InputMask = "d", DisplayMask = "d")]
	[PXUIField(DisplayName = "Expiration Date", FieldClass="LotSerial")]
	[PXDefault()]
	public class INExpireDateAttribute : PXEntityAttribute, IPXFieldSelectingSubscriber, IPXRowSelectedSubscriber, IPXFieldDefaultingSubscriber, IPXRowPersistingSubscriber
	{
		protected Type _InventoryType;

		public virtual bool ForceDisable
		{
			get;
			set;
		}

		public INExpireDateAttribute(Type InventoryType)
			:base()
		{
			_InventoryType = InventoryType;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			var itemType = sender.GetItemType();
			if (!typeof(ILSMaster).IsAssignableFrom(itemType))
			{
				throw new PXArgumentException(nameof(itemType), Messages.TypeMustImplementInterface, sender.GetItemType().GetLongName(), typeof(ILSMaster).GetLongName());
			}
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXFieldDefaultingSubscriber) || typeof(ISubscriber) == typeof(IPXRowPersistingSubscriber))
			{
				subscribers.Add(this as ISubscriber);
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}

		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXCache.TryDispose(sender.GetAttributes(e.Row, _FieldName));
			if (PXGraph.ProxyIsActive)
			{
				sender.SetAltered(_FieldName, true);
			}
		}

		protected virtual PXResult<InventoryItem, INLotSerClass> ReadInventoryItem(PXCache sender, int? InventoryID)
		{
			InventoryItem item = InventoryItem.PK.Find(sender.Graph, InventoryID);

			if (item != null)
			{
				INLotSerClass lsclass = INLotSerClass.PK.Find(sender.Graph, item.LotSerClassID);

				return new PXResult<InventoryItem, INLotSerClass>(item, lsclass ?? new INLotSerClass());
			}

			return null;
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				((PXUIFieldAttribute) _Attributes[_UIAttrIndex]).Enabled = IsFieldEnabled(sender, (ILSMaster)e.Row);
			}
		}

		protected virtual bool IsFieldEnabled(PXCache sender, ILSMaster row)
		{
			return sender.AllowUpdate && IsTrackExpiration(sender, row) && !ForceDisable;
		}

		protected virtual bool IsTrackExpiration(PXCache sender, ILSMaster row)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);

			if (item == null) return false;

			return INLotSerialNbrAttribute.IsTrackExpiration(item, row.TranType, row.InvtMult);
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (((ILSMaster)e.Row).SubItemID == null || ((ILSMaster)e.Row).LocationID == null) return;

			if (IsTrackExpiration(sender, (ILSMaster)e.Row) && ((ILSMaster)e.Row).BaseQty != 0m)
			{							
				((IPXRowPersistingSubscriber)_Attributes[_DefAttrIndex]).RowPersisting(sender, e);
			}
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			((IPXFieldDefaultingSubscriber)_Attributes[_DefAttrIndex]).FieldDefaulting(sender, e);
		}
	}

	#endregion

	#region PXEmptyAutoIncValueException

	public class PXEmptyAutoIncValueException : PXException
	{
		public PXEmptyAutoIncValueException(string Source)
			: base(Messages.EmptyAutoIncValue, Source)
		{
		}

		public PXEmptyAutoIncValueException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			PXReflectionSerializer.RestoreObjectProps(this, info);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			PXReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}

	}

	#endregion

	#region LotSerialNbrAttribute

	[PXDBString(INLotSerialStatusByCostCenter.lotSerialNbr.Length, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial")]
	public class LotSerialNbrAttribute : PXAggregateAttribute
	{

		protected int _DBAttrIndex = -1;

		public LotSerialNbrAttribute()
		{
			foreach (PXEventSubscriberAttribute attr in _Attributes)
			{
				if (attr is PXDBFieldAttribute)
				{
					_DBAttrIndex = _Attributes.IndexOf(attr);
					break;
				}
			}
		}

		public virtual bool IsKey
		{
			get
			{
				return ((PXDBStringAttribute)_Attributes[_DBAttrIndex]).IsKey;
			}
			set
			{
				((PXDBStringAttribute)_Attributes[_DBAttrIndex]).IsKey = value;
			}
		}

		public virtual Type BqlField
		{
			get
			{
				return ((PXDBStringAttribute)_Attributes[_DBAttrIndex]).BqlField;
			}
			set
			{
				var dbAttribute = ((PXDBStringAttribute)_Attributes[_DBAttrIndex]);
				dbAttribute.BqlField = value;
				BqlTable = dbAttribute.BqlTable;
			}
		}
	}

	#endregion

	#region INLotSerialNbrAttribute

	[PXDBString(INLotSerialStatusByCostCenter.lotSerialNbr.Length, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial")]
	[PXDefault("")]
	public class INLotSerialNbrAttribute : PXEntityAttribute, IPXFieldVerifyingSubscriber, IPXFieldDefaultingSubscriber, IPXRowPersistingSubscriber, IPXFieldSelectingSubscriber, IPXRowSelectedSubscriber
	{
		private const string _NumFormatStr = "{0}";

		protected Type _InventoryType;
		protected Type _SubItemType;
		protected Type _LocationType;
		protected Type _CostCenterType;

		public virtual bool ForceDisable
		{
			get;
			set;
		}

		public INLotSerialNbrAttribute(){}

		public INLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType, Type CostCenterType)
            : base()
        {
			var itemType = BqlCommand.GetItemType(InventoryType);
			if (!typeof(ILSMaster).IsAssignableFrom(itemType))
            {
				throw new PXArgumentException(nameof(itemType), Messages.TypeMustImplementInterface, itemType.GetLongName(), typeof(ILSMaster).GetLongName());
            }

            _InventoryType = InventoryType;
            _SubItemType = SubItemType;
            _LocationType = LocationType;
			_CostCenterType = CostCenterType;

			InitializeSelector(GetLotSerialSearch(InventoryType, SubItemType, LocationType, CostCenterType),
				typeof(INLotSerialStatusByCostCenter.lotSerialNbr),
				typeof(INLotSerialStatusByCostCenter.siteID),
				typeof(INLotSerialStatusByCostCenter.locationID),
				typeof(INLotSerialStatusByCostCenter.qtyOnHand),
				typeof(INLotSerialStatusByCostCenter.qtyAvail),
				typeof(INLotSerialStatusByCostCenter.expireDate));
		}

		public INLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType, Type ParentLotSerialNbrType, Type CostCenterType)
	            : this(InventoryType, SubItemType, LocationType, CostCenterType)
		{
			InitializeDefault(ParentLotSerialNbrType);
		}

		protected virtual Type GetLotSerialSearch(Type inventoryType, Type subItemType, Type locationType, Type costCenterType)
		{
			var costCenterExpression = typeof(IConstant).IsAssignableFrom(costCenterType) ? costCenterType :
				BqlTemplate.FromType(typeof(Optional<BqlPlaceholder.D>)).Replace<BqlPlaceholder.D>(costCenterType).ToType();

			return BqlTemplate.OfCommand<
				Search<INLotSerialStatusByCostCenter.lotSerialNbr,
				Where<INLotSerialStatusByCostCenter.inventoryID, Equal<Optional<BqlPlaceholder.A>>,
					And<INLotSerialStatusByCostCenter.subItemID, Equal<Optional<BqlPlaceholder.B>>,
					And<INLotSerialStatusByCostCenter.locationID, Equal<Optional<BqlPlaceholder.C>>,
					And<INLotSerialStatusByCostCenter.costCenterID, Equal<BqlPlaceholder.D>,
					And<INLotSerialStatusByCostCenter.qtyOnHand, Greater<decimal0>>>>>>>
				>
				.Replace<BqlPlaceholder.A>(inventoryType)
				.Replace<BqlPlaceholder.B>(subItemType)
				.Replace<BqlPlaceholder.C>(locationType)
				.Replace<BqlPlaceholder.D>(costCenterExpression)
				.ToType();
		}

		protected virtual Type GetIntransitLotSerialSearch(Type inventoryType, Type subItemType, Type locationType, Type costCenterType, Type tranType,
			Type transferNbrType, Type transferLineNbrType)
		{
			var costCenterExpression = typeof(IConstant).IsAssignableFrom(costCenterType) ? costCenterType :
				BqlTemplate.FromType(typeof(Optional<BqlPlaceholder.G>)).Replace<BqlPlaceholder.G>(costCenterType).ToType();

			return BqlTemplate.OfCommand<
				Search2<INLotSerialStatusByCostCenter.lotSerialNbr,
				LeftJoin<INTransitLine,
					On<INTransitLine.costSiteID, Equal<INLotSerialStatusByCostCenter.locationID>,
						And<Optional<BqlPlaceholder.A>, Equal<INTranType.transfer>>>>,
				Where<INLotSerialStatusByCostCenter.inventoryID, Equal<Optional<BqlPlaceholder.B>>,
					And<INLotSerialStatusByCostCenter.subItemID, Equal<Optional<BqlPlaceholder.C>>,
					And<INLotSerialStatusByCostCenter.costCenterID, Equal<BqlPlaceholder.G>,
					And<INLotSerialStatusByCostCenter.qtyOnHand, Greater<decimal0>,
					And<Where<Optional<BqlPlaceholder.A>, NotEqual<INTranType.transfer>,
							And<INLotSerialStatusByCostCenter.locationID, Equal<Optional<BqlPlaceholder.D>>,
						Or<INTransitLine.transferNbr, Equal<Optional<BqlPlaceholder.E>>,
							And<INTransitLine.transferLineNbr, Equal<Optional<BqlPlaceholder.F>>>>>>>>>>>>>
				.Replace<BqlPlaceholder.A>(tranType)
				.Replace<BqlPlaceholder.B>(inventoryType)
				.Replace<BqlPlaceholder.C>(subItemType)
				.Replace<BqlPlaceholder.D>(locationType)
				.Replace<BqlPlaceholder.E>(transferNbrType)
				.Replace<BqlPlaceholder.F>(transferLineNbrType)
				.Replace<BqlPlaceholder.G>(costCenterExpression)
				.ToType();
		}

		protected virtual void InitializeSelector(Type searchType, params Type[] fieldList)
		{
			PXSelectorAttribute attr = new PXSelectorAttribute(searchType, fieldList);
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		protected virtual void InitializeDefault(Type parentLotSerialNbrType)
		{
			_Attributes[_DefAttrIndex] = new PXDefaultAttribute(parentLotSerialNbrType) { PersistingCheck = PXPersistingCheck.NullOrBlank };
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
        {
            if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber) || typeof(ISubscriber) == typeof(IPXFieldDefaultingSubscriber) || typeof(ISubscriber) == typeof(IPXRowPersistingSubscriber))
            {
                subscribers.Add(this as ISubscriber);
            }
            else if (typeof(ISubscriber) == typeof(IPXFieldSelectingSubscriber))
            {
                base.GetSubscriber<ISubscriber>(subscribers);

                subscribers.Remove(this as ISubscriber);
                subscribers.Add(this as ISubscriber);
                subscribers.Reverse();
            }
            else
            {
                base.GetSubscriber<ISubscriber>(subscribers);
            }
        }

        public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
            if (item == null || ((ILSMaster)e.Row).SubItemID == null || ((ILSMaster)e.Row).LocationID == null)
            {
                return;
            }

			if (((INLotSerClass)item).LotSerTrack == INLotSerTrack.NotNumbered &&
				!string.IsNullOrEmpty((string)e.NewValue) && ((ILSMaster)e.Row).InvtMult != 0)
			{
				RaiseFieldIsDisabledException();
			}

			decimal QtyValue = ((ILSMaster)e.Row).Qty.GetValueOrDefault();
			object pendingValue = sender.GetValuePending(e.Row, nameof(ILSMaster.Qty));
			if ( pendingValue != null && pendingValue  != PXCache.NotSetValue)
			{
				string strValue = pendingValue.ToString();//pending value can be both either decimal or it's string representation.
				decimal.TryParse(strValue, out QtyValue);
			}
			
			bool decreasingStock = ((ILSMaster)e.Row).InvtMult * QtyValue < 0;
			if (((INLotSerClass)item).LotSerTrack != INLotSerTrack.NotNumbered && decreasingStock && ((INLotSerClass)item).LotSerAssign == INLotSerAssign.WhenReceived && string.IsNullOrEmpty((string)e.NewValue) == false)
            {
                ((IPXFieldVerifyingSubscriber)_Attributes[_SelAttrIndex]).FieldVerifying(sender, e);
            }
        }

		protected virtual void RaiseFieldIsDisabledException()
		{
			throw new PXSetPropertyException(ErrorMessages.GIFieldIsDisabled, FieldName);
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
				if (item == null)
				{
					return;
				}

				if ((((INLotSerClass)item).LotSerTrack ?? INLotSerTrack.NotNumbered) == INLotSerTrack.NotNumbered)
				{
					((IPXFieldDefaultingSubscriber)_Attributes[_DefAttrIndex]).FieldDefaulting(sender, e);
				}
			}
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
			if (item == null || ((ILSMaster)e.Row).SubItemID == null || ((ILSMaster)e.Row).LocationID == null)
			{
				return;
			}

			if (IsTracked((ILSMaster)e.Row, item, ((ILSMaster)e.Row).TranType, ((ILSMaster)e.Row).InvtMult) && ((ILSMaster)e.Row).Qty != 0)
			{
				((IPXRowPersistingSubscriber)_Attributes[_DefAttrIndex]).RowPersisting(sender, e);
			}
		}

		protected virtual PXResult<InventoryItem, INLotSerClass> ReadInventoryItem(PXCache sender, int? InventoryID)
		{
			InventoryItem item = InventoryItem.PK.Find(sender.Graph, InventoryID);

			if (item != null)
			{
				INLotSerClass lsclass = INLotSerClass.PK.Find(sender.Graph, item.LotSerClassID);

				return new PXResult<InventoryItem, INLotSerClass>(item, lsclass ?? new INLotSerClass());
			}

			return null;
		}

		protected virtual bool IsTracked(ILSMaster row, INLotSerClass lotSerClass, string tranType, int? invMult)
		{
			if (lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed && invMult < 0 && row.IsIntercompany == true)
			{
				tranType = INTranType.Transfer;
			}
			return INLotSerialNbrAttribute.IsTrack(lotSerClass, tranType, invMult);
		}

		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXCache.TryDispose(sender.GetAttributes(e.Row, _FieldName));
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var master = (ILSMaster)e.Row;
			if (master != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, master.InventoryID);
				((PXUIFieldAttribute) _Attributes[_UIAttrIndex]).Enabled =
					ForceDisable != true &&
					item != null && sender.AllowUpdate &&
					IsTracked(master, item, master.TranType, master.InvtMult);
			}
		}

		public static string MakeFormatStr(PXCache sender, INLotSerClass lsclass)
		{
			StringBuilder format = new StringBuilder();

			if (lsclass != null)
			{
				foreach (INLotSerSegment seg in PXSelect<INLotSerSegment, 
					Where<INLotSerSegment.lotSerClassID, Equal<Required<INLotSerSegment.lotSerClassID>>>, 
					OrderBy<Asc<INLotSerSegment.lotSerClassID, Asc<INLotSerSegment.segmentID>>>>.Select(sender.Graph, lsclass.LotSerClassID))
				{
					switch (seg.SegmentType)
					{
						case INLotSerSegmentType.FixedConst:
							format.Append(seg.SegmentValue);
							break;
						case INLotSerSegmentType.NumericVal:
							format.Append("{1}");
							break;
						case INLotSerSegmentType.DateConst:
							format.Append("{0");
							if (!string.IsNullOrEmpty(seg.SegmentValue))
								format.Append(":").Append(seg.SegmentValue);
							format.Append("}");
							break;
						case INLotSerSegmentType.DayConst:
							format.Append("{0:dd}");
							break;
						case INLotSerSegmentType.MonthConst:
							format.Append("{0:MM}");
							break;
						case INLotSerSegmentType.MonthLongConst:
							format.Append("{0:MMM}");
							break;
						case INLotSerSegmentType.YearConst:
							format.Append("{0:yy}");
							break;
						case INLotSerSegmentType.YearLongConst:
							format.Append("{0:yyyy}");
							break;
						default:
							throw new PXException();
					}
				}
			}
			return format.ToString();
		}

		/// <summary>
		/// Read ILotSerNumVal implemented object which store auto-incremental value
		/// </summary>
		/// <param name="graph">graph</param>
		/// <param name="item">settings</param>
		/// <returns></returns>
		public static ILotSerNumVal ReadLotSerNumVal(PXGraph graph, PXResult<InventoryItem, INLotSerClass> item)
		{
			if (item == null || (INLotSerClass)item == null)
				return null;
			if (((INLotSerClass)item).LotSerNumShared == true)
				return INLotSerClassLotSerNumVal.PK.FindDirty(graph, ((INLotSerClass)item).LotSerClassID);
			return InventoryItemLotSerNumVal.PK.FindDirty(graph, ((InventoryItem)item).InventoryID);
		}

		/// <summary>
		/// Return the length of auto-incremental number
		/// </summary>
		/// <param name="lotSerNum">auto-incremental number value</param>
		/// <returns></returns>
		public static int GetNumberLength(ILotSerNumVal lotSerNum)
			=>  lotSerNum == null || string.IsNullOrEmpty(lotSerNum.LotSerNumVal) ? 6 : lotSerNum.LotSerNumVal.Length;

		/// <summary>
		/// Return default(empty) auto-incremental number value
		/// </summary>
		/// <param name="sender">cache</param>
		/// <param name="lsClass">Lot/Ser class</param>
		/// <param name="lotSerNum">Auto-incremental number value</param>
		/// <returns></returns>
		public static string GetNextNumber(PXCache sender, INLotSerClass lsClass, ILotSerNumVal lotSerNum)
		{
			string numval = new string('0', GetNumberLength(lotSerNum));
			return string.Format(lsClass.LotSerFormatStr, sender.Graph.Accessinfo.BusinessDate, numval).ToUpper();
		}

		/// <summary>
		/// Return  auto-incremental number format
		/// </summary>
		/// <param name="lsClass">Lot/Ser class</param>
		/// <param name="lotSerNum">Auto-incremental number value</param>
		/// <returns></returns>
		public static string GetNextFormat(INLotSerClass lsClass, ILotSerNumVal lotSerNum)
		{
			string numFormat = "{1:" + new string('0', GetNumberLength(lotSerNum)) + "}";
			return lsClass.LotSerFormatStr.Replace("{1}", numFormat);
		}

		/// <summary>
		/// Return shared Lot\Ser Class identifier
		/// </summary>
		/// <param name="lsClass">Lot/Ser class</param>
		/// <returns></returns>
		public static string GetNextClassID(INLotSerClass lsClass)
		{
			return (bool)lsClass.LotSerNumShared
							? lsClass.LotSerClassID
							: null;
		}

		protected class PXUnknownSegmentTypeException: PXException
		{
			public PXUnknownSegmentTypeException():base(Messages.UnknownSegmentType){}

			public PXUnknownSegmentTypeException(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		/// <summary>
		/// Return auto-incremental number mask
		/// </summary>
		/// <param name="sender">cache</param>
		/// <param name="lsClass">Lot/Ser class</param>
		/// <param name="lotSerNum">Auto-incremental number value</param>
		/// <returns></returns>
		public static string GetDisplayMask(PXCache sender, INLotSerClass lsClass, ILotSerNumVal lotSerNum)
		{
			if (lsClass == null)
				return null;
			StringBuilder mask = new StringBuilder();
			foreach (INLotSerSegment seg in PXSelect<INLotSerSegment, 
				Where<INLotSerSegment.lotSerClassID, Equal<Required<INLotSerSegment.lotSerClassID>>>, 
				OrderBy<Asc<INLotSerSegment.lotSerClassID, Asc<INLotSerSegment.segmentID>>>>.Select(sender.Graph, lsClass.LotSerClassID))
			{
				switch (seg.SegmentType)
				{
					case INLotSerSegmentType.FixedConst:
						mask.Append(!string.IsNullOrEmpty(seg.SegmentValue) ? new string('C', seg.SegmentValue.Length) : string.Empty);
						break;
					case INLotSerSegmentType.NumericVal:
						mask.Append( new string('9', GetNumberLength(lotSerNum)));
						break;
					case INLotSerSegmentType.DateConst:
						mask.Append(!string.IsNullOrEmpty(seg.SegmentValue)
						            	? new string('C', seg.SegmentValue.Length)
						            	: new string('C', string.Format("{0}", sender.Graph.Accessinfo.BusinessDate).Length));
						break;
					case INLotSerSegmentType.DayConst:
					case INLotSerSegmentType.MonthConst:
					case INLotSerSegmentType.YearConst:
						mask.Append(new string('9', 2));
						break;
					case INLotSerSegmentType.MonthLongConst:
						mask.Append(new string('C', 3));
						break;
					case INLotSerSegmentType.YearLongConst:
						mask.Append(new string('9', 4));
						break;
					default:
						throw new PXUnknownSegmentTypeException();
				}
			} 
			return mask.ToString();
		}

		public class LSParts
		{
			public LSParts(int flen, int nlen, int llen)
			{
				_flen = flen;
				_nlen = nlen;
				_llen = llen;
			}

			private readonly int _flen;
			private readonly int _nlen;
			private readonly int _llen;

			public int flen
			{
				get { return _flen; }
			}

			public int nlen
			{
				get { return _nlen; }
			}
			
			public int llen
			{
				get { return _llen; }
			}
			
			public int len
			{
				get { return _flen + _nlen + _llen; }
			}

			public int nidx
			{
				get { return _flen; }
			}

			public int lidx
			{
				get { return _flen + _nlen; }
			}
		}

		/// <summary>
		/// Get auto-incremantal number parts settings
		/// </summary>
		/// <param name="sender">cache</param>
		/// <param name="lsclass">Lot/Ser class</param>
		/// <param name="lotSerNum">auto-incremantal number value</param>
		/// <returns></returns>
		public static LSParts GetLSParts(PXCache sender, INLotSerClass lsclass, ILotSerNumVal lotSerNum)
		{
			if (lsclass == null)
				return null;
			int flen = 0, nlen = 0, llen = 0;
			foreach (INLotSerSegment seg in PXSelect<INLotSerSegment, 
				Where<INLotSerSegment.lotSerClassID, Equal<Required<INLotSerSegment.lotSerClassID>>>, 
				OrderBy<Asc<INLotSerSegment.lotSerClassID, Asc<INLotSerSegment.segmentID>>>>.Select(sender.Graph, lsclass.LotSerClassID))
			{
				int tmp = 0;
				switch (seg.SegmentType)
				{
					case INLotSerSegmentType.FixedConst:
						tmp = seg.SegmentValue.Length;
						break;
					case INLotSerSegmentType.NumericVal:
						nlen = GetNumberLength(lotSerNum);
						break;
					case INLotSerSegmentType.DateConst:
						tmp = !string.IsNullOrEmpty(seg.SegmentValue)
										? seg.SegmentValue.Length
										: string.Format("{0}", sender.Graph.Accessinfo.BusinessDate).Length;
						break;
					case INLotSerSegmentType.DayConst:
					case INLotSerSegmentType.MonthConst:
					case INLotSerSegmentType.YearConst:
						tmp = 2;
						break;
					case INLotSerSegmentType.MonthLongConst:
						tmp = 3;
						break;
					case INLotSerSegmentType.YearLongConst:
						tmp = 4;
						break;
					default:
						throw new PXUnknownSegmentTypeException();
				}
				if (nlen == 0)
					flen += tmp;
				else
					llen += tmp;
			}
			return new LSParts(flen, nlen, llen);
		}

		/// <summary>
		/// Return the new child(detail) object with filled properties for further generation of lot/ser number
		/// </summary>
		/// <typeparam name="TLSDetail"></typeparam>
		/// <param name="sender"></param>
		/// <param name="lsClass"></param>
		/// <param name="lotSerNum"></param>
		/// <returns></returns>
		public static TLSDetail GetNextSplit<TLSDetail>(PXCache sender, INLotSerClass lsClass, ILotSerNumVal lotSerNum)
			where TLSDetail : class, ILSDetail, new()
		{
			TLSDetail det = new TLSDetail();
			det.LotSerialNbr = GetNextNumber(sender, lsClass, lotSerNum);
			det.AssignedNbr = GetNextFormat(lsClass, lotSerNum);
			det.LotSerClassID = GetNextClassID(lsClass);
			if (det is ILSGeneratedDetail gdet)
				gdet.HasGeneratedLotSerialNbr = lsClass.AutoNextNbr == true && !string.IsNullOrEmpty(det.LotSerialNbr);
			return det;
		}

		public static string MakeNumber(string FormatStr, string NumberStr, DateTime date)
		{
			if (FormatStr.Contains(_NumFormatStr))
			{
				string numval = new string('0', NumberStr.Length - FormatStr.Length + _NumFormatStr.Length);
				return string.Format(FormatStr, date, numval).ToUpper();
			}
			else
				return string.Format(FormatStr, date, 0).ToUpper();
		}

		public static bool StringsEqual(string FormatStr, string NumberStr)
		{
			int numIndex = 0;
			for (int i = 0; i < FormatStr.Length; i++)
			{
				if (FormatStr[i] == '{' && i + 5 <= FormatStr.Length && FormatStr[i + 2] == ':')
				{
					int lenIndex = FormatStr.IndexOf("}", i + 3);
					if (lenIndex != -1)
					{
						int lenght = lenIndex - i - 3;
						if (FormatStr[i + 1] == '1')
						{
							if (NumberStr.Length < numIndex + lenght)
								return false;

							for (int n = 0; n < lenght; n++)
								if (NumberStr[numIndex + n] != '0')
									return false;
						}
						numIndex += lenght;
						i = lenIndex;

						if (i >= FormatStr.Length - 1 && numIndex < NumberStr.Length)
							return false;

						continue;
					}
				}

                if (NumberStr == null || NumberStr.Length <= numIndex) return false;
				if (char.ToUpper(FormatStr[i]) != char.ToUpper(NumberStr[numIndex++])) return false;
			}
			return true;
		}

		public static string UpdateNumber(string FormatStr, string NumberStr, string number)
		{
			int numIndex = 0;
			StringBuilder result = new StringBuilder();
			for (int i = 0; i < FormatStr.Length; i++)
			{
				if (FormatStr[i] == '{' && i + 5 <= FormatStr.Length && FormatStr[i + 2] == ':')
				{
					int lenIndex = FormatStr.IndexOf("}", i + 3);
					if (lenIndex != -1)
					{
						int lenght = lenIndex - i - 3;
						if (FormatStr[i + 1] == '1')
							result.Append(number);
						else
							result.Append(NumberStr.Substring(numIndex, lenght));
						numIndex += lenght;
						i = lenIndex;

						continue;
					}
				}
				if (NumberStr.Length <= numIndex) break;
				result.Append(NumberStr[numIndex++]);
			}
			return result.ToString().ToUpper();
		}

		/// <summary>
		/// Return child(detail) objects list with filled properties for further generation of lot/ser number
		/// </summary>
		/// <typeparam name="TLSDetail">child(detail) entity type</typeparam>
		/// <param name="sender">cache</param>
		/// <param name="item">settings</param>
		/// <param name="mode">Track mode</param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static List<TLSDetail> CreateNumbers<TLSDetail>(PXCache sender, PXResult<InventoryItem, INLotSerClass> item, INLotSerTrack.Mode mode, decimal count)
			where TLSDetail : class, ILSDetail, new()
		{
			return CreateNumbers<TLSDetail>(sender, item, ReadLotSerNumVal(sender.Graph, item), mode, false, count);
		}

		/// <summary>
		/// Return child(detail) objects list with filled properties for further generation of lot/ser number
		/// </summary>
		/// <typeparam name="TLSDetail">child(detail) entity type</typeparam>
		/// <param name="sender">cache</param>
		/// <param name="lsClass">Lot/Ser class</param>
		/// <param name="lotSerNum">Auto-incremental number value</param>
		/// <param name="mode">Track mode</param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static List<TLSDetail> CreateNumbers<TLSDetail>(PXCache sender, INLotSerClass lsClass, ILotSerNumVal lotSerNum, INLotSerTrack.Mode mode, decimal count)
			where TLSDetail : class, ILSDetail, new()
		{
			return CreateNumbers<TLSDetail>(sender, lsClass, lotSerNum, mode, false, count);
		}

		/// <summary>
		/// Return child(detail) objects list with filled properties for further generation of lot/ser number
		/// </summary>
		/// <typeparam name="TLSDetail">child(detail) entity type</typeparam>
		/// <param name="sender">cache</param>
		/// <param name="lsClass">Lot/Ser class</param>
		/// <param name="lotSerNum">Auto-incremental number value</param>
		/// <param name="mode">Track mode</param>
		/// <param name="ForceAutoNextNbr"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static List<TLSDetail> CreateNumbers<TLSDetail>(PXCache sender, INLotSerClass lsClass, ILotSerNumVal lotSerNum, INLotSerTrack.Mode mode, bool ForceAutoNextNbr, decimal count)
			where TLSDetail : class, ILSDetail, new()
		{
			List<TLSDetail> ret = new List<TLSDetail>();

			if (lsClass != null)
			{
				string LotSerTrack = (mode & INLotSerTrack.Mode.Create) > 0 ? lsClass.LotSerTrack : INLotSerTrack.NotNumbered;
				bool AutoNextNbr = (mode & INLotSerTrack.Mode.Manual) == 0 && (lsClass.AutoNextNbr == true || ForceAutoNextNbr);

				switch (LotSerTrack)
				{
					case "N":
						TLSDetail detail = new TLSDetail();
						detail.AssignedNbr = string.Empty;
						detail.LotSerialNbr = string.Empty;
						detail.LotSerClassID = string.Empty;
						if (detail is ILSGeneratedDetail gdetail)
							gdetail.HasGeneratedLotSerialNbr = false;

						ret.Add(detail);
						break;
					case "L":
						if (AutoNextNbr)
						{
							ret.Add(GetNextSplit<TLSDetail>(sender, lsClass, lotSerNum));
						}
						break;
					case "S":
						if (AutoNextNbr)
						{
							for (int i = 0; i < (int)count; i++)
							{
								ret.Add(GetNextSplit<TLSDetail>(sender, lsClass, lotSerNum));
							}
						}
						break;
				}
			}
			return ret;
		}

		public static INLotSerTrack.Mode TranTrackMode(INLotSerClass lotSerClass, string tranType, int? invMult)
		{
			if (lotSerClass == null || lotSerClass.LotSerTrack == null || lotSerClass.LotSerTrack == INLotSerTrack.NotNumbered) return INLotSerTrack.Mode.None;

			switch (tranType)
			{
				case INTranType.Invoice:
				case INTranType.DebitMemo:
				case INTranType.Issue:
					return lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.Create
						: invMult == 1 ? INLotSerTrack.Mode.Create | INLotSerTrack.Mode.Manual : INLotSerTrack.Mode.Issue;

				case INTranType.Transfer:
					return
						lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.None
							: INLotSerTrack.Mode.Issue;

				case INTranType.Disassembly:
					return
						lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.None
							: invMult ==  1 ? INLotSerTrack.Mode.Create
							: invMult == -1 ? INLotSerTrack.Mode.Issue
							: INLotSerTrack.Mode.Manual;
				case INTranType.Assembly:
					if (invMult == -1)//component 
					{
						return lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.Create : INLotSerTrack.Mode.Issue;
					}
					else if (invMult == 1) //kit
					{
						return lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.None : INLotSerTrack.Mode.Create;
					}
					else
					{
						return INLotSerTrack.Mode.Manual;
					}
				case INTranType.Adjustment:
				case INTranType.StandardCostAdjustment:
				case INTranType.NegativeCostAdjustment:
				case INTranType.ReceiptCostAdjustment:
					return lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.None
						: invMult ==  1 ? INLotSerTrack.Mode.Create | INLotSerTrack.Mode.Manual
						: invMult == -1 ? INLotSerTrack.Mode.Issue | INLotSerTrack.Mode.Manual
						: INLotSerTrack.Mode.Manual;;					

				case INTranType.Receipt:
					return lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.None : INLotSerTrack.Mode.Create;

				case INTranType.Return:
				case INTranType.CreditMemo:
					return INLotSerTrack.Mode.Create | INLotSerTrack.Mode.Manual;

				default:
					return INLotSerTrack.Mode.None;
			}
		}
		public static bool IsTrack(INLotSerClass lotSerClass, string tranType, int? invMult)
		{
			return TranTrackMode(lotSerClass, tranType, invMult) != INLotSerTrack.Mode.None;
		}
		public static bool IsTrackExpiration(INLotSerClass lotSerClass, string tranType, int? invMult)
		{
			return lotSerClass.LotSerTrackExpiration == true && IsTrack(lotSerClass, tranType, invMult);
		}
		public static bool IsTrackSerial(INLotSerClass lotSerClass, string tranType, int? invMult)
		{
			return lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered && IsTrack(lotSerClass, tranType, invMult);		
		}
	}

	#endregion

	#region INtranLotSerialNbrAttribute
	public class INTranLotSerialNbrAttribute : INLotSerialNbrAttribute
	{
		public INTranLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType, Type ParentLotSerialNbrType, Type costCenterType)
			: base(InventoryType, SubItemType, LocationType, ParentLotSerialNbrType, costCenterType)
		{
		}

		public INTranLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType, Type costCenterType)
			: base(InventoryType, SubItemType, LocationType, costCenterType)
		{
		}

		protected override bool IsTracked(ILSMaster row, INLotSerClass lotSerClass, string tranType, int? invMult)
		{
			if (tranType == INTranType.Issue && lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed)
			{
				if (row is INTran tran && tran.OrigModule == BatchModule.PO) return false;
				else if (row is INTranSplit split && split.OrigModule == BatchModule.PO) return false;
			}

			return base.IsTracked(row, lotSerClass, tranType, invMult);
		}
	} 
	#endregion

	#region TransferLotSerialNbrAttribute

	public class TransferLotSerialNbrAttribute : INLotSerialNbrAttribute
	{
		public TransferLotSerialNbrAttribute(Type inventoryType, Type subItemType, Type locationType, Type costCenterType, Type tranType, Type transferNbrType, Type transferLineNbrType)
			: this(inventoryType, subItemType, locationType, costCenterType, tranType, transferNbrType, transferLineNbrType, null)
		{
		}

		public TransferLotSerialNbrAttribute(Type inventoryType, Type subItemType, Type locationType, Type costCenterType, Type tranType, Type transferNbrType, Type transferLineNbrType,
			Type parentLotSerialNbrType)
		{
			InitializeSelector(GetIntransitLotSerialSearch(inventoryType, subItemType, locationType, costCenterType, tranType, transferNbrType, transferLineNbrType),
				typeof(INLotSerialStatusByCostCenter.lotSerialNbr),
				typeof(INLotSerialStatusByCostCenter.qtyOnHand),
				typeof(INLotSerialStatusByCostCenter.qtyAvail),
				typeof(INLotSerialStatusByCostCenter.expireDate));

			if (parentLotSerialNbrType != null)
				InitializeDefault(parentLotSerialNbrType);
		}
	}

	#endregion

	#region INUnitAttribute

	[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
	[PXUIField(DisplayName = "UOM", Visibility = PXUIVisibility.Visible)]
	public class INUnitAttribute : PXEntityAttribute, IPXFieldVerifyingSubscriber, IPXRowSelectedSubscriber, IPXRowPersistingSubscriber
	{
        public enum VerifyingMode
        {
			Custom,
			UnitCatalog,
			GlobalUnitConversion,
			InventoryUnitConversion
		}
        #region Fields & Properties
        private readonly VerifyingMode _verifyingMode;

		protected Type InventoryType = null;
		protected Type BaseUnitType = null;

		private string _AccountIDField = null;
		private string _AccountRequireUnitsField = null;

		private PXSelectorAttribute _selectorWithAggregate;
		private PXSelectorAttribute _selectorNoAggregate;

		/// <summary>
		/// run verifying process if inventory was setted
		/// </summary>
		private readonly bool _verifyOnSettedInventory = true;

		public override bool DirtyRead
		{
			get { return base.DirtyRead; }
			set
			{
				if (value != base.DirtyRead && AttributeLevel == PXAttributeLevel.Type)
				{
					_Attributes[_SelAttrIndex] = value ? _selectorNoAggregate : _selectorWithAggregate;
					base.DirtyRead = value;
				}
			}
		}

		public bool VerifyOnCopyPaste { get; set; } = true;

		#endregion

		#region Constructors

		protected INUnitAttribute(VerifyingMode verifyingMode):base()
        {
            _verifyingMode = verifyingMode;
        }

		public INUnitAttribute()
			: this(VerifyingMode.UnitCatalog)
		{
            Init(typeof(Search4<INUnit.fromUnit, 
				Where<INUnit.unitType, Equal<INUnitType.global>>, 
				Aggregate<GroupBy<INUnit.fromUnit>>>));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dummy">Is dummy parameter. Was created for constructor type identification</param>
        /// <param name="BaseUnitType"></param>
		public INUnitAttribute(Type dummy, Type BaseUnitType)
			: this(VerifyingMode.GlobalUnitConversion)
		{
			this.BaseUnitType = BaseUnitType;
			Init(BqlTemplate.OfCommand<
				Search<INUnit.fromUnit,
					Where<INUnit.unitType, Equal<INUnitType.global>,
				And<INUnit.toUnit, Equal<Optional<BqlPlaceholder.A>>>>>>
				.Replace<BqlPlaceholder.A>(BaseUnitType).ToType());
		}

		public INUnitAttribute(Type InventoryType, Type AccountIDType, Type AccountRequireUnitsType)
			: this()
		{
			this.InventoryType = InventoryType;
			_AccountIDField = AccountIDType.Name;
			_AccountRequireUnitsField = AccountRequireUnitsType.Name;
            _verifyOnSettedInventory = false;
		}

		//it is assumed that only conversions to BASE item unit exist.
		public INUnitAttribute(Type InventoryType)
			: this(VerifyingMode.InventoryUnitConversion)
		{
			this.InventoryType = InventoryType;
			Init(BqlTemplate.OfCommand<
				Search<INUnit.fromUnit,
				Where<INUnit.unitType, Equal<INUnitType.inventoryItem>,
					And<INUnit.inventoryID, Equal<Optional<BqlPlaceholder.A>>,
				Or<INUnit.unitType, Equal<INUnitType.global>,
					And<Optional<BqlPlaceholder.A>, IsNull>>>>>>
				.Replace<BqlPlaceholder.A>(InventoryType).ToType());
		}
		#endregion

		private void Init(Type searchType)
			=> Init(searchType, BqlCommand.CreateInstance(searchType).AggregateNew<Aggregate<GroupBy<INUnit.fromUnit>>>().GetType());

		protected void Init(Type searchNoAggregate, Type searchWithAggregate)
		{
			_selectorNoAggregate = new PXSelectorAttribute(searchNoAggregate);
			_selectorWithAggregate = searchNoAggregate == searchWithAggregate 
				? _selectorNoAggregate
				: new PXSelectorAttribute(searchWithAggregate);
			_Attributes.Add(DirtyRead ? _selectorNoAggregate : _selectorWithAggregate);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				subscribers.Add(this as ISubscriber);
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}

		#region Implementation
		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null || string.IsNullOrEmpty(_AccountIDField) || string.IsNullOrEmpty(_AccountRequireUnitsField))
				return;

			object AccountID = sender.GetValue(e.Row, _AccountIDField);
			object AccountRequireUnits = sender.GetValue(e.Row, _AccountRequireUnitsField);

			if (AccountRequireUnits == null)
			{
                Account account = Account.PK.Find(sender.Graph, AccountID as int?);

                if (account != null)
				{
					sender.SetValue(e.Row, _AccountRequireUnitsField, account.RequireUnits);
				}
				else
				{
					sender.SetValue(e.Row, _AccountRequireUnitsField, null);
				}
			}
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (string.IsNullOrEmpty(_AccountRequireUnitsField))
				return;

            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;

			object AccountRequireUnits = sender.GetValue(e.Row, _AccountRequireUnitsField);
			string FieldValue = (string)sender.GetValue(e.Row, _FieldOrdinal);

			if (AccountRequireUnits != null && (bool)AccountRequireUnits && string.IsNullOrEmpty(FieldValue))
			{
				var acctID = sender.GetValue(e.Row, _AccountIDField);
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(sender.Graph, acctID);

				if (account == null)
					throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
				else
					throw new PXRowPersistingException(_FieldName, null, Messages.UOMRequiredForAccount, _FieldName, account.AccountCD);
			}
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
            if (e.Row == null || e.NewValue == null)
                return;
			if(_FieldName.IsIn(nameof(INUnit.FromUnit), nameof(INUnit.ToUnit)))
                return;
            if (!VerifyOnCopyPaste && sender.Graph.IsCopyPasteContext)
                return;
            if (!_verifyOnSettedInventory && sender.GetValue(e.Row, InventoryType.Name) != null)
                return;
            var unit = ReadUnit(sender, e.Row, (string)e.NewValue);
			UnitVerifying(sender, e, unit);
        }

		protected virtual void UnitVerifying(PXCache cache, PXFieldVerifyingEventArgs e, INUnit unit)
		{
			if (unit == null)
			{
				if (e.ExternalCall)
					throw new PXSetPropertyException(_verifyingMode == VerifyingMode.UnitCatalog ? ErrorMessages.ElementDoesntExist : ErrorMessages.ElementDoesntExistOrNoRights, _FieldName);

				if (_verifyingMode == VerifyingMode.UnitCatalog)
				{
					throw new PXSetPropertyException(ErrorMessages.ValueDoesntExist, _FieldName, e.NewValue);
				}
				else
				{
					var inventory = ReadInventoryItem(cache, e.Row);

					if (inventory != null)
					{
						throw new PXSetPropertyException(Messages.UOMIsNotSpecifiedForTheItem, e.NewValue, inventory.InventoryCD);
					}
					else
					{
						throw new PXSetPropertyException(ErrorMessages.ValueDoesntExistOrNoRights, _FieldName, e.NewValue);
					}
				}
			}
		}

		protected virtual INUnit ReadUnit(PXCache sender, object data, string fromUnitID)
		{
			INUnit unit = null;
			switch (_verifyingMode)
					{
				case VerifyingMode.UnitCatalog:
					unit = ReadGlobalUnit(sender, fromUnitID);
					break;
				case VerifyingMode.GlobalUnitConversion:
					var toUnitID = GetBaseUnit(sender, data);
					unit = INUnit.UK.ByGlobal.Find(sender.Graph, fromUnitID, toUnitID);
					break;
				case VerifyingMode.InventoryUnitConversion:
					var inventory = InventoryItem.PK.Find(sender.Graph, (int?)GetSelectorParameterValue(sender, data, InventoryType));
					if (inventory == null)
						unit = ReadGlobalUnit(sender, fromUnitID);
					else
						unit = INUnit.UK.ByInventory.Find(sender.Graph, inventory.InventoryID, fromUnitID);
					break;
				}
			return unit;
		}

		public virtual INUnit ReadConversion(PXCache cache, object data, string fromUnitID)
		{
			var conversionInfo = ReadConversionInfo(cache, data, fromUnitID);
			return conversionInfo == null ? null : conversionInfo.Conversion;
		}

		public virtual InventoryItem ReadInventoryItem(PXCache cache, object data)
		{
			if(_verifyingMode == VerifyingMode.InventoryUnitConversion)
				return InventoryItem.PK.Find(cache.Graph, (int?)GetSelectorParameterValue(cache, data, InventoryType));
			return null;
		}

		public virtual ConversionInfo ReadConversionInfo(PXCache cache, object data, string fromUnitID)
		{
			if (string.IsNullOrEmpty(fromUnitID))
				return null;
			INUnit conversion;
			InventoryItem inventory = null;
			switch (_verifyingMode)
			{
				case VerifyingMode.Custom:
					return null;
				case VerifyingMode.UnitCatalog:
					conversion = EmptyConversion(fromUnitID);
					break;
				case VerifyingMode.GlobalUnitConversion:
					var toUnitID = GetBaseUnit(cache, data);
					conversion = fromUnitID == toUnitID
						? EmptyConversion(fromUnitID)
						: INUnit.UK.ByGlobal.Find(cache.Graph, fromUnitID, toUnitID);
					break;
				case VerifyingMode.InventoryUnitConversion:
					inventory = InventoryItem.PK.Find(cache.Graph, (int?)GetSelectorParameterValue(cache, data, InventoryType));
					if (inventory == null)
						conversion = EmptyConversion(fromUnitID);
					else
					{
						if (fromUnitID == inventory.BaseUnit)
						{
							conversion = EmptyConversion(fromUnitID);
							conversion.UnitType = INUnitType.InventoryItem;
							conversion.InventoryID = inventory.InventoryID;
						}
						else
							conversion = INUnit.UK.ByInventory.Find(cache.Graph, inventory.InventoryID, fromUnitID);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(_verifyingMode));
			}
			return new ConversionInfo(conversion, inventory);
		}

		private INUnit EmptyConversion(string unit) => new INUnit
		{
			UnitType = INUnitType.Global,
			FromUnit = unit,
			ToUnit = unit,
			UnitRate = 1,
			PriceAdjustmentMultiplier = 1,
			UnitMultDiv = MultDiv.Multiply
		};

		#endregion

		protected virtual string GetBaseUnit(PXCache cache, object data)
		{
			if(BaseUnitType == null)
				return null;
			string unit = (string)GetSelectorParameterValue(cache, data, BaseUnitType);
			if(unit == null)
			{
				var itemType = BqlCommand.GetItemType(BaseUnitType);
				var itemCache = cache.Graph.Caches[itemType];
				if(itemCache.Keys.Count == 0)
				{
					var cmd = BqlTemplate.OfCommand<Select<BqlPlaceholder.A>>.Replace<BqlPlaceholder.A>(itemType).ToCommand();
					var view = cache.Graph.TypedViews.GetView(cmd, true);
					var item = view.SelectSingle();
					unit = (string)itemCache.GetValue(item, BaseUnitType.Name);
				}
			}
			return unit;
		}

		protected virtual object GetSelectorParameterValue(PXCache cache, object data, Type parameterFieldType)
		{
			var view = cache.Graph.TypedViews.GetView(NonDimensionSelectorAttribute.GetSelect(), !NonDimensionSelectorAttribute.DirtyRead);
			object[] values = view.PrepareParameters(new[] { data }, null);
			return values == null || values.Length == 0
				? null
				: values[0];
		}

		#region Runtime
		private static INUnit ReadGlobalUnit(PXCache sender, string fromUnitID)
		{
			return PXSelectReadonly<INUnit,
					Where<INUnit.unitType, Equal<INUnitType.global>,
						And<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>>>>.SelectWindowed(sender.Graph, 0, 1, fromUnitID);
		}
		public static decimal ConvertFromBase<InventoryIDField, UOMField>(PXCache sender, object Row, decimal value, INPrecision prec)
			where InventoryIDField : IBqlField
			where UOMField : IBqlField
		{
			if (value == 0) return 0;

			string ToUnit = (string)sender.GetValue<UOMField>(Row);
			try
			{
				return ConvertFromBase<InventoryIDField>(sender, Row, ToUnit, value, prec);
			}
			catch (PXUnitConversionException ex)
			{
				sender.RaiseExceptionHandling<UOMField>(Row, ToUnit, ex);
			}
			return 0m;
		}
		public static decimal ConvertFromBase<InventoryIDField>(PXCache sender, object Row, string ToUnit, decimal value, INPrecision prec)
			where InventoryIDField : IBqlField
		{
			return Convert<InventoryIDField>(sender, Row, ToUnit, value, prec, true);
		}

		public static decimal ConvertFromBase<InventoryIDField>(PXCache sender, object Row, string ToUnit, decimal value, INPrecision prec, INMidpointRounding rounding)
			where InventoryIDField : IBqlField
		{
			return Convert<InventoryIDField>(sender, Row, ToUnit, value, prec, true, rounding);
		}

		public static decimal ConvertToBase<InventoryIDField, UOMField>(PXCache sender, object Row, decimal value, INPrecision prec)
			where InventoryIDField : IBqlField
			where UOMField : IBqlField
		{
			if (value == 0) return 0;

			string FromUnit = (string)sender.GetValue<UOMField>(Row);
			try
			{
				return ConvertToBase<InventoryIDField>(sender, Row, FromUnit, value, prec);
			}
			catch (PXUnitConversionException ex)
			{
				sender.RaiseExceptionHandling<UOMField>(Row, FromUnit, ex);
			}
			return 0m;
		}
		public static decimal ConvertToBase<InventoryIDField>(PXCache sender, object Row, string FromUnit, decimal value, INPrecision prec)
			where InventoryIDField : IBqlField
		{
			return Convert<InventoryIDField>(sender, Row, FromUnit, value, prec, false);
		}
		public static decimal ConvertFromTo<InventoryIDField>(PXCache sender, object Row, string FromUnit, string ToUnit, decimal value, INPrecision prec)
			where InventoryIDField : IBqlField
		{
			if (string.Equals(FromUnit, ToUnit))
			{
				return value;
			}
			decimal baseValue = ConvertToBase<InventoryIDField>(sender, Row, FromUnit, value, prec);
			return ConvertFromBase<InventoryIDField>(sender, Row, ToUnit, baseValue, prec);
		}

		private static decimal Convert<InventoryIDField>(PXCache sender, object Row, string FromUnit, decimal value, INPrecision prec, bool ViceVersa, INMidpointRounding rounding)
			where InventoryIDField : IBqlField
		{
			if (value == 0 || FromUnit == null)
				return value;

			object InventoryID = sender.GetValue<InventoryIDField>(Row);
			if (InventoryID == null)
				return value;

			var inventory = InventoryItem.PK.Find(sender.Graph, (int?)InventoryID);
			if (inventory == null)
			{
				PXTrace.WriteError(ErrorMessages.ValueDoesntExistOrNoRights, Messages.InventoryItem, InventoryID);
				throw new PXUnitConversionException();
			}

            if (FromUnit == inventory.BaseUnit)
				return Round(value, prec, rounding);

			INUnit unit = INUnit.UK.ByInventory.Find(sender.Graph, inventory.InventoryID, FromUnit);
			if (unit == null)
				throw new PXUnitConversionException();

				return ConvertValue(value, unit, prec, ViceVersa, UsePriceAdjustmentMultiplier(sender.Graph)); 
			}

		private static decimal Convert<InventoryIDField>(PXCache sender, object Row, string FromUnit, decimal value,
			INPrecision prec, bool ViceVersa)
			where InventoryIDField : IBqlField
		{
			return Convert<InventoryIDField>(sender, Row, FromUnit, value, prec, ViceVersa, INMidpointRounding.ROUND);
		}

		public static decimal ConvertFromBase(PXCache sender, Int32? InventoryID, string ToUnit, decimal value, INPrecision prec)
		{
			return Convert(sender, InventoryID, ToUnit, value, prec, true);
		}

		public static decimal ConvertToBase(PXCache sender, Int32? InventoryID, string FromUnit, decimal value, INPrecision prec)
		{
			return Convert(sender, InventoryID, FromUnit, value, prec, false);
		}

		public static decimal ConvertToBase(PXCache sender, Int32? InventoryID, string FromUnit, decimal value, decimal? baseValue, INPrecision prec)
		{
			return Convert(sender, InventoryID, FromUnit, value, baseValue, prec, false);
		}

		private static decimal Convert(PXCache sender, Int32? InventoryID, string FromUnit, decimal value, INPrecision prec, bool ViceVersa)
		{
			return Convert(sender, InventoryID, FromUnit, value, null, prec, ViceVersa);
		}

		private static decimal Convert(PXCache sender, Int32? InventoryID, string FromUnit, decimal value, decimal? baseValue, INPrecision prec, bool ViceVersa)
		{
			if (value == 0 || FromUnit == null)
				return value;

            InventoryItem item = item = InventoryItem.PK.Find(sender.Graph, InventoryID);

            if(item == null)
			{
				PXTrace.WriteError(ErrorMessages.ValueDoesntExistOrNoRights, Messages.InventoryItem, InventoryID);
				throw new PXUnitConversionException();
			}

            if (FromUnit == item.BaseUnit)
				{
                if (baseValue != null)
			{
                    decimal revValue = Round((decimal)baseValue, prec);
                    if (revValue == value)
                        return (decimal)baseValue;
			}
                return Round(value, prec);
			}

			INUnit unit = INUnit.UK.ByInventory.Find(sender.Graph, item.InventoryID, FromUnit);
			if (unit == null)
				throw new PXUnitConversionException();

				bool usePriceAdjustmentMultiplier = UsePriceAdjustmentMultiplier(sender.Graph);
				if (baseValue != null)
				{
					decimal revValue = ConvertValue((decimal) baseValue, unit, prec, !ViceVersa, usePriceAdjustmentMultiplier);
					if (revValue == value)
						return (decimal) baseValue;
				}
				return ConvertValue(value, unit, prec, ViceVersa, usePriceAdjustmentMultiplier);
			}

		internal static decimal Convert(PXCache sender, INUnit unit, decimal value, INPrecision prec, bool ViceVersa)
		{
			if (value == 0) return 0;
			if (unit == null) return value;
			return ConvertValue(value, unit, prec, ViceVersa, UsePriceAdjustmentMultiplier(sender.Graph));
		}

		public static decimal ConvertFromBase(PXCache sender, INUnit unit, decimal value, INPrecision prec)
		{
			return Convert(sender, unit, value, prec, true);
		}

		public static decimal ConvertToBase(PXCache sender, INUnit unit, decimal value, INPrecision prec)
		{
			return Convert(sender, unit, value, prec, false);
		}

		public static bool IsFractional(INUnit conv)
		{
			return conv != null && (conv.UnitMultDiv == MultDiv.Divide && conv.UnitRate != 1m || decimal.Remainder((decimal)conv.UnitRate, 1m) != 0m);
		}

		/// <summary>
		/// Converts units using Global converion Table.
		/// </summary>
		/// <exception cref="PXException">Is thrown if converion is not found in the table.</exception>
		[Obsolete]
		public static decimal ConvertGlobalUnits(PXGraph graph, string from, string to, decimal value, INPrecision prec)
		{
			decimal result = 0;

			if (TryConvertGlobalUnits(graph, from, to, value, prec, out result))
			{
				return result;
			}
			else
			{
				throw new PXException(Messages.ConversionNotFound, from, to);
			}

		}

		/// <summary>
		/// Converts units using Global converion Table.
		/// </summary>
		/// <exception cref="PXUnitConversionException">Is thrown if converion is not found in the table.</exception>
		public static decimal ConvertGlobal(PXGraph graph, string from, string to, decimal value, INPrecision prec)
		{
			decimal result = 0;

			if (TryConvertGlobalUnits(graph, from, to, value, prec, out result))
			{
				return result;
			}
			else
			{
				throw new PXUnitConversionException(from, to);
			}
		}

		public static bool TryConvertGlobalUnits(PXGraph graph, string from, string to, decimal value, INPrecision prec, out decimal result)
		{
			if (value == 0)
			{
				result = 0;
				return true;
			}

			result = 0;
			if (from == to)
			{
				result = value;
				return true;
			}
						
			var unit = INUnit.UK.ByGlobal.Find(graph, from, to);

			if (unit == null)
				return false;
			result = ConvertValue(value, unit, prec);
			return true;
		}

		public static decimal ConvertValue(decimal value, INUnit unit, INPrecision prec, bool viceVersa, bool usePriceAdjustmentMultiplier, INMidpointRounding rounding)
		{
			if (unit.UnitMultDiv == MultDiv.Multiply && !viceVersa || unit.UnitMultDiv == MultDiv.Divide && viceVersa)
				value *= (decimal) unit.UnitRate;
			else
				value /= (decimal) unit.UnitRate;

			if (usePriceAdjustmentMultiplier && prec == INPrecision.UNITCOST)
			{
				if (viceVersa)
					value /= (decimal)unit.PriceAdjustmentMultiplier;
				else
					value *= (decimal)unit.PriceAdjustmentMultiplier;
			}

			return Round(value, prec, rounding);
		}

		public static decimal ConvertValue(decimal value, INUnit unit, INPrecision prec, bool viceVersa = false, bool usePriceAdjustmentMultiplier = false)
		{
			return ConvertValue(value, unit, prec, viceVersa, usePriceAdjustmentMultiplier, INMidpointRounding.ROUND);
		}

        private static decimal Round(decimal value, INPrecision prec)
        {
			return Round(value, prec, INMidpointRounding.ROUND);
		}

		private static decimal Round(decimal value, INPrecision prec, INMidpointRounding rounding)
        {
			if (prec == INPrecision.NOROUND)
				return value;

			int precision = DefinePrecision(prec);
			return rounding == INMidpointRounding.ROUND
				? Math.Round(value, precision, MidpointRounding.AwayFromZero)
				//Update to Math.Round with MidpointRounding.ToZero as it will be possible.
				: value > 0
					? Math.Floor(value * (long)Math.Pow(10, precision)) / (long)Math.Pow(10, precision)
					: Math.Ceiling(value * (long)Math.Pow(10, precision)) / (long)Math.Pow(10, precision);
		}

		private static int DefinePrecision(INPrecision prec)
		{
			int precision = 6;

			switch (prec)
			{
				case INPrecision.QUANTITY:
					precision = CommonSetupDecPl.Qty;
					break;
				case INPrecision.UNITCOST:
					precision = CommonSetupDecPl.PrcCst;
					break;
			}
			return precision;
		}

		public static bool UsePriceAdjustmentMultiplier(PXGraph graph)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>() == false)
				return false;

			if (graph is ARInvoiceEntry || graph is SOOrderEntry || graph is ARCashSaleEntry)
			{
				SOSetup soSetup = PXSelect<SOSetup>.SelectSingleBound(graph, null);
				bool usePriceAdjustmentMultiplier = soSetup?.UsePriceAdjustmentMultiplier ?? false;
				return usePriceAdjustmentMultiplier;
		}

			return false;
		}
		#endregion
	}

	public class INUnboundUnitAttribute: INUnitAttribute
	{
		public INUnboundUnitAttribute()
		{
			ReplaceDBField();
		}

		private void ReplaceDBField()
		{
			var dbStringAttribute = _Attributes.OfType<PXDBStringAttribute>().FirstOrDefault();
			if (dbStringAttribute == null)
				throw new PXArgumentException(nameof(dbStringAttribute));
			var index = _Attributes.IndexOf(dbStringAttribute);
			_Attributes.RemoveAt(index);
			_Attributes.Insert(index, new PXStringAttribute(dbStringAttribute.Length)
			{
				IsUnicode = dbStringAttribute.IsUnicode,
				InputMask = dbStringAttribute.InputMask
			});
		}
	}

	#endregion

	#region InventoryRawAttribute

	[PXDBString(InputMask = "", IsUnicode = true)]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.SelectorVisible)]
	public sealed class InventoryRawAttribute : PXEntityAttribute
	{
		public const string DimensionName = "INVENTORY";

		private Type _whereType;

		public InventoryRawAttribute()
			: base()
		{
			Type SearchType = typeof(Search<InventoryItem.inventoryCD, Where2<Match<Current<AccessInfo.userName>>, And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>>>>);
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(InventoryItem.inventoryCD));
			attr.DescriptionField = typeof(InventoryItem.descr);
			attr.CacheGlobal = true;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public InventoryRawAttribute(Type WhereType)
			: this()
		{
			if (WhereType != null)
			{
				_whereType = WhereType;

				Type SearchType = BqlCommand.Compose(
					typeof(Search<,>),
					typeof(InventoryItem.inventoryCD),
					typeof(Where2<,>),
					typeof(Match<>),
					typeof(Current<AccessInfo.userName>),
					typeof(And<,,>),
					typeof(InventoryItem.itemStatus),
					typeof(NotEqual<InventoryItemStatus.unknown>),
					typeof(And<,,>),
					typeof(InventoryItem.isTemplate),
					typeof(Equal<False>),
					typeof(And<>),
					_whereType);
				PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(InventoryItem.inventoryCD));
				attr.DescriptionField = typeof(InventoryItem.descr);
				attr.CacheGlobal = true;
				_Attributes[_SelAttrIndex] = attr;
			}
		}
	}

	#endregion

	#region TemplateInventoryRawAttribute

	[PXDBString(InputMask = "", IsUnicode = true)]
	[PXUIField(DisplayName = "Template ID", Visibility = PXUIVisibility.SelectorVisible)]
	public sealed class TemplateInventoryRawAttribute : PXEntityAttribute
	{
		public TemplateInventoryRawAttribute()
			: base()
		{
			Type SearchType = typeof(Search<InventoryItem.inventoryCD, Where2<Match<Current<AccessInfo.userName>>,
				And<InventoryItem.isTemplate, Equal<True>>>>);
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(InventoryRawAttribute.DimensionName, SearchType, typeof(InventoryItem.inventoryCD));
			attr.DescriptionField = typeof(InventoryItem.descr);
			attr.CacheGlobal = true;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	#endregion

	#region INPrimaryAlternateType

	public enum INPrimaryAlternateType
	{
		/// <summary>Vendor part number</summary>
		VPN,
		/// <summary>Customer part number</summary>
		CPN
	}

	#endregion

	#region CrossItemAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	public class CrossItemAttribute : InventoryIncludingTemplatesAttribute, IPXFieldVerifyingSubscriber
	{
		protected class FoundInventory : Tuple<string, string, string, string, bool>
		{
			public static FoundInventory Found(string alternateID, string inventoryCD, string subItemCD, string uom, bool uniqueReference)
				=> new FoundInventory(alternateID, inventoryCD, subItemCD, uom, uniqueReference);

			public static FoundInventory NotFound(String alternateID) 
				=> new FoundInventory(alternateID, null, null, null, false);

			private FoundInventory(string alternateID, string inventoryCD, string subItemCD, string uom, bool uniqueReference) 
				: base(alternateID, inventoryCD, subItemCD, uom, uniqueReference) { }
			public string AlternateID => Item1;
			public string InventoryCD => Item2;
			public string SubItemCD => Item3;
			public string UOM => Item4;
			public bool UniqueReference => Item5;

			public bool IsFound => InventoryCD.IsNullOrEmpty() == false;
		}

		[PXLocalizable]
		public class CrossItemMessages
		{
			public const string ElementDoesntExist = "The specified inventory ID or alternate ID cannot be found in the system.";
			public const string ValueDoesntExist = "The specified inventory ID or alternate ID \"{1}\" cannot be found in the system.";
			public const string ElementDoesntExistOrNoRights = "The specified inventory ID or alternate ID cannot be found in the system. Please verify whether you have proper access rights to this object.";
			public const string ValueDoesntExistOrNoRights = "The specified inventory ID or alternate ID \"{1}\" cannot be found in the system. Please verify whether you have proper access rights to this object.";
			public const string ManyItemsForCurrentAlternateID = "The specified alternate ID is assigned to multiple inventory items. Please make sure that the correct inventory ID has been specified in the row.";
		}

		#region State
		protected INPrimaryAlternateType? _PrimaryAltType;
		protected string _AlternateID = "AlternateID";
		protected string _SubItemID = "SubItemID";
		protected string _UOM = "UOM";


		private static readonly ReadOnlyDictionary<INPrimaryAlternateType?, Type> AltTypeToDefaultBAccountFieldMap =
			new ReadOnlyDictionary<INPrimaryAlternateType?, Type>(
				new Dictionary<INPrimaryAlternateType?, Type>
				{
					[INPrimaryAlternateType.CPN] = typeof(Customer.bAccountID),
					[INPrimaryAlternateType.VPN] = typeof(Vendor.bAccountID),
				});

		public Type BAccountField
		{
			get { return _bAccountField ?? AltTypeToDefaultBAccountFieldMap.GetOrDefault(_PrimaryAltType, null); }
			set { _bAccountField = value; }
		}
		private Type _bAccountField;

		public string[] AlternateTypePriority { get; set; } = { INAlternateType.CPN, INAlternateType.VPN, INAlternateType.Barcode, INAlternateType.GIN, INAlternateType.Global };

		public bool EnableAlternateSubstitution { get; set; } = true;

		protected Type[] InventoryRestrictingConditions
		{
			get
			{
				if (_inventoryRestrictingConditions == null)
					_inventoryRestrictingConditions =
						GetAttributes()
							.OfType<PXRestrictorAttribute>()
							.Select(r => r.RestrictingCondition)
							.Where(r => BqlCommand.Decompose(r).All(c => typeof(IBqlField).IsAssignableFrom(c) == false || c.DeclaringType.IsIn(typeof(InventoryItem), typeof(FeaturesSet))))
							.ToArray();

				return _inventoryRestrictingConditions;
			}
		}
		private Type[] _inventoryRestrictingConditions;

		public bool WarningOnNonUniqueSubstitution { get; set; }

		protected int _templateItemsRestrictorIndex = -1;
		public virtual bool AllowTemplateItems
		{
			get
			{
				return _templateItemsRestrictorIndex < 0;
			}
			set
			{
				if (value && _templateItemsRestrictorIndex >= 0)
				{
					_Attributes.RemoveAt(_templateItemsRestrictorIndex);
					_templateItemsRestrictorIndex = -1;
				}
				else if (!value && _templateItemsRestrictorIndex < 0)
				{
					_Attributes.Add(new PXRestrictorAttribute(typeof(Where<InventoryItem.isTemplate, Equal<False>>), Messages.InventoryItemIsATemplate)
					{
						ShowWarning = true
					});
					_templateItemsRestrictorIndex = _Attributes.Count - 1;
				}
			}
		}

		#endregion
		#region Ctor
		public CrossItemAttribute() : base(
			typeof(Search<InventoryItem.inventoryID, Where2<Match<Current<AccessInfo.userName>>, And<Where<InventoryItem.stkItem, Equal<True>, Or<InventoryItem.kitItem, Equal<True>>>>>>), 
			typeof(InventoryItem.inventoryCD), 
			typeof(InventoryItem.descr))
		{
			ReplaceSelectorMessages(SelectorAttribute);
			AllowTemplateItems = false;
		}
		
		public CrossItemAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField, INPrimaryAlternateType PrimaryAltType)
			: base(SearchType, SubstituteKey, DescriptionField)
		{
			_PrimaryAltType = PrimaryAltType;
			ReplaceSelectorMessages(SelectorAttribute);
			AllowTemplateItems = false;
		}

		public CrossItemAttribute(INPrimaryAlternateType PrimaryAltType)
			: this()
		{
			_PrimaryAltType = PrimaryAltType;
		}
		
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdating.RemoveHandler(sender.GetItemType(), _FieldName, SelectorAttribute.FieldUpdating);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, this.FieldUpdating);
		}
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				subscribers.Remove(_Attributes[_SelAttrIndex] as ISubscriber);
			}
		}

		public static void ReplaceSelectorMessages(PXDimensionSelectorAttribute selector)
		{
			selector.CustomMessageElementDoesntExist = CrossItemMessages.ElementDoesntExist;
			selector.CustomMessageElementDoesntExistOrNoRights = CrossItemMessages.ElementDoesntExistOrNoRights;
			selector.CustomMessageValueDoesntExist = CrossItemMessages.ValueDoesntExist;
			selector.CustomMessageValueDoesntExistOrNoRights = CrossItemMessages.ValueDoesntExistOrNoRights;
		}
		#endregion
		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			try
			{
				SelectorAttribute.FieldUpdating(sender, e);
				return;
			}
			catch (PXSetPropertyException)
			{
			}

			var foundAlternate = FindAlternate(sender, e.NewValue as string);
			e.NewValue = foundAlternate?.InventoryCD ?? e.NewValue;

			SelectorAttribute.FieldUpdating(sender, e);
			SetValuesPending(sender, e.Row, foundAlternate);

			RaiseWarningIfReferenceIsNotUnique(sender, e.Row, foundAlternate);
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs a)
		{
			try
			{
				SelectorAttribute.FieldVerifying(sender, a);
				return;
			}
			catch (PXSetPropertyException)
			{
				if (a.Row == null)
				{
					throw;
				}
			}

			PXFieldUpdatingEventArgs e = new PXFieldUpdatingEventArgs(a.Row, sender.GetValuePending(a.Row, _FieldName));

			var foundAlternate = FindAlternate(sender, e.NewValue as string);
			e.NewValue = foundAlternate?.InventoryCD ?? e.NewValue;

			SelectorAttribute.FieldUpdating(sender, e);
			a.NewValue = e.NewValue;
			SelectorAttribute.FieldVerifying(sender, a);
			SetValuesPending(sender, e.Row, foundAlternate);

			RaiseWarningIfReferenceIsNotUnique(sender, a.Row, foundAlternate);
		}

		private void RaiseWarningIfReferenceIsNotUnique(PXCache cache, object row, FoundInventory foundAlternate)
			{
			if (WarningOnNonUniqueSubstitution && foundAlternate?.UniqueReference == false)
				cache.RaiseExceptionHandling(_FieldName, row, foundAlternate.AlternateID, new PXSetPropertyException(CrossItemMessages.ManyItemsForCurrentAlternateID, PXErrorLevel.Warning));
			}

		private void SetValuesPending(PXCache cache, object row, FoundInventory foundInventory)
			{
			if (foundInventory == null)
				return;

			if (foundInventory.AlternateID != null && cache.Fields.Contains(_AlternateID))
			{
				cache.SetValuePending(row, _AlternateID, foundInventory.AlternateID);
			}
			if (foundInventory.SubItemCD != null && cache.Fields.Contains(_SubItemID))
			{
				cache.SetValuePending(row, _SubItemID, foundInventory.SubItemCD);
		}
			if (foundInventory.UOM != null && cache.Fields.Contains(_UOM))
			{
				cache.SetValuePending(row, _UOM, foundInventory.UOM);
			}
		}

		protected FoundInventory FindAlternate(PXCache sender, string alternateID) 
			=> GenericCall.Of(() => findAlternate<IBqlField>(sender, alternateID)).ButWith(BAccountField);

		protected virtual FoundInventory findAlternate<BAccountID>(PXCache sender, string alternateID)
			where BAccountID : IBqlField
		{
			if (EnableAlternateSubstitution == false)
				return FoundInventory.NotFound(alternateID);

			PXResult<INItemXRef> mostSuitableXRef = null;
			bool severalRefs = false;

			if (String.IsNullOrEmpty(alternateID) == false)
			{
				//Sorting order is important for correct alternateType pick-up. Default attribute takes records from the tail
				PXSelectBase<INItemXRef> cmd =
					new PXSelectJoin<INItemXRef,
						InnerJoin<InventoryItem, On<INItemXRef.FK.InventoryItem>,
						LeftJoin<INSubItem, On<INItemXRef.FK.SubItem>>>,
						Where2<Match<Current<AccessInfo.userName>>, 
							And<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>>>>(sender.Graph);

				foreach (Type restriction in InventoryRestrictingConditions)
					cmd.WhereAnd(restriction);

				switch (_PrimaryAltType)
				{
					case INPrimaryAlternateType.CPN:
						cmd.WhereAnd<
							Where<INItemXRef.alternateType, Equal<INAlternateType.cPN>,
							And<INItemXRef.bAccountID, Equal<Current<BAccountID>>,
							Or<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>,
								And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>>>();
						break;
					case INPrimaryAlternateType.VPN:
						cmd.WhereAnd<
							Where<INItemXRef.alternateType, Equal<INAlternateType.vPN>,
							And<INItemXRef.bAccountID, Equal<Current<BAccountID>>,
							Or<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>,
								And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>>>();
						break;
					default:
						cmd.WhereAnd<
							Where<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>,
							And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>();
						break;
				}

				var refs = cmd.Select(alternateID).OrderBy(r => ((INItemXRef) r).AlternateType, AlternateTypePriority.ToArray()).ToArray();
				mostSuitableXRef = refs.FirstOrDefault();
				severalRefs = refs.Length > 1;
			}

			if (mostSuitableXRef == null)
				return FoundInventory.NotFound(alternateID);

			InventoryItem item = mostSuitableXRef.GetItem<InventoryItem>();
			INItemXRef itemXRef = mostSuitableXRef.GetItem<INItemXRef>();
			string inventoryCD = item.InventoryCD;
			string uom = itemXRef.UOM;

			if (_PrimaryAltType != null)
				{
					string alternateType = INAlternateType.ConvertFromPrimary(this._PrimaryAltType.Value);
				if (String.IsNullOrEmpty(alternateType) == false && itemXRef.AlternateType == alternateType)
					{
					alternateID = itemXRef.AlternateID; //Place typed value
					}
					else if(alternateType == INAlternateType.CPN || alternateType == INAlternateType.VPN)
					{
					PXSelectBase<INItemXRef> cmd =
						new PXSelect<INItemXRef,
							Where<INItemXRef.inventoryID, Equal<Required<INItemXRef.inventoryID>>,
							And<INItemXRef.subItemID, Equal<Required<INItemXRef.subItemID>>,
							And<INItemXRef.alternateType, Equal<Required<INItemXRef.alternateType>>,
							And<INItemXRef.uOM, Equal<Required<INItemXRef.uOM>>,
							And<INItemXRef.bAccountID, Equal<Current<BAccountID>>>
							>>>>>(sender.Graph);

					INItemXRef itemX = cmd.SelectSingle(itemXRef.InventoryID, itemXRef.SubItemID, alternateType, itemXRef.UOM);
						if (itemX == null)
					{
						alternateID = itemXRef.AlternateID;
					}
					}
				}
				else
			{
				alternateID = itemXRef.AlternateID;
			}

			string subItemCD = null;
				//Skip assignment for the case when AlternateID is the same as InventoryID
			if (alternateID != null && string.Equals(inventoryCD.Trim(), alternateID.Trim()))
				{
				alternateID = null;
				uom = null;
				}
			else if (item.StkItem == true)
				{
				subItemCD = mostSuitableXRef.GetItem<INSubItem>().SubItemCD;
				}

			return FoundInventory.Found(alternateID, inventoryCD, subItemCD, uom, severalRefs == false);
			}

		#endregion
		#region Runtime
		public static void SetEnableAlternateSubstitution<TField>(PXCache cache, object row, bool enableAlternateSubstitution)
			where TField : IBqlField
		{
			foreach (var crossItemAttribute in cache.GetAttributes<TField>(row).OfType<CrossItemAttribute>())
				crossItemAttribute.EnableAlternateSubstitution = enableAlternateSubstitution;
		}

		public static void SetEnableAlternateSubstitution(PXCache cache, object row, Type field, bool enableAlternateSubstitution)
		{
			foreach (var crossItemAttribute in cache.GetAttributes(row, field.Name).OfType<CrossItemAttribute>())
				crossItemAttribute.EnableAlternateSubstitution = enableAlternateSubstitution;
		}
		#endregion
	}

	#endregion

	public enum AlternateIDOnChangeAction
	{
		StoreLocally,
		InsertNew,
		UpdateOriginal,
		AskUser,
	}

	#region AlternativeItemAttribute
	[PXUIField(DisplayName = "Alternate ID")]
	[PXDBString(50, IsUnicode = true, InputMask = "")]
	public class AlternativeItemAttribute : PXAggregateAttribute, IPXRowUpdatingSubscriber, IPXRowDeletingSubscriber, IPXRowInsertingSubscriber
	{
		#region State
		protected INPrimaryAlternateType? _PrimaryAltType;
		protected Type _InventoryID;
		protected Type _SubItemID;
		protected Type _UOM;
		protected Type _BAccountID;
		protected Type _AlternateIDChangeAction;
		
		protected AlternateIDOnChangeAction? _OnChangeAction;
		protected bool _KeepSinglePrimaryAltID = true;

		private PXView _xRefView;

		#endregion

		#region Ctor
		public AlternativeItemAttribute(INPrimaryAlternateType PrimaryAltType, Type InventoryID, Type SubItemID, Type uom)
			: this(PrimaryAltType, null, InventoryID, SubItemID, uom) {}

		public AlternativeItemAttribute(INPrimaryAlternateType PrimaryAltType, Type BAccountID, Type InventoryID, Type SubItemID, Type uom)
		{
			_PrimaryAltType = PrimaryAltType;
			_BAccountID = BAccountID;
			_InventoryID = InventoryID;
			_SubItemID = SubItemID;
			_UOM = uom;

			Type defaultType = GenericCall.Of(() => BuildDefaultRuleType<IBqlField, IBqlField, IBqlField, BqlNone>())
										  .ButWith(InventoryID, SubItemID, uom, CreateWhereAltType(PrimaryAltType, BAccountID));

			Type formulaType =
				_BAccountID != null
				? BqlCommand.Compose(typeof(Default<,,,>), InventoryID, SubItemID, uom, BAccountID)
				: BqlCommand.Compose(typeof(Default<,,>), InventoryID, SubItemID, uom);
		
			this._Attributes.Add(new PXDefaultAttribute(defaultType) { PersistingCheck = PXPersistingCheck.Nothing });
			this._Attributes.Add(new PXFormulaAttribute(formulaType));
		}

		private static Type BuildDefaultRuleType<TInventoryID, TSubItemID, TUOM, TWhereAltType>() 
			where TInventoryID : IBqlField
			where TSubItemID : IBqlField
			where TUOM: IBqlField
			where TWhereAltType : IBqlWhere, new()
			=> typeof(
				Coalesce<
						Search<INItemXRef.alternateID,
							Where<INItemXRef.inventoryID, Equal<Current<TInventoryID>>,
								And<INItemXRef.subItemID, Equal<Current<TSubItemID>>,
							And2<Where<INItemXRef.uOM, Equal<Current2<TUOM>>, Or<INItemXRef.uOM, IsNull>>,
								And<TWhereAltType>>>>,
						OrderBy<Asc<Switch<Case<Where<INItemXRef.alternateType, Equal<INAlternateType.global>>, int1,
										   Case<Where<INItemXRef.alternateType, Equal<INAlternateType.barcode>>, int2>>,
										   int0>,
								Desc<INItemXRef.uOM>>>>,
						Search2<INItemXRef.alternateID,
							InnerJoin<InventoryItem, On<INItemXRef.FK.InventoryItem>>,
							Where<INItemXRef.inventoryID, Equal<Current<TInventoryID>>,
								And<INItemXRef.subItemID, Equal<InventoryItem.defaultSubItemID>,
							And2<Where<INItemXRef.uOM, Equal<Current2<TUOM>>, Or<INItemXRef.uOM, IsNull>>,
								And<TWhereAltType>>>>,
						OrderBy<Asc<Switch<Case<Where<INItemXRef.alternateType, Equal<INAlternateType.global>>, int1,
										   Case<Where<INItemXRef.alternateType, Equal<INAlternateType.barcode>>, int2>>,
										   int0>,
								Desc<INItemXRef.uOM>>>>>);

		private static Type CreateWhereAltType(INPrimaryAlternateType? primaryAltType, Type bAccountID)
		{
			Type whereAltType;
			switch (primaryAltType)
			{
				case INPrimaryAlternateType.CPN:
					whereAltType = GenericCall.Of(() => CreateWhereForNonGlobalAltType<IBqlOperand, IBqlField>())
											  .ButWith(typeof(INAlternateType.cPN), bAccountID ?? typeof(Customer.bAccountID));
					break;
				case INPrimaryAlternateType.VPN:
					whereAltType = GenericCall.Of(() => CreateWhereForNonGlobalAltType<IBqlOperand, IBqlField>())
											  .ButWith(typeof(INAlternateType.vPN), bAccountID ?? typeof(Vendor.bAccountID));
					break;
				default:
					whereAltType = typeof(Where<INItemXRef.alternateType, Equal<INAlternateType.global>>);
					break;
			}
			return whereAltType;
		}

		private static Type CreateWhereForNonGlobalAltType<TAltType, TBAccountField>() 
			where TAltType : IBqlOperand 
			where TBAccountField : IBqlField
			=> typeof(Where<INItemXRef.alternateType, Equal<TAltType>,
				And<INItemXRef.bAccountID, Equal<Current<TBAccountField>>,
					Or<INItemXRef.alternateType, Equal<INAlternateType.global>,
					Or<INItemXRef.alternateType, Equal<INAlternateType.barcode>>>>>);
		
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (!sender.Graph.Views.TryGetValue("_" + typeof(INItemXRef) + "_", out _xRefView))
			{
				_xRefView = new PXView(sender.Graph, false, new Select<INItemXRef>());
				sender.Graph.Views.Add("_" + typeof(INItemXRef) + "_", _xRefView);
			}

			if (!sender.Graph.Views.Caches.Contains(typeof(INItemXRef)))
				sender.Graph.Views.Caches.Add(typeof(INItemXRef));

		}
		#endregion

		#region Implementation

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			UpdateAltNumber(
				sender,
				GetBAccountID(sender, e.Row),
				GetInventoryID(sender, e.Row),
				GetSubItemID(sender, e.Row), 
				GetUOM(sender, e.Row),
				null,
				GetAlternateID(sender, e.Row));
		}

		public virtual void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			bool isChangedInvID = IsChanged(sender, _InventoryID, e.Row, e.NewRow);
			bool isChangedSubID = IsChanged(sender, _SubItemID, e.Row, e.NewRow);
			bool isChangedBAccountID = IsChanged(sender, _BAccountID, e.Row, e.NewRow);
			bool isChangedUOM = IsChanged(sender, _UOM, e.Row, e.NewRow);
			bool isChangedAltID = IsChanged(sender, _FieldName, e.Row, e.NewRow);

			if (isChangedInvID || isChangedSubID || isChangedBAccountID || isChangedUOM || isChangedAltID)
			{
				DeleteUnsavedNumber(
					sender,
					GetBAccountID(sender, e.Row),
					GetInventoryID(sender, e.Row),
					GetSubItemID(sender, e.Row), 
					GetUOM(sender, e.Row),
					GetAlternateID(sender, e.Row));

				if (!(isChangedInvID || isChangedSubID || isChangedBAccountID || isChangedUOM) && isChangedAltID)
				{
					UpdateAltNumber(
						sender,
						GetBAccountID(sender, e.NewRow),
						GetInventoryID(sender, e.NewRow),
						GetSubItemID(sender, e.NewRow), 
						GetUOM(sender, e.NewRow),
						GetAlternateID(sender, e.Row),
						GetAlternateID(sender, e.NewRow));
				}
			}
		}

		public virtual void RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			DeleteUnsavedNumber(sender,
				GetBAccountID(sender, e.Row),
				GetInventoryID(sender, e.Row),
				GetSubItemID(sender, e.Row), 
				GetUOM(sender, e.Row),
				GetAlternateID(sender, e.Row));
		}


		private void DeleteUnsavedNumber(PXCache sender, int? bAccountID, int? inventoryId, int? subItem, string uom, string altId)
		{
			if (inventoryId == null || subItem == null || altId == null) return;

			PXCache cache = sender.Graph.Caches[typeof(INItemXRef)];
			foreach (INItemXRef item in cache.Inserted)
			{
				if (item.BAccountID == bAccountID &&
						item.AlternateID == altId &&
						item.InventoryID == inventoryId &&
						item.SubItemID == subItem &&
						item.UOM == uom)
					cache.Delete(item);
			}
		}

		private void UpdateAltNumber(PXCache cache, int? bAccountID, int? inventoryId, int? subItemId, string uom, string oldAltID, string newAltID)
		{
			if (inventoryId == null || subItemId == null || newAltID == null || _PrimaryAltType == null || string.IsNullOrWhiteSpace(newAltID) || cache.Graph.IsImport || cache.Graph.IsCopyPasteContext) return;
			AlternateIDOnChangeAction action = this.GetOnChangeAction(cache.Graph);

			if (action == AlternateIDOnChangeAction.StoreLocally || cache.Graph.IsCopyPasteContext) return;
			PXSelectBase<INItemXRef> cmdFullSearch = 
				new PXSelect<INItemXRef,
					Where<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>,
					And<INItemXRef.inventoryID, Equal<Required<INItemXRef.inventoryID>>,
                    And<INItemXRef.subItemID, Equal<Required<INItemXRef.subItemID>>,
					And<Where<INItemXRef.uOM, Equal<Required<INItemXRef.uOM>>, Or<INItemXRef.uOM, IsNull>>>>>>,
				OrderBy<Asc<INItemXRef.alternateType, Desc<INItemXRef.alternateID>>>>(cache.Graph);
			AddAlternativeTypeWhere(cmdFullSearch, _PrimaryAltType, false);
			INItemXRef existing = cmdFullSearch.Select(newAltID, inventoryId, subItemId, uom, bAccountID);
            if (existing != null)
                return; //Applicable record with new AlternateID exists - no need to update Xref

			// Uniqueness validation
			PXSelectBase<INItemXRef> cmdAlt = 
				new PXSelect<INItemXRef,
					Where<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>,
					And<Where<INItemXRef.inventoryID, NotEqual<Required<INItemXRef.inventoryID>>,
							 Or<INItemXRef.subItemID, NotEqual<Required<INItemXRef.subItemID>>,
							 Or<INItemXRef.uOM, NotEqual<Required<INItemXRef.uOM>>>>>>>>(cache.Graph);
			AddAlternativeTypeWhere(cmdAlt, _PrimaryAltType, false);
			var cmdAltObj = cmdAlt.Select(newAltID, inventoryId, subItemId, uom, bAccountID).RowCast<INItemXRef>().FirstOrDefault();
			if (cmdAltObj != null && (cmdAltObj.InventoryID != inventoryId || cmdAltObj.SubItemID != subItemId))
				throw new AlternatieIDNotUniqueException(newAltID);
			if(cmdAltObj != null && cmdAltObj.UOM != uom)
			{
				var inventoryItem = InventoryItem.PK.Find(cache.Graph, cmdAltObj.InventoryID);
				throw new AlternatieIDNotUniqueException(cmdAltObj.AlternateID, inventoryItem.InventoryCD, cmdAltObj.UOM);
			}

			INItemXRef xref = null;
			if(action == AlternateIDOnChangeAction.UpdateOriginal || action == AlternateIDOnChangeAction.AskUser)
			{
				PXSelectBase<INItemXRef> cmdInv =
					new PXSelect<INItemXRef,
					Where<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>,
					And<INItemXRef.inventoryID, Equal<Required<INItemXRef.inventoryID>>,
					And<INItemXRef.subItemID, Equal<Required<INItemXRef.subItemID>>,
					And<Where<INItemXRef.uOM, Equal<Required<INItemXRef.uOM>>, Or<INItemXRef.uOM, IsNull>>>>>>, 
					OrderBy<Asc<INItemXRef.alternateType, Desc<INItemXRef.alternateID, Desc<INItemXRef.uOM>>>>>(cache.Graph);
				AddAlternativeTypeWhere(cmdInv, _PrimaryAltType, true);
				xref = cmdInv.Select(oldAltID, inventoryId, subItemId, uom, bAccountID);

				if (xref != null)
				{
					if (string.IsNullOrEmpty(xref.AlternateID) || action != AlternateIDOnChangeAction.AskUser || UserWantsToUpdateXRef())
						_xRefView.Cache.Delete(xref);
					else
							return; // Store locally						
					}
				else
				{
					if (this._KeepSinglePrimaryAltID)
					{
						PXSelectBase<INItemXRef> cmdOtherInv = 
							new PXSelect<INItemXRef,
							Where<INItemXRef.alternateID, NotEqual<Required<INItemXRef.alternateID>>,
							And<INItemXRef.alternateID, NotEqual<Empty>,
							And<INItemXRef.alternateID, IsNotNull,
							And<INItemXRef.inventoryID, Equal<Required<INItemXRef.inventoryID>>,
							And<INItemXRef.subItemID, Equal<Required<INItemXRef.subItemID>>,
							And<Where<INItemXRef.uOM, Equal<Required<INItemXRef.uOM>>, Or<INItemXRef.uOM, IsNull>>>>>>>>>(cache.Graph);
						AddAlternativeTypeWhere(cmdOtherInv, _PrimaryAltType, true);
						if (cmdOtherInv.Select(newAltID, inventoryId, subItemId, uom, bAccountID).Any())
							return; // There is another
					}
				}
			}

			bool createNew = xref == null;

			xref = createNew
				? new INItemXRef()
				: (INItemXRef) _xRefView.Cache.CreateCopy(xref);
			xref.InventoryID = inventoryId;
			xref.SubItemID = subItemId;
			xref.BAccountID = bAccountID;
			xref.AlternateID = newAltID;
			if (createNew || xref.UOM != null)
				xref.UOM = uom;
			xref.AlternateType = INAlternateType.ConvertFromPrimary(_PrimaryAltType.Value);
			_xRefView.Cache.Update(xref);
			_xRefView.Answer = WebDialogResult.None;
		} 

		private Boolean UserWantsToUpdateXRef() => _xRefView.Ask(Messages.ConfirmationXRefUpdate, MessageButtons.YesNo, false) == WebDialogResult.Yes;

		public static void AddAlternativeTypeWhere(PXSelectBase<INItemXRef> cmd, INPrimaryAlternateType? primaryAlternateType, bool typeExclusive)
		{
			switch (primaryAlternateType)
			{
				case INPrimaryAlternateType.CPN:
					if (typeExclusive)
					{
						cmd.WhereAnd<Where<INItemXRef.alternateType, Equal<INAlternateType.cPN>,
						And<INItemXRef.bAccountID, Equal<Required<INItemXRef.bAccountID>>>>>();
					}
					else
					{
						cmd.WhereAnd<Where<INItemXRef.alternateType, Equal<INAlternateType.cPN>,
						And<INItemXRef.bAccountID, Equal<Required<INItemXRef.bAccountID>>,
							Or<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>,
								And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>>>();
					}
					break;
				case INPrimaryAlternateType.VPN:
					if (typeExclusive)
					{
						cmd.WhereAnd<Where<INItemXRef.alternateType, Equal<INAlternateType.vPN>,
							And<INItemXRef.bAccountID, Equal<Required<INItemXRef.bAccountID>>>>>();
					}
					else
					{
						cmd.WhereAnd<Where<INItemXRef.alternateType, Equal<INAlternateType.vPN>,
						And<INItemXRef.bAccountID, Equal<Required<INItemXRef.bAccountID>>,
							Or<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>,
								And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>>>();
					}
					break;
				default:
					cmd.WhereAnd<Where<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>,
							And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>();
					break;
			}
		}

		#region Fields value getters
		private string GetAlternateID(PXCache sender, object row) => (string)sender.GetValue(row, _FieldName);
		private int? GetSubItemID(PXCache cache, object row) => GetCurrentValue<int?>(cache, _SubItemID, row);
		private int? GetInventoryID(PXCache cache, object row) => GetCurrentValue<int?>(cache, _InventoryID, row);
		private string GetUOM(PXCache cache, object row) => GetCurrentValue<string>(cache, _UOM, row);
		private int? GetBAccountID(PXCache cache, object row)
			=> _BAccountID == null
				? (_PrimaryAltType == INPrimaryAlternateType.VPN
					? GetCurrentValue<int?>(cache, typeof(Vendor.bAccountID))
					: GetCurrentValue<int?>(cache, typeof(Customer.bAccountID)))
				: GetCurrentValue<int?>(cache, _BAccountID, row);

		private TOut GetCurrentValue<TOut>(PXCache cache, Type field, object row)
		{
			PXCache source = cache.Graph.Caches[BqlCommand.GetItemType(field)];
			return (TOut)source.GetValue(row, field.Name);
		}

		private TOut GetCurrentValue<TOut>(PXCache cache, Type field)
		{
			PXCache source = cache.Graph.Caches[BqlCommand.GetItemType(field)];
			return (TOut)source.GetValue(source.Current, field.Name);
		} 
		#endregion

		private bool IsChanged(PXCache cache, Type fieldSource, object row, object newrow)
			=> fieldSource != null
				&& BqlCommand.GetItemType(fieldSource).IsAssignableFrom(cache.GetItemType())
				&& IsChanged(cache, fieldSource.Name, row, newrow);

		private bool IsChanged(PXCache cache, string fieldName, object row, object newrow) 
			=> Equals(cache.GetValue(newrow, fieldName), cache.GetValue(row, fieldName)) == false;

		private AlternateIDOnChangeAction GetOnChangeAction(PXGraph caller) 
		{
			if (this._OnChangeAction == null) 
			{
				this._OnChangeAction = AlternateIDOnChangeAction.AskUser;
			}
			return this._OnChangeAction.Value;
		}
		#endregion
	}
	#endregion

	#region PriceWorksheetAlternateItemAttribute
	[PXDBString(50, IsUnicode = true, InputMask = "")]
	[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
	[PXUIField(DisplayName = "Alternate ID", Visibility = PXUIVisibility.Dynamic)]
	public class PriceWorksheetAlternateItemAttribute : PXAggregateAttribute
	{
		public class PriceWrapper
		{
			public PriceWrapper(object priceWorksheetDetail)
			{
				if (priceWorksheetDetail == null)
					throw new ArgumentNullException(nameof(priceWorksheetDetail));

				var arPrice = priceWorksheetDetail as ARPriceWorksheetDetail;
				if (arPrice != null)
				{
					PriceWorksheetDetail = arPrice;

					AlternateID = arPrice.AlternateID;
					BAccountID = arPrice.CustomerID;
					InventoryID = arPrice.InventoryID;
					SubItemID = arPrice.SubItemID;

					AlternateType = arPrice.PriceType == PriceTypeList.Customer ? INPrimaryAlternateType.CPN.AsNullable() : null;

					UOMField = typeof(ARPriceWorksheetDetail.uOM);
					InventoryIDField = typeof(ARPriceWorksheetDetail.inventoryID);
					AlternateIDField = typeof(ARPriceWorksheetDetail.alternateID);
					RestrictInventoryByAlternateIDField = typeof(ARPriceWorksheetDetail.restrictInventoryByAlternateID);

					return;
				}

				var apPrice = priceWorksheetDetail as APPriceWorksheetDetail;
				if (apPrice != null)
				{
					PriceWorksheetDetail = apPrice;

					AlternateID = apPrice.AlternateID;
					BAccountID = apPrice.VendorID;
					InventoryID = apPrice.InventoryID;
					SubItemID = apPrice.SubItemID;

					AlternateType = INPrimaryAlternateType.VPN;

					UOMField = typeof(APPriceWorksheetDetail.uOM);
					InventoryIDField = typeof(APPriceWorksheetDetail.inventoryID);
					AlternateIDField = typeof(APPriceWorksheetDetail.alternateID);
					RestrictInventoryByAlternateIDField = typeof(APPriceWorksheetDetail.restrictInventoryByAlternateID);

					return;
				}

				throw new PXArgumentException("Attribute supports only {0} and {1} entities", typeof(ARPriceWorksheetDetail), typeof(APPriceWorksheetDetail));
			}

			public object PriceWorksheetDetail { get; }

			public string AlternateID { get; }
			public int? BAccountID { get; }
			public int? InventoryID { get; }
			public int? SubItemID { get; }

			public INPrimaryAlternateType? AlternateType { get; }

			public Type UOMField { get; }
			public Type InventoryIDField { get; }
			public Type AlternateIDField { get; }
			public Type RestrictInventoryByAlternateIDField { get; }
		}

		public string[] AlternateTypePriority { get; set; } = { INAlternateType.CPN, INAlternateType.VPN, INAlternateType.Barcode, INAlternateType.GIN, INAlternateType.Global };


		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldVerifying.AddHandler(typeof(INItemXRef), nameof(INItemXRef.BAccountID), INItemXRef_BAccountID_FieldVerifying);
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), "InventoryID", PriceWorksheetDetail_InventoryID_FieldUpdated);
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), "AlternateID", PriceWorksheetDetail_AlternateID_FieldUpdated);
			sender.Graph.RowSelected.AddHandler(sender.GetItemType(), PriceWorksheetDetail_RowSelected);
		}


		protected virtual void INItemXRef_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var xRef = (INItemXRef)e.Row;
			if (xRef.AlternateType != INAlternateType.VPN && xRef.AlternateType != INAlternateType.CPN)
				e.Cancel = true;
		}

		protected virtual void PriceWorksheetDetail_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var priceWrapper = new PriceWrapper(e.Row);
			PresetUOMWithSpecialUnits(sender, priceWrapper);
			UpdateUOMFromCrossReference(sender, priceWrapper, false);
		}

		protected virtual void PriceWorksheetDetail_AlternateID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			UpdateUOMFromCrossReference(sender, new PriceWrapper(e.Row), false);
		}

		protected virtual void PriceWorksheetDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;
			UpdateUOMFromCrossReference(sender, new PriceWrapper(e.Row), true);
		}


		private static void PresetUOMWithSpecialUnits(PXCache sender, PriceWrapper priceWrapper)
		{
			var inventory = InventoryItem.PK.Find(sender.Graph, priceWrapper.InventoryID);
			if (priceWrapper.AlternateType == INPrimaryAlternateType.VPN)
		{
				if (inventory?.PurchaseUnit != null)
					sender.SetValueExt(priceWrapper.PriceWorksheetDetail, priceWrapper.UOMField.Name, inventory.PurchaseUnit);
			}
			else
			{
				if (inventory?.SalesUnit != null)
					sender.SetValueExt(priceWrapper.PriceWorksheetDetail, priceWrapper.UOMField.Name, inventory.SalesUnit);
			}
		}

		private static void UpdateUOMFromCrossReference(PXCache cache, PriceWrapper priceWorksheetDetail, bool warningSettingOnly)
		{
			bool loadSalesPricesUsingAlternateID =
				priceWorksheetDetail.AlternateType != INPrimaryAlternateType.VPN
				&& PXAccess.FeatureInstalled<FeaturesSet.distributionModule>()
				&& new PXSetupOptional<ARSetup>(cache.Graph).Current.LoadSalesPricesUsingAlternateID == true;
			bool loadVendorsPricesUsingAlternateID =
				priceWorksheetDetail.AlternateType == INPrimaryAlternateType.VPN
				&& PXAccess.FeatureInstalled<FeaturesSet.distributionModule>()
				&& new PXSetupOptional<APSetup>(cache.Graph).Current.LoadVendorsPricesUsingAlternateID == true;

			if (loadVendorsPricesUsingAlternateID == false && loadSalesPricesUsingAlternateID == false)
				return;

			ClearWarning(cache, priceWorksheetDetail);
			RestrictInventoryByAlternateID(cache, priceWorksheetDetail, false);

			if (priceWorksheetDetail.AlternateID.IsNullOrEmpty())
				return;

			var xRefs = SelectXRefs(cache, priceWorksheetDetail);

			if (priceWorksheetDetail.InventoryID == null)
			{
				if (xRefs.Length == 0)
				{
					SetWarning(cache, priceWorksheetDetail, Messages.NoSpecifiedAltID);
		}
				else if (xRefs.Length == 1)
				{
					if (warningSettingOnly) return;
					var xref = xRefs.Single();

					cache.SetValueExt(priceWorksheetDetail.PriceWorksheetDetail, priceWorksheetDetail.InventoryIDField.Name, xref.InventoryID);
					if (xref.UOM != null)
					{
						cache.SetValueExt(priceWorksheetDetail.PriceWorksheetDetail, priceWorksheetDetail.UOMField.Name, xref.UOM);
						RestrictInventoryByAlternateID(cache, priceWorksheetDetail, true);//keep flag to do not reset UOM by PXFormulaAttribute
					}
				}
				else
				{
					SetWarning(cache, priceWorksheetDetail, Messages.ManyAltIDsForSingleInventoryID);
					RestrictInventoryByAlternateID(cache, priceWorksheetDetail, true);
				}
			}
			else
		{
				var xref = xRefs.FirstOrDefault();
				if (xref == null)
			{
					if (PXAccess.FeatureInstalled<FeaturesSet.crossReferenceUniqueness>() && ExistsGlobalXRefWithSameAltID(cache, priceWorksheetDetail))
						SetWarning(cache, priceWorksheetDetail, Messages.AltIDIsNotDefinedAndWillNotBeAddedOnRelease);
					else
						SetWarning(cache, priceWorksheetDetail, Messages.AltIDIsNotDefinedAndWillBeAddedOnRelease);
				}
				else if (warningSettingOnly == false && xref.UOM != null)
				{
					cache.SetValueExt(priceWorksheetDetail.PriceWorksheetDetail, priceWorksheetDetail.UOMField.Name, xref.UOM);
					RestrictInventoryByAlternateID(cache, priceWorksheetDetail, true);//keep flag to do not reset UOM by PXFormulaAttribute
				}
			}
		}

		private static bool ExistsGlobalXRefWithSameAltID(PXCache cache, PriceWrapper priceWorksheetDetail)
		{
			PXSelectBase<INItemXRef> cmdInv = 
				new PXSelectReadonly<INItemXRef, 
				Where<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>,
				And<INItemXRef.alternateType, Equal<INAlternateType.global>>>>(cache.Graph);

			return cmdInv
				.Select(priceWorksheetDetail.AlternateID)
				.RowCast<INItemXRef>()
				.Any(xr => (xr.InventoryID == priceWorksheetDetail.InventoryID && xr.SubItemID == priceWorksheetDetail.SubItemID) == false);
		}

		private static INItemXRef[] SelectXRefs(PXCache cache, PriceWrapper priceWorksheetDetail)
		{
			PXSelectBase<INItemXRef> cmdInv =
				new PXSelectReadonly<INItemXRef,
					Where<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>>,
					OrderBy<Asc<INItemXRef.alternateType, Desc<INItemXRef.alternateID>>>>(cache.Graph);
			var parameters = new List<object> { priceWorksheetDetail.AlternateID };

			AlternativeItemAttribute.AddAlternativeTypeWhere(cmdInv, priceWorksheetDetail.AlternateType, false);
			if (priceWorksheetDetail.AlternateType != null)
				parameters.Add(priceWorksheetDetail.BAccountID);

			if (priceWorksheetDetail.InventoryID != null)
		{
				cmdInv.WhereAnd<
					Where<INItemXRef.inventoryID, Equal<Required<INItemXRef.inventoryID>>>>();
				parameters.Add(priceWorksheetDetail.InventoryID);
			}

			if (priceWorksheetDetail.SubItemID != null)
			{
				cmdInv.WhereAnd<
					Where<INItemXRef.subItemID, Equal<Required<INItemXRef.subItemID>>>>();
				parameters.Add(priceWorksheetDetail.SubItemID);
			}

			var xRefs = cmdInv.Select(parameters.ToArray()).RowCast<INItemXRef>().ToArray();

			string[] alternateTypePriority =
				cache.GetAttributesOfType<PriceWorksheetAlternateItemAttribute>(
					priceWorksheetDetail.PriceWorksheetDetail,
					priceWorksheetDetail.AlternateIDField.Name)
					.FirstOrDefault()?
					.AlternateTypePriority;

			var typeGroups = xRefs.GroupBy(x => x.AlternateType).OrderBy(gx => gx.Key, alternateTypePriority);

			return (typeGroups.FirstOrDefault(gx => gx.Any()) ?? Enumerable.Empty<INItemXRef>()).ToArray();
			}

		private static void SetWarning(PXCache cache, PriceWrapper priceWorksheetDetail, string message)
		{
			cache.RaiseExceptionHandling(
				priceWorksheetDetail.AlternateIDField.Name,
				priceWorksheetDetail.PriceWorksheetDetail,
				priceWorksheetDetail.AlternateID,
				message.IsNullOrEmpty()
					? null
					: new PXSetPropertyException(message, PXErrorLevel.Warning));
		}

		private static void ClearWarning(PXCache cache, PriceWrapper priceWorksheetDetail) => SetWarning(cache, priceWorksheetDetail, null);
		private static void RestrictInventoryByAlternateID(PXCache cache, PriceWrapper priceWorksheetDetail, bool enable)
			=> cache.SetValue(priceWorksheetDetail.PriceWorksheetDetail, priceWorksheetDetail.RestrictInventoryByAlternateIDField.Name, enable);

		public static bool XRefsExists(PXCache cache, object priceWorksheetDetail) => SelectXRefs(cache, new PriceWrapper(priceWorksheetDetail)).Any();
	}
	#endregion

	#region StockItemAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
    [PXRestrictor(typeof(Where<InventoryItem.stkItem, Equal<boolTrue>>), Messages.InventoryItemIsNotAStock)]
    public class StockItemAttribute : InventoryAttribute
	{
		public StockItemAttribute()
            : base()
        {
        }
	}

	#endregion

	#region NonStockItemAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	public class NonStockItemAttribute : InventoryAttribute
	{
		public static Type Search => typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>);

		public static PXRestrictorAttribute CreateRestrictor()
			=> new PXRestrictorAttribute(typeof(Where<InventoryItem.stkItem, Equal<boolFalse>, And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>>>>), Messages.InventoryItemIsAStock);

		public static PXRestrictorAttribute CreateRestrictorDependingOnFeature<TFeature>()
			where TFeature : IBqlField
			=> new PXRestrictorAttribute(
				typeof(Where2<FeatureInstalled<TFeature>, Or<InventoryItem.stkItem, Equal<boolFalse>>>),
				Messages.InventoryItemIsAStock);

		public NonStockItemAttribute() : base(Search, typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{			
			_Attributes.Add(CreateRestrictor());
		}
	}

	#endregion

    #region NonStockNonKitItemAttribute

    public class NonStockNonKitItemAttribute : NonStockItemAttribute
    {
		public static PXRestrictorAttribute CreateStandardRestrictor()
			=> new PXRestrictorAttribute(
				typeof(Where<InventoryItem.stkItem, NotEqual<False>, Or<InventoryItem.kitItem, NotEqual<True>>>),
				Messages.CannotAddNonStockKit);

		public static PXRestrictorAttribute CreateCustomRestrictor<TOrigNbrField>(String aErrorMsg)
			where TOrigNbrField : IBqlField
			=> new PXRestrictorAttribute(
				typeof(Where<Current<TOrigNbrField>, IsNotNull, Or<InventoryItem.stkItem, NotEqual<False>, Or<InventoryItem.kitItem, NotEqual<True>>>>),
				aErrorMsg);

		public NonStockNonKitItemAttribute()
		{
			_Attributes.Add(CreateStandardRestrictor());
		}

		public NonStockNonKitItemAttribute(String aErrorMsg, Type origNbrField)
		{
			_Attributes.Add(GenericCall.Of(() => CreateCustomRestrictor<IBqlField>(aErrorMsg)).ButWith(origNbrField));
		}
        }

	#endregion

	#region NonStockNonKitCrossItemAttribute

	public class NonStockNonKitCrossItemAttribute : CrossItemAttribute
	{
		private NonStockNonKitCrossItemAttribute(Type search, INPrimaryAlternateType primaryAltType) : base(
			search,
			typeof(InventoryItem.inventoryCD),
			typeof(InventoryItem.descr),
			primaryAltType) {}

		public NonStockNonKitCrossItemAttribute(INPrimaryAlternateType primaryAltType)
			: this(NonStockItemAttribute.Search, primaryAltType)
		{
			_Attributes.Add(NonStockItemAttribute.CreateRestrictor());
			_Attributes.Add(NonStockNonKitItemAttribute.CreateStandardRestrictor());
    }

		public NonStockNonKitCrossItemAttribute(INPrimaryAlternateType primaryAltType, String aErrorMsg, Type origNbrField)
			: this(NonStockItemAttribute.Search, primaryAltType)
		{
			_Attributes.Add(NonStockItemAttribute.CreateRestrictor());
			_Attributes.Add(GenericCall.Of(() => NonStockNonKitItemAttribute.CreateCustomRestrictor<IBqlField>(aErrorMsg)).ButWith(origNbrField));
		}

		public NonStockNonKitCrossItemAttribute(INPrimaryAlternateType primaryAltType, String aErrorMsg, Type origNbrField, Type allowStkFeature)
			: this(NonStockItemAttribute.Search, primaryAltType)
		{
			_Attributes.Add(GenericCall.Of(() => NonStockItemAttribute.CreateRestrictorDependingOnFeature<IBqlField>()).ButWith(allowStkFeature));
			_Attributes.Add(GenericCall.Of(() => NonStockNonKitItemAttribute.CreateCustomRestrictor<IBqlField>(aErrorMsg)).ButWith(origNbrField));
		}
	}
	#endregion

	#region BaseInventoryAttribute

	/// <summary>
	/// Provides a base selector for the Inventory Items. The list is filtered by the user access rights.
	/// </summary>
	[PXDBInt]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	public abstract class BaseInventoryAttribute : PXEntityAttribute
	{
		#region State
		public const string DimensionName = "INVENTORY";

		public class dimensionName : BqlString.Constant<dimensionName>
		{
			public dimensionName() : base(DimensionName) {; }
		}
		#endregion
		#region Ctor
		public BaseInventoryAttribute()
			: this(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{
		}

		public BaseInventoryAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField)
			: base()
		{
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, SubstituteKey);
			attr.CacheGlobal = true;
			attr.DescriptionField = DescriptionField;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public BaseInventoryAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField, Type[] fields)
		{
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, SubstituteKey, fields);
			attr.CacheGlobal = true;
			attr.DescriptionField = DescriptionField;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
		#endregion
	}

	#endregion

	#region InventoryIncludingTemplatesAttribute

	/// <summary>
	/// Provides a selector for the Inventory Items including Template Items.
	/// The list is filtered by the user access rights and Inventory Item status - inactive and marked to delete items are not shown.
	/// </summary>
	[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>>), PM.Messages.ReservedForProject, ShowWarning = true)]
	[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
		And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>),
		Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
	public class InventoryIncludingTemplatesAttribute : BaseInventoryAttribute
	{
		#region Ctor
		public InventoryIncludingTemplatesAttribute()
			: base()
		{
		}

		public InventoryIncludingTemplatesAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField)
			: base(SearchType, SubstituteKey, DescriptionField)
		{
		}

		public InventoryIncludingTemplatesAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField, Type[] fields)
			: base(SearchType, SubstituteKey, DescriptionField, fields)
		{
		}
		#endregion
	}
	#endregion

	#region AnyInventoryAttribute

	/// <summary>
	/// Provides a base selector for the Inventory Items. The list is filtered by the user access rights and excludes Template and Unknown items.
	/// </summary>
	[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>>), PM.Messages.ReservedForProject, ShowWarning = true)]
	[PXRestrictor(typeof(Where<InventoryItem.isTemplate, Equal<False>>), Messages.InventoryItemIsATemplate, ShowWarning = true)]
	public class AnyInventoryAttribute : BaseInventoryAttribute
	{
		#region Ctor
		public AnyInventoryAttribute()
			: this(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{
		}

		public AnyInventoryAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField)
			: base(SearchType, SubstituteKey, DescriptionField)
		{
		}

		public AnyInventoryAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField, Type[] fields)
			: base(SearchType, SubstituteKey, DescriptionField, fields)
		{
		}
		#endregion
	}

	#endregion

	#region TemplateInventoryAttribute

	/// <summary>
	/// Provides a base selector for the Template Inventory Items. The list is filtered by the user access rights.
	/// </summary>
	[PXUIField(DisplayName = "Template ID", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<InventoryItem.isTemplate, Equal<True>>), Messages.InventoryItemIsNotATemplate, ShowWarning = true)]
	public class TemplateInventoryAttribute : BaseInventoryAttribute
	{
		#region Ctor
		public TemplateInventoryAttribute()
			: this(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{
		}

		public TemplateInventoryAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField)
			: base(SearchType, SubstituteKey, DescriptionField)
		{
		}
		#endregion
	}

	#endregion

	#region InventoryAttribute

	/// <summary>
	/// Provides a selector for the Inventory Items.
	/// The list is filtered by the user access rights and Inventory Item status - inactive and marked to delete items are not shown.
	/// </summary>
	[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
		And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>),
		Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
	public class InventoryAttribute : AnyInventoryAttribute
	{
		#region Ctor
		public InventoryAttribute()
			: base()
		{
		}

		public InventoryAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField)
			: base(SearchType, SubstituteKey, DescriptionField)
		{
		}

		public InventoryAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField, Type[] fields)
			: base(SearchType, SubstituteKey, DescriptionField, fields)
		{
		}
		#endregion
	}
	#endregion

	#region SubItemRawAttribute
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subitem ID", Visibility = PXUIVisibility.SelectorVisible, FieldClass = DimensionName)]
	public class SubItemRawAttribute : PXEntityAttribute
	{
		public bool SuppressValidation;
		public const string DimensionName = "INSUBITEM";

		public SubItemRawAttribute()
			: base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(DimensionName);
			attr.ValidComboRequired = false;

			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	#endregion

	#region SubItemRawExtAttribute
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subitem ID", Visibility = PXUIVisibility.SelectorVisible, FieldClass = DimensionName)]
	public class SubItemRawExtAttribute : PXEntityAttribute
	{
		public const string DimensionName = "INSUBITEM";

		#region Ctors
		public SubItemRawExtAttribute()
			: base()
		{
		}

		public SubItemRawExtAttribute(Type inventoryItem)
			: this()
		{
			if (inventoryItem != null)
			{
				Type SearchType = BqlCommand.Compose(
					typeof(Search<,>),
					typeof(INSubItem.subItemCD),
					typeof(Where2<,>),
					typeof(Match<>),
					typeof(Current<AccessInfo.userName>),
					typeof(And<>),
					typeof(Where<,,>),
					typeof(Optional<>), inventoryItem, typeof(IsNull),
					typeof(Or<>),
					typeof(Where<>),
					typeof(Match<>),
					typeof(Optional<>),
					inventoryItem);

				var attr = new PXDimensionSelectorAttribute(DimensionName, SearchType);
				attr.ValidComboRequired = false;
				_Attributes.Add(attr);
				_SelAttrIndex = _Attributes.Count - 1;
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			{
				((PXDimensionSelectorAttribute)this._Attributes[_Attributes.Count - 1]).ValidComboRequired = false;
				((PXDimensionSelectorAttribute)this._Attributes[_Attributes.Count - 1]).SetSegmentDelegate(null);
				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, FieldDefaulting);
			}

			base.CacheAttached(sender);
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = this.Definitions.DefaultSubItemCD;
			e.Cancel = true;
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				subscribers.Clear();
			}
		}


		#endregion

		#region Default SubItemID
		protected virtual Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("INSubItem.DefinitionCD", typeof(INSubItem)));
				}
				return defs;
			}
		}

		protected class Definition : IPrefetchable
		{
			private string _DefaultSubItemCD;
			public string DefaultSubItemCD
			{
				get { return _DefaultSubItemCD; }
			}

			public void Prefetch()
			{
				using (PXDataRecord record = PXDatabase.SelectSingle<INSubItem>(
					new PXDataField<INSubItem.subItemCD>(),
					new PXDataFieldOrder<INSubItem.subItemID>()))
				{
					_DefaultSubItemCD = null;
					if (record != null)
						_DefaultSubItemCD = record.GetString(0);
				}
			}
		}
		#endregion

	}
	#endregion

	#region inventoryModule

	public sealed class inventoryModule : PX.Data.BQL.BqlString.Constant<inventoryModule>
	{
		public inventoryModule()
			: base(typeof(inventoryModule).Namespace)
		{
		}
	}

	#endregion

	#region warehouseType

	public sealed class warehouseType : PX.Data.BQL.BqlString.Constant<warehouseType>
	{
		public warehouseType()
			: base(typeof(PX.Objects.IN.INSite).FullName)
		{
		}
	}

	#endregion

	#region itemType

	public sealed class itemType : PX.Data.BQL.BqlString.Constant<itemType>
	{
		public itemType()
			: base(typeof(PX.Objects.IN.InventoryItem).FullName)
		{
		}
	}

	#endregion

	#region itemClassType

	public sealed class itemClassType : PX.Data.BQL.BqlString.Constant<itemClassType>
	{
		public itemClassType()
			: base(typeof(PX.Objects.IN.INItemClass).FullName)
		{
		}
	}

	#endregion

	#region INSetupSelect

	public sealed class INSetupSelect : Data.PXSetupSelect<INSetup>
	{
		public INSetupSelect(PXGraph graph) : base(graph) { }
	}

	#endregion

	#region SubItemAttribute
	[PXDBInt]
	[PXUIField(DisplayName = "Subitem", Visibility = PXUIVisibility.Visible, FieldClass = SubItemAttribute.DimensionName)]
	public class SubItemAttribute : PXEntityAttribute
	{
		const string DefaultSubItemValue = "0";

		#region dimensionName

		public class dimensionName : PX.Data.BQL.BqlString.Constant<dimensionName>
		{
			public dimensionName() : base(DimensionName) { }
		}

		#endregion

		private class INSubItemDimensionAttribute : PXDimensionAttribute
		{
			Type _inventoryID;

			public INSubItemDimensionAttribute(Type inventoryID, string dimension)
				: base(dimension)
			{
				_inventoryID = inventoryID;
			}

			protected override bool FindValueBySegmentDelegate(PXCache sender, object row, string segmentDescr, short segmentID, string val, string currentValue)
			{
				bool match = base.FindValueBySegmentDelegate(sender, row, segmentDescr, segmentID, val, currentValue);

				if (!match)
				{
					Dictionary<string, ValueDescr> values = _Definition.Values[_Dimension][segmentID];
					if (values.ContainsKey(currentValue))
					{
						int? inventoryID = (int?)sender.GetValue(row, _inventoryID.Name);

						if (inventoryID != null)
						{
							var inventoryItem = InventoryItem.PK.Find(sender.Graph, inventoryID);
							if (val == DefaultSubItemValue && inventoryItem != null && inventoryItem.StkItem != true)
								return true;

							throw new PXSetPropertyException(Messages.SubItemIsDisabled, values[currentValue].Descr ?? currentValue,
								segmentDescr ?? segmentID.ToString(), inventoryItem?.InventoryCD ?? inventoryID.ToString());
						}
					}
				}

				return match;
			}
		}

		#region INSubItemDimensionSelector
		private class INSubItemDimensionSelector: PXDimensionSelectorAttribute
		{
			private readonly Type _inventoryID = null;

			private bool _ValidateValueOnFieldUpdating;
			public bool ValidateValueOnFieldUpdating
			{
				get
				{
					return _ValidateValueOnFieldUpdating;
				}
				set
				{
					this.SetSegmentDelegate(value ? (PXSelectDelegate<short?>)DoSegmentSelect : null);

					_ValidateValueOnFieldUpdating = value;
				}
			}

			public bool ValidateValueOnPersisting
			{
				get;
				set;
			}

			public INSubItemDimensionSelector(Type inventoryID, Type search)
				: base(DimensionName, search, typeof(INSubItem.subItemCD))
			{				
				this._inventoryID = inventoryID;				
				if (this._inventoryID != null)
				{
					int dimensionAttributeIndex = _Attributes.IndexOf(DimensionAttribute);
					_Attributes[dimensionAttributeIndex] = new INSubItemDimensionAttribute(inventoryID, DimensionName);

					this.CacheGlobal = false;
					this.ValidateValueOnFieldUpdating = true;
				}				 
			}		

			private IEnumerable DoSegmentSelect([PXShort] short? segment)
			{
				PXGraph graph = PXView.CurrentGraph;
				if (_inventoryID == null) yield break;

                int? inventoryID = null;
                if (PXView.Currents != null)
                    foreach (object item in PXView.Currents)
                    {
                        if (item.GetType() == _inventoryID.DeclaringType)
                            inventoryID = (int?)graph.Caches[_inventoryID.DeclaringType].GetValue(item, _inventoryID.Name);
                    }

				int startRow = PXView.StartRow;
				int totalRows = 0;
				
				PXView intView = new PXView(PXView.CurrentGraph, false,
					BqlCommand.CreateInstance(
						typeof(Select2<,,>),
						typeof(INSubItemSegmentValue),
						typeof(InnerJoin<SegmentValue,
										On<SegmentValue.segmentID, Equal<INSubItemSegmentValue.segmentID>,
									And<SegmentValue.value, Equal<INSubItemSegmentValue.value>,
									And<SegmentValue.dimensionID, Equal<SubItemAttribute.dimensionName>>>>>),
						typeof(Where<,,>), typeof(INSubItemSegmentValue.segmentID), typeof(Equal<Required<SegmentValue.segmentID>>),
						typeof(And<,>), typeof(INSubItemSegmentValue.inventoryID), typeof(Equal<>), typeof(Optional<>), _inventoryID));

				foreach (PXResult<INSubItemSegmentValue, SegmentValue> rec in intView
					.Select(PXView.Currents, new object[]{segment, inventoryID}, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
								 ref startRow, PXView.MaximumRows, ref totalRows))
				{
					SegmentValue value = rec;
					PXDimensionAttribute.SegmentValue ret = new PXDimensionAttribute.SegmentValue();					
					ret.Value = value.Value;					
					ret.Descr = value.Descr;
					ret.IsConsolidatedValue = value.IsConsolidatedValue;
					yield return ret;
				}
				PXView.StartRow = 0;
			}

			public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
			{
				base.RowPersisting(sender, e);

				if (ValidateValueOnPersisting &&
					e.Row != null && e.Operation == PXDBOperation.Insert)
				{
					try
					{
						object subItem = sender.GetValue(e.Row, _FieldOrdinal);
						sender.RaiseFieldVerifying(_FieldName, e.Row, ref subItem);
					}
					catch (PXSetPropertyException exc)
					{
						object value = sender.GetValue(e.Row, _FieldOrdinal);
						if (sender.RaiseExceptionHandling(_FieldName, e.Row, value, exc))
						{
							throw new PXRowPersistingException(_FieldName, value, exc.Message);
						}
					}
				}
			}
		}
		#endregion 

		#region Fields

		public const string DimensionName = "INSUBITEM";

        private bool _Disabled = false;
        public bool Disabled
        {
            get { return _Disabled; }
            set { _Disabled = value; }
        }

		public bool ValidateValueOnFieldUpdating
		{
			get => SelectorAttribute is INSubItemDimensionSelector selector
				? selector.ValidateValueOnFieldUpdating
				: false;
			set
			{
				if (SelectorAttribute is INSubItemDimensionSelector selector)
					selector.ValidateValueOnFieldUpdating = value;
				}
			}

		public bool ValidateValueOnPersisting
		{
			get => SelectorAttribute is INSubItemDimensionSelector selector
				? selector.ValidateValueOnPersisting
				: false;
			set
			{
				if (SelectorAttribute is INSubItemDimensionSelector selector)
					selector.ValidateValueOnPersisting = value;
				}
			}

		#endregion

		#region Ctors

		public SubItemAttribute()
			: base()	
		{
			//var attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(INSubItem.subItemCD));
			//attr.CacheGlobal = true;
			
			var attr = new PXDimensionSelectorAttribute(DimensionName, typeof(Search<INSubItem.subItemID, Where<Match<Current<AccessInfo.userName>>>>), typeof(INSubItem.subItemCD));
			attr.CacheGlobal = true;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public SubItemAttribute(Type inventoryID)
			: this(inventoryID, null)
		{					
		}

		public SubItemAttribute(Type inventoryID, Type JoinType)
			: base()
		{
			Type SearchType =
				JoinType == null ? typeof(Search<INSubItem.subItemID, Where<Match<Current<AccessInfo.userName>>>>) :
				BqlCommand.Compose(
				typeof(Search2<,,>),
				typeof(INSubItem.subItemID),
				JoinType,
				typeof(Where<>),
				typeof(Match<>),
				typeof(Current<AccessInfo.userName>));

			var attr =
				new INSubItemDimensionSelector(inventoryID, SearchType);
				
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;	
		}

		#endregion

		#region Implementation

		public override void CacheAttached(PXCache sender)
		{
			if (_Disabled || !PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			{
				((PXDimensionSelectorAttribute) this._Attributes[_Attributes.Count - 1]).ValidComboRequired = false;
				((PXDimensionSelectorAttribute)this._Attributes[_Attributes.Count - 1]).CacheGlobal = true;
				((PXDimensionSelectorAttribute)this._Attributes[_Attributes.Count - 1]).SetSegmentDelegate(null);
				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, FieldDefaulting);
			}

			base.CacheAttached(sender);
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.Definitions.DefaultSubItemID == null)
			{
				object newValue = DefaultSubItemValue;
				sender.RaiseFieldUpdating(_FieldName, e.Row, ref newValue);
				e.NewValue = newValue;
			}
			else
			{
				e.NewValue = this.Definitions.DefaultSubItemID;
			}

			e.Cancel = true;
		}

		#endregion

		#region Default SubItemID
		protected virtual Definition Definitions => GetDefinition();

		private static Definition GetDefinition()
		{
			Definition defs = PX.Common.PXContext.GetSlot<Definition>();
			if (defs == null)
			{
				defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("INSubItem.Definition", typeof(INSubItem)));
			}
			return defs;
		}
		
		protected class Definition : IPrefetchable
		{
			private int? _DefaultSubItemID;
			public int? DefaultSubItemID
			{
				get { return _DefaultSubItemID; }
			}

			public void Prefetch()
			{
				using (PXDataRecord record = PXDatabase.SelectSingle<INSubItem>(
					new PXDataField<INSubItem.subItemID>(),
					new PXDataFieldOrder<INSubItem.subItemID>()))
				{
					_DefaultSubItemID = null;
					if (record != null)
						_DefaultSubItemID = record.GetInt32(0);
				}
			}
		}

		public class defaultSubItemID : BqlInt.Constant<defaultSubItemID>
		{
			public defaultSubItemID() : base(-1) { }

			public override int Value => GetDefinition().DefaultSubItemID ?? base.Value;
		}
		#endregion
	}
	#endregion

	#region SiteRawAttribute

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Warehouse ID", Visibility = PXUIVisibility.SelectorVisible)]
	public sealed class SiteRawAttribute : PXEntityAttribute
	{
		public string DimensionName = "INSITE";
		public SiteRawAttribute(bool isTransitAllowed)
			: base()
		{
			Type SearchType = isTransitAllowed ? 
                typeof(Search<INSite.siteCD, Where<Match<Current<AccessInfo.userName>>>>) 
                : typeof(Search<INSite.siteCD, Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>, And<Match<Current<AccessInfo.userName>>>>>);
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(INSite.siteCD));
			attr.CacheGlobal = true;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	#endregion

	#region SiteAvailAttribute

	[PXDBInt]
	[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
	public class SOSiteAvailAttribute : SiteAvailAttribute
	{
		#region Ctor
		public SOSiteAvailAttribute()
			: base(typeof(SOLine.inventoryID), typeof(SOLine.subItemID), typeof(SOLine.costCenterID))
		{
		}
		#endregion

		public override void InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			using (new SOOrderPriceCalculationScope().AppendContext<SOLine.inventoryID>())
				base.InventoryID_FieldUpdated(sender, e);
		}
	}

	[PXDBInt()]
	[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
    public class POSiteAvailAttribute : SiteAvailAttribute
	{
		#region Ctor
		public POSiteAvailAttribute(Type InventoryType, Type SubItemType, Type costCenterType)
			: base(InventoryType, SubItemType, costCenterType)
		{
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldUpdated.RemoveHandler(sender.GetItemType(), _inventoryType.Name, InventoryID_FieldUpdated);
		}
		#endregion
		#region Implementation
		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
		}
        #endregion
	}

	[PXDBInt]
	[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
	public class SiteAvailAttribute : SiteAttribute, IPXFieldDefaultingSubscriber
	{
		[PXHidden]
		private class InventoryPh : BqlPlaceholderBase { }
		[PXHidden]
		private class SubItemPh : BqlPlaceholderBase { }
		[PXHidden]
		private class CostCenterPh : BqlPlaceholderBase { }

		#region State
		public Type DocumentBranchType { get; set; }

		protected Type _inventoryType;
		protected Type _subItemType;
		protected Type _costCenterType;
		#endregion

		#region Ctor
		public SiteAvailAttribute(Type InventoryType)
		{
			_inventoryType = InventoryType;

			Type SearchType = BqlCommand.AppendJoin<
				LeftJoin<Address, On<INSite.FK.Address>,
				LeftJoin<Country, On<Country.countryID, Equal<Address.countryID>>,
				LeftJoin<State, On<State.stateID, Equal<Address.state>>>>>
				>(Search);

			Type lookupJoin = BqlTemplate.OfJoin<
				LeftJoin<INSiteStatusByCostCenter,
					On<INSiteStatusByCostCenter.siteID, Equal<INSite.siteID>,
					And<INSiteStatusByCostCenter.inventoryID, Equal<Optional<InventoryPh>>,
					And<INSiteStatusByCostCenter.costCenterID, Equal<CostCenterPh>>>>,
				LeftJoin<INItemSiteSettings,
					On<INItemSiteSettings.siteID, Equal<INSite.siteID>,
					And<INItemSiteSettings.inventoryID, Equal<CostCenter.freeStock>>>>>>
				.Replace<InventoryPh>(InventoryType)
				.ToType();

			Type[] colsType = { typeof(INSite.siteCD), typeof(INSiteStatusByCostCenter.qtyOnHand), typeof(INSite.descr), typeof(Address.addressLine1), typeof(Address.addressLine2), typeof(Address.city), typeof(Country.description), typeof(State.name) };
			_Attributes[_SelAttrIndex] = CreateSelector(SearchType, lookupJoin, colsType);
		}

		public SiteAvailAttribute(Type InventoryType, Type SubItemType, Type CostCenterType)
		{
			_inventoryType = InventoryType;
			_subItemType = SubItemType;
			_costCenterType = CostCenterType;
			var costCenterExpression = GetCostCenterExpression();

			Type SearchType = BqlCommand.AppendJoin<
				LeftJoin<Address, On<INSite.FK.Address>>
				>(Search);

			Type lookupJoin =
				BqlTemplate.OfJoin<
					LeftJoin<INSiteStatusByCostCenter,
						On<INSiteStatusByCostCenter.siteID, Equal<INSite.siteID>,
						And<INSiteStatusByCostCenter.inventoryID, Equal<Optional<InventoryPh>>,
						And<INSiteStatusByCostCenter.subItemID, Equal<Optional<SubItemPh>>,
						And<CostCenterPh>>>>>>
				.Replace<InventoryPh>(InventoryType)
				.Replace<SubItemPh>(SubItemType)
				.Replace<CostCenterPh>(costCenterExpression)
				.ToType();

			Type[] colsType = {typeof(INSite.siteCD), typeof(INSiteStatusByCostCenter.qtyOnHand), typeof(INSiteStatusByCostCenter.active), typeof(INSite.descr)};
			_Attributes[_SelAttrIndex] = CreateSelector(SearchType, lookupJoin, colsType);
		}

		public SiteAvailAttribute(Type InventoryType, Type SubItemType, Type CostCenterType, Type[] colsType)
		{
			_inventoryType = InventoryType;
			_subItemType = SubItemType;
			_costCenterType = CostCenterType;
			var costCenterExpression = GetCostCenterExpression();

			Type lookupJoin = BqlTemplate.OfJoin<
				LeftJoin<INSiteStatusByCostCenter,
					On<INSiteStatusByCostCenter.siteID, Equal<INSite.siteID>,
					And<INSiteStatusByCostCenter.inventoryID, Equal<Optional<InventoryPh>>,
					And<INSiteStatusByCostCenter.subItemID, Equal<Optional<SubItemPh>>,
					And<CostCenterPh>>>>>>
				.Replace<InventoryPh>(InventoryType)
				.Replace<SubItemPh>(SubItemType)
				.Replace<CostCenterPh>(costCenterExpression)
				.ToType();

			_Attributes[_SelAttrIndex] = CreateSelector(Search, lookupJoin, colsType);
		}

		private Type GetCostCenterExpression()
		{
			if (typeof(IConstant).IsAssignableFrom(_costCenterType))
			{
				return BqlTemplate.OfCondition<Where<INSiteStatusByCostCenter.costCenterID, Equal<CostCenterPh>>>
					.Replace<CostCenterPh>(_costCenterType).ToType();
			}
			else
			{
				return BqlTemplate.OfCondition<Where<INSiteStatusByCostCenter.costCenterID, EqualSameCostCenter<Current2<CostCenterPh>>>>
					.Replace<CostCenterPh>(_costCenterType).ToType();
			}
		}

		private static PXDimensionSelectorAttribute CreateSelector(Type searchType, Type lookupJoin, Type[] colsType)
			=> new PXDimensionSelectorAttribute(DimensionName, searchType, lookupJoin, typeof(INSite.siteCD), true, colsType) { DescriptionField = typeof(INSite.descr) };

		private static Type Search { get; } = typeof(
			Search<INSite.siteID,
			Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>,
				And<Match<Current<AccessInfo.userName>>>>>);
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _inventoryType.Name, InventoryID_FieldUpdated);
		}
		#endregion

		#region Implementation
		public virtual void InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			try
			{
				sender.SetDefaultExt(e.Row, _FieldName);
			}
			catch (PXUnitConversionException) { }
			catch (PXSetPropertyException)
			{
				PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null);
				sender.SetValue(e.Row, _FieldOrdinal, null);
			}
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
            if (e.Cancel || e.Row == null)
                return;

            var inventoryID = sender.GetValue(e.Row, _inventoryType.Name);
            if (inventoryID == null)
                return;

			string baseCuryID = GetBaseCuryID(sender.Graph);

			var inventoryCurySettings = InventoryItemCurySettings.PK.Find(sender.Graph, (int)inventoryID, baseCuryID);
            if (inventoryCurySettings?.DfltSiteID == null)
                return;

			INSite site = PXSelectReadonly<INSite,
					Where<INSite.siteID, Equal<Required<INSite.siteID>>,
					And<Match<INSite, Current<AccessInfo.userName>>>>>
					.Select(sender.Graph, inventoryCurySettings.DfltSiteID);

            if(site != null)
                e.NewValue = site.SiteID;
		}

		protected virtual string GetBaseCuryID(PXGraph graph)
		{
			if (DocumentBranchType == null)
				return graph.Accessinfo.BaseCuryID;

			Type documentType = BqlCommand.GetItemType(DocumentBranchType);
			PXCache documentCache = graph.Caches[documentType];

			var branchID = documentCache.GetValue(documentCache.Current, DocumentBranchType.Name) as int?;
			if (documentCache?.GetStateExt(null, DocumentBranchType.Name) is PXBranchSelectorState)
			{
				return PXAccess.GetBranchByBAccountID(branchID)?.BaseCuryID ??
					PXAccess.GetOrganizationByID(branchID)?.BaseCuryID;
			}
			else
			{
				return PXAccess.GetBranch(branchID)?.BaseCuryID ??
					PXAccess.GetOrganizationByID(branchID)?.BaseCuryID;
			}
		}
		#endregion
	}
	#endregion

	#region SiteAttribute
	[PXDBInt()]
	[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
	[PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD), CacheGlobal = true, ShowWarning = true)]
	public class SiteAttribute : PXEntityAttribute
	{
		public const string DimensionName = "INSITE";
		public bool SetDefaultValue = true;

        public class transitSiteID : PX.Data.BQL.BqlInt.Constant<transitSiteID>
        {

			public transitSiteID() : base(-1) { }

            public override int Value
            {
                get
                {
                    Definition defs = PX.Common.PXContext.GetSlot<Definition>();
                    if (defs == null)
                    {
                        defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("INSite.Definition", typeof(INSite), typeof(INSetup)));
                    }
                    return defs?.TransitSiteID ?? 0;
                }
            }
        }

		public class dimensionName : PX.Data.BQL.BqlString.Constant<dimensionName>
		{
			public dimensionName() : base(DimensionName) { ;}
		}

		protected Type _whereType;

        public SiteAttribute()
            : this(false)
        {
        }

        public SiteAttribute(bool allowTransit)
			: this(typeof(Where<Match<Current<AccessInfo.userName>>>), false, allowTransit)
		{
		}
		public SiteAttribute(Type WhereType, bool allowTransit)
			: this(WhereType, true, allowTransit)
		{			
		}

		public SiteAttribute(Type WhereType, bool validateAccess, bool allowTransit)
		{			
			if (WhereType != null)
			{
				_whereType = WhereType;

                List<Type> bql = new List<Type>();

				if (validateAccess)
                {
                    bql.Add(typeof(Search<,>));
                    bql.Add(typeof(INSite.siteID));
                    bql.Add(typeof(Where2<,>));
                    bql.Add(typeof(Match<>));
                    bql.Add(typeof(Current<AccessInfo.userName>));
                    if(allowTransit)
                    {
                        bql.Add(typeof(And<>));
                    }
                    else
                    {
                        bql.Add(typeof(And<,,>));
                        bql.Add(typeof(INSite.siteID));
                        bql.Add(typeof(NotEqual<transitSiteID>));
						bql.Add(typeof(And<>));
					}
                    bql.Add(_whereType);
                }
                else
                {
                    bql.Add(typeof(Search<,>));
                    bql.Add(typeof(INSite.siteID));
                    if(!allowTransit)
                    {
                        bql.Add(typeof(Where2<,>));
                        bql.Add(typeof(Where<,>));
                        bql.Add(typeof(INSite.siteID));
                        bql.Add(typeof(NotEqual<transitSiteID>));
                        bql.Add(typeof(And<>));
                    }
                    bql.Add(_whereType);
                }

                Type SearchType = BqlCommand.Compose(bql.ToArray());

                PXDimensionSelectorAttribute attr;
				_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(INSite.siteCD), 
                    new Type[]
                    {
                        typeof (INSite.siteCD),typeof (INSite.descr)
                    }));
				attr.CacheGlobal = true;
				attr.DescriptionField = typeof(INSite.descr);
				_SelAttrIndex = _Attributes.Count - 1;
			}
		}

		#region Implemetation
		public override void CacheAttached(PXCache sender)
		{
			if (SetDefaultValue && (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && PXAccess.FeatureInstalled<FeaturesSet.inventory>()) && sender.Graph.GetType() != typeof(PXGraph))
			{
				if (Definitions.DefaultSiteID == null)
				{
					((PXDimensionSelectorAttribute)this._Attributes[_SelAttrIndex]).ValidComboRequired = false;
				}
				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, Feature_FieldDefaulting);
			}

			base.CacheAttached(sender);
		}

		public virtual void Feature_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!e.Cancel)
			{
				if (Definitions.DefaultSiteID == null)
				{
					object newValue = INSite.Main;
					sender.RaiseFieldUpdating(_FieldName, e.Row, ref newValue);
					e.NewValue = newValue;
				}
				else
				{
					e.NewValue = Definitions.DefaultSiteID;
				}

				e.Cancel = true;
			}
		}
        #endregion

        #region Default SiteID
        protected virtual Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("INSite.Definition", typeof(INSite), typeof(INSetup)));
				}
				return defs;
			}
		}

        public virtual Definition SiteDefinitions => Definitions;

		public class Definition : IPrefetchable
		{
			private int? _DefaultSiteID;
			public int? DefaultSiteID
			{
				get { return _DefaultSiteID; }
			}

            private int? _TransitSiteID;
            public int? TransitSiteID
            {
                get { return _TransitSiteID; }
            }

            public void Prefetch()
			{
                using (PXDataRecord record = PXDatabase.SelectSingle<INSetup>(
                    new PXDataField<INSetup.transitSiteID>()))
                {
                    _TransitSiteID = -1;
                    if (record != null)
                        _TransitSiteID = record.GetInt32(0);
                }

                var dflst = new List<PXDataField>();                
                dflst.Add(new PXDataField<INSite.siteID>());
				if (_TransitSiteID != null)
				{
                dflst.Add(new PXDataFieldValue("SiteID", PXDbType.Int, 4, _TransitSiteID, PXComp.NE));
				}
				dflst.Add(new PXDataFieldValue<INSite.active>(true));
                dflst.Add(new PXDataFieldOrder<INSite.siteID>());
                
                using (PXDataRecord record = PXDatabase.SelectSingle<INSite>(dflst.ToArray()))
                {
                    _DefaultSiteID = null;
                    if (record != null)
                        _DefaultSiteID = record.GetInt32(0);
                }
            }
		}
		#endregion

	}

    [PXDBInt()]
    [PXUIField(DisplayName = "To Site ID", Visibility = PXUIVisibility.Visible)]
    public class ToSiteAttribute : SiteAttribute
	{
		protected class BranchScopeDimensionSelector : PXDimensionSelectorAttribute
		{
			private int InterbranchRestrictorAttributeId = -1;
			protected BqlCommand _BranchScopeCondition;

			protected PXRestrictorAttribute InterbranchRestrictor => (PXRestrictorAttribute)_Attributes[InterbranchRestrictorAttributeId];

			public BranchScopeDimensionSelector(Type restrictionBranchId, string dimension, Type type, Type substituteKey, BqlCommand branchScopeCondition, params Type[] fieldList)
				: base(dimension, type, substituteKey, fieldList)
			{
				_BranchScopeCondition = branchScopeCondition;

				var interBranchRestrictionCondition = BqlTemplate.OfCondition<
					Where<SameOrganizationBranch<INSite.branchID, Current<BqlPlaceholder.A>>>>
					.Replace<BqlPlaceholder.A>(restrictionBranchId)
					.ToType();

				_Attributes.Add(new InterBranchRestrictorAttribute(interBranchRestrictionCondition));
				InterbranchRestrictorAttributeId = _Attributes.Count - 1;
			}

			public BranchScopeDimensionSelector(string dimension, Type type, Type substituteKey, BqlCommand branchScopeCondition, params Type[] fieldList)
				: this(typeof(AccessInfo.branchID), dimension, type, substituteKey, branchScopeCondition, fieldList)
			{
			}

			public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				if (_BranchScopeCondition.Meet(sender, sender.Current))
				{
					using (new PXReadBranchRestrictedScope())
					{
						base.FieldVerifying(sender, e);
					}
				}
				else
				{
					base.FieldVerifying(sender, e);
				}
			}

			public void SelectorFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				if (_BranchScopeCondition.Meet(sender, sender.Current))
				{
					using (new PXReadBranchRestrictedScope())
					{
						SelectorAttribute.FieldVerifying(sender, e);
						InterbranchRestrictor.FieldVerifying(sender, e);
					}
					e.Cancel = true;
				}
			}

			protected bool RaiseFieldSelectingInternal(PXCache sender, object row, ref object returnValue, bool forceState)
			{
				var args = new PXFieldSelectingEventArgs(null, null, true, true);
				this.FieldSelecting(sender, args);
				returnValue = args.ReturnState;
				return !args.Cancel;
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);

				sender.Graph.FieldVerifying.AddHandler(BqlTable, _FieldName, SelectorFieldVerifying);
				sender.Graph.FieldUpdating.RemoveHandler(BqlTable, _FieldName, base.FieldUpdating);
				sender.Graph.FieldUpdating.AddHandler(BqlTable, _FieldName, this.FieldUpdating);

				object state = null;
				RaiseFieldSelectingInternal(sender, null, ref state, true);

				string viewName = ((PX.Data.PXFieldState)state).ViewName;
				PXView view = sender.Graph.Views[viewName];

				PXView outerview = new PXView(sender.Graph, true, view.BqlSelect);
				view = sender.Graph.Views[viewName] = new PXView(sender.Graph, true, view.BqlSelect, (PXSelectDelegate)delegate()
				{
					int startRow = PXView.StartRow;
					int totalRows = 0;
					List<object> res;

					if (_BranchScopeCondition.Meet(sender, sender.Current))
					{
						using (new PXReadBranchRestrictedScope())
						{
							res = outerview.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
							PXView.StartRow = 0;
						}
					}
					else
					{
						res = outerview.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
						PXView.StartRow = 0;
					}

					return res;

				});

				if (_DirtyRead)
				{
					view.IsReadOnly = false;
				}
			}

			public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
			{
				if (_BranchScopeCondition.Meet(sender, sender.Current))
				{
					using (new PXReadBranchRestrictedScope())
					{
						base.FieldUpdating(sender, e);
					}
				}
				else
				{
					base.FieldUpdating(sender, e);
				}
			}

		}


		public ToSiteAttribute()
			: this(typeof(INTransferType.twoStep), typeof(AccessInfo.branchID))
		{			
		}

		public ToSiteAttribute(Type transferTypeField)
			: this(transferTypeField, typeof(AccessInfo.branchID))
		{
		}

		public ToSiteAttribute(Type transferTypeField, Type restrictionBranchId)
		{
			BranchScopeDimensionSelector selectorAttr = PrepareSelectorAttr(transferTypeField, restrictionBranchId);
			_Attributes[_SelAttrIndex] = selectorAttr;
			selectorAttr.CacheGlobal = true;
			selectorAttr.DescriptionField = typeof(INSite.descr);
		}

		private BranchScopeDimensionSelector PrepareSelectorAttr(Type transferTypeField, Type restrictionBranchId)
		{
			Type selectorType = BqlTemplate.OfCommand<Search<INSite.siteID,
				Where2<Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>, And<Match<Current<AccessInfo.userName>>>>,
					Or<BqlPlaceholder.A, Equal<INTransferType.twoStep>, And<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>>>>>
						.Replace<BqlPlaceholder.A>(typeof(IBqlField).IsAssignableFrom(transferTypeField)
							? typeof(Current<>).MakeGenericType(transferTypeField)
							: transferTypeField)
						.ToType();

			var branchScopeCondition = BqlTemplate.OfCommand<
				Select<INRegister,
				Where<BqlPlaceholder.A, Equal<INTransferType.twoStep>>>>
				.Replace<BqlPlaceholder.A>(transferTypeField).ToCommand();

			return new BranchScopeDimensionSelector(
				restrictionBranchId,
				DimensionName,
				selectorType,
				typeof(INSite.siteCD),
				branchScopeCondition,
				new Type[] { typeof(INSite.siteCD), typeof(INSite.descr) });
		}
	}

	/// <summary>
	/// Version of <see cref="SiteAttribute"/> that does not create default Warehouse if there are no warehouses
	/// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
	[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
	public class NullableSiteAttribute : SiteAttribute
	{
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldDefaulting.RemoveHandler(sender.GetItemType(), _FieldName, Feature_FieldDefaulting);
		}
		#endregion
	}
	#endregion

	#region ItemSiteAttribute
	
	public class ItemSiteAttribute : PXSelectorAttribute
	{
		public ItemSiteAttribute()
			:base(typeof(Search2<INItemSite.siteID,				
				InnerJoin<INSite, On<INSite.siteID, Equal<INItemSite.siteID>,
				      And<Where<CurrentMatch<INSite, AccessInfo.userName>>>>>,
				Where<INItemSite.inventoryID, Equal<Current<INItemSite.inventoryID>>>>))
		{
			this._SubstituteKey = typeof (INSite.siteCD);			
			this._UnconditionalSelect = BqlCommand.CreateInstance(typeof(Search<INSite.siteID, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>));
			this._NaturalSelect = BqlCommand.CreateInstance(typeof(Search<INSite.siteCD, Where<INSite.siteCD, Equal<Required<INSite.siteCD>>>>));
		}
		public override void SubstituteKeyCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
            if (ShouldPrepareCommandForSubstituteKey(e))
            {
				e.Cancel = true;
				foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(_FieldName))
				{
					if (attr is PXDBFieldAttribute)
					{
						SimpleTable siteExt = new SimpleTable<INSite>(_Type.Name+"Ext");

						var siteQuery = new Query()
							.Select(siteExt.Column(this._SubstituteKey))
							.From(siteExt)
							.Where(siteExt.Column<INSite.siteID>()
								.EQ(new Column(((PXDBFieldAttribute)attr).DatabaseFieldName,
									e.Table ?? _BqlTable)));

						e.Expr = new SubQuery(siteQuery).Embrace();


						if (e.Value != null)
						{
							e.DataValue = e.Value;
							e.DataType = PXDbType.NVarChar;
							e.DataLength = ((string)e.Value).Length;
						}
						break;
					}
				}
			}
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}
	}
	#endregion

	#region ReplenishmentSourceSiteAttribute
	public class ReplenishmentSourceSiteAttribute : SiteAttribute
	{
		public ReplenishmentSourceSiteAttribute(Type replenishmentSource)
			:base()
		{
			DescriptionField = typeof (INSite.descr);
			this.source = replenishmentSource;
		}
		private Type source;

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.RowSelected.AddHandler(sender.GetItemType(), OnRowSelected);
			sender.Graph.RowUpdated.AddHandler(sender.GetItemType(), OnRowUpdated);
		}
		protected virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, _FieldName,
				e.Row != null &&
				INReplenishmentSource.IsTransfer((string)sender.GetValue(e.Row, this.source.Name)) );
		}
		protected virtual void OnRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (e.Row == null) return;
			if (!INReplenishmentSource.IsTransfer((string)sender.GetValue(e.Row, this.source.Name)))
				sender.SetValue(e.Row, _FieldName, null);				
		}		

	}
	#endregion

	#region LocationRawAttribute

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Location ID", Visibility = PXUIVisibility.SelectorVisible)]
	public sealed class LocationRawAttribute : PXEntityAttribute
	{
		public string DimensionName = "INLOCATION";
		public LocationRawAttribute()
			: base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(DimensionName);
			attr.ValidComboRequired = false;

			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	#endregion

	#region LocationAvailAttribute

    public class LocationRestrictorAttribute : PXRestrictorAttribute
    {
        protected Type _IsReceiptType;
        protected Type _IsSalesType;
        protected Type _IsTransferType;

        public LocationRestrictorAttribute(Type IsReceiptType, Type IsSalesType, Type IsTransferType)
            : base(typeof(Where<True>), string.Empty)
        {
            _IsReceiptType = IsReceiptType;
            _IsSalesType = IsSalesType;
            _IsTransferType = IsTransferType;
        }

        protected override BqlCommand WhereAnd(PXCache sender, PXSelectorAttribute selattr, Type Where)
        {
            return selattr.PrimarySelect.WhereAnd(Where);
        }

        public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            INLocation location = null;
            try
            {
				location = INLocation.PK.Find(sender.Graph, (int?)e.NewValue);
            }
            catch (FormatException) { }

            if (_AlteredCmd != null && location != null)
            {
                bool? IsReceipt = VerifyExpr(sender, e.Row, _IsReceiptType);
                bool? IsSales = VerifyExpr(sender, e.Row, _IsSalesType);
                bool? IsTransfer = VerifyExpr(sender, e.Row, _IsTransferType);

                if (IsReceipt == true && location.ReceiptsValid == false)
                {
                    ThrowErrorItem(Messages.LocationReceiptsInvalid, e, location.LocationCD);
                }

                if (IsSales == true)
                {
                    if (location.SalesValid == false && (e.ExternalCall || location.IsSorting == false))
                    {
                        ThrowErrorItem(Messages.LocationSalesInvalid, e, location.LocationCD);
                    }
                }

                if (IsTransfer == true)
                {
                    if (location.TransfersValid == false && (e.ExternalCall || location.IsSorting == false))
                    {
                        ThrowErrorItem(Messages.LocationTransfersInvalid, e, location.LocationCD);
                    }
                }
            }
        }

        public virtual void ThrowErrorItem(string message, PXFieldVerifyingEventArgs e, object ErrorValue)
        {
            e.NewValue = ErrorValue;
            throw new PXSetPropertyException(message);
        }

        protected bool? VerifyExpr(PXCache cache, object data, Type whereType)
        {
            object value = null;
            bool? ret = null;
            IBqlWhere where = (IBqlWhere)Activator.CreateInstance(whereType);
            where.Verify(cache, data, new List<object>(), ref ret, ref value);

            return ret;
        }
    }

    public class PrimaryItemRestrictorAttribute : PXRestrictorAttribute
    {
        public bool IsWarning;

        protected Type _InventoryType;
        protected Type _IsReceiptType;
        protected Type _IsSalesType;
        protected Type _IsTransferType;

        public PrimaryItemRestrictorAttribute(Type InventoryType, Type IsReceiptType, Type IsSalesType, Type IsTransferType)
            : base(typeof(Where<True>), string.Empty)
        {
            _InventoryType = InventoryType;
            _IsReceiptType = IsReceiptType;
            _IsSalesType = IsSalesType;
            _IsTransferType = IsTransferType;
        }

        protected override BqlCommand WhereAnd(PXCache sender, PXSelectorAttribute selattr, Type Where)
        {
            return selattr.PrimarySelect.WhereAnd(Where);
        }

        public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            INLocation location = null;
            try
            {
                location = INLocation.PK.Find(sender.Graph, (int?)e.NewValue);
            }
            catch (FormatException) { }

            if (_AlteredCmd != null && location != null && location.PrimaryItemValid != INPrimaryItemValid.PrimaryNothing)
            {
                bool? IsReceipt = VerifyExpr(sender, e.Row, _IsReceiptType);
                bool? IsSales = VerifyExpr(sender, e.Row, _IsSalesType);
                bool? IsTransfer = VerifyExpr(sender, e.Row, _IsTransferType);

                if (IsReceipt == true || IsTransfer == true)
                {
					var ItemID = (int?)sender.GetValue(e.Row, _InventoryType.Name);
					if (ItemID == null)
						return;
					InventoryItem item;

					switch (location.PrimaryItemValid)
                    {
                        case INPrimaryItemValid.PrimaryItemError:
                            if (Equals(ItemID, location.PrimaryItemID) == false)
                            {
                                ThrowErrorItem(Messages.NotPrimaryLocation, e, location.LocationCD);
                            }
                            break;
                        case INPrimaryItemValid.PrimaryItemClassError:
                            item = InventoryItem.PK.Find(sender.Graph, ItemID);
                            if (item != null && item.ItemClassID != location.PrimaryItemClassID)
                            {
                                ThrowErrorItem(Messages.NotPrimaryLocation, e, location.LocationCD);
                            }
                            break;
                        case INPrimaryItemValid.PrimaryItemWarning:
                            if (Equals(ItemID, location.PrimaryItemID) == false)
                            {
                                sender.RaiseExceptionHandling(_FieldName, e.Row, e.NewValue, new PXSetPropertyException(Messages.NotPrimaryLocation, PXErrorLevel.Warning));
                            }
                            break;
                        case INPrimaryItemValid.PrimaryItemClassWarning:
                            item = InventoryItem.PK.Find(sender.Graph, ItemID);
                            if (item != null && item.ItemClassID != location.PrimaryItemClassID)
                            {
                                sender.RaiseExceptionHandling(_FieldName, e.Row, e.NewValue, new PXSetPropertyException(Messages.NotPrimaryLocation, PXErrorLevel.Warning));
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        public virtual void ThrowErrorItem(string message, PXFieldVerifyingEventArgs e, object ErrorValue)
        {
            e.NewValue = ErrorValue;
            throw new PXSetPropertyException(message);
        }

        protected bool? VerifyExpr(PXCache cache, object data, Type whereType)
        {
            object value = null;
            bool? ret = null;
            IBqlWhere where = (IBqlWhere)Activator.CreateInstance(whereType);
            where.Verify(cache, data, new List<object>(), ref ret, ref value);

            return ret;
        }
    }

	[PXDBInt()]
    [PXUIField(DisplayName = "Location", Visibility = PXUIVisibility.Visible, FieldClass = LocationAttribute.DimensionName)]
    [PXRestrictor(typeof(Where<INLocation.active, Equal<True>>), Messages.InactiveLocation, typeof(INLocation.locationCD), CacheGlobal = true)]
	public class LocationAvailAttribute : LocationAttribute, IPXFieldDefaultingSubscriber
	{
		[PXHidden]
		private class InventoryPh : BqlPlaceholderBase { }
		[PXHidden]
		private class SubItemPh : BqlPlaceholderBase { }
		[PXHidden]
		private class SiteIDPh : BqlPlaceholderBase { }
		[PXHidden]
		private class CostCenterPh : BqlPlaceholderBase { }

		#region State
		protected Type _InventoryType;
		protected Type _costCenterType;
		protected Type _IsSalesType;
		protected Type _IsReceiptType;
		protected Type _IsTransferType;
		protected Type _IsStandardCostAdjType;
		protected BqlCommand _Select;
		#endregion

		#region Ctor

		public LocationAvailAttribute(Type InventoryType, Type SubItemType, Type CostCenterType, Type SiteIDType, bool IsSalesType, bool IsReceiptType, bool IsTransferType)
			: this(InventoryType, SubItemType, CostCenterType, SiteIDType, null, null, null)
		{
			_IsSalesType = IsSalesType ? typeof(Where<True>) : typeof(Where<False>);
			_IsReceiptType = IsReceiptType ? typeof(Where<True>) : typeof(Where<False>);
			_IsTransferType = IsTransferType ? typeof(Where<True>) : typeof(Where<False>);
			_IsStandardCostAdjType = typeof(Where<False>);

            this._Attributes.Add(new PrimaryItemRestrictorAttribute(InventoryType, _IsReceiptType, _IsSalesType, _IsTransferType));
            this._Attributes.Add(new LocationRestrictorAttribute(_IsReceiptType, _IsSalesType, _IsTransferType));
		}

		public LocationAvailAttribute(Type InventoryType, Type SubItemType, Type CostCenterType, Type SiteIDType, Type TranType, Type InvtMultType)
			: this(InventoryType, SubItemType, CostCenterType, SiteIDType, TranType, InvtMultType, true)
		{
		}

		public LocationAvailAttribute(Type InventoryType, Type SubItemType, Type CostCenterType, Type SiteIDType, Type TranType, Type InvtMultType, bool VerifyAllowedOperations)
			: this(InventoryType, SubItemType, CostCenterType, SiteIDType, null, null, null)
		{
			_IsSalesType = BqlCommand.Compose(typeof(Where<,>), TranType, typeof(In3<INTranType.invoice, INTranType.debitMemo>));
			_IsReceiptType = BqlCommand.Compose(typeof(Where<,>), TranType, typeof(In3<INTranType.receipt, INTranType.issue, INTranType.return_, INTranType.creditMemo>));
			_IsTransferType = BqlCommand.Compose(typeof(Where<,,>), TranType, typeof(Equal<INTranType.transfer>), typeof(And<,>), InvtMultType, typeof(In3<short1, shortMinus1>));
			_IsStandardCostAdjType = BqlCommand.Compose(typeof(Where<,>), TranType, typeof(In3<INTranType.standardCostAdjustment, INTranType.negativeCostAdjustment>));

            this._Attributes.Add(new PrimaryItemRestrictorAttribute(InventoryType, _IsReceiptType, _IsSalesType, _IsTransferType));

			if (VerifyAllowedOperations)
            this._Attributes.Add(new LocationRestrictorAttribute(_IsReceiptType, _IsSalesType, _IsTransferType));
        }

		public LocationAvailAttribute(Type InventoryType, Type SubItemType, Type CostCenterType, Type SiteIDType, Type IsSalesType, Type IsReceiptType, Type IsTransferType)
			: base(SiteIDType)
		{
			_InventoryType = InventoryType;
			_costCenterType = CostCenterType;
			var costCenterExpression = typeof(IConstant).IsAssignableFrom(_costCenterType) ? _costCenterType :
				BqlTemplate.FromType(typeof(Optional<CostCenterPh>)).Replace<CostCenterPh>(CostCenterType).ToType();

			_IsSalesType = IsSalesType;
			_IsReceiptType = IsReceiptType;
			_IsTransferType = IsTransferType;
			_IsStandardCostAdjType = typeof(Where<False>);

			Type search = BqlTemplate.OfCommand<
				Search<INLocation.locationID,
				Where<INLocation.siteID, Equal<Optional<SiteIDPh>>>>>
				.Replace<SiteIDPh>(SiteIDType)
				.ToType();

			Type lookupJoin = BqlTemplate.OfJoin<
				LeftJoin<INLocationStatusByCostCenter,
					On<INLocationStatusByCostCenter.locationID, Equal<INLocation.locationID>,
					And<INLocationStatusByCostCenter.inventoryID, Equal<Optional<InventoryPh>>,
					And<INLocationStatusByCostCenter.subItemID, Equal<Optional<SubItemPh>>,
					And<INLocationStatusByCostCenter.costCenterID, Equal<CostCenterPh>>>>>>>
				.Replace<InventoryPh>(InventoryType)
				.Replace<SubItemPh>(SubItemType)
				.Replace<CostCenterPh>(costCenterExpression)
				.ToType();

			Type[] fieldList =
			{
				typeof(INLocation.locationCD),
				typeof(INLocationStatusByCostCenter.qtyOnHand),
				typeof(INLocationStatusByCostCenter.active),
				typeof(INLocation.primaryItemID),
				typeof(INLocation.primaryItemClassID),
				typeof(INLocation.receiptsValid),
				typeof(INLocation.salesValid),
				typeof(INLocation.transfersValid),
				typeof(INLocation.projectID),
				typeof(INLocation.taskID)
			};
			var attr = new LocationDimensionSelectorAttribute(search, GetSiteIDKeyRelation(SiteIDType), lookupJoin, fieldList);
			_Attributes[_SelAttrIndex] = attr;

			_Select = BqlTemplate.OfCommand<
				Select<INItemSite,
				Where<INItemSite.inventoryID, Equal<Current<InventoryPh>>,
					And<INItemSite.siteID, Equal<Current2<SiteIDPh>>>>>>
				.Replace<InventoryPh>(_InventoryType)
				.Replace<SiteIDPh>(_SiteIDType)
				.ToCommand();

            if (IsReceiptType != null && IsSalesType != null && IsTransferType != null)
            {
                this._Attributes.Add(new PrimaryItemRestrictorAttribute(InventoryType, IsReceiptType, IsSalesType, IsTransferType));
                this._Attributes.Add(new LocationRestrictorAttribute(IsReceiptType, IsSalesType, IsTransferType));
            }
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			//sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _SiteIDType.Name, SiteID_FieldUpdated);
		}

		#endregion

		#region Implementation
		public override void SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (sender.GetValue(e.Row, _FieldOrdinal) != null)
			{
				base.SiteID_FieldUpdated(sender, e);

				object locationid;
				if ((locationid = sender.GetValue(e.Row, _FieldOrdinal)) != null && (int?)locationid > 0)
					return;
					PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null);
			}

					try
					{
				if (e.ExternalCall)
				{
					//SetValuePending are works only for IDictionary as first 'data' parameter
					sender.SetValuePending(e.Row, _FieldName, PXCache.NotSetValue);
						sender.SetDefaultExt(e.Row, _FieldName);
					}
				else
				{
					object newValue;
					sender.RaiseFieldDefaulting(_FieldName, e.Row, out newValue);
					if (newValue != null)
						sender.SetValueExt(e.Row, _FieldName, newValue);
				}
			}
					catch (PXSetPropertyException)
					{
						PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null);
						sender.SetValue(e.Row, _FieldOrdinal, null);
					}
				}

		protected bool? VerifyExpr(PXCache cache, object data, Type whereType)
		{
			if(whereType == null)
			{
				return false;
			}

			object value = null;
			bool? ret = null;
			IBqlWhere where = (IBqlWhere)Activator.CreateInstance(whereType);
			where.Verify(cache, data, new List<object>(), ref ret, ref value);

			return ret;
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			bool IsStandardCostAdj = (bool)VerifyExpr(sender, e.Row, _IsStandardCostAdjType);

			if (IsStandardCostAdj)
			{
				e.NewValue = null;
				e.Cancel = true;
				return;
			}
			
			PXView view = sender.Graph.TypedViews.GetView(_Select, false);

			var itemsite = (INItemSite)view.SelectSingleBound(new object[] { e.Row });

			if (!UpdateDefault<INItemSite.dfltReceiptLocationID, INItemSite.dfltShipLocationID>(sender, e, itemsite))
			{
				var insite = (INSite)PXSelectorAttribute.Select(sender, e.Row, _SiteIDType.Name);
				UpdateDefault<INSite.receiptLocationID, INSite.shipLocationID>(sender, e, insite);
			}
		}

		private bool UpdateDefault<ReceiptLocationID, ShipLocationID>(PXCache sender, PXFieldDefaultingEventArgs e,
		                                                              object source)
			where ReceiptLocationID : IBqlField
			where	ShipLocationID : IBqlField
		{
			if(source == null) return false;
			PXCache cache = sender.Graph.Caches[source.GetType()];

			if(cache.Keys.Exists(key => cache.GetValue(source, key) == null)) return false;				

			bool IsReceipt = (bool)VerifyExpr(sender, e.Row, _IsReceiptType);			
			
			object newvalue = (IsReceipt) ? cache.GetValue<ReceiptLocationID>(source) : cache.GetValue<ShipLocationID>(source);
			object val = (IsReceipt) ? cache.GetValueExt<ReceiptLocationID>(source) : cache.GetValueExt<ShipLocationID>(source);

			if (val is PXFieldState)
			{
				e.NewValue = ((PXFieldState)val).Value;
			}
			else
			{
				e.NewValue = val;
			}

			try
			{
				sender.RaiseFieldVerifying(_FieldName, e.Row, ref newvalue);
			}
			catch (PXSetPropertyException)
			{
				e.NewValue = null;
			}
			return true;
		}

		#endregion
	}

	#endregion

	#region LocationAttribute

    public interface IFeatureAccessProvider
    {
        bool IsFeatureInstalled<TFeature>();
    }


	[PXDBInt()]
    [PXUIField(DisplayName = "Location", Visibility = PXUIVisibility.Visible, FieldClass = LocationAttribute.DimensionName)]
    public class LocationAttribute : PXEntityAttribute
	{
		public const string DimensionName = "INLOCATION";

		public class dimensionName : PX.Data.BQL.BqlString.Constant<dimensionName>
		{
			public dimensionName() : base(DimensionName) { ;}
		}

		protected Type _SiteIDType;

		protected bool _KeepEntry = true;
		protected bool _ResetEntry = true;

		public bool KeepEntry
		{
			get
			{
				return this._KeepEntry;
			}
			set
			{
				this._KeepEntry = value;
			}
		}

		public bool ResetEntry
		{
			get
			{
				return this._ResetEntry;
			}
			set
			{
				this._ResetEntry = value;
			}
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		public LocationAttribute()
			: base()
		{
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, typeof(Search<INLocation.locationID>), typeof(INLocation.locationCD))
			{
				CacheGlobal = true,
				DescriptionField = typeof(INLocation.descr)
			};
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public LocationAttribute(Type SiteIDType)
			: base()
		{
			_SiteIDType = SiteIDType ?? throw new PXArgumentException(nameof(SiteIDType), ErrorMessages.ArgumentNullException);

			Type search = BqlTemplate.OfCommand<
					Search<INLocation.locationID,
					Where<INLocation.siteID, Equal<Optional<BqlPlaceholder.A>>>>>
				.Replace<BqlPlaceholder.A>(_SiteIDType)
				.ToType();

			var attr = new LocationDimensionSelectorAttribute(search, GetSiteIDKeyRelation(SiteIDType));
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		protected Type GetSiteIDKeyRelation(Type siteIDField) => typeof(Field<>.IsRelatedTo<>).MakeGenericType(siteIDField, typeof(INLocation.siteID));

        public bool IsWarehouseLocationEnabled(PXCache sender)
        {
            return ((sender.Graph is IFeatureAccessProvider && ((IFeatureAccessProvider)sender.Graph).IsFeatureInstalled<FeaturesSet.warehouseLocation>())
                ||
                (!(sender.Graph is IFeatureAccessProvider) && PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
                );
        }

		public override void CacheAttached(PXCache sender)
		{
			if (_SiteIDType != null && !IsWarehouseLocationEnabled(sender)
                && sender.Graph.GetType() != typeof(PXGraph))
			{
				((PXDimensionSelectorAttribute)this._Attributes[_SelAttrIndex]).ValidComboRequired = false;

				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, Feature_FieldDefaulting);
				sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, Feature_FieldUpdating);
				sender.Graph.FieldUpdating.RemoveHandler(sender.GetItemType(), _FieldName, ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldUpdating);

				if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && sender.GetItemType() == typeof(IN.INSite))
				{
					_JustPersisted = new Dictionary<int?, int?>();
					sender.Graph.RowPersisting.AddHandler<INLocation>(Feature_RowPersisting);
					sender.Graph.RowPersisted.AddHandler<INLocation>(Feature_RowPersisted);
				}

				if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && !sender.Graph.Views.Caches.Contains(typeof(INSite)))
				{
					sender.Graph.Views.Caches.Add(typeof(INSite));
				}

				if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && !sender.Graph.Views.Caches.Contains(typeof(INLocation)))
				{
					sender.Graph.Views.Caches.Add(typeof(INLocation));
				}
			}

			base.CacheAttached(sender);

			if (_SiteIDType != null)
			{
				sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _SiteIDType.Name, SiteID_FieldUpdated);

				if (IsWarehouseLocationEnabled(sender))
				{
					string name = _FieldName.ToLower();
					sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), name, FieldUpdating);
					sender.Graph.FieldUpdating.RemoveHandler(sender.GetItemType(), name, ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldUpdating);

					sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), name, FieldSelecting);
					sender.Graph.FieldSelecting.RemoveHandler(sender.GetItemType(), name, ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldSelecting);

					PXDimensionSelectorAttribute.SetValidCombo(sender, name, false);
				}
			}
		}

		protected Dictionary<Int32?, Int32?> _JustPersisted;

		public virtual void Feature_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Int32? _KeyToAbort = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && _KeyToAbort < 0)
			{ 
				PXCache cache = sender.Graph.Caches[typeof(INSite)];
				INSite record = ((IEnumerable<INSite>)cache.Inserted).First();

				sender.SetValue(e.Row, _SiteIDType.Name, record.SiteID);

				_JustPersisted.Add(record.SiteID, _KeyToAbort);
			}
		}

		public virtual void Feature_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			Int32? _NewKey = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Aborted)
			{
				Int32? _KeyToAbort;
				if (_JustPersisted.TryGetValue(_NewKey, out _KeyToAbort))
				{
					sender.SetValue(e.Row, _SiteIDType.Name, _KeyToAbort);
				}
			}
		}

		public virtual void Feature_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue == null || e.Cancel == true) return;

			Int32? siteval = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);
			PXCache sitecache = sender.Graph.Caches[typeof(INSite)];
			INSite _current;
			if ((_current = INSite.PK.Find(sender.Graph, siteval)) == null)
			{
				_current = (object.ReferenceEquals(sitecache, sender) ? e.Row : siteval == null ? null : ((IEnumerable<INSite>)sitecache.Inserted).FirstOrDefault(a => a.SiteID == siteval)) as INSite;
			}

			PXFieldUpdating fu = ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldUpdating;

			PXFieldDefaulting siteid_fielddefaulting = (cache, args) =>
			{
				INLocation row = args.Row as INLocation;
				if (row != null && _current != null)
				{
					args.NewValue = _current.SiteID;
					args.Cancel = true;
				}
			};

			var dummy_cache = sender.Graph.Caches<INLocation>();
			sender.Graph.FieldDefaulting.AddHandler<INLocation.siteID>(siteid_fielddefaulting);

			try
			{
				fu(sender, e);
			}
			finally
			{
				sender.Graph.FieldDefaulting.RemoveHandler<INLocation.siteID>(siteid_fielddefaulting);
			}
		}

		public virtual void Feature_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!e.Cancel)
			{
				Int32? siteval = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);
				object newValue = null;
				if (siteval != null)
				{

					if (!this.Definitions.DefaultLocations.TryGetValue(siteval, out newValue))
					{
						try
						{
							newValue = INLocation.Main;
							sender.RaiseFieldUpdating(_FieldName, e.Row, ref newValue);
						}
						catch (InvalidOperationException)
						{
						}
					}
				}
				e.NewValue = newValue;
				e.Cancel = true;
			}
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Cancel) return;

			PXDimensionSelectorAttribute attr = ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]);
			attr.DirtyRead = true;
			attr.FieldSelecting(sender, e, attr.SuppressViewCreation, true);

			var state = e.ReturnState as PXSegmentedState;
			if (state != null)
			{
				state.ValidCombos = true;
			}

			if ((int?)sender.GetValue(e.Row, _FieldOrdinal) < 0)
			{
				Int32? siteval = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);
				PXCache cache = sender.Graph.Caches[typeof(INSite)];
				INSite site = INSite.PK.Find(sender.Graph, siteval);

				if ((string)cache.GetValue<INSite.locationValid>(site) == INLocationValid.Warn)
				{
					sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyException(ErrorMessages.ElementDoesntExist, PXErrorLevel.Warning, Messages.Location));
				}
			}
		}

		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue == null || e.Cancel == true) return;

			Int32? siteval = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);
			PXCache sitecache = sender.Graph.Caches[typeof(INSite)];
			INSite _current = INSite.PK.Find(sender.Graph, siteval);
			if (_current == null)
			{
				_current = sitecache.Current as INSite;
			}

			PXFieldUpdating fu = ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldUpdating;

			PXFieldDefaulting siteid_fielddefaulting = (cache, args) =>
				{
					INLocation row = args.Row as INLocation;
					if (row != null && _current != null)
					{
						args.NewValue = _current.SiteID;
						args.Cancel = true;
					}
				};

			PXRowInserting location_inserting = (cache, args) =>
				{
					INLocation row = args.Row as INLocation;
					if (row != null)
					{
						if (_current == null || _current.LocationValid == INLocationValid.Validate)
						{
							args.Cancel = true;
						}
					}
				};

			sender.Graph.RowInserting.AddHandler<INLocation>(location_inserting);
			sender.Graph.FieldDefaulting.AddHandler<INLocation.siteID>(siteid_fielddefaulting);

			try
			{
				fu(sender, e);
			}
			catch (PXSetPropertyException ex)
			{
				ex.ErrorValue = e.NewValue;
				throw;
			}
			finally
			{
				sender.Graph.RowInserting.RemoveHandler<INLocation>(location_inserting);
				sender.Graph.FieldDefaulting.RemoveHandler<INLocation.siteID>(siteid_fielddefaulting);
			}
		}

		public virtual void SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_KeepEntry)
			{
				object val = sender.GetValueExt(e.Row, _FieldName);
				sender.SetValue(e.Row, _FieldOrdinal, null);


				PXRowInserting location_inserting = (cache, args) =>
				{
					args.Cancel = true;
				};

				sender.Graph.RowInserting.AddHandler<INLocation>(location_inserting);

				try
				{
					object newval = val is PXFieldState ? ((PXFieldState)val).Value : val;
					sender.SetValueExt(e.Row, _FieldName, newval);
				}
				catch (PXException) { }
				finally
				{
					sender.Graph.RowInserting.RemoveHandler<INLocation>(location_inserting);
				}
			}
			else if (_ResetEntry)
			{
				sender.SetValueExt(e.Row, _FieldName, null);							 
			}
		}

		#region Default LocationID
		protected virtual Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("INLocation.Definition", typeof(INLocation)));
				}
				return defs;
			}
		}

		protected class Definition : IPrefetchable
		{
			public Dictionary<int?, object> DefaultLocations;

			public void Prefetch()
			{
				DefaultLocations = new Dictionary<int?, object>(); 
				
				foreach (PXDataRecord record in PXDatabase.SelectMulti<INLocation>(
					new PXDataField<INLocation.siteID>(),
					new PXDataField<INLocation.locationID>(),
					new PXDataFieldOrder<INLocation.siteID>(),
					new PXDataFieldOrder<INLocation.locationID>()))
				{
					int? siteID = record.GetInt32(0);

					if (!DefaultLocations.ContainsKey(siteID))
					{
						DefaultLocations.Add(siteID, record.GetInt32(1));
					}
				}
			}
		}
		#endregion

		#region Location Selector and Dimension attributes
		public class LocationSelectorAttribute : PXSelectorAttribute.WithCachingByCompositeKeyAttribute
		{
			public LocationSelectorAttribute(Type search, Type additionalKeysRelation)
				: base(search, additionalKeysRelation)
			{
				SubstituteKey = typeof(INLocation.locationCD);
			}

			public LocationSelectorAttribute(Type search, Type additionalKeysRelation, Type lookupJoin, Type[] fieldList)
				: base(search, additionalKeysRelation, lookupJoin, fieldList)
			{
				SubstituteKey = typeof(INLocation.locationCD);
			}

			protected override void OnItemCached(PXCache foreignCache, object foreignItem, bool isItemDeleted)
			{
				base.OnItemCached(foreignCache, foreignItem, isItemDeleted);
				if(!isItemDeleted)
					INLocation.PK.PutToGlobalCache(foreignCache.Graph, (INLocation)foreignItem);
			}
		}

		public class LocationDimensionSelectorAttribute: PXDimensionSelector.WithCachingByCompositeKeyAttribute
		{
			public LocationDimensionSelectorAttribute(Type search, Type additionalKeysRelation) : base(DimensionName)
			{
				Initialize(new LocationSelectorAttribute(search, additionalKeysRelation));
			}

			public LocationDimensionSelectorAttribute(Type search, Type additionalKeysRelation, Type lookupJoin, Type[] fieldList) : base(DimensionName)
			{
				Initialize(new LocationSelectorAttribute(search, additionalKeysRelation, lookupJoin, fieldList));
				DirtyRead = true;
			}

			private void Initialize(LocationSelectorAttribute locationSelectorAttribute)
			{
				RegisterSelector(locationSelectorAttribute);
				ValidComboRequired = true;
				OnlyKeyConditions = true;
				DescriptionField = typeof(INLocation.descr);
			}
		}
		#endregion


	}

	#endregion

	#region PXDBPriceCostAttribute

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class PXDBPriceCostAttribute : PXDBDecimalAttribute
	{
		protected bool _keepNullValue;
		#region Implementation
		public static decimal Round(decimal value)
		{
			return (decimal)Math.Round(value, CommonSetupDecPl.PrcCst, MidpointRounding.AwayFromZero);
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			base.RowSelecting(sender, e);

			if (!_keepNullValue && sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, 0m);
			}
		}
		#endregion
		#region Initialization
		public PXDBPriceCostAttribute()
			: this(false)
		{
		}

		public PXDBPriceCostAttribute(bool keepNullValue)
		{
			_keepNullValue = keepNullValue;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_Precision = CommonSetupDecPl.PrcCst;
		}
		#endregion
	}

	#endregion

	#region PXDBPriceCostCalced
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class PXDBPriceCostCalcedAttribute : PXDBCalcedAttribute
	{
		#region Ctor
		public PXDBPriceCostCalcedAttribute(Type operand, Type type)
			: base(operand, type)
		{
		}
		#endregion
		#region Implementation
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			base.RowSelecting(sender, e);

			if (sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, 0m);
			}
		}
		#endregion
	}
	#endregion

	#region PXPriceCostAttribute

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class PXPriceCostAttribute : PXDecimalAttribute
	{
		#region Implementation
		public static decimal Round(decimal value)
		{
			return (decimal)Math.Round(value, CommonSetupDecPl.PrcCst, MidpointRounding.AwayFromZero);
		}
		#endregion
		#region Static methods
		public static Decimal MinPrice(InventoryItem ii, INItemCost cost, InventoryItemCurySettings itemCurySettings)
		{					
			if (ii.ValMethod != INValMethod.Standard)
				{
				if (cost.AvgCost != 0m)
					return (cost.AvgCost ?? 0) + ((cost.AvgCost ?? 0) * 0.01m * (ii.MinGrossProfitPct ?? 0));
				else
					return (cost.LastCost ?? 0) + ((cost.LastCost ?? 0) * 0.01m * (ii.MinGrossProfitPct ?? 0));
				}
				else
				{
					return (itemCurySettings?.StdCost ?? 0) + ((itemCurySettings?.StdCost ?? 0) * 0.01m * (ii.MinGrossProfitPct ?? 0));
				}
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_Precision = CommonSetupDecPl.PrcCst;
		}
		#endregion
	}

	#endregion

	#region PXBaseQuantityAttribute

	public class PXBaseQuantityAttribute : PXQuantityAttribute
	{
		internal override INUnit ReadConversion(PXCache cache, object data)
		{
			var conversion = base.ReadConversion(cache, data);
			if (conversion != null && conversion.RecordID != null)
			{
				conversion = PXCache<INUnit>.CreateCopy(conversion);
				conversion.UnitMultDiv = (conversion.UnitMultDiv == MultDiv.Multiply) ? MultDiv.Divide : MultDiv.Multiply;
			}
			return conversion;
		}

		public PXBaseQuantityAttribute()
			: base()
		{
		}

		public PXBaseQuantityAttribute(Type keyField, Type resultField)
			: base(keyField, resultField)
		{
		}
	}

	#endregion

	#region PXQuantityAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class PXQuantityAttribute : PXDecimalAttribute, IPXFieldVerifyingSubscriber, IPXRowInsertingSubscriber, IPXRowPersistingSubscriber
	{
		#region State
		protected int _ResultOrdinal;
		protected int _KeyOrdinal;
		protected Type _KeyField = null;
		protected Type _ResultField = null;
		protected bool _HandleEmptyKey = false; 
		protected int? _OverridePrecision = null;


		#endregion

		#region Ctor
		public PXQuantityAttribute()
		{
		}
		public PXQuantityAttribute(byte precision)
		{
			_OverridePrecision = precision;
		}

		public PXQuantityAttribute(Type keyField, Type resultField)
		{
			_KeyField = keyField;
			_ResultField = resultField;
		}

		public bool HandleEmptyKey
		{
			set { this._HandleEmptyKey = value; }
			get { return this._HandleEmptyKey; }
		}
		#endregion

		#region Runtime
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_Precision = _OverridePrecision ?? CommonSetupDecPl.Qty;

			if (_ResultField != null)
			{
				_ResultOrdinal = sender.GetFieldOrdinal(_ResultField.Name);
			}

			if (_KeyField != null)
			{
				_KeyOrdinal = sender.GetFieldOrdinal(_KeyField.Name);
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_KeyField), _KeyField.Name, KeyFieldUpdated);
			}
		}
		#endregion

		#region Implementation
		internal virtual INUnit ReadConversion(PXCache cache, object data)
				{
			var unitAttribute = cache.GetAttributesOfType<INUnitAttribute>(data, _KeyField.Name).FirstOrDefault();
			return unitAttribute == null
				? null
				: unitAttribute.ReadConversion(cache, data, (string)cache.GetValue(data, _KeyField.Name));
		}

		protected virtual void CalcBaseQty(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			decimal? resultval = null;

			if (_ResultField != null)
			{
				if (e.NewValue != null)
				{
					bool handled = false;
					if (this._HandleEmptyKey)
					{
						object value = sender.GetValue(e.Row, _KeyField.Name);
						if (String.IsNullOrEmpty((String)value))
						{
							resultval = (decimal)e.NewValue;
							handled = true;
						}
					}
					if (!handled)
					{
                        INUnit conv = ReadConversion(sender, e.Row);
                        if(conv != null)
                        {
                            if(conv.FromUnit == conv.ToUnit)
                            {
                                _ensurePrecision(sender, e.Row);
                                resultval = Math.Round((decimal)e.NewValue, (int)_Precision, MidpointRounding.AwayFromZero);
                            }
                            else if (conv.UnitRate != 0m)
						{
							_ensurePrecision(sender, e.Row);
							resultval = Math.Round((decimal)e.NewValue * (conv.UnitMultDiv == MultDiv.Multiply ? (decimal)conv.UnitRate : 1 / (decimal)conv.UnitRate), (int)_Precision, MidpointRounding.AwayFromZero);
						}
                        }
                        else
						{
                            if (!e.ExternalCall)
							throw new PXUnitConversionException((string)sender.GetValue(e.Row, _KeyField.Name));
						}
					}
				}
				sender.SetValue(e.Row, _ResultOrdinal, resultval);
			}
		}

		protected virtual void CalcBaseQty(PXCache sender, object data)
		{
			object NewValue = sender.GetValue(data, _FieldName);
			try
			{
				CalcBaseQty(sender, new PXFieldVerifyingEventArgs(data, NewValue, false));
			}
			catch (PXUnitConversionException)
			{
				sender.SetValue(data, _ResultField.Name, null);
			}
		}

		protected virtual void CalcTranQty(PXCache sender, object data)
		{
			decimal? resultval = null;

			if (_ResultField != null)
			{
				object NewValue = sender.GetValue(data, _ResultOrdinal);

				if (NewValue != null)
				{
                    var conv = ReadConversion(sender, data);
                    if(conv != null)
                    {
                        if(conv.FromUnit == conv.ToUnit)
                        {
                            _ensurePrecision(sender, data);
                            resultval = Math.Round((decimal)NewValue, (int)_Precision, MidpointRounding.AwayFromZero);
                        }
                        else if (conv.UnitRate != 0m)
					{
						_ensurePrecision(sender, data);
						resultval = Math.Round((decimal)NewValue * (conv.UnitMultDiv == MultDiv.Multiply ? 1 / (decimal)conv.UnitRate : (decimal)conv.UnitRate), (int)_Precision, MidpointRounding.AwayFromZero);
					}
				}
				}
				sender.SetValue(data, _FieldOrdinal, resultval);
			}
		}

		public static void CalcBaseQty<TField>(PXCache cache, object data)
			where TField : class, IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<TField>(data))
			{
				if (attr is PXDBQuantityAttribute)
				{
					((PXQuantityAttribute)attr).CalcBaseQty(cache, data);
					break;
				}
			}
		}

		public static void CalcTranQty<TField>(PXCache cache, object data)
			where TField : class, IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<TField>(data))
			{
				if (attr is PXQuantityAttribute)
				{
					((PXQuantityAttribute)attr).CalcTranQty(cache, data);
					break;
				}
			}
		}

		public virtual void KeyFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcBaseQty(sender, e.Row);
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;
			object NewValue = sender.GetValue(e.Row, _FieldOrdinal);
			CalcBaseQty(sender, new PXFieldVerifyingEventArgs(e.Row, NewValue, false));
		}

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CalcBaseQty(sender, e.Row);
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CalcBaseQty(sender, e);
		}
		#endregion
	}

	#endregion

	#region PXDBBaseQuantityAttribute

	public class PXDBBaseQuantityAttribute : PXDBQuantityAttribute
	{
        internal override ConversionInfo ReadConversionInfo(PXCache cache, object data)
		{
			var convInfo = base.ReadConversionInfo(cache, data);
			if (convInfo?.Conversion != null && convInfo.Conversion.FromUnit != convInfo.Conversion.ToUnit)
			{
				var conversion = PXCache<INUnit>.CreateCopy(convInfo.Conversion);
				conversion.UnitMultDiv = (convInfo.Conversion.UnitMultDiv == MultDiv.Multiply) ? MultDiv.Divide : MultDiv.Multiply;
				convInfo = new ConversionInfo(conversion, convInfo.Inventory);
			}
			return convInfo;
		}

		public PXDBBaseQuantityAttribute()
			: base()
		{
		}

		public PXDBBaseQuantityAttribute(Type keyField, Type resultField)
			: base(keyField, resultField)
		{
		}
	}

	#endregion

	#region PXDBQuantityAttribute

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class PXDBQuantityAttribute : PXDBDecimalAttribute, IPXFieldVerifyingSubscriber, IPXRowInsertingSubscriber
	{
		#region State
		private Dictionary<object, PXDBQuantityAttribute> _rowAttributes;
		protected Type _KeyField = null;
		protected Type _ResultField = null;
		protected bool _HandleEmptyKey = false;
		protected int? _OverridePrecision = null;

		public Type KeyField
		{
			get
			{
				return _KeyField;
			}
		}

		public InventoryUnitType DecimalVerifyUnits { get; set; }

		public DecimalVerifyMode DecimalVerifyMode { get; set; }

		/// <summary>
		/// Enable conversion other units to specified units for decimal verifying(<see cref="DecimalVerifyUnits"/>)
		/// </summary>
		public bool ConvertToDecimalVerifyUnits { get; set; }

		#endregion

		#region Ctor
		public PXDBQuantityAttribute()
		{
			ConvertToDecimalVerifyUnits = true;
			DecimalVerifyMode = DecimalVerifyMode.Error;
		}

		public PXDBQuantityAttribute(byte precision):this()
		{
			_OverridePrecision = precision;
		}

		public PXDBQuantityAttribute(Type keyField, Type resultField):this()
		{
			_KeyField = keyField;
			_ResultField = resultField;
		}

		public PXDBQuantityAttribute(Type keyField, Type resultField, InventoryUnitType decimalVerifyUnits) : this(keyField, resultField)
		{
			DecimalVerifyUnits = decimalVerifyUnits;
		}

		public PXDBQuantityAttribute(int precision, Type keyField, Type resultField)
			: this(keyField, resultField)
		{
			_OverridePrecision = precision;
		}

		public PXDBQuantityAttribute(int precision, Type keyField, Type resultField, InventoryUnitType decimalVerifyUnits)
			: this(keyField, resultField, decimalVerifyUnits)
		{
			_OverridePrecision = precision;
		}

		public bool HandleEmptyKey
		{
			set { this._HandleEmptyKey = value; }
			get { return this._HandleEmptyKey; }
		}

		#endregion

		#region Runtime

		public static PXNotDecimalUnitException VerifyForDecimal(PXCache cache, object data)
		{
			PXNotDecimalUnitException ex = null;
			cache.Adjust<PXDBQuantityAttribute>(data).ForAllFields(a =>
			{
				var newEx = a.VerifyForDecimalValue(cache, data);
				if (newEx != null && (ex == null || newEx.ErrorLevel > ex.ErrorLevel))
					ex = newEx;
			});
			return ex;
		}

		public static PXNotDecimalUnitException VerifyForDecimal<TField>(PXCache cache, object data)
			where TField : IBqlField
		{
			PXNotDecimalUnitException error = null;
			cache.Adjust<PXDBQuantityAttribute>(data).For<TField>(a => error = a.VerifyForDecimalValue(cache, data));
			return error;
		}

		#endregion

		#region Implementation
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_Precision = _OverridePrecision ?? CommonSetupDecPl.Qty;

			if (_ResultField == null)
				return;

			if (_KeyField != null)
			{
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_KeyField), _KeyField.Name, KeyFieldUpdated);
				_rowAttributes = new Dictionary<object, PXDBQuantityAttribute>(sender.GetComparer());
				_rowAttributes.Add(0, this);
			}
		}

		protected string GetFromUnit(PXCache cache, object data) => (string)cache.GetValue(data, _KeyField.Name);

		internal virtual ConversionInfo ReadConversionInfo(PXCache cache, object data)
		{
			var unitAttribute = cache.GetAttributesOfType<INUnitAttribute>(data, _KeyField.Name).FirstOrDefault();
			return unitAttribute == null
				? null
				: unitAttribute.ReadConversionInfo(cache, data, GetFromUnit(cache, data));
		}

		internal virtual InventoryItem ReadInventoryItem(PXCache cache, object data)
		{
			var unitAttribute = cache.GetAttributesOfType<INUnitAttribute>(data, _KeyField.Name).FirstOrDefault();
			return unitAttribute == null
				? null
				: unitAttribute.ReadInventoryItem(cache, data);
		}

		protected virtual void CalcBaseQty(PXCache sender, QtyConversionArgs e)
		{
			if (_ResultField != null)
			{
				decimal? resultval = CalcResultValue(sender, e);
				if (e.ExternalCall)
				{
					sender.SetValueExt(e.Row, this._ResultField.Name, resultval);
				}
				else
				{
					sender.SetValue(e.Row, this._ResultField.Name, resultval);
				}
			}
		}

		protected virtual decimal? CalcResultValue(PXCache sender, QtyConversionArgs e)
		{
			decimal? resultval = null;
			if (_ResultField != null)
			{
				if (e.NewValue != null)
				{
					bool handled = false;
					if (this._HandleEmptyKey)
					{
						if (string.IsNullOrEmpty(GetFromUnit(sender, e.Row)))
						{
							resultval = (decimal)e.NewValue;
							handled = true;
						}
					}
					if (!handled)
					{
						if ((decimal)e.NewValue == 0)
						{
							resultval = 0m;
						}
						else
						{
							ConversionInfo convInfo = ReadConversionInfo(sender, e.Row);
							if (convInfo?.Conversion != null)
							{
								resultval = ConvertValue(sender, e.Row, (decimal)e.NewValue, convInfo.Conversion);
								var exception = VerifyForDecimalValue(sender, convInfo.Inventory, e.Row, (decimal)e.NewValue, resultval);
								if (exception?.ErrorLevel == PXErrorLevel.Error && e.ThrowNotDecimalUnitException && !exception.IsLazyThrow)
									throw exception;
							}
							else
							{
								if (!e.ExternalCall)
									throw new PXUnitConversionException(GetFromUnit(sender, e.Row));
							}
						}
					}
				}
			}
			return resultval;
		}

		protected decimal? ConvertValue(PXCache cache, object row, decimal? value, INUnit conv)
		{
			decimal? resultval = null;
			if (conv.FromUnit == conv.ToUnit)
			{
				resultval = ConvertValue(cache, row, (decimal)value, (v, invert) => v);
			}
			else if (conv.UnitRate != 0m)
			{
				resultval = ConvertValue(cache, row, (decimal)value, 
					(v, invert) => v * (conv.UnitMultDiv == MultDiv.Multiply == invert ? 1 / (decimal)conv.UnitRate : (decimal)conv.UnitRate));
			}
			return resultval;
		}

		private decimal? ConvertValue(PXCache cache, object row, decimal value, Func<decimal, bool, decimal> converter)
		{
			_ensurePrecision(cache, row);

			decimal? resultFieldCurrentValue = (decimal?)cache.GetValue(row, _ResultField.Name);
			if (resultFieldCurrentValue != null)
			{
				decimal revValue = Math.Round(converter(resultFieldCurrentValue ?? 0m, true), (int)_Precision, MidpointRounding.AwayFromZero);
				if (revValue == value)
					return resultFieldCurrentValue;
			}
			return Math.Round(converter(value, false), (int)_Precision, MidpointRounding.AwayFromZero);
		}

		protected virtual void CalcBaseQty(PXCache sender, object data)
		{
			object NewValue = sender.GetValue(data, _FieldName);
			try
			{
				CalcBaseQty(sender, new QtyConversionArgs(data, NewValue, false));
			}
			catch (PXUnitConversionException)
			{
				sender.SetValue(data, _ResultField.Name, null);
			}
		}

		protected virtual void CalcTranQty(PXCache sender, object data)
		{
			decimal? resultval = null;

			if (_ResultField != null)
			{
				object NewValue = sender.GetValue(data, _ResultField.Name);

				if (NewValue != null)
				{
					bool handled = false;
					if (this._HandleEmptyKey)
					{
						if (string.IsNullOrEmpty(GetFromUnit(sender, data)))
						{
							resultval = (decimal)NewValue;
							handled = true;
						}
					}
					if (!handled)
					{
						if ((decimal)NewValue == 0m)
						{
							resultval = 0m;
						}
						else
						{
							ConversionInfo convInfo = ReadConversionInfo(sender, data);
							if (convInfo?.Conversion != null)
							{
								INUnit conv = convInfo.Conversion;
								if (conv.FromUnit == conv.ToUnit)
								{
									_ensurePrecision(sender, data);
									resultval = Math.Round((decimal)NewValue, (int)_Precision, MidpointRounding.AwayFromZero);
								}
								else if(conv.UnitRate != 0m)
								{
								_ensurePrecision(sender, data);
								resultval = Math.Round((decimal)NewValue * (conv.UnitMultDiv == MultDiv.Multiply ? 1 / (decimal)conv.UnitRate : (decimal)conv.UnitRate), (int)_Precision, MidpointRounding.AwayFromZero);
								}
								VerifyForDecimalValue(sender, convInfo.Inventory, data, resultval, (decimal)NewValue);
							}
						}
					}
				}
				sender.SetValue(data, _FieldOrdinal, resultval);
			}
		}

		public static void CalcBaseQty<TField>(PXCache cache, object data)
			where TField : class, IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<TField>(data))
			{
				if (attr is PXDBQuantityAttribute)
				{
					((PXDBQuantityAttribute)attr).CalcBaseQty(cache, data);
					break;
				}
			}
		}

		public static void CalcTranQty<TField>(PXCache cache, object data)
			where TField : class, IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<TField>(data))
			{
				if (attr is PXDBQuantityAttribute)
				{
					((PXDBQuantityAttribute)attr).CalcTranQty(cache, data);
					break;
				}
			}
		}

		public static decimal Round(decimal? value)
		{
			decimal value0 = value ?? 0m;
			return Math.Round(value0, CommonSetupDecPl.Qty, MidpointRounding.AwayFromZero);
		}

		public virtual void KeyFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcBaseQty(sender, e.Row);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;
			object NewValue = sender.GetValue(e.Row, _FieldOrdinal);
			CalcBaseQty(sender, new QtyConversionArgs(e.Row, NewValue, false) { ThrowNotDecimalUnitException = true });
		}

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CalcBaseQty(sender, e.Row);
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXFieldUpdatingEventArgs args = new PXFieldUpdatingEventArgs(e.Row, e.NewValue);
			if (!e.ExternalCall)
			{
				base.FieldUpdating(sender, args);
			}
			CalcBaseQty(sender, new QtyConversionArgs(args.Row, args.NewValue, true));
			e.NewValue = args.NewValue;
		}

		public void SetDecimalVerifyMode(object row, DecimalVerifyMode mode)
		{
			if (AttributeLevel == PXAttributeLevel.Type)
				return;
			if (row == null)
			{
				if (AttributeLevel == PXAttributeLevel.Cache)
					DecimalVerifyMode = mode;
			}
			else
			{
				if (AttributeLevel == PXAttributeLevel.Item)
				{
					DecimalVerifyMode = mode;
					if (_rowAttributes != null)
						_rowAttributes.First().Value._rowAttributes[row] = this;
				}
			}
		}

		private DecimalVerifyMode GetDecimalVerifyMode(object data)
		{
			PXDBQuantityAttribute rowAttribute;
			if (AttributeLevel == PXAttributeLevel.Item || _rowAttributes == null || !_rowAttributes.TryGetValue(data, out rowAttribute))
				return DecimalVerifyMode;
			return rowAttribute.DecimalVerifyMode;
		}

		public virtual PXNotDecimalUnitException VerifyForDecimalValue(PXCache cache, object data)
		{
			if (_KeyField == null
				|| data == null
				|| DecimalVerifyUnits == InventoryUnitType.None
				|| GetDecimalVerifyMode(data) == DecimalVerifyMode.Off)
				return null;
			var qty = (decimal?)cache.GetValue(data, _FieldOrdinal);
			if((qty ?? 0) == 0)
				return null;
			InventoryItem inventory = ReadInventoryItem(cache, data);
			var baseQty = (decimal?)cache.GetValue(data, _ResultField.Name);
			return VerifyForDecimalValue(cache, inventory, data, qty, baseQty);
		}

		protected virtual PXNotDecimalUnitException VerifyForDecimalValue(PXCache cache, InventoryItem inventory, object data, decimal? originalValue, decimal? baseValue)
		{
			if ((baseValue ?? 0) == 0
				|| DecimalVerifyUnits == InventoryUnitType.None
				|| inventory == null
				|| (originalValue ?? 0m) == 0m)
				return null;
			var verifyMode = GetDecimalVerifyMode(data);
			if (verifyMode == DecimalVerifyMode.Off)
				return null;
			InventoryUnitType integerUnits = PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>()
				? inventory.ToIntegerUnits()
				: (inventory.DecimalBaseUnit == true ? InventoryUnitType.None : InventoryUnitType.BaseUnit | InventoryUnitType.SalesUnit | InventoryUnitType.PurchaseUnit);
			if (integerUnits == InventoryUnitType.None)
				return null;
			string fromUnit = GetFromUnit(cache, data);
			InventoryUnitType unitTypes = inventory.ToUnitTypes(fromUnit);
			foreach (var verifyUnitType in DecimalVerifyUnits.Split())
			{
				if ((verifyUnitType & integerUnits) == 0)
					continue;
				string unitID;
				decimal value;
				bool needToConvert = false;
				if (verifyUnitType == InventoryUnitType.BaseUnit)
				{
					unitID = inventory.BaseUnit;
					value = (decimal)baseValue;
				}
				else if((verifyUnitType & unitTypes) > 0)
				{
					unitID = fromUnit;
					value = (decimal)originalValue;
				}
				else
				{
					unitID = inventory.GetUnitID(verifyUnitType);
					value = (decimal)baseValue;
					needToConvert = true;
				}
				if (fromUnit != unitID && !ConvertToDecimalVerifyUnits)
					continue;

				if (needToConvert)
				{
					var conv = INUnit.UK.ByInventory.Find(cache.Graph, inventory.InventoryID, unitID);
					_ensurePrecision(cache, data);
					value = Math.Round(value * (conv.UnitMultDiv == MultDiv.Multiply ? 1 / (decimal)conv.UnitRate : (decimal)conv.UnitRate),
						(int)_Precision, MidpointRounding.AwayFromZero);
				}
				if (value % 1 != 0)
				{
					var exception = new PXNotDecimalUnitException(verifyUnitType, inventory.InventoryCD, unitID, 
						verifyMode == DecimalVerifyMode.Error ? PXErrorLevel.Error: PXErrorLevel.Warning);
					cache.RaiseExceptionHandling(FieldName, data, originalValue, exception);
					return exception;
				}
			}
			return null;
		}

		#endregion

		protected class QtyConversionArgs
		{
			public object Row { get; }

			public object NewValue { get; }

			public bool ExternalCall { get; }

			public bool ThrowNotDecimalUnitException { get; set; }

			public QtyConversionArgs(object row, object newValue, bool externalCall)
			{
				Row = row;
				NewValue = newValue;
				ExternalCall = externalCall;
			}
		}
	}

	/// <summary>
	/// Decimal verifying modes
	/// </summary>
	public enum DecimalVerifyMode
	{
		/// <summary>
		/// Verifying is off
		/// </summary>
		Off,
		/// <summary>
		/// Generate warning
		/// </summary>
		Warning,
		/// <summary>
		/// Generate error
		/// </summary>
		Error
	}

	#endregion

	#region PXSetupOptional

	public class PXSetupOptional<Table> : PXSelectReadonly<Table>
		where Table : class, IBqlTable, new()
	{
		protected Table _Record;
		public PXSetupOptional(PXGraph graph)
			: base(graph)
		{
			graph.Defaults[typeof(Table)] = getRecord;
		}
		private object getRecord()
		{
			if (_Record == null)
			{
				_Record = base.Select();
				if (_Record == null)
				{
					_Record = new Table();
					PXCache cache = this._Graph.Caches[typeof(Table)];
					foreach (Type field in cache.BqlFields)
					{
						object newvalue;
						cache.RaiseFieldDefaulting(field.Name.ToLower(), _Record, out newvalue);
						cache.SetValue(_Record, field.Name.ToLower(), newvalue);
					}
					base.StoreCached(new PXCommandKey(new object[] { }), new List<object>{ _Record });
				}
			}
			return _Record;
		}
	}

	public class PXSetupOptional<Table, Where> : PXSelectReadonly<Table, Where>
		where Table : class, IBqlTable, new()
		where Where : IBqlWhere, new()
	{
		protected Table _Record;
		public PXSetupOptional(PXGraph graph)
			: base(graph)
		{
			graph.Defaults[typeof(Table)] = getRecord;
		}
		private object getRecord()
		{
			if (_Record == null)
			{
				_Record = base.Select();
				if (_Record == null)
				{
					_Record = new Table();
					PXCache cache = this._Graph.Caches[typeof(Table)];
					foreach (Type field in cache.BqlFields)
					{
						object newvalue;
						cache.RaiseFieldDefaulting(field.Name.ToLower(), _Record, out newvalue);
						cache.SetValue(_Record, field.Name.ToLower(), newvalue);
					}
					base.StoreCached(new PXCommandKey(new object[] { }), new List<object>{ _Record });
				}
			}
			return _Record;
		}
	}

	#endregion

	#region PXCalcQuantityAttribute
	public class PXCalcQuantityAttribute : PXDecimalAttribute
	{
		#region State
		protected int _SourceOrdinal;
		protected int _KeyOrdinal;
		protected Type _KeyField = null;
		protected Type _SourceField = null;
		protected bool _LegacyBehavior;

		#endregion

		#region Ctor
		public PXCalcQuantityAttribute()
		{			
		}

		/// <summary>
		/// Calculates TranQty using BaseQty and UOM. TranQty will be calculated on RowSelected event only.
		/// </summary>
		/// <param name="keyField">UOM field</param>
		/// <param name="sourceField">BaseQty field</param>
		public PXCalcQuantityAttribute(Type keyField, Type sourceField) : this(keyField, sourceField, true)
		{
		}

		/// <summary>
		/// Calculates TranQty using BaseQty and UOM.
		/// </summary>
		/// <param name="keyField">UOM field</param>
		/// <param name="sourceField">BaseQty field</param>
		/// <param name="legacyBehavior">When set to True, TranQty will be calculated on RowSelected and on FieldSelecting (when needed) events.</param>
		public PXCalcQuantityAttribute(Type keyField, Type sourceField, bool legacyBehavior)
		{
			_KeyField = keyField;
			_SourceField = sourceField;
			_LegacyBehavior = legacyBehavior;
		}
		#endregion

		#region Runtime
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_Precision = CommonSetupDecPl.Qty;

			if (_SourceField != null)
			{
				_SourceOrdinal = sender.GetFieldOrdinal(_SourceField.Name);
			}

			sender.Graph.RowSelected.AddHandler(sender.GetItemType(), RowSelected);

			if (!_LegacyBehavior)
				sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _FieldName, TranQtyFieldSelecting);

			if (_KeyField != null)
			{
				_KeyOrdinal = sender.GetFieldOrdinal(_KeyField.Name);
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_KeyField), _KeyField.Name, KeyFieldUpdated);
			}

		}
		#endregion

		#region Implementation
		internal virtual INUnit ReadConversion(PXCache cache, object data)
				{
			var unitAttribute = cache.GetAttributesOfType<INUnitAttribute>(data, _KeyField.Name).FirstOrDefault();
			return unitAttribute == null
				? null
				: unitAttribute.ReadConversion(cache, data, (string)cache.GetValue(data, _KeyField.Name));
		}

		protected virtual void CalcTranQty(PXCache sender, object data)
		{
			decimal? resultval = GetTranQty(sender, data);

			sender.SetValue(data, _FieldOrdinal, resultval ?? 0m);
		}

		protected virtual decimal? GetTranQty(PXCache sender, object data)
		{
			decimal? resultval = null;

			if (_SourceField != null)
			{
				object NewValue = sender.GetValue(data, _SourceOrdinal);

				if (NewValue != null)
				{
					INUnit conv = ReadConversion(sender, data);
                    if (conv != null)
                    {
						if (conv.FromUnit == conv.ToUnit)
						{
							_ensurePrecision(sender, data);
							resultval = Math.Round((decimal)NewValue, (int)_Precision, MidpointRounding.AwayFromZero);
						}
						else if (conv.UnitRate != 0m)
					{
						_ensurePrecision(sender, data);
						resultval = Math.Round((decimal)NewValue * (conv.UnitMultDiv == MultDiv.Multiply ? 1 / (decimal)conv.UnitRate : (decimal)conv.UnitRate), (int)_Precision, MidpointRounding.AwayFromZero);
					}
				}
			}
			}
			return resultval;
		}

		public virtual void KeyFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcTranQty(sender, e.Row);
		}
		
		protected virtual void TranQtyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			SOShipLine row = e.Row as SOShipLine;
			if (e.Row != null && e.ReturnValue == null)
			{
				e.ReturnValue = GetTranQty(sender, e.Row);
			}
		}
		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (sender.GetValue(e.Row, _FieldOrdinal) == null)
				CalcTranQty(sender, e.Row);
		}
		#endregion
	}
    #endregion    

    #region INSiteStatusLookup
    public class INSiteStatusLookup<Status, StatusFilter> : PXSelectBase<Status>
		where Status : class, IBqlTable, new()
		where StatusFilter : class, IBqlTable, new()
	{
		protected class LookupView : PXView
		{
			public LookupView(PXGraph graph, BqlCommand command)
				: base(graph, true, command)
			{				
			}

			public LookupView(PXGraph graph, BqlCommand command, Delegate handler)
				: base(graph, true, command, handler)
			{
			}

			protected PXSearchColumn CorrectFieldName(PXSearchColumn orig, bool idFound)
			{
				switch (orig.Column.ToLower())
				{
					case "inventoryid":
						if (!idFound)
							return new PXSearchColumn("InventoryCD", orig.Descending, orig.OrigSearchValue ?? orig.SearchValue);
						else
							return null;
					case "subitemid":
						return new PXSearchColumn("SubItemCD", orig.Descending, orig.OrigSearchValue ?? orig.SearchValue);
					case "siteid":
						return new PXSearchColumn("SiteCD", orig.Descending, orig.OrigSearchValue ?? orig.SearchValue);
					case "locationid":
						return new PXSearchColumn("LocationCD", orig.Descending, orig.OrigSearchValue ?? orig.SearchValue);
				}
				return orig;
			}

			protected override List<object> InvokeDelegate(object[] parameters)
			{
				if (MaximumRows == 1 && StartRow == 0 &&
					Searches?.Length == Cache.Keys.Count &&
					Searches.All(s => s != null) &&
					SortColumns?.Length == Cache.Keys.Count &&
					SortColumns.SequenceEqual(Cache.Keys, StringComparer.OrdinalIgnoreCase))
				{
					return base.InvokeDelegate(parameters);
				}

				var context = PXView._Executing.Peek();
				var orig = context.Sorts;

				bool idFound = false;
				var result = new List<PXSearchColumn>();
				const string iD = "InventoryCD";
				for (int i = 0; i < orig.Length - this.Cache.Keys.Count; i++)
				{
					result.Add(CorrectFieldName(orig[i], false));
					if (orig[i].Column == iD)
						idFound = true;
				}

				for (int i = orig.Length - this.Cache.Keys.Count; i < orig.Length; i++)
				{
					var col = CorrectFieldName(orig[i], idFound);
					if (col != null)
						result.Add(col);
				}
				context.Sorts = result.ToArray();

				return base.InvokeDelegate(parameters);
			}
		}

		public const string Selected = "Selected";
		public const string QtySelected = "QtySelected";
		private PXView intView;
		#region Ctor
		public INSiteStatusLookup(PXGraph graph)
		{
			this.View = new PXView(graph, false, 
				BqlCommand.CreateInstance(BqlCommand.Compose(typeof(Select<>), typeof(Status))), 
				new PXSelectDelegate(viewHandler));
			InitHandlers(graph);
		}

		public INSiteStatusLookup(PXGraph graph, Delegate handler)
		{
			this.View = new PXView(graph, false,
				BqlCommand.CreateInstance(typeof(Select<>), typeof(Status)),
				handler);			
			InitHandlers(graph);
		}
		#endregion

		#region Implementations

		private void InitHandlers(PXGraph graph)
		{
			graph.RowSelected.AddHandler(typeof(StatusFilter), OnFilterSelected);
			graph.RowSelected.AddHandler(typeof(Status), OnRowSelected);
			graph.FieldUpdated.AddHandler(typeof(Status), Selected, OnSelectedUpdated);
		}

		protected virtual void OnFilterSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INSiteStatusFilter row = e.Row as INSiteStatusFilter;
			if (row != null)
				PXUIFieldAttribute.SetVisible(sender.Graph.Caches[typeof(Status)], typeof(INSiteStatusByCostCenter.siteID).Name, row.SiteID == null);
		}

		
		protected virtual void OnSelectedUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			bool? selected = (bool?)sender.GetValue(e.Row, Selected);
			decimal? qty = (decimal?)sender.GetValue(e.Row, QtySelected);
			if (selected == true)
			{
				if(qty == null || qty == 0m)
					sender.SetValue(e.Row, QtySelected, 1m);
			}
			else
				sender.SetValue(e.Row, QtySelected, 0m);
		}

		protected virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, Selected, true);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, QtySelected, true);
		}	

		protected virtual PXView CreateIntView(PXGraph graph)
		{
			PXCache cache = graph.Caches[typeof(Status)];

			List<Type> select = new List<Type>();
			select.Add(typeof(Select<,>));
			select.Add(typeof(Status));
			select.Add(CreateWhere(graph));

            //select.Add(typeof(Aggregate<>));
            /*
			List<Type> groupFields = cache.BqlKeys;
			groupFields.AddRange(cache.BqlFields.Where(field => field.IsDefined(typeof (PXExtraKeyAttribute), false)));			

			for (int i = 0; i < groupFields.Count; i++)
			{
				select.Add((i != groupFields.Count - 1) ? typeof(GroupBy<,>) : typeof(GroupBy<>));
				select.Add(groupFields[i]);
			}
			*/
            Type selectType = BqlCommand.Compose(select.ToArray());

			return new LookupView(graph, BqlCommand.CreateInstance(selectType));
		}

		protected virtual IEnumerable viewHandler()
		{
			if (intView == null) intView = CreateIntView(this.View.Graph);
			var startRow = PXView.StartRow;
			var totalRows = 0;

			var rows = new PXDelegateResult();

			foreach (var rec in intView.Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
								 ref startRow, PXView.MaximumRows, ref totalRows))
			{
				Status item = PXResult.Unwrap<Status>(rec);
				Status result = item;
				Status updated = this.Cache.Locate(item) as Status;
				if (updated != null && this.Cache.GetValue(updated, Selected) as bool? == true)
				{
					Decimal? qty = this.Cache.GetValue(updated, QtySelected) as Decimal?;
					this.Cache.RestoreCopy(updated,item);
					this.Cache.SetValue(updated, Selected, true);
					this.Cache.SetValue(updated, QtySelected, qty);
					result = updated;
				}
				rows.Add(result);
			}
			PXView.StartRow = 0;

			if (PXView.ReverseOrder)
				rows.Reverse();

			rows.IsResultSorted = true;

			return rows;
		}

		protected static Type CreateWhere(PXGraph graph)
		{
			PXCache filter = graph.Caches[typeof(INSiteStatusFilter)];
			PXCache cache = graph.Caches[typeof(Status)];

			Type where = typeof(Where<boolTrue, Equal<boolTrue>>);
			foreach (string field in filter.Fields)
			{
				if (cache.Fields.Contains(field))
				{
					if (filter.Fields.Contains(field + "Wildcard")) continue;
					if (field.Contains("SubItem") && !PXAccess.FeatureInstalled<FeaturesSet.subItem>()) continue;
					if (field.Contains("Site") && !PXAccess.FeatureInstalled<FeaturesSet.warehouse>()) continue;
					if (field.Contains("Location") && !PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>()) continue;
					Type sourceType = filter.GetBqlField(field);
					Type destinationType = cache.GetBqlField(field);
					if (sourceType != null && destinationType != null)
					{
						where = BqlCommand.Compose(
							typeof(Where2<,>),
							typeof(Where<,,>),
							typeof(Current<>), sourceType, typeof(IsNull),
							typeof(Or<,>), destinationType, typeof(Equal<>), typeof(Current<>), sourceType,
							typeof(And<>), where
						);
					}
				}
				string f;
				if (field.Length > 8 && field.EndsWith("Wildcard") && cache.Fields.Contains(f = field.Substring(0, field.Length - 8)))
				{
					if (field.Contains("SubItem") && !PXAccess.FeatureInstalled<FeaturesSet.subItem>()) continue;
					if (field.Contains("Site") && !PXAccess.FeatureInstalled<FeaturesSet.warehouse>()) continue;
					if (field.Contains("Location") && !PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>()) continue;
					Type like = filter.GetBqlField(field);
					Type dest = cache.GetBqlField(f);
					where = BqlCommand.Compose(
						typeof(Where2<,>),
						typeof(Where<,,>), typeof(Current<>), like, typeof(IsNull),
						typeof(Or<,>), dest, typeof(Like<>), typeof(Current<>), like,
						typeof(And<>), where
						);
				}
			}		
			return where;
		}

		protected static Type GetTypeField<Source>(string field)
		{
			Type sourceType = typeof(Source);
			Type fieldType = null;
			while (fieldType == null && sourceType != null)
			{
				fieldType = sourceType.GetNestedType(field, System.Reflection.BindingFlags.Public);
				sourceType = sourceType.BaseType;
			}
			return fieldType;
		}

		private class Zero : PX.Data.BQL.BqlDecimal.Constant<Zero>
		{
			public Zero() : base(0m) { }
		}
		#endregion
	}
	#endregion

	#region INBarCodeItemLookup
	public class INBarCodeItemLookup<Filter> : PXFilter<Filter>
		where Filter : INBarCodeItem, new()
	{
		#region Ctor
		public INBarCodeItemLookup(PXGraph graph)
			:base(graph)
		{
			InitHandlers(graph);
		}

		public INBarCodeItemLookup(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			InitHandlers(graph);
		}
		#endregion

		private void InitHandlers(PXGraph graph)
		{
			graph.RowSelected.AddHandler(typeof(Filter), OnFilterSelected);

			graph.FieldUpdated.AddHandler(typeof(Filter), typeof(INBarCodeItem.barCode).Name, Filter_BarCode_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(Filter), typeof(INBarCodeItem.inventoryID).Name, Filter_InventoryID_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(Filter), typeof(INBarCodeItem.byOne).Name, Filter_ByOne_FieldUpdated);					
		}

		public virtual void Reset(bool keepDescription)
		{
			Filter s = this.Current;
			this.Cache.Remove(s);
			this.Cache.Insert(this.Cache.CreateInstance());
			this.Current.ByOne = s.ByOne;
			this.Current.AutoAddLine = s.AutoAddLine;		
			if(keepDescription)
				this.Current.Description = s.Description;							
		}
		
		#region Filter Events
		protected virtual void Filter_BarCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.ExternalCall)
			{
				var rec = (PXResult<INItemXRef, InventoryItem, INSubItem>)
									PXSelectJoin<INItemXRef,
										InnerJoin<InventoryItem,
														On2<INItemXRef.FK.InventoryItem,
														And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
														And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noPurchases>,
														And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noRequest>,
														And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>>>>,
										InnerJoin<INSubItem,
													 On<INItemXRef.FK.SubItem>>>,
										Where<INItemXRef.alternateID, Equal<Current<INBarCodeItem.barCode>>,
											And<INItemXRef.alternateType, In3<INAlternateType.barcode, INAlternateType.gIN>>>>
										.SelectSingleBound(this._Graph, new object[] { e.Row });
				if (rec != null)
				{
					sender.SetValue<INBarCodeItem.inventoryID>(e.Row, null);
					sender.SetValuePending<INBarCodeItem.inventoryID>(e.Row, ((InventoryItem)rec).InventoryCD);
					sender.SetValuePending<INBarCodeItem.subItemID>(e.Row, ((INSubItem)rec).SubItemCD);

					INItemXRef xRef = (INItemXRef)rec;
					if (!string.IsNullOrEmpty(xRef.UOM))
						sender.SetValuePending<INBarCodeItem.uOM>(e.Row, xRef.UOM);
				}
				else
				{
					sender.SetValuePending<INBarCodeItem.inventoryID>(e.Row, null);
					sender.SetValuePending<INBarCodeItem.subItemID>(e.Row, null);
				}
			}
		}
		protected virtual void Filter_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.ExternalCall)
			{
				Filter row = e.Row as Filter;
				if (e.OldValue != null && row.InventoryID != null)
					row.BarCode = null;
				sender.SetDefaultExt<INBarCodeItem.subItemID>(e);
				sender.SetDefaultExt<INBarCodeItem.qty>(e);
			}
		}
		protected virtual void Filter_ByOne_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if(e.ExternalCall)
			{
				Filter row = e.Row as Filter;				
				if (row != null && row.ByOne == true)
					row.Qty = 1m;
			}
		}
		
		protected virtual void OnFilterSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Filter row = e.Row as Filter;
			if (row != null)
			{
				var item = InventoryItem.PK.Find(_Graph, row.InventoryID);
				var lotclass = INLotSerClass.PK.Find(_Graph, item?.LotSerClassID);

				bool requestLotSer = lotclass != null && lotclass.LotSerTrack != INLotSerTrack.NotNumbered &&
														 lotclass.LotSerAssign == INLotSerAssign.WhenReceived;

				PXUIFieldAttribute.SetEnabled<INBarCodeItem.lotSerialNbr>(sender, null, requestLotSer || sender.Graph.IsContractBasedAPI);
				PXDefaultAttribute.SetPersistingCheck<INBarCodeItem.lotSerialNbr>(sender, null, requestLotSer ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXUIFieldAttribute.SetEnabled<INBarCodeItem.expireDate>(sender, null, (requestLotSer && lotclass.LotSerTrackExpiration == true) || sender.Graph.IsContractBasedAPI);
				PXDefaultAttribute.SetPersistingCheck<INBarCodeItem.expireDate>(sender, null, requestLotSer && lotclass.LotSerTrackExpiration == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXUIFieldAttribute.SetEnabled<INBarCodeItem.uOM>(sender, null, !(requestLotSer && lotclass.LotSerTrack == INLotSerTrack.SerialNumbered) && row.ByOne != true && row.InventoryID != null);
				PXUIFieldAttribute.SetEnabled<INBarCodeItem.inventoryID>(sender, null, (string.IsNullOrEmpty(row.BarCode) || row.InventoryID == null));
			}
		}
		#endregion
	}
	#endregion

	#region INOpenPeriod
	public class INOpenPeriodAttribute : OpenPeriodAttribute
	{
		#region Ctor
		public INOpenPeriodAttribute()
			: this(null)
		{
		}	

		public INOpenPeriodAttribute(Type sourceType,
			Type branchSourceType = null,
			Type branchSourceFormulaType = null,
			Type organizationSourceType = null,
			Type useMasterCalendarSourceType = null,
			Type defaultType = null,
			bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
			SelectionModesWithRestrictions selectionModeWithRestrictions = SelectionModesWithRestrictions.Undefined,
			Type masterFinPeriodIDType = null)
			: base(typeof(Search<FinPeriod.finPeriodID,
					Where<FinPeriod.iNClosed, Equal<False>,
						And<FinPeriod.status, Equal<FinPeriod.status.open>>>>),
					sourceType,
					branchSourceType: branchSourceType,
					branchSourceFormulaType: branchSourceFormulaType,
					organizationSourceType: organizationSourceType,
					useMasterCalendarSourceType: useMasterCalendarSourceType,
					defaultType: defaultType,
					redefaultOrRevalidateOnOrganizationSourceUpdated: redefaultOrRevalidateOnOrganizationSourceUpdated,
					selectionModeWithRestrictions: selectionModeWithRestrictions,
					masterFinPeriodIDType: masterFinPeriodIDType)
		{
		}

		#endregion


		#region Implementation

		protected override PeriodValidationResult ValidateOrganizationFinPeriodStatus(PXCache sender, object row, FinPeriod finPeriod)
		{
			PeriodValidationResult result = base.ValidateOrganizationFinPeriodStatus(sender, row, finPeriod);

			if (!result.HasWarningOrError && finPeriod.INClosed == true)
			{
				result = HandleErrorThatPeriodIsClosed(sender, finPeriod, errorMessage: Messages.FinancialPeriodClosedInIN);
			}

			return result;
		}

		#endregion
	}
	#endregion
	
	#region INParentItemClassAttribute

	/// <summary>
	/// The attribute is supposed to find and assign Parent Item Class for a newly created Child Item Class.
	/// </summary>
	public class INParentItemClassAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber, IPXRowInsertedSubscriber
	{
		protected readonly bool _DefaultStkItemFromParent;
		public bool InsertCurySettings { get; set; }

		public INParentItemClassAttribute(bool defaultStkItemFromParent = false)
		{
			_DefaultStkItemFromParent = defaultStkItemFromParent;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (_DefaultStkItemFromParent)
			{
				sender.Graph.FieldDefaulting.AddHandler<INItemClass.stkItem>(StkItemDefaulting);
			}
		}

		protected virtual void StkItemDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INItemClass parent = LookupNearestParent(sender, e);
			e.NewValue = (parent != null) ? parent.StkItem : true;
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INItemClass parent = LookupNearestParent(sender, e);
			if (parent != null)
			{
				e.NewValue = parent.ItemClassID;
			}
			else
			{
				ItemClassDefaulting(sender, e);
			}
		}

		protected virtual void ItemClassDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INSetup inSetup = PXSelectReadonly<INSetup>.Select(sender.Graph);
			if (inSetup != null)
			{
				bool? stkItem = (bool?)sender.GetValue<INItemClass.stkItem>(e.Row);
				e.NewValue = (stkItem == false) ? inSetup.DfltNonStkItemClassID : inSetup.DfltStkItemClassID;
			}
		}

		protected virtual INItemClass LookupNearestParent(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			string segmentedKey = (string)sender.GetValue<INItemClass.itemClassCD>(e.Row) ?? string.Empty;
			segmentedKey = segmentedKey.Trim();
			if (string.IsNullOrEmpty(segmentedKey))
			{
				return null;
			}

			Segment[] segments = PXSelectReadonly<Segment,
				Where<Segment.dimensionID, Equal<INItemClass.dimension>>, OrderBy<Asc<Segment.segmentID>>>
				.Select(sender.Graph).RowCast<Segment>().ToArray();
			if (segments.Length == 0)
			{
				return null;
			}

			int[] lengthsBySegments = new int[segments.Length];
			int filledSegmentsCnt = 0;
			for (int i = 0; i < segments.Length; i++)
			{
				lengthsBySegments[i] = segments[i].Length.Value + (i == 0 ? 0 : lengthsBySegments[i - 1]);
				if (segmentedKey.Length > lengthsBySegments[i])
				{
					filledSegmentsCnt++;
				}
			}

			INItemClass parent = null;
			while (parent == null && filledSegmentsCnt > 0)
			{
				int length = lengthsBySegments[filledSegmentsCnt - 1];
				string partOfSegmentedKey = segmentedKey.Substring(0, length);
				parent = PXSelectReadonly<INItemClass, Where<INItemClass.itemClassCD, Equal<Required<INItemClass.itemClassCD>>>>
					.Select(sender.Graph, partOfSegmentedKey);

				filledSegmentsCnt--;
			}

			return parent;
		}

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (!InsertCurySettings)
				return;

			CopyCurySettings(sender.Graph, e.Row as INItemClass);
		}

		public virtual void CopyCurySettings(PXGraph graph, INItemClass itemClass)
		{
			if (string.IsNullOrEmpty(itemClass?.ItemClassCD))
				return;

			var curySettingsSelect =
				new SelectFrom<INItemClassCurySettings>
				.Where<INItemClassCurySettings.itemClassID.IsEqual<@P.AsInt>>
				.View(graph);

			var curySettingsCache = curySettingsSelect.Cache;

			using (new ReadOnlyScope(curySettingsCache))
			{
				var curySettingsRows = curySettingsSelect.Select(itemClass.ItemClassID);
				foreach (var curySettings in curySettingsRows)
					curySettingsCache.Delete(curySettings);

				var parentCurySettingsRows = curySettingsSelect.Select(itemClass.ParentItemClassID)
					.RowCast<INItemClassCurySettings>().ToList();

				if (!parentCurySettingsRows.Any(
					s => string.Equals(s.CuryID, graph.Accessinfo.BaseCuryID, StringComparison.OrdinalIgnoreCase)))
				{
					parentCurySettingsRows.Add(new INItemClassCurySettings() { CuryID = graph.Accessinfo.BaseCuryID });
				}

				PXFieldVerifying cancelVerifying = (c, ea) => ea.Cancel = true;
				graph.FieldVerifying.AddHandler<INItemClassCurySettings.dfltSiteID>(cancelVerifying);

				try
				{
					foreach (INItemClassCurySettings parentCurySettings in parentCurySettingsRows)
					{
						var newCurySettings = (INItemClassCurySettings)curySettingsCache.CreateCopy(parentCurySettings);
						newCurySettings.ItemClassID = itemClass.ItemClassID;
						newCurySettings = (INItemClassCurySettings)curySettingsCache.Insert(newCurySettings);
						if (newCurySettings == null)
							throw new PXException(Messages.CopyingSettingsFailed);

					}
				}
				finally
				{
					graph.FieldVerifying.RemoveHandler<INItemClassCurySettings.dfltSiteID>(cancelVerifying);
				}
			}
		}
	}

	#endregion

	#region INSyncUomsAttribute

	/// <summary>
	/// The attribute is supposed to synchronize values of different units of measure settings in the case if the Multiple Units of Measure feature is disabled.
	/// </summary>
	public class INSyncUomsAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
	{
		protected readonly Type[] _uomFieldList;

		public INSyncUomsAttribute(params Type[] uomFieldList)
		{
			_uomFieldList = uomFieldList;
		}

		public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>())
			{
				var newValue = sender.GetValue(e.Row, _FieldOrdinal);
				foreach (Type uomField in _uomFieldList)
				{
					sender.SetValue(e.Row, uomField.Name, newValue);
				}
			}
		}
	}

	#endregion

	/// <summary>
	/// FinPeriod selector that extends <see cref="FinPeriodSelectorAttribute"/>. 
	/// Displays and accepts only Closed Fin Periods. 
	/// When Date is supplied through aSourceType parameter FinPeriod is defaulted with the FinPeriod for the given date.
	/// </summary>
	public class INClosedPeriodAttribute : FinPeriodSelectorAttribute
	{
		public INClosedPeriodAttribute(Type aSourceType)
			: base(typeof(Search<FinPeriod.finPeriodID, 
				Where<FinPeriod.status, NotEqual<FinPeriod.status.inactive>,
						Or<FinPeriod.iNClosed, Equal<True>>>, 
				OrderBy<Desc<FinPeriod.finPeriodID>>>))
		{
		}

		public INClosedPeriodAttribute()
			: this(null)
		{

		}
	}
	
	#region SubItemStatusVeryfier
	public class SubItemStatusVeryfierAttribute : PXEventSubscriberAttribute
	{
		protected readonly Type inventoryID;
		protected readonly Type siteID;
		protected readonly string[] statusrestricted;

		public SubItemStatusVeryfierAttribute(Type inventoryID, Type siteID, params string[] statusrestricted)
		{
			this.inventoryID = inventoryID;
			this.siteID = siteID;
			this.statusrestricted = statusrestricted;
		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler(sender.GetItemType(), _FieldName, OnSubItemFieldVerifying);
			sender.Graph.FieldVerifying.AddHandler(sender.GetItemType(), siteID.Name, OnSiteFieldVerifying);
		}
		
		protected virtual void OnSubItemFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{			
			if(!Validate(sender, 
				(int?)sender.GetValue(e.Row, inventoryID.Name),
				(int?)e.NewValue,
				(int?)sender.GetValue(e.Row, siteID.Name)))
				throw new PXSetPropertyException(Messages.RestictedSubItem);
		}

		protected virtual void OnSiteFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{			
			if (!Validate(sender,
				(int?)sender.GetValue(e.Row, inventoryID.Name),
				(int?)sender.GetValue(e.Row, _FieldName),
				(int?)e.NewValue))
				throw new PXSetPropertyException(Messages.RestictedSubItem);			
		}

		private bool Validate(PXCache sender, int? invetroyID, int? subitemID, int? siteID)
		{
			INItemSiteReplenishment settings =
				PXSelect<INItemSiteReplenishment,
					Where<INItemSiteReplenishment.inventoryID, Equal<Required<INItemSiteReplenishment.inventoryID>>,
						And<INItemSiteReplenishment.subItemID, Equal<Required<INItemSiteReplenishment.subItemID>>,
							And<INItemSiteReplenishment.siteID, Equal<Required<INItemSiteReplenishment.siteID>>>>>>.SelectWindowed(
								sender.Graph, 0, 1, invetroyID, subitemID, siteID);
			if(settings != null)
				foreach (string status in statusrestricted)
				{
					if(status == settings.ItemStatus) return false;
				}
			return true;
		}
	}
	#endregion

	#region PXSelectorWithoutVerificationAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class PXSelectorWithoutVerificationAttribute : PXSelectorAttribute
	{
        public PXSelectorWithoutVerificationAttribute(Type type)
            : base(type)
		{
		}

        public PXSelectorWithoutVerificationAttribute(Type type, params Type[] fieldList)
            : base(type, fieldList)
		{
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			//base.FieldVerifying(sender, e);
		}
	}

	#endregion

	#region INRegisterCacheNameAttribute

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class INRegisterCacheNameAttribute : PX.Data.PXCacheNameAttribute
	{
		public INRegisterCacheNameAttribute(string name)
			: base(name)
		{
		}

		public override string GetName(object row)
		{
			var register = row as INRegister;
			if (register == null) return base.GetName();

			var result = Messages.Receipt;
			switch (register.DocType)
			{
				case INDocType.Issue:
					result = Messages.Issue;
					break;
				case INDocType.Transfer:
					result = Messages.Transfer;
					break;
				case INDocType.Adjustment:
					result = Messages.Adjustment;
					break;
				case INDocType.Production:
					result = Messages.Production;
					break;
				case INDocType.Disassembly:
					result = Messages.Disassembly;
					break;
			}
			return result;
		}
	}

	#endregion


	public class INSubItemSegmentValueList : 
		PXSelect<INSubItemSegmentValue, Where<INSubItemSegmentValue.inventoryID, Equal<Current<InventoryItem.inventoryID>>>>		
	{
        [PXHidden]
		public class SValue : IBqlTable
		{
			#region SegmentID
			public abstract class segmentID : PX.Data.BQL.BqlShort.Field<segmentID> { }
			protected Int16? _SegmentID;
			[PXDBShort(IsKey = true)]			
			[PXUIField(DisplayName = "Segment ID", Visibility = PXUIVisibility.Invisible, Visible = false)]
			public virtual Int16? SegmentID
			{
				get
				{
					return this._SegmentID;
				}
				set
				{
					this._SegmentID = value;
				}
			}
			#endregion
			#region Value
			public abstract class value : PX.Data.BQL.BqlString.Field<value> { }
			protected String _Value;
			[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
			[PXDefault()]
			[PXUIField(DisplayName = "Value", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]			
			public virtual String Value
			{
				get
				{
					return this._Value;
				}
				set
				{
					this._Value = value;
				}
			}
			#endregion
			#region Descr
			public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
			protected String _Descr;
			[PXDBString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual String Descr
			{
				get
				{
					return this._Descr;
				}
				set
				{
					this._Descr = value;
				}
			}
			#endregion
			#region Active
			public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
			protected Boolean? _Active;
			[PXDBBool()]
			[PXDefault((bool)false)]
			[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.Visible)]
			public virtual Boolean? Active
			{
				get
				{
					return this._Active;
				}
				set
				{
					this._Active = value;
				}
			}
			#endregion			
		}

		/// <summary>
		/// String pattern for dynamic Subitem views
		/// </summary>
		public const string SubItemViewsPattern = "SubItem_";

		/// <summary>
		/// Gets the number of Subitem segments if the appropriate feature is on,
		/// otherwise returns <c>null</c>
		/// </summary>
		public int? SegmentsNumber
		{
			get;
			protected set;
		}

		public INSubItemSegmentValueList(PXGraph graph)
			:base(graph)
		{
			graph.Caches[typeof (SValue)].AllowInsert = graph.Caches[typeof (SValue)].AllowDelete = false;
			if (!PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			{
				this.AllowSelect = false;
				return;
			}

			SegmentsNumber = 0;
			foreach (Segment s in PXSelect<Segment,
				Where<Segment.dimensionID, Equal<SubItemAttribute.dimensionName>>,
				OrderBy<Asc<Segment.segmentID>>>.Select(graph))
			{
				SegmentsNumber++;
				int? segmentID = s.SegmentID;
				graph.Views.Add("DimensionsSubItem",
					new PXView(graph, false, BqlCommand.CreateInstance(typeof(Select<Segment, 
						Where<Segment.dimensionID, Equal<SubItemAttribute.dimensionName>>,
						OrderBy<Asc<Segment.segmentID>>>)))
					);

				var subitemSegmentView = new PXView(graph, false,
					BqlCommand.CreateInstance(typeof(Select<SValue>)),
					(PXSelectDelegate)delegate ()
					{
						PXCache cache = graph.Caches[typeof(SValue)];
						List<SValue> list = new List<SValue>();
						foreach (PXResult<SegmentValue, INSubItemSegmentValue> r in
							PXSelectJoin<SegmentValue,
								LeftJoin<INSubItemSegmentValue,
											On<INSubItemSegmentValue.inventoryID, Equal<Current<InventoryItem.inventoryID>>,
										 And<INSubItemSegmentValue.segmentID, Equal<SegmentValue.segmentID>,
										 And<INSubItemSegmentValue.value, Equal<SegmentValue.value>>>>>,
							Where<SegmentValue.dimensionID, Equal<SubItemAttribute.dimensionName>,
 								And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>>>>.Select(graph, segmentID))
						{
							SegmentValue segValue = r;
							INSubItemSegmentValue itemValue = r;
							SValue result = (SValue)cache.CreateInstance();
							result.SegmentID = segValue.SegmentID;
							result.Value = segValue.Value;
							result.Descr = segValue.Descr;
							if (itemValue.InventoryID != null)
								result.Active = true;
							result = (SValue)(cache.Insert(result) ?? cache.Locate(result));
							list.Add(result);
						}
						return list;
					});
				graph.Views.Add(SubItemViewsPattern + s.SegmentID, subitemSegmentView);
				PXDependToCacheAttribute.AddDependencies(subitemSegmentView, new[] { typeof(InventoryItem) });

				graph.RowUpdated.AddHandler<SValue>(OnRowUpdated);
			}
		}

		protected virtual void OnRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SValue row = e.Row as SValue;
			if (row == null) return;
			INSubItemSegmentValue result = (INSubItemSegmentValue)this.Cache.CreateInstance();
			this.Cache.SetDefaultExt<INSubItemSegmentValue.inventoryID>(result);			
			result.SegmentID = row.SegmentID;
			result.Value = row.Value;
			if(row.Active == true)			
				this.Cache.Update(result);
			else
				this.Cache.Delete(result);							
		}
	}
}
