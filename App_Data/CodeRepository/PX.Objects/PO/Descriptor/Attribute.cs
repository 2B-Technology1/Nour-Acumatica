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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM.Extensions;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects.Extensions.MultiCurrency;

namespace PX.Objects.PO
{
	/// <summary>
	/// Extension of the string PXStringList attribute, which allows to define list <br/>
	/// of hidden(possible) values and their lables. Usually, this list must be wider, then list of <br/>
	/// enabled values - which mean that UI control may display more values then user is allowed to select in it<br/>
	/// </summary>
	public class PXStringListExtAttribute : PXStringListAttribute
	{
		#region State
		protected string[] _HiddenValues;
		protected string[] _HiddenLabels;
		protected string[] _HiddenLabelsLocal;
		#endregion

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="allowedValues">List of the string values, that user can select in the UI</param>
        /// <param name="allowedLabels">List of the labels for these values</param>
        /// <param name="hiddenValues">List of the string values, that may appear in the list. Normally, it must contain all the values from allowedList </param>
        /// <param name="hiddenLabels">List of the labels for these values</param>
		public PXStringListExtAttribute(string[] allowedValues, string[] allowedLabels, string[] hiddenValues, string[] hiddenLabels)
			: base(allowedValues, allowedLabels)
		{
			_HiddenValues = hiddenValues;
			_HiddenLabels = hiddenLabels;
			_HiddenLabelsLocal = null;
			_ExclusiveValues = false;
		}

		protected PXStringListExtAttribute(Tuple<string, string>[] allowedPairs, Tuple<string, string>[] hiddenPairs)
			: this(
				allowedPairs.Select(t => t.Item1).ToArray(),
				allowedPairs.Select(t => t.Item2).ToArray(),
				hiddenPairs.Select(t => t.Item1).ToArray(),
				hiddenPairs.Select(t => t.Item2).ToArray()
				) {}

	    public override void  CacheAttached(PXCache sender)
		{
			TryLocalize(sender);
 			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, OnFieldUpdating);
		}
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);
			if (_HiddenLabelsLocal == null)
			{
				if (!System.Globalization.CultureInfo.InvariantCulture.Equals(System.Threading.Thread.CurrentThread.CurrentCulture) &&
					_HiddenLabels != null && _HiddenValues != null)
				{
					_HiddenLabelsLocal = new string[_HiddenLabels.Length];

					_HiddenLabels.CopyTo(_HiddenLabelsLocal, 0);
					if (_BqlTable != null)
					{
						for (int i = 0; i < _HiddenLabels.Length; i++)
						{
							string value = PXUIFieldAttribute.GetNeutralDisplayName(sender, _FieldName) + " -> " + _HiddenLabels[i];
							string temp = PXLocalizer.Localize(value, _BqlTable.FullName);
							if (!string.IsNullOrEmpty(temp) && temp != value)
								_HiddenLabelsLocal[i] = temp;
						}
					}
				}
				else
					_HiddenLabelsLocal = _HiddenLabels;
			}

			if (e.Row != null && e.ReturnValue != null && IndexAllowedValue((string)e.ReturnValue) < 0)
			{
				int index = IndexValue((string)e.ReturnValue);
				if (index >= 0)
					e.ReturnValue = _HiddenLabelsLocal != null ? _HiddenLabelsLocal[index] : _HiddenLabels[index];
			}
		}

		protected int IndexAllowedValue(string value)
		{
			if (_AllowedValues != null)
				for (int i = 0; i < _AllowedValues.Length; i++)
					if (string.Compare(_AllowedValues[i], value, true) == 0)
						return i;
			return -1;
		}

		protected int IndexValue(string value)
		{
			if (_HiddenValues != null)
				for (int i = 0; i < _HiddenValues.Length; i++)
					if (string.Compare(_HiddenValues[i], value, true) == 0)
						return i;
			return -1;
		}

		protected string SearchValueByName(string name)
		{
			if (_HiddenValues != null)
				for (int i = 0; i < _HiddenValues.Length; i++)
				{
					if (_HiddenLabelsLocal != null && string.Compare(_HiddenLabelsLocal[i], name, true) == 0)
						return _HiddenValues[i];
					if (_HiddenLabels != null && string.Compare(_HiddenLabels[i], name, true) == 0)
						return _HiddenValues[i];
				}
			return null;
		}

		#region IPXFieldUpdatingSubscriber Members

		protected virtual void OnFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if(e.NewValue != null)
			{
				if(IndexValue((string)e.NewValue) != -1) return;
				e.NewValue = SearchValueByName((string) e.NewValue);
			}
		}

		#endregion
	}

    /// <summary>
    /// Specialized PXStringList attribute for PO Order Line types.<br/>
    /// Provides a list of possible values for line types depending upon InventoryID <br/>
    /// specified in the row. For stock- and not-stock inventory items the allowed values <br/>
    /// are different. If item is changed and old value is not compatible with inventory item <br/>
    /// - it will defaulted to the applicable value.<br/>
    /// <example>
    /// [POLineTypeList(typeof(POLine.inventoryID))]
    /// </example>
    /// </summary>
	public class POLineTypeListAttribute : PXStringListExtAttribute, IPXFieldVerifyingSubscriber, IPXFieldDefaultingSubscriber
	{
		#region State
		protected Type _inventoryID;
		#endregion

		#region Ctor
		/// <summary>
		/// Ctor, short version. List of allowed values is defined as POLineType.GoodsForInventory, POLineType.NonStock, POLineType.Service, POLineType.Freight, POLineType.Description
		/// </summary>
		/// <param name="inventoryID">Must be IBqlField. Represents an InventoryID field in the row</param>
		public POLineTypeListAttribute(Type inventoryID)
			: this(
				inventoryID,
				new[]
				{
					Pair(POLineType.GoodsForInventory, Messages.GoodsForInventory),
					Pair(POLineType.NonStock, Messages.NonStockItem),
					Pair(POLineType.Service, Messages.Service),
					Pair(POLineType.Freight, Messages.Freight),
					Pair(POLineType.Description, Messages.Description),
				})
		{ }

		protected POLineTypeListAttribute(Type inventoryID, Tuple<string, string>[] allowedPairs)
			: this(
				inventoryID,
				allowedPairs.Select(t => t.Item1).ToArray(),
				allowedPairs.Select(t => t.Item2).ToArray()
				)
		{ }

		/// <summary>
		/// Ctor. Shorter version. User may define a list of allowed values and their descriptions
		/// List for hidden values is defined as POLineType.GoodsForInventory, POLineType.GoodsForSalesOrder,
		/// POLineType.GoodsForReplenishment, POLineType.GoodsForDropShip, POLineType.NonStockForDropShip,
		/// POLineType.NonStockForSalesOrder, POLineType.NonStock, POLineType.Service,
		/// POLineType.Freight, POLineType.Description - it includes all the values for the POLine types.
		/// </summary>
		/// <param name="inventoryID">Must be IBqlField. Represents an InventoryID field in the row</param>
		/// <param name="allowedValues">List of allowed values. </param>
		/// <param name="allowedLabels">List of allowed values labels. Will be shown in the combo-box in UI</param>
		public POLineTypeListAttribute(Type inventoryID, string[] allowedValues, string[] allowedLabels)
			: this(
				inventoryID,
				allowedValues,
				allowedLabels,
				new[]
				{
					Pair(POLineType.GoodsForInventory, Messages.GoodsForInventory),
					Pair(POLineType.GoodsForSalesOrder, Messages.GoodsForSalesOrder),
                    Pair(POLineType.GoodsForServiceOrder, Messages.GoodsForServiceOrder),
					Pair(POLineType.GoodsForReplenishment, Messages.GoodsForReplenishment),
					Pair(POLineType.GoodsForDropShip, Messages.GoodsForDropShip),
					Pair(POLineType.NonStockForDropShip, Messages.NonStockForDropShip),
					Pair(POLineType.NonStockForSalesOrder, Messages.NonStockForSalesOrder),
                    Pair(POLineType.NonStockForServiceOrder, Messages.NonStockForServiceOrder),
					Pair(POLineType.GoodsForProject, Messages.GoodsForProject),
					Pair(POLineType.NonStockForProject, Messages.NonStockForProject),
					Pair(POLineType.NonStock, Messages.NonStockItem),
					Pair(POLineType.Service, Messages.Service),
					Pair(POLineType.Freight, Messages.Freight),
					Pair(POLineType.Description, Messages.Description),
				}
				)
		{ }

		protected POLineTypeListAttribute(Type inventoryID, string[] allowedValues, string[] allowedLabels, Tuple<string, string>[] hiddenPairs)
			: this(
				inventoryID,
				allowedValues,
				allowedLabels,
				hiddenPairs.Select(t => t.Item1).ToArray(),
				hiddenPairs.Select(t => t.Item2).ToArray())
		{ }

		protected POLineTypeListAttribute(Type inventoryID, Tuple<string, string>[] allowedPairs, Tuple<string, string>[] hiddenPairs)
			: this(
				inventoryID,
				allowedPairs.Select(t => t.Item1).ToArray(),
				allowedPairs.Select(t => t.Item2).ToArray(),
				hiddenPairs.Select(t => t.Item1).ToArray(),
				hiddenPairs.Select(t => t.Item2).ToArray())
		{ }

		/// <summary>
		/// Ctor. Full version. User may define a list of allowed values and their descriptions, and a list of hidden values.
		/// </summary>
		/// <param name="inventoryID">Must be IBqlField. Represents an InventoryID field of the row</param>
		/// <param name="allowedValues">List of allowed values. </param>
		/// <param name="allowedLabels"> Labels for the allowed values. List should have the same size as the list of the values</param>
		/// <param name="hiddenValues"> List of possible values for the control. Must include all the values from the allowedValues list</param>
		/// <param name="hiddenLabels"> Labels for the possible values. List should have the same size as the list of the values</param>
		public POLineTypeListAttribute(Type inventoryID, string[] allowedValues, string[] allowedLabels, string[] hiddenValues, string[] hiddenLabels)
			: base(allowedValues, allowedLabels, hiddenValues, hiddenLabels)
		{
			_inventoryID = inventoryID;
		}
		#endregion

		#region Implementation
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _inventoryID.Name, InventoryIDUpdated);
		}


		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			int? inventoryID = (int?)sender.GetValue(e.Row, _inventoryID.Name);
			if (e.Row != null && e.NewValue != null )
			{
				if (inventoryID != null && !POLineType.IsProjectDropShip((string)e.NewValue))
				{
					InventoryItem item = InventoryItem.PK.Find(sender.Graph, inventoryID);
					InventoryItem nonStock = item != null && item.StkItem == false ? item : null;

					if (nonStock != null && nonStock.KitItem == true)
					{
						INKitSpecStkDet component = PXSelect<INKitSpecStkDet, Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.SelectWindowed(sender.Graph, 0, 1, nonStock.InventoryID);
						if (component != null) nonStock = null;
					}

					if ((nonStock != null && !POLineType.IsNonStock((string) e.NewValue)) ||
							(nonStock == null && !POLineType.IsStock((string)e.NewValue)))
						throw new PXSetPropertyException(Messages.UnsupportedLineType);
				}
				/*
				else if((string)e.NewValue != POLineType.Freight && (string) e.NewValue != POLineType.Description)
						throw new PXSetPropertyException(Messages.UnsupportedLineType);
				*/
				if(IndexValue((string)e.NewValue) == -1)
					throw new PXSetPropertyException(Messages.UnsupportedLineType);
			}
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			int? inventoryID = (int?)sender.GetValue(e.Row, _inventoryID.Name);
			if (inventoryID != null)
			{
				InventoryItem item = InventoryItem.PK.Find(sender.Graph, inventoryID);

				if (item != null && item.StkItem != true)
				{
					if (!PXAccess.FeatureInstalled<FeaturesSet.inventory>())
					{
						e.NewValue = POLineType.Service;
					}
					else
					{
						if (item.KitItem == true)
						{
							INKitSpecStkDet component = PXSelect<INKitSpecStkDet, Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.SelectWindowed(sender.Graph, 0, 1, item.InventoryID);

							if (component != null)
								e.NewValue = POLineType.GoodsForInventory;
							return;
						}
						e.NewValue = POLineType.GoodsForInventory;
						if (item.NonStockReceipt != null)
							e.NewValue = item.NonStockReceipt == true
								? POLineType.NonStock
								: POLineType.Service;
					}
				}
				else
					e.NewValue = POLineType.GoodsForInventory;
				e.Cancel = true;
			}
		}

    	protected virtual void InventoryIDUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (sender.GetValuePending(e.Row, _FieldName).IsIn(null, PXCache.NotSetValue))
			{
			sender.SetDefaultExt(e.Row, _FieldName);
		}
		}
		#endregion
	}

	public class POLineTypeList2Attribute : PXStringListAttribute, IPXFieldDefaultingSubscriber, IPXFieldVerifyingSubscriber
	{
		protected Type docTypeField;
		protected Type inventoryIDField;


		public POLineTypeList2Attribute(Type docType, Type inventoryID) : base(
			new[]
			{
				Pair(POLineType.GoodsForInventory, Messages.GoodsForInventory),
				Pair(POLineType.GoodsForSalesOrder, Messages.GoodsForSalesOrder),
                Pair(POLineType.GoodsForServiceOrder, Messages.GoodsForServiceOrder),
				Pair(POLineType.GoodsForReplenishment, Messages.GoodsForReplenishment),
				Pair(POLineType.GoodsForDropShip, Messages.GoodsForDropShip),
				Pair(POLineType.NonStockForDropShip, Messages.NonStockForDropShip),
				Pair(POLineType.NonStockForSalesOrder, Messages.NonStockForSalesOrder),
                Pair(POLineType.NonStockForServiceOrder, Messages.NonStockForServiceOrder),
				Pair(POLineType.GoodsForProject, Messages.GoodsForProject),
				Pair(POLineType.NonStockForProject, Messages.NonStockForProject),
				Pair(POLineType.NonStock, Messages.NonStockItem),
				Pair(POLineType.Service, Messages.Service),
				Pair(POLineType.Freight, Messages.Freight),
				Pair(POLineType.Description, Messages.Description),
			})
		{
			this.docTypeField = docType;
			this.inventoryIDField = inventoryID;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), sender.GetField(inventoryIDField), InventoryIDUpdated);
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (ShowFilteredComboValues(sender, e.Row))
			{
				string retValue = (string)e.ReturnValue;

				Tuple<List<string>, List<string>, string> valueslables = PopulateValues(sender, e.Row);

				if (string.IsNullOrEmpty(retValue) || valueslables.Item1.Contains(retValue))
				{
					e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 1, false, _FieldName, false, 1, null, valueslables.Item1.ToArray(), valueslables.Item2.ToArray(), true, valueslables.Item3);
					((PXStringState)e.ReturnState).Enabled = valueslables.Item1.Count > 1;
				}
				else
				{
					e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 1, false, _FieldName, false, 1, null,
						this._AllowedValues, this._AllowedLabels,
						true, retValue);
					((PXStringState)e.ReturnState).Enabled = false;
				}
			}
			else
			{
				base.FieldSelecting(sender, e);
			}
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (sender.Graph.IsImportFromExcel && ShowFilteredComboValues(sender, e.Row))
			{
				var value = sender.GetValue(e.Row, FieldName) as string;
				var values = PopulateValues(sender, e.Row);

				if (!string.IsNullOrEmpty(value) && !values.Item1.Contains(e.NewValue as string))
				{
					e.NewValue = value;
				}
			}
		}

		protected virtual bool ShowFilteredComboValues(PXCache sender, object row)
		{
			//Issue - all the possible values are displayed in the dropdown regardless of the actual populated values.
			if (row == null)
				return false;

			if (string.IsNullOrEmpty(sender.Graph.PrimaryView))//This is in the context of a report - return all possible values do that filter can be populated.
				return false;

			return true;
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			string docType = null;
			if (docTypeField != null)
				docType = (string)sender.GetValue(e.Row, sender.GetField(docTypeField));
			int? inventoryID = (int?)sender.GetValue(e.Row, sender.GetField(inventoryIDField));

			Tuple<List<string>, List<string>, string> valueslables = PopulateValues(sender, docType, inventoryID);

			e.NewValue = valueslables.Item3;
		}

		private string LocaleLabel(string value, string neutralLabel)
		{
			if (_AllowedValues != null && _AllowedLabels != null && _AllowedValues.Length == _AllowedLabels.Length)
			{
				for (int i = 0; i < _AllowedValues.Length; i++)
					if (_AllowedValues[i] == value)
						return _AllowedLabels[i];
			}

			return neutralLabel;
		}

		protected virtual Tuple<List<string>, List<string>, string> PopulateValues(PXCache cache, object row)
		{
			string docType = null;
			if (docTypeField != null)
				docType = (string)cache.GetValue(row, cache.GetField(docTypeField));

			int? inventoryID = (int?)cache.GetValue(row, cache.GetField(inventoryIDField));

			return PopulateValues(cache, docType, inventoryID);
		}

		/// <summary>
		/// Populate list of available LineTypes based on current state of entity.
		/// </summary>
		/// <returns>
		/// Item1 - List of available values
		/// Item2 - List of available labels
		/// Item3 - default value.
		/// </returns>
		protected virtual Tuple<List<string>, List<string>, string> PopulateValues(PXCache sender, string docType, int? inventoryID)
		{
			string defaultValue = null;
			List<string> values = new List<string>();
			List<string> labels = new List<string>();

			InventoryItem item = null;
			if (inventoryID != null)
				item = InventoryItem.PK.Find(sender.Graph, inventoryID);

			if (item != null)
			{
				bool stockItem = false;

				if (item.StkItem == true)
				{
					stockItem = true;
				}
				else if (item.KitItem == true)
				{
					INKitSpecStkDet component = PXSelect<INKitSpecStkDet, Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.SelectWindowed(sender.Graph, 0, 1, item.InventoryID);
					if (component != null)
					{
						stockItem = true;
					}
				}

				if (stockItem)
				{
					switch (docType)
					{
						case POOrderType.DropShip:
							{
								values.Add(POLineType.GoodsForDropShip);
								labels.Add(LocaleLabel(POLineType.GoodsForDropShip, Messages.GoodsForDropShip));
								break;
							}
						case POOrderType.ProjectDropShip:
							{
								values.Add(POLineType.GoodsForProject);
								labels.Add(LocaleLabel(POLineType.GoodsForProject, Messages.GoodsForProject));
								break;
							}
						default:
							{
								values.Add(POLineType.GoodsForInventory);
								labels.Add(LocaleLabel(POLineType.GoodsForInventory, Messages.GoodsForInventory));
								break;
							}
					}
				}
				else
				{
					if (!PXAccess.FeatureInstalled<FeaturesSet.inventory>())
					{
						values.Add(POLineType.Service);
						labels.Add(LocaleLabel(POLineType.Service, Messages.Service));
					}
					else
					{
						switch (docType)
						{
							case POOrderType.DropShip:
								{
									if (item.NonStockReceipt == true && item.NonStockShip == true)
									{
										values.Add(POLineType.NonStockForDropShip);
										labels.Add(LocaleLabel(POLineType.NonStockForDropShip, Messages.NonStockForDropShip));
									}
									else if (item.NonStockReceipt == true)
									{
										values.Add(POLineType.NonStock);
										labels.Add(LocaleLabel(POLineType.NonStock, Messages.NonStockItem));
									}
									else
									{
										values.Add(POLineType.Service);
										labels.Add(Messages.Service);
									}
									break;
								}
							case POOrderType.ProjectDropShip:
								{
									if (item.NonStockReceipt == true)
									{
										values.Add(POLineType.NonStockForProject);
										labels.Add(LocaleLabel(POLineType.NonStockForProject, Messages.NonStockForProject));
									}
									else
									{
										values.Add(POLineType.Service);
										labels.Add(Messages.Service);
									}
									break;
								}
							default:
								{
									if (item.NonStockReceipt == true)
									{
										values.Add(POLineType.NonStock);
										labels.Add(LocaleLabel(POLineType.NonStock, Messages.NonStockItem));
									}
									else
									{
										values.Add(POLineType.Service);
										labels.Add(LocaleLabel(POLineType.Service, Messages.Service));
									}
									break;
								}
						}
					}
				}
			}
			else
			{
				values.Add(POLineType.Service);
				labels.Add(LocaleLabel(POLineType.Service, Messages.Service));

				values.Add(POLineType.Description);
				labels.Add(LocaleLabel(POLineType.Description, Messages.Description));

				values.Add(POLineType.Freight);
				labels.Add(LocaleLabel(POLineType.Freight, Messages.Freight));
			}


			if (values.Count > 0)
				defaultValue = values[0];


			return new Tuple<List<string>, List<string>, string>(values, labels, defaultValue);
		}

		protected virtual void InventoryIDUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (sender.GetValuePending(e.Row, _FieldName).IsIn(null, PXCache.NotSetValue))
			{
			sender.SetDefaultExt(e.Row, _FieldName);
		}
	}
	}

	/// <summary>
	/// Specialized PXStringList attribute for Receipt Line types.<br/>
	/// Provides a list of possible values for line types depending upon InventoryID <br/>
	/// specified in the row. For stock- and not-stock inventory items the allowed values <br/>
	/// are different. If item is changed and old value is not compatible with inventory item <br/>
	/// - it will defaulted to the applicable value.<br/>
	/// <example>
	/// [POReceiptLineTypeList(typeof(POLine.inventoryID))]
	/// </example>
	/// </summary>
	public class POReceiptLineTypeListAttribute : POLineTypeListAttribute
	{
		public POReceiptLineTypeListAttribute(Type inventoryID)
			:base(
			inventoryID,
			new string[] { POLineType.GoodsForInventory, POLineType.NonStock, POLineType.Service, POLineType.Freight },
			new string[] { Messages.GoodsForInventory, Messages.NonStockItem, Messages.Service, Messages.Freight })
		{

		}
	}

    /// <summary>
    /// Specialized for POOrder version of the VendorAttribute, which defines a list of vendors, <br/>
    /// which may be used in the PO Order (for example, employee are filtered <br/>
    /// out for all order types except Transfer ) <br/>
    /// Depends upon POOrder current. <br/>
    /// <example>
    /// [POVendor()]
    /// </example>
    /// </summary>
	[PXDBInt]
	[PXUIField(DisplayName = "Vendor", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(
		typeof(Where<Vendor.vStatus, IsNull,
			Or<Vendor.vStatus, In3<VendorStatus.active, VendorStatus.oneTime, VendorStatus.holdPayments>>>),
		AP.Messages.VendorIsInStatus, typeof(Vendor.vStatus))]
	public class POVendorAttribute : VendorAttribute
	{
		public POVendorAttribute()
			: base(typeof(Search<BAccountR.bAccountID,
				Where<Vendor.type, NotEqual<BAccountType.employeeType>>>))
		{
		}
	}

	/// <summary>
	/// Specialized for PO version of the Address attribute.<br/>
	/// Uses POAddress tables for Address versions storage <br/>
	/// Prameters AddressID, IsDefault address are assigned to the <br/>
	/// corresponded fields in the POAddress table. <br/>
	/// Cache for POShipAddress(inherited from POAddress) must be present in the graph <br/>
	/// Special derived type is needed to be able to use different instances of POAddress <br/>
	/// (like PORemitAddress and POShipAddress)in the same graph - otherwise is not possible <br/>
	/// to enable/disable controls in the forms correctly <br/>
	/// Depends upon row instance. <br/>
	/// <example>
	///[POShipAddress(typeof(Select2<Address,
	///               LeftJoin<Location, On<Address.bAccountID, Equal<Location.bAccountID>,
	///                And<Address.addressID, Equal<Location.defAddressID>,
	///                And<Current<POOrder.shipDestType>, NotEqual<POShippingDestination.site>,
	///                And<Location.bAccountID, Equal<Current<POOrder.shipToBAccountID>>,
	///                And<Location.locationID, Equal<Current<POOrder.shipToLocationID>>>>>>>,
	///                LeftJoin<INSite, On<Address.addressID, Equal<INSite.addressID>,
	///                  And<Current<POOrder.shipDestType>, Equal<POShippingDestination.site>,
	///                    And<INSite.siteID, Equal<Current<POOrder.siteID>>>>>,
	///                LeftJoin<POShipAddress, On<POShipAddress.bAccountID, Equal<Address.bAccountID>,
	///                    And<POShipAddress.bAccountAddressID, Equal<Address.addressID>,
	///                    And<POShipAddress.revisionID, Equal<Address.revisionID>,
	///                    And<POShipAddress.isDefaultAddress, Equal<boolTrue>>>>>>>>,
	///                Where<Location.locationCD, IsNotNull, Or<INSite.siteCD, IsNotNull>>>))]
	/// </example>
	/// </summary>
	public class POShipAddressAttribute : AddressAttribute
	{
		/// <summary>
		/// Internaly, it expects POShipAddress as a POAddress type.
		/// </summary>
		/// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/>
		/// a source Address record from which PO address is defaulted and for selecting default version of POAddress, <br/>
		/// created  from source Address (having  matching ContactID, revision and IsDefaultContact = true) <br/>
		/// if it exists - so it must include both records. See example above. <br/>
		/// </param>
		public POShipAddressAttribute(Type SelectType)
			: base(typeof(POShipAddress.addressID), typeof(POShipAddress.isDefaultAddress), SelectType)
		{

		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<POShipAddress.overrideAddress>(Record_Override_FieldVerifying);
		}


		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<POShipAddress.overrideAddress>(sender, e.Row, sender.AllowUpdate);
				PXUIFieldAttribute.SetEnabled<POShipAddress.isValidated>(sender, e.Row, false);
			}
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<POShipAddress, POShipAddress.addressID>(sender, DocumentRow, Row);
		}

		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<POShipAddress, POShipAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<POShipAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null && (string)sender.GetValue<POOrder.shipDestType>(e.Row) != POShipDestType.Site)
			{
				var errors = PXUIFieldAttribute.GetErrors(sender, e.Row, new PXErrorLevel[] { PXErrorLevel.Error });
				if (errors != null && errors.Count > 0)
					return;
			}

			base.RowPersisting(sender, e);
		}

		public override void DefaultAddress<TAddress, TAddressID>(PXCache sender, object DocumentRow, object AddressRow)
		{
			int? siteID = (int?)sender.GetValue<POOrder.siteID>(DocumentRow);
			int? projectID = (int?)sender.GetValue<POOrder.projectID>(DocumentRow);
			string shipDestType = (string)sender.GetValue<POOrder.shipDestType>(DocumentRow);

			string shipDestination = null;
			object[] parameters = null;
			if (siteID != null && shipDestType == POShippingDestination.Site)
			{
				shipDestination = shipDestType;
				parameters = new object[] { siteID };
			}
			if (projectID != null && shipDestType == POShippingDestination.ProjectSite)
			{
				shipDestination = shipDestType;
				parameters = new object[] { projectID };
			}

			PXView view = CreateAddressView(sender, DocumentRow, shipDestination);

			int startRow = -1;
			int totalRows = 0;
			bool addressFound = false;

			var addresses = view.Select(null, parameters, null, null, null, null, ref startRow, 1, ref totalRows);

			if (addresses.Any())
			{
				foreach (PXResult res in view.Select(null, parameters, null, null, null, null, ref startRow, 1, ref totalRows))
				{
					if (shipDestination == POShippingDestination.ProjectSite)
					{
						PMSiteAddress pmAddress = PXResult.Unwrap<PMSiteAddress>(res);
						Address address = PropertyTransfer.Transfer(pmAddress, new Address());
						address.AddressID = pmAddress.AddressID;

						addressFound = DefaultAddress<TAddress, TAddressID>(sender, FieldName, DocumentRow, AddressRow, new PXResult<Address, POShipAddress>(address, new POShipAddress()));
					}
					else
					{
						addressFound = DefaultAddress<TAddress, TAddressID>(sender, FieldName, DocumentRow, AddressRow, res);
					}
					break;
				}
			}
			else if (shipDestination == POShippingDestination.ProjectSite)
			{
				Address address = new Address();
				address.RevisionID = 0;

				addressFound = DefaultAddress<TAddress, TAddressID>(sender, FieldName, DocumentRow, AddressRow, new PXResult<Address, POShipAddress>(address, new POShipAddress()));
			}

			if (!addressFound && !_Required)
				this.ClearRecord(sender, DocumentRow);

			if (!addressFound && _Required && shipDestination == POShippingDestination.Site)
				throw new SharedRecordMissingException();
		}

		protected virtual PXView CreateAddressView(PXCache sender, object DocumentRow, string shipDestination)
		{
			switch (shipDestination)
			{
				case POShippingDestination.Site:
					{
						BqlCommand altSelect = BqlCommand.CreateInstance(
											typeof(SelectFrom<Address>.
											InnerJoin<INSite>.
												On<INSite.FK.Address.And<INSite.siteID.IsEqual<@P.AsInt>>>.
											LeftJoin<POShipAddress>.
												On<POShipAddress.bAccountID.IsEqual<Address.bAccountID>.
													And<POShipAddress.bAccountAddressID.IsEqual<Address.addressID>.
													And<POShipAddress.revisionID.IsEqual<Address.revisionID>.
													And<POShipAddress.isDefaultAddress.IsEqual<boolTrue>>>>>.
											Where<boolTrue.IsEqual<boolTrue>>));

						return sender.Graph.TypedViews.GetView(altSelect, false);
					}
				case POShippingDestination.ProjectSite:
					{
						BqlCommand altSelect = BqlCommand.CreateInstance(
										typeof(SelectFrom<PMSiteAddress>.
										InnerJoin<PMProject>.
											On<PMProject.siteAddressID.IsEqual<PMSiteAddress.addressID>.And<PMProject.contractID.IsEqual<@P.AsInt>>>.
										Where<boolTrue.IsEqual<boolTrue>>));

						return sender.Graph.TypedViews.GetView(altSelect, false);
					}
				default:
					{
						return sender.Graph.TypedViews.GetView(_Select, false);
					}
			}
		}
	}

    /// <summary>
    /// Specialized for PO version of the Address attribute.<br/>
    /// Uses POAddress tables for Address versions storage <br/>
    /// Prameters AddressID, IsDefault address are assigned to the <br/>
    /// corresponded fields in the POAddress table. <br/>
    /// Cache for PORemitAddress(inherited POAddess) must be present in the graph <br/>
    /// Special derived type is needed to be able to use different instances of POAddress <br/>
    /// (like PORemitAddress and POShipAddress)in the same graph - otherwise is not possible <br/>
    /// to enable/disable controls in the forms correctly <br/>
    /// Depends upon row instance.
    /// <example>
    /// [PORemitAddress(typeof(Select2<BAccount2,
    ///		InnerJoin<Location, On<Location.bAccountID, Equal<BAccount2.bAccountID>>,
    ///		InnerJoin<Address, On<Address.bAccountID, Equal<Location.bAccountID>, And<Address.addressID, Equal<Location.defAddressID>>>,
    ///		LeftJoin<PORemitAddress, On<PORemitAddress.bAccountID, Equal<Address.bAccountID>,
    ///			And<PORemitAddress.bAccountAddressID, Equal<Address.addressID>,
    ///			And<PORemitAddress.revisionID, Equal<Address.revisionID>, And<PORemitAddress.isDefaultAddress, Equal<boolTrue>>>>>>>>,
    ///		Where<Location.bAccountID, Equal<Current<POOrder.vendorID>>, And<Location.locationID, Equal<Current<POOrder.vendorLocationID>>>>>))]
    /// </example>
    /// </summary>
	public class PORemitAddressAttribute : AddressAttribute
	{
        /// <summary>
        /// Internaly, it expects PORemitAddress as a POAddress type.
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/>
        /// a source Address record from which PO address is defaulted and for selecting default version of POAddress, <br/>
        /// created  from source Address (having  matching ContactID, revision and IsDefaultContact = true) <br/>
        /// if it exists - so it must include both records. See example above. <br/>
        /// </param>
		public PORemitAddressAttribute(Type SelectType)
			: base(typeof(PORemitAddress.addressID), typeof(PORemitAddress.isDefaultAddress), SelectType)
		{

		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<PORemitAddress.overrideAddress>(Record_Override_FieldVerifying);
		}


		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PORemitAddress.overrideAddress>(sender, e.Row, sender.AllowUpdate);
				PXUIFieldAttribute.SetEnabled<PORemitAddress.isValidated>(sender, e.Row, false);
			}
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<PORemitAddress, PORemitAddress.addressID>(sender, DocumentRow, Row);
		}

		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<PORemitAddress, PORemitAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<PORemitAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}


	}

    /// <summary>
    /// Specialized for PO version of the Contact attribute.<br/>
    /// Uses POContact tables for Contact versions storage <br/>
    /// Parameters ContactID, IsDefaultContact are assigned to the <br/>
    /// corresponded fields in the POContact table. <br/>
    /// Cache for PORShipContact (inherited from POContact) must be present in the graph <br/>
    /// Special derived type is needed to be able to use different instances of POContact <br/>
    /// (like PORemitContact and POShipContact)in the same graph - otherwise is not possible <br/>
    /// to enable/disable controls in the forms correctly <br/>
    /// Depends upon row instance.
    ///<example>
    ///[POShipContactAttribute(typeof(Select2<Contact,
    ///                LeftJoin<Location, On<Contact.bAccountID, Equal<Location.bAccountID>,
    ///                    And<Contact.contactID, Equal<Location.defContactID>,
    ///                    And<Current<POOrder.shipDestType>, NotEqual<POShippingDestination.site>,
    ///                    And<Location.bAccountID, Equal<Current<POOrder.shipToBAccountID>>,
    ///                    And<Location.locationID, Equal<Current<POOrder.shipToLocationID>>>>>>>,
    ///                LeftJoin<INSite, On<Contact.contactID, Equal<INSite.contactID>,
    ///                  And<Current<POOrder.shipDestType>, Equal<POShippingDestination.site>,
    ///                    And<INSite.siteID, Equal<Current<POOrder.siteID>>>>>,
    ///                LeftJoin<POShipContact, On<POShipContact.bAccountID, Equal<Contact.bAccountID>,
    ///                    And<POShipContact.bAccountContactID, Equal<Contact.contactID>,
    ///                    And<POShipContact.revisionID, Equal<Contact.revisionID>,
    ///                    And<POShipContact.isDefaultContact, Equal<boolTrue>>>>>>>>,
    ///                Where<Location.locationCD, IsNotNull, Or<INSite.siteCD, IsNotNull>>>))]
    ///</example>
    ///</summary>
	public class POShipContactAttribute : ContactAttribute
	{
        /// <summary>
        /// Ctor. Internaly, it expects POShipContact as a POContact type
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/>
        /// a source Contact record from which PO Contact is defaulted and for selecting version of POContact, <br/>
        /// created from source Contact (having  matching ContactID, revision and IsDefaultContact = true).<br/>
        /// - so it must include both records. See example above. <br/>
        /// </param>
		public POShipContactAttribute(Type SelectType)
			: base(typeof(POShipContact.contactID), typeof(POShipContact.isDefaultContact), SelectType)
		{

		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<POShipContact.overrideContact>(Record_Override_FieldVerifying);
		}


		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<POShipContact, POShipContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<POShipContact, POShipContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<POShipContact.overrideContact>(sender, e.Row, sender.AllowUpdate);
			}
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<POShipContact>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null && (string)sender.GetValue<POOrder.shipDestType>(e.Row) != POShipDestType.Site)
			{
				var errors = PXUIFieldAttribute.GetErrors(sender, e.Row, new PXErrorLevel[] { PXErrorLevel.Error });
				if (errors != null && errors.Count > 0)
					return;
			}

			base.RowPersisting(sender, e);
		}

		public override void DefaultContact<TContact, TContactID>(PXCache sender, object DocumentRow, object ContactRow)
		{
			int? siteID = (int?)sender.GetValue<POOrder.siteID>(DocumentRow);
			int? projectID = (int?)sender.GetValue<POOrder.projectID>(DocumentRow);
			string shipDestType = (string)sender.GetValue<POOrder.shipDestType>(DocumentRow);

			string shipDestination = null;
			object[] parameters = null;
			if (siteID != null && shipDestType == POShippingDestination.Site)
			{
				shipDestination = shipDestType;
				parameters = new object[] { siteID };
			}
			if (projectID != null && shipDestType == POShippingDestination.ProjectSite)
			{
				shipDestination = shipDestType;
				parameters = new object[] { projectID };
			}

			PXView view = CreateContactView(sender, DocumentRow, shipDestination);

			int startRow = -1;
			int totalRows = 0;
			bool contactFound = false;

			var contacts = view.Select(null, parameters, null, null, null, null, ref startRow, 1, ref totalRows);

			if (contacts.Any())
			{
				foreach (PXResult res in contacts)
				{
					if (shipDestination == POShippingDestination.ProjectSite)
					{
						PMContact pmContact = PXResult.Unwrap<PMContact>(res);
						Contact contact = PropertyTransfer.Transfer(pmContact, new Contact());
						contact.EMail = pmContact.Email;

						contactFound = DefaultContact<TContact, TContactID>(sender, FieldName, DocumentRow, ContactRow, new PXResult<Contact, POShipContact>(contact, new POShipContact()));
					}
					else
					{
						contactFound = DefaultContact<TContact, TContactID>(sender, FieldName, DocumentRow, ContactRow, res);
					}
					break;
				}
			}
			else if (shipDestination == POShippingDestination.ProjectSite)
			{
				Contact contact = new Contact();
				contact.RevisionID = 0;

				contactFound = DefaultContact<TContact, TContactID>(sender, FieldName, DocumentRow, ContactRow, new PXResult<Contact, POShipContact>(contact, new POShipContact()));
			}

			if (!contactFound && !_Required)
				this.ClearRecord(sender, DocumentRow);

			if (!contactFound && _Required && shipDestination == POShippingDestination.Site)
				throw new SharedRecordMissingException();

		}

		protected virtual PXView CreateContactView(PXCache sender, object DocumentRow, string shipDestination)
		{
			switch (shipDestination)
			{
				case POShippingDestination.Site:
					{
						BqlCommand altSelect = BqlCommand.CreateInstance(
										typeof(SelectFrom<Contact>.
										InnerJoin<INSite>.
											On<INSite.FK.Contact.And<INSite.siteID.IsEqual<@P.AsInt>>>.
										LeftJoin<POShipContact>.
											On<POShipContact.bAccountID.IsEqual<Contact.bAccountID>.
												And<POShipContact.bAccountContactID.IsEqual<Contact.contactID>.
												And<POShipContact.revisionID.IsEqual<Contact.revisionID>.
												And<POShipContact.isDefaultContact.IsEqual<boolTrue>>>>>.
										Where<boolTrue.IsEqual<boolTrue>>));

						return sender.Graph.TypedViews.GetView(altSelect, false);
					}
				case POShippingDestination.ProjectSite:
					{
						BqlCommand altSelect = BqlCommand.CreateInstance(
										typeof(SelectFrom<PMContact>.
										InnerJoin<PMProject>.
											On<PMProject.billContactID.IsEqual<PMContact.contactID>.And<PMProject.contractID.IsEqual<@P.AsInt>>>.
										Where<boolTrue.IsEqual<boolTrue>>));

						return sender.Graph.TypedViews.GetView(altSelect, false);
					}
				default:
					{
						return sender.Graph.TypedViews.GetView(_Select, false);
					}
			}
		}
	}

    /// <summary>
    /// Specialized for PO version of the Contact attribute.<br/>
    /// Uses POContact tables for Contact versions storage <br/>
    /// Parameters ContactID, IsDefaultContact are assigned to the <br/>
    /// corresponded fields in the POContact table. <br/>
    /// Cache for PORemitContact (inherited from POContact) must be present in the graph <br/>
    /// Special derived type is needed to be able to use different instances of POContact <br/>
    /// (like PORemitContact and POShipContact)in the same graph otherwise is not possible <br/>
    /// to enable/disable controls in the forms correctly <br/>
    /// Depends upon row instance.
    /// <example>
    /// [APContact(typeof(Select2<Location,
    ///		InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Location.bAccountID>>,
    ///		InnerJoin<Contact, On<Contact.contactID, Equal<Location.remitContactID>,
    ///		    And<Where<Contact.bAccountID, Equal<Location.bAccountID>,
    ///		    Or<Contact.bAccountID, Equal<BAccountR.parentBAccountID>>>>>,
    ///		LeftJoin<APContact, On<APContact.vendorID, Equal<Contact.bAccountID>,
    ///		    And<APContact.vendorContactID, Equal<Contact.contactID>,
    ///		    And<APContact.revisionID, Equal<Contact.revisionID>,
    ///		    And<APContact.isDefaultContact, Equal<boolTrue>>>>>>>>,
    ///		Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>,
    ///		And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>>>>))]
    /// </example>
    /// </summary>
	public class PORemitContactAttribute : ContactAttribute
	{
        /// <summary>
        /// Ctor. Internaly, it expects PORemitContact as a POContact type
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/>
        /// a source Contact record from which PO Contact is defaulted and for selecting version of POContact, <br/>
        /// created from source Contact (having  matching ContactID, revision and IsDefaultContact = true).<br/>
        /// - so it must include both records. See example above. <br/>
        /// </param>
		public PORemitContactAttribute(Type SelectType)
			: base(typeof(PORemitContact.contactID), typeof(PORemitContact.isDefaultContact), SelectType)
		{

		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<PORemitContact.overrideContact>(Record_Override_FieldVerifying);
		}


		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<PORemitContact, PORemitContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<PORemitContact, PORemitContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PORemitContact.overrideContact>(sender, e.Row, sender.AllowUpdate);
			}
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<PORemitContact>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}

		}

	}

	public class POTaxAttribute : TaxAttribute
	{
		#region CuryRetainageAmt
		protected abstract class curyRetainageAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageAmt> { }
		protected Type CuryRetainageAmt = typeof(curyRetainageAmt);
		protected string _CuryRetainageAmt
		{
			get
			{
				return CuryRetainageAmt.Name;
			}
		}
		#endregion

		protected override bool CalcGrossOnDocumentLevel { get => true; set => base.CalcGrossOnDocumentLevel = value; }
		protected override bool AskRecalculationOnCalcModeChange { get => false; set => base.AskRecalculationOnCalcModeChange = value; }

		protected virtual short SortOrder
		{
			get
			{
				return 0;
			}
		}

		public POTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryDocBal = typeof(POOrder.curyOrderTotal);
			this.CuryLineTotal = typeof(POOrder.curyLineTotal);
			this.DocDate = typeof(POOrder.orderDate);
            this.CuryTranAmt = typeof(POLine.curyExtCost);
            this.GroupDiscountRate = typeof(POLine.groupDiscountRate);
			this.CuryTaxTotal = typeof(POOrder.curyTaxTotal);
			this.CuryDiscTot = typeof(POOrder.curyDiscTot);
			this.TaxCalcMode = typeof(POOrder.taxCalcMode);

			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<POLine.lineType, NotIn3<POLineType.freight, POLineType.description, POLineType.service>>, POLine.curyLineAmt>, decimal0>), typeof(SumCalc<POOrder.curyGoodsExtCostTotal>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<POLine.lineType, Equal<POLineType.service>>, POLine.curyLineAmt>, decimal0>), typeof(SumCalc<POOrder.curyServiceExtCostTotal>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<POLine.lineType, Equal<POLineType.freight>>, POLine.curyLineAmt>, decimal0>), typeof(SumCalc<POOrder.curyFreightTot>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(POLine.curyExtCost), typeof(SumCalc<POOrder.curyLineTotal>)) { ValidateAggregateCalculation = true });
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(POLine.curyDiscAmt), typeof(SumCalc<POOrder.curyLineDiscTotal>)));
		}

		public override int CompareTo(object other)
		{
			return this.SortOrder.CompareTo(((POTaxAttribute)other).SortOrder);
		}

		protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType)
		{
			// Normally, CuryTranAmt is reduced by CuryRetainageAmt for each POLine.
			// In case when "Retain Taxes" flag is disabled in APSetup - we should calculate
			// taxable amount based on the full CuryTranAmt amount, this why we should add
			// CuryRetainageAmt to the CuryTranAmt value.
			//
			decimal curyRetainageAmt = IsRetainedTaxes(sender.Graph)
				? 0m
				: (decimal)(sender.GetValue(row, _CuryRetainageAmt) ?? 0m);

			decimal curyTranAmt = (base.GetCuryTranAmt(sender, row) ?? 0m);
			decimal? value = (curyTranAmt + curyRetainageAmt) *
				(decimal?)sender.GetValue(row, _GroupDiscountRate) *
				(decimal?)sender.GetValue(row, _DocumentDiscountRate);

			if (value != 0m)
			{
				CurrencyInfo currencyInfo = sender.Graph.FindImplementation<IPXCurrencyHelper>().GetDefaultCurrencyInfo();
				return currencyInfo.RoundCury((decimal)value);
			}
			else
			{
				return 0m;
			}
		}

		protected override void SetTaxAmt(PXCache sender, object row, decimal? value)
		{
			sender.SetValue<POLine.hasInclusiveTaxes>(row, value.HasValue && value != 0m);
		}

		protected override decimal CalcLineTotal(PXCache sender, object row)
		{
			return (decimal?)ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m;
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<POLine.lineNbr>>(graph, new object[] { row, ((POOrderEntry)graph).Document.Current }, taxchk, parameters);
		}

		protected List<object> SelectTaxes<Where, LineNbr>(PXGraph graph, object[] currents, PXTaxCheck taxchk, params object[] parameters)
			where Where : IBqlWhere, new()
			where LineNbr : IBqlOperand
		{
			List<ITaxDetail> taxDetails;
			IDictionary<string, PXResult<Tax, TaxRev>> tails;

			List<object> ret = new List<object>();

			BqlCommand selectTaxes = new Select2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
					And<TaxRev.outdated, Equal<boolFalse>,
					And2<
						Where<TaxRev.taxType, Equal<TaxType.purchase>, And<Tax.reverseTax, Equal<False>,
						Or<TaxRev.taxType, Equal<TaxType.sales>,
							And<
								Where<Tax.reverseTax, Equal<boolTrue>,
									Or<Tax.taxType, Equal<CSTaxType.use>,
									Or<Tax.taxType, Equal<CSTaxType.withholding>,
									Or<Tax.taxType, Equal<CSTaxType.perUnit>>>>>>>>>,
					And<Current<POOrder.orderDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>();

			switch (taxchk)
			{
				case PXTaxCheck.Line:
					taxDetails = PXSelect<POTax,
						Where<POTax.orderType, Equal<Current<POOrder.orderType>>,
							And<POTax.orderNbr, Equal<Current<POOrder.orderNbr>>,
							And<POTax.lineNbr, Equal<LineNbr>>>>>
						.SelectMultiBound(graph, currents)
						.RowCast<POTax>()
						.ToList<ITaxDetail>();

					if (taxDetails == null || taxDetails.Count == 0) return ret;

					tails = CollectInvolvedTaxes<Where>(graph, taxDetails, selectTaxes, currents, parameters);

					foreach (POTax record in taxDetails)
					{
						InsertTax(graph, taxchk, record, tails, ret);
					}
					return ret;

				case PXTaxCheck.RecalcLine:
					taxDetails = PXSelect<POTax,
						Where<POTax.orderType, Equal<Current<POOrder.orderType>>,
							And<POTax.orderNbr, Equal<Current<POOrder.orderNbr>>,
							And<POTax.lineNbr, Less<intMax>>>>>
						.SelectMultiBound(graph, currents)
						.RowCast<POTax>()
						//Taxes were cached in different order(using StoreCached).
						.OrderBy(_ => _.LineNbr)
						.ThenBy(_ => _.TaxID)
						.ToList<ITaxDetail>();

					if (taxDetails == null || taxDetails.Count == 0) return ret;

					tails = CollectInvolvedTaxes<Where>(graph, taxDetails, selectTaxes, currents, parameters);

					foreach (POTax record in taxDetails)
					{
						InsertTax(graph, taxchk, record, tails, ret);
					}
					return ret;

				case PXTaxCheck.RecalcTotals:
					taxDetails = PXSelect<POTaxTran,
						Where<POTaxTran.orderType, Equal<Current<POOrder.orderType>>,
							And<POTaxTran.orderNbr, Equal<Current<POOrder.orderNbr>>>>>
						.SelectMultiBound(graph, currents)
						.RowCast<POTaxTran>()
						.ToList<ITaxDetail>();

					if (taxDetails == null || taxDetails.Count == 0) return ret;

					tails = CollectInvolvedTaxes<Where>(graph, taxDetails, selectTaxes, currents, parameters);

					foreach (POTaxTran record in taxDetails)
					{
						InsertTax(graph, taxchk, record, tails, ret);
					}
					return ret;

				default:
					return ret;
			}
		}

		protected override List<object> SelectDocumentLines(PXGraph graph, object row)
		{
			List<object> ret = PXSelect<POLine,
								Where<POLine.orderType, Equal<Current<POOrder.orderType>>,
									And<POLine.orderNbr, Equal<Current<POOrder.orderNbr>>>>>
									.SelectMultiBound(graph, new object[] { row })
									.RowCast<POLine>()
									.Select(_ => (object)_)
									.ToList();
			return ret;
		}

		protected override void DefaultTaxes(PXCache sender, object row, bool DefaultExisting)
		{
			if (SortOrder == 0)
				base.DefaultTaxes(sender, row, DefaultExisting);
		}

		protected override void ClearTaxes(PXCache sender, object row)
		{
			if (SortOrder == 0)
				base.ClearTaxes(sender, row);
		}

		protected override decimal? GetDocLineFinalAmtNoRounding(PXCache sender, object row, string TaxCalcType)
		{
			decimal curyTranAmt = (base.GetCuryTranAmt(sender, row) ?? 0m);
			decimal? groupDiscount = (decimal?)sender.GetValue(row, _GroupDiscountRate);
			decimal? docDiscount = (decimal?)sender.GetValue(row, _DocumentDiscountRate);
			decimal? value = curyTranAmt * groupDiscount * docDiscount;

			return (decimal)value;
		}

		public override void CacheAttached(PXCache sender)
		{
            inserted = new Dictionary<object, object>();
            updated = new Dictionary<object, object>();

            if (this.EnableTaxCalcOn(sender.Graph))
			{
				base.CacheAttached(sender);
				sender.Graph.FieldUpdated.AddHandler(typeof(POOrder), _CuryTaxTotal, POOrder_CuryTaxTot_FieldUpdated);
                sender.Graph.FieldUpdated.AddHandler(typeof(POOrder), typeof(POOrder.curyDiscTot).Name, POOrder_CuryDiscTot_FieldUpdated);
            }
			else
			{
				this.TaxCalc = TaxCalc.NoCalc;
			}
		}

        protected virtual void POOrder_CuryDiscTot_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            bool calc = true;
            if (IsExternalTax(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID)) || (e.Row != null && ((POOrder)e.Row).ExternalTaxesImportInProgress == true))
                calc = false;

            this._ParentRow = e.Row;
            CalcTotals(sender, e.Row, calc);
            this._ParentRow = null;
        }

		virtual protected bool EnableTaxCalcOn(PXGraph aGraph)
		{
			return (aGraph is POOrderEntry);
		}


		protected override void CalcDocTotals(
			PXCache sender,
			object row,
			decimal CuryTaxTotal,
			decimal CuryInclTaxTotal,
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			base.CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);

			if (ParentGetStatus(sender.Graph) != PXEntryStatus.Deleted)
			{
				decimal doc_CuryWhTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryWhTaxTotal) ?? 0m);

				if (object.Equals(CuryWhTaxTotal, doc_CuryWhTaxTotal) == false)
				{
					ParentSetValue(sender.Graph, _CuryWhTaxTotal, CuryWhTaxTotal);
				}
			}
		}

		protected override void _CalcDocTotals(
			PXCache sender,
			object row,
			decimal curyTaxTotal,
			decimal curyInclTaxTotal,
			decimal curyWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			//Note: POTaxAttribute is called first and cannot rely on the fields that will be calculated by other POXXTaxAttributes (based on the SortOrder)

			decimal curyDiscountTotal = (decimal)(ParentGetValue(sender.Graph, _CuryDiscTot)??0m);
			decimal curyLineTotal = CalcLineTotal(sender, row);
			decimal curyDocTotal = curyLineTotal + curyTaxTotal - curyInclTaxTotal - curyDiscountTotal;

			decimal docCuryTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryTaxTotal) ?? 0m);
			decimal docCuryDocBal = (decimal)(ParentGetValue(sender.Graph, _CuryDocBal) ?? 0m);//_CuryDocBal is actually curyOrderTotal

			if (curyTaxTotal != docCuryTaxTotal)
			{
				ParentSetValue(sender.Graph, _CuryTaxTotal, curyTaxTotal);
			}

			if (!string.IsNullOrEmpty(_CuryDocBal) && curyDocTotal != docCuryDocBal)
			{
				ParentSetValue(sender.Graph, _CuryDocBal, curyDocTotal);
			}
		}

		protected virtual void POOrder_CuryTaxTot_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (IsExternalTax(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID)) || (e.Row != null && ((POOrder)e.Row).ExternalTaxesImportInProgress == true))
			{
				decimal? curyTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
				decimal? curyWhTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryWhTaxTotal);

				CalcDocTotals(sender, e.Row, curyTaxTotal.GetValueOrDefault(), 0, curyWhTaxTotal.GetValueOrDefault(), 0);
			}
		}

        protected override void AdjustTaxableAmount(PXCache sender, object row, List<object> taxitems, ref decimal CuryTaxableAmt, string TaxCalcType)
        {
            decimal CuryLineTotal = (decimal?)ParentGetValue<POOrder.curyLineTotal>(sender.Graph) ?? 0m;
            decimal CuryDiscountTotal = (decimal)(ParentGetValue<POOrder.curyDiscTot>(sender.Graph) ?? 0m);

            if (CuryLineTotal != 0m && CuryTaxableAmt != 0m)
            {
                if (Math.Abs(CuryTaxableAmt - CuryLineTotal) < 0.00005m)
                {
                    CuryTaxableAmt -= CuryDiscountTotal;
                }
            }
        }

		protected override bool IsRetainedTaxes(PXGraph graph)
		{
			PXCache cache = graph.Caches[typeof(APSetup)];
			APSetup apsetup = cache.Current as APSetup;
			POOrder document = ParentRow(graph) as POOrder;

			return
				PXAccess.FeatureInstalled<FeaturesSet.retainage>() &&
				document?.RetainageApply == true &&
				apsetup?.RetainTaxes == true;
		}
	}

	public class POUnbilledTaxAttribute : POTaxAttribute
	{
		protected override short SortOrder
		{
			get
			{
				return 1;
			}
		}

		public POUnbilledTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this._CuryTaxableAmt = typeof(POTax.curyUnbilledTaxableAmt).Name;
			this._CuryTaxAmt = typeof(POTax.curyUnbilledTaxAmt).Name;

			this.CuryDocBal = typeof(POOrder.curyUnbilledOrderTotal);
			this.CuryLineTotal = typeof(POOrder.curyUnbilledLineTotal);
			this.CuryTaxTotal = typeof(POOrder.curyUnbilledTaxTotal);
			this.DocDate = typeof(POOrder.orderDate);
			this.CuryDiscTot = typeof(POOrder.curyDiscTot);

			this.CuryTranAmt = typeof(POLine.curyUnbilledAmt);

			this._Attributes.Clear();
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(POLine.curyUnbilledAmt), typeof(SumCalc<POOrder.curyUnbilledLineTotal>)));
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<POLine.lineNbr>>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		#region Per Unit Taxes
		protected override string TaxableQtyFieldNameForTaxDetail => nameof(POTax.UnbilledTaxableQty);
		#endregion

		//Only base attribute should re-default taxes
		protected override void ReDefaultTaxes(PXCache cache, List<object> details)
		{
		}
		protected override void ReDefaultTaxes(PXCache cache, object clearDet, object defaultDet, bool defaultExisting = true)
		{
		}

		protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType)
		{
			// base behavior to avoid doubling of retainage (it is already in POLine.curyOpenAmt)
			decimal? value = ((decimal?)sender.GetValue(row, _CuryTranAmt) ?? 0m) *
				(decimal?)sender.GetValue(row, _GroupDiscountRate) *
				(decimal?)sender.GetValue(row, _DocumentDiscountRate);

			if (value != 0m)
			{
				CurrencyInfo currencyInfo = sender.Graph.FindImplementation<IPXCurrencyHelper>().GetDefaultCurrencyInfo();
				return currencyInfo.RoundCury((decimal)value);
			}
			else
			{
				return 0m;
			}
		}

		protected override bool IsRetainedTaxes(PXGraph graph)
		{
			return false;
		}

		protected override void _CalcDocTotals(
			PXCache sender,
			object row,
			decimal curyOpenTaxTotal,
			decimal curyOpenInclTaxTotal,
			decimal curyOpenWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			//POOpenTaxAttribute can rely on POTaxAttribute beeen called at this point in time (since the SortOrder is applied)

			decimal curyDiscountTotal = (decimal)(ParentGetValue(sender.Graph, _CuryDiscTot) ?? 0m);
			decimal curyOpenLineTotal = (decimal)(ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m);//_CuryLineTotal is actually a curyOpenLineTotal in this context.
			decimal curyLineTotal = (decimal?)ParentGetValue<POOrder.curyLineTotal>(sender.Graph) ?? 0m;

			decimal curyOpenDiscTotal = CalcUnbilledDiscTotal(sender, row, curyLineTotal, curyDiscountTotal, curyOpenLineTotal);

			decimal curyOpenDocTotal = curyOpenLineTotal + curyOpenTaxTotal - curyOpenInclTaxTotal - curyOpenDiscTotal;

			decimal docCuryTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryTaxTotal) ?? 0m);//_CuryTaxTotal is actually curyOpenTaxTotal
			decimal docCuryDocBal = (decimal)(ParentGetValue(sender.Graph, _CuryDocBal) ?? 0m);//_CuryDocBal is actually curyOpenOrderTotal

			if (curyOpenTaxTotal != docCuryTaxTotal)
			{
				ParentSetValue(sender.Graph, _CuryTaxTotal, curyOpenTaxTotal);
			}

			if (!string.IsNullOrEmpty(_CuryDocBal) && curyOpenDocTotal != docCuryDocBal)
			{
				ParentSetValue(sender.Graph, _CuryDocBal, curyOpenDocTotal);
			}
		}

		protected override void AdjustTaxableAmount(PXCache sender, object row, List<object> taxitems, ref decimal CuryTaxableAmt, string TaxCalcType)
		{
			decimal curyUnbilledLineTotal = (decimal?)ParentGetValue<POOrder.curyUnbilledLineTotal>(sender.Graph) ?? 0m;

			if (curyUnbilledLineTotal != 0m && CuryTaxableAmt != 0m && curyUnbilledLineTotal == CuryTaxableAmt)
			{
				decimal curyLineTotal = (decimal?)ParentGetValue<POOrder.curyLineTotal>(sender.Graph) ?? 0m;
				decimal curyDiscountTotal = (decimal?)ParentGetValue<POOrder.curyDiscTot>(sender.Graph) ?? 0m;

				CuryTaxableAmt -= CalcUnbilledDiscTotal(sender, row, curyLineTotal, curyDiscountTotal, curyUnbilledLineTotal);
			}
		}

		protected virtual decimal CalcUnbilledDiscTotal(PXCache sender, object row, decimal curyLineTotal, decimal curyDiscTotal, decimal curyUnbilledLineTotal)
		{
			CurrencyInfo currencyInfo = sender.Graph.FindImplementation<IPXCurrencyHelper>().GetDefaultCurrencyInfo();
			return (Math.Abs(curyLineTotal - curyUnbilledLineTotal) < 0.00005m)
				? curyDiscTotal
				: (curyLineTotal == 0 ? 0 : currencyInfo.RoundCury(curyUnbilledLineTotal * curyDiscTotal / curyLineTotal));
		}

		protected override void SetTaxDetailCuryExpenseAmt(PXCache cache, TaxDetail taxdet, decimal CuryExpenseAmt)
		{
			//Expense amount should not be recalculated for unbilled values
		}

		public override object Insert(PXCache cache, object item)
		{
			return InsertCached(cache, item);
		}

		public override object Update(PXCache sender, object item)
		{
			return UpdateCached(sender, item);
		}

		public override object Delete(PXCache cache, object item)
		{
			return DeleteCached(cache, item);
		}

		protected virtual object InsertCached(PXCache sender, object item)
		{
			List<object> recordsList = getRecordsList(sender);

			PXCache pcache = sender.Graph.Caches[typeof(POOrder)];
			string OrderType = (string)pcache.GetValue<POOrder.orderType>(pcache.Current);
			string OrderNbr = (string)pcache.GetValue<POOrder.orderNbr>(pcache.Current);

			List<PXRowInserted> insertedHandlersList = storeCachedInsertList(sender, recordsList, OrderType, OrderNbr);
			List<PXRowDeleted> deletedHandlersList = storeCachedDeleteList(sender, recordsList, OrderType, OrderNbr);

			try
			{
				return sender.Insert(item);
			}
			finally
			{
				foreach (PXRowInserted handler in insertedHandlersList)
					sender.Graph.RowInserted.RemoveHandler<POTax>(handler);
				foreach (PXRowDeleted handler in deletedHandlersList)
					sender.Graph.RowDeleted.RemoveHandler<POTax>(handler);
			}
		}

		protected virtual object UpdateCached(PXCache sender, object item)
		{
			List<object> recordsList = getRecordsList(sender);

			PXCache pcache = sender.Graph.Caches[typeof(POOrder)];
			string OrderType = (string)pcache.GetValue<POOrder.orderType>(pcache.Current);
			string OrderNbr = (string)pcache.GetValue<POOrder.orderNbr>(pcache.Current);

			List<PXRowUpdated> updatedHandlersList = storeCachedUpdateList(sender, recordsList, OrderType, OrderNbr);

			try
			{
				return sender.Update(item);
			}
			finally
			{
				foreach (PXRowUpdated handler in updatedHandlersList)
					sender.Graph.RowUpdated.RemoveHandler<POTax>(handler);
			}
		}

		protected virtual object DeleteCached(PXCache sender, object item)
		{
			List<object> recordsList = getRecordsList(sender);

			PXCache pcache = sender.Graph.Caches[typeof(POOrder)];
			string OrderType = (string)pcache.GetValue<POOrder.orderType>(pcache.Current);
			string OrderNbr = (string)pcache.GetValue<POOrder.orderNbr>(pcache.Current);

			List<PXRowInserted> insertedHandlersList = storeCachedInsertList(sender, recordsList, OrderType, OrderNbr);
			List<PXRowDeleted> deletedHandlersList = storeCachedDeleteList(sender, recordsList, OrderType, OrderNbr);

			try
			{
				return sender.Delete(item);
			}
			finally
			{
				foreach (PXRowInserted handler in insertedHandlersList)
					sender.Graph.RowInserted.RemoveHandler<POTax>(handler);
				foreach (PXRowDeleted handler in deletedHandlersList)
					sender.Graph.RowDeleted.RemoveHandler<POTax>(handler);
			}
		}

		protected List<Object> getRecordsList(PXCache sender)
		{
			var recordsList = new List<object>(PXSelect<POTax,
				Where<POTax.orderType, Equal<Current<POOrder.orderType>>,
					And<POTax.orderNbr, Equal<Current<POOrder.orderNbr>>>>>
				.SelectMultiBound(sender.Graph, new object[] { sender.Graph.Caches[typeof(POOrder).Name].Current })
				.RowCast<POTax>());

			return recordsList;
		}

		protected List<PXRowInserted> storeCachedInsertList(PXCache sender, List<Object> recordsList, string OrderType, string OrderNbr)
		{
			List<PXRowInserted> handlersList = new List<PXRowInserted>();

			PXRowInserted inserted = delegate (PXCache cache, PXRowInsertedEventArgs e)
			{
				recordsList.Add(e.Row);

				PXSelect<POTax,
					Where<POTax.orderType, Equal<Current<POOrder.orderType>>,
						And<POTax.orderNbr, Equal<Current<POOrder.orderNbr>>>>>
					.StoreResult(sender.Graph, recordsList, PXQueryParameters.ExplicitParameters(OrderType, OrderNbr));
			};

			sender.Graph.RowInserted.AddHandler<POTax>(inserted);
			handlersList.Add(inserted);

			return handlersList;
		}

		protected List<PXRowUpdated> storeCachedUpdateList(PXCache sender, List<Object> recordsList, string OrderType, string OrderNbr)
		{
			var handlersList = new List<PXRowUpdated>();

			PXRowUpdated updated = delegate (PXCache cache, PXRowUpdatedEventArgs e)
			{
				var comparer = cache.GetComparer();
				int ndx = recordsList.FindIndex(t => comparer.Equals(t, e.OldRow));
				if (ndx >= 0)
				{
					recordsList[ndx] = e.Row;
				}

				PXSelect<POTax,
					Where<POTax.orderType, Equal<Current<POOrder.orderType>>,
						And<POTax.orderNbr, Equal<Current<POOrder.orderNbr>>>>>
					.StoreResult(sender.Graph, recordsList, PXQueryParameters.ExplicitParameters(OrderType, OrderNbr));
			};

			sender.Graph.RowUpdated.AddHandler<POTax>(updated);
			handlersList.Add(updated);

			return handlersList;
		}

		protected List<PXRowDeleted> storeCachedDeleteList(PXCache sender, List<Object> recordsList, string OrderType, string OrderNbr)
		{
			List<PXRowDeleted> handlersList = new List<PXRowDeleted>();

			PXRowDeleted deleted = delegate(PXCache cache, PXRowDeletedEventArgs e)
			{
				recordsList.Remove(e.Row);

				PXSelect<POTax,
					Where<POTax.orderType, Equal<Current<POOrder.orderType>>,
						And<POTax.orderNbr, Equal<Current<POOrder.orderNbr>>>>>
					.StoreResult(sender.Graph, recordsList, PXQueryParameters.ExplicitParameters(OrderType, OrderNbr));
			};

			sender.Graph.RowDeleted.AddHandler<POTax>(deleted);
			handlersList.Add(deleted);

			return handlersList;
		}
	}


	public class POUnbilledTaxRAttribute : POUnbilledTaxAttribute
	{
		public POUnbilledTaxRAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryTranAmt = typeof(POLineUOpen.curyUnbilledAmt);

			this._Attributes[0] = new PXUnboundFormulaAttribute(typeof(POLineUOpen.curyUnbilledAmt), typeof(SumCalc<POOrder.curyUnbilledLineTotal>));
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<POLineUOpen.lineNbr>>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_ChildType = sender.GetItemType();
			TaxCalc = TaxCalc.Calc;
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _CuryTranAmt, CuryOpenAmt_FieldUpdated);
		}

		override protected bool EnableTaxCalcOn(PXGraph aGraph)
		{
			return (aGraph is POOrderEntry || aGraph is POReceiptEntry || aGraph is AP.APReleaseProcess);
		}

		public virtual void CuryOpenAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row, _ParentType);
			CalcTaxes(sender, e.Row, PXTaxCheck.Line);
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row, _ParentType);
			base.RowInserted(sender, e);
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row, _ParentType);
			base.RowUpdated(sender, e);
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row, _ParentType);
			base.RowDeleted(sender, e);
		}
	}

    /// <summary>
    /// Specialized for POLine version of the CrossItemAttribute.<br/>
    /// Providing an Inventory ID selector for the field, it allows also user <br/>
    /// to select both InventoryID and SubItemID by typing AlternateID in the control<br/>
    /// As a result, if user type a correct Alternate id, values for InventoryID, SubItemID, <br/>
    /// and AlternateID fields in the row will be set.<br/>
    /// In this attribute, InventoryItems with a status inactive, markedForDeletion,<br/>
    /// noPurchase and noRequest are filtered out. It also fixes  INPrimaryAlternateType parameter to VPN <br/>
    /// This attribute may be used in combination with AlternativeItemAttribute on the AlternateID field of the row <br/>
    /// <example>
    /// [POLineInventoryItem(Filterable = true)]
    /// </example>
    /// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noPurchases>>), IN.Messages.ItemCannotPurchase)]
	public class POLineInventoryItemAttribute : CrossItemAttribute
	{

        /// <summary>
        /// Default ctor
        /// </summary>
		public POLineInventoryItemAttribute()
			: base(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr), INPrimaryAlternateType.VPN)
		{
		}
	}

	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	public class POReceiptLineInventoryAttribute : CrossItemAttribute
	{
		public POReceiptLineInventoryAttribute(Type receiptType)
			: base(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr), INPrimaryAlternateType.VPN)
		{
			var condition = BqlTemplate.OfCondition<
				Where2<Where<Current2<BqlPlaceholder.A>, Equal<POReceiptType.transferreceipt>,
					Or<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noPurchases>>>,
					And2<Not<FeatureInstalled<FeaturesSet.pOReceiptsWithoutInventory>>, Or<InventoryItem.nonStockReceiptAsService, Equal<True>>>>>
				.Replace<BqlPlaceholder.A>(receiptType)
				.ToType();

			_Attributes.Add(new PXRestrictorAttribute(condition, IN.Messages.ItemCannotPurchase));
		}
	}

	#region POOpenPeriod
	public class POOpenPeriodAttribute : OpenPeriodAttribute
	{
		#region Ctor
		public POOpenPeriodAttribute(Type sourceType,
			Type branchSourceType,
			Type branchSourceFormulaType = null,
			Type organizationSourceType = null,
			Type useMasterCalendarSourceType = null,
			Type defaultType = null,
			bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
			bool useMasterOrganizationIDByDefault = false,
			Type masterFinPeriodIDType = null)
			: base(
				typeof(Search<FinPeriod.finPeriodID,
							Where<FinPeriod.status, Equal<FinPeriod.status.open>,
									And<FinPeriod.aPClosed, Equal<False>,
								And<Where<FinPeriod.iNClosed, Equal<False>, Or<Not<FeatureInstalled<FeaturesSet.inventory>>>>>>>>),
				sourceType,
				branchSourceType: branchSourceType,
				branchSourceFormulaType: branchSourceFormulaType,
				organizationSourceType: organizationSourceType,
				useMasterCalendarSourceType: useMasterCalendarSourceType,
				defaultType: defaultType,
				redefaultOrRevalidateOnOrganizationSourceUpdated: redefaultOrRevalidateOnOrganizationSourceUpdated,
				useMasterOrganizationIDByDefault: useMasterOrganizationIDByDefault,
				masterFinPeriodIDType: masterFinPeriodIDType)
		{
		}

		public POOpenPeriodAttribute(Type SourceType)
			: this(SourceType, null)
		{
		}

		public POOpenPeriodAttribute()
			: this(null)
		{
		}
		#endregion

		#region Implementation

		protected override PeriodValidationResult ValidateOrganizationFinPeriodStatus(PXCache sender, object row, FinPeriod finPeriod)
		{
			PeriodValidationResult result = base.ValidateOrganizationFinPeriodStatus(sender, row, finPeriod);

			if (!result.HasWarningOrError)
			{
				if (finPeriod.APClosed == true)
				{
					result = HandleErrorThatPeriodIsClosed(sender, finPeriod, errorMessage: AP.Messages.FinancialPeriodClosedInAP);
				}

				if (finPeriod.INClosed == true && PXAccess.FeatureInstalled<FeaturesSet.inventory>())
				{
					result.Aggregate(HandleErrorThatPeriodIsClosed(sender, finPeriod, errorMessage: IN.Messages.FinancialPeriodClosedInIN));
				}
			}

			return result;
		}

		#endregion
	}
	#endregion

    /// <summary>
    /// This attribute defines, if the vendor and it's location specified
    /// are the preffered Vendor for the inventory item. May be placed on field of boolean type,
    /// to display this information dynamically
    /// <example>
    /// [PODefaultVendor(typeof(POVendorInventory.inventoryID), typeof(POVendorInventory.vendorID), typeof(POVendorInventory.vendorLocationID))]
    /// </example>
    /// </summary>
	public class PODefaultVendor : PXEventSubscriberAttribute
	{
		private Type inventoryID;
		private Type vendorID;
		private Type vendorLocationID;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="inventoryID">Must be IBqlField. Field which contains inventory id, for which Vendor/location is checked for beeng a preferred Vendor</param>
        /// <param name="vendorID">Must be IBqlField. Field which contains VendorID of the vendor checking for beeng a preferred Vendor</param>
        /// <param name="vendorLocationID">Must be IBqlField. Field which contains VendorLocationID of the vendor checking for beeng a preferred Vendor</param>
		public PODefaultVendor(Type inventoryID, Type vendorID, Type vendorLocationID)
		{
			this.inventoryID = inventoryID;
			this.vendorID    = vendorID;
			this.vendorLocationID = vendorLocationID;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.RowSelecting.AddHandler(sender.GetItemType(), RowSelecting);
		}

		#region IPXRowSelectingSubscriber Members
		protected virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			object itemID = sender.GetValue(e.Row, this.inventoryID.Name);
			object vendorID = sender.GetValue(e.Row, this.vendorID.Name);
			object vendorLocationID = sender.GetValue(e.Row, this.vendorLocationID.Name);

			if (itemID != null && vendorID != null)
			{
				Vendor vendor = Vendor.PK.Find(sender.Graph, (int?)vendorID);
				InventoryItemCurySettings curySettings = null;
				if (vendor?.BaseCuryID != null)
				{
					curySettings = InventoryItemCurySettings.PK.Find(sender.Graph, (int?)itemID, vendor.BaseCuryID);
				}
				else if (vendor != null) // The branch is extended as vendor should be default for all currencies.
				{
					// Acuminator disable once PX1042 DatabaseQueriesInRowSelecting [false positive]
					curySettings = SelectFrom<InventoryItemCurySettings>
						.Where<InventoryItemCurySettings.inventoryID.IsEqual<@P.AsInt>>
						.View.ReadOnly.Select(sender.Graph, itemID);
				}

				sender.SetValue(e.Row, _FieldName, curySettings != null &&
					object.Equals(curySettings.PreferredVendorID, vendorID) &&
					object.Equals(curySettings.PreferredVendorLocationID, vendorLocationID));
			}
		}
		#endregion
	}

	public class POXRefUpdate : PXEventSubscriberAttribute
	{
		public POXRefUpdate(Type inventoryID, Type subItem, Type vendorID)
		{
		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

		}
	}

	public class POVendorInventorySelect<Table, Join, Where, PrimaryType> : PXSelectJoin<Table, Join, Where>
		where Table : POVendorInventory, new()
		where Join : class, PX.Data.IBqlJoin, new()
		where Where : PX.Data.IBqlWhere, new()
		where PrimaryType : class, IBqlTable, new()
	{
		protected const string _UPDATEVENDORPRICE_COMMAND = "UpdateVendorPrice";
		protected const string _UPDATEVENDORPRICE_VIEW    = "VendorInventory$UpdatePrice";

		public POVendorInventorySelect(PXGraph graph)
			: base(graph)
		{
			graph.Views.Caches.Add(typeof(INItemXRef));
			graph.RowSelected.AddHandler<Table>(OnRowSelected);
			graph.RowInserted.AddHandler<Table>(OnRowInserted);
			graph.RowUpdated.AddHandler<Table>(OnRowUpdated);
			graph.RowDeleted.AddHandler<Table>(OnRowDeleted);
			graph.RowPersisting.AddHandler<Table>(OnRowPersisting);
			graph.RowSelected.AddHandler<PrimaryType>(OnParentRowSelected);

			var filter = new PXFilter<POVendorPriceUpdate>(graph);
			graph.Views.Add(_UPDATEVENDORPRICE_VIEW, filter.View);
		}

		public POVendorInventorySelect(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			graph.Views.Caches.Add(typeof(INItemXRef));
			graph.RowSelected.AddHandler<Table>(OnRowSelected);
			graph.RowInserted.AddHandler<Table>(OnRowInserted);
			graph.RowUpdated.AddHandler<Table>(OnRowUpdated);
			graph.RowDeleted.AddHandler<Table>(OnRowDeleted);
			graph.RowPersisting.AddHandler<Table>(OnRowPersisting);
			graph.RowSelected.AddHandler<PrimaryType>(OnParentRowSelected);

			var filter = new PXFilter<POVendorPriceUpdate>(graph);
			graph.Views.Add(_UPDATEVENDORPRICE_VIEW, filter.View);
		}

		private void AddAction(PXGraph graph, string name, string displayName, PXButtonDelegate handler)
		{
			var uiAtt = new PXUIFieldAttribute { DisplayName = PXMessages.LocalizeNoPrefix(displayName)};
			graph.Actions[name] = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(
				new Type[] { typeof(PrimaryType) }),
				new object[] { graph, name, handler, uiAtt });
		}

		protected virtual InventoryItem ReadInventory(object current)
		{
			return InventoryItem.PK.FindDirty(this._Graph, ((Table)current).InventoryID);
		}

		protected virtual void OnRowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			Table current = e.Row as Table;
			if (current == null) return;
			if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				INSetup setup = (INSetup)cache.Graph.Caches[typeof(INSetup)].Current;
				if (setup != null && setup.UseInventorySubItem == true)
				{
					InventoryItem item = ReadInventory(current);
					if (item != null && item.DefaultSubItemID == null && item.StkItem == true)
						current.OverrideSettings = true;
				}
			}
			if (!cache.Graph.IsCopyPasteContext)
			{
			UpdateXRef(current);
		}
		}

		protected virtual void OnParentRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				INSetup setup = (INSetup)sender.Graph.Caches[typeof(INSetup)].Current;
				PXUIFieldAttribute.SetVisible<POVendorInventory.overrideSettings>
					(sender.Graph.Caches[typeof(POVendorInventory)], null, setup.UseInventorySubItem == true);
			}
		}
		protected virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Table row = (Table)e.Row;
			PXUIFieldAttribute.SetVisible<POVendorInventory.curyID>(sender, null,
				PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());

			if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				if (row == null) return;
				INSetup setup = (INSetup)sender.Graph.Caches[typeof(INSetup)].Current;

				InventoryItem item = ReadInventory(row);

				bool isEnabled =
					row.OverrideSettings == true ||
					item == null ||
					setup.UseInventorySubItem != true ||
					item.DefaultSubItemID == row.SubItemID;
				PXUIFieldAttribute.SetEnabled<POVendorInventory.overrideSettings>(sender, row, setup.UseInventorySubItem == true && item != null && item.StkItem == true);
				PXUIFieldAttribute.SetEnabled<POVendorInventory.addLeadTimeDays>(sender, row, isEnabled);
				PXUIFieldAttribute.SetEnabled<POVendorInventory.eRQ>(sender, row, isEnabled);
				PXUIFieldAttribute.SetEnabled<POVendorInventory.lotSize>(sender, row, isEnabled);
				PXUIFieldAttribute.SetEnabled<POVendorInventory.maxOrdQty>(sender, row, isEnabled);
				PXUIFieldAttribute.SetEnabled<POVendorInventory.minOrdFreq>(sender, row, isEnabled);
				PXUIFieldAttribute.SetEnabled<POVendorInventory.minOrdQty>(sender, row, isEnabled);
				PXUIFieldAttribute.SetEnabled<POVendorInventory.subItemID>(sender, row, item != null && item.StkItem == true);
			}
		}

		protected virtual void OnRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Table current = e.Row as Table;
			Table old     = e.OldRow as Table;
			if (current == null) return;

			InventoryItem item = ReadInventory(current);

			if (item != null && item.DefaultSubItemID != null && item.DefaultSubItemID == current.SubItemID)
			{
				foreach (POVendorInventory vi in
					PXSelect<POVendorInventory,
					Where<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>,
						And2<Where<POVendorInventory.vendorLocationID, Equal<Required<POVendorInventory.vendorLocationID>>,
									 Or<Where<Required<POVendorInventory.vendorLocationID>, IsNull, And<POVendorInventory.vendorLocationID, IsNull>>>>,
						And<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
						And<POVendorInventory.subItemID, NotEqual<Required<POVendorInventory.subItemID>>,
						And<POVendorInventory.overrideSettings, Equal<boolFalse>>>>>>>
						.Select(sender.Graph, current.VendorID, current.VendorLocationID, current.VendorLocationID, current.InventoryID, current.SubItemID))
				{
					if (vi.RecordID == current.RecordID) continue;
					POVendorInventory rec = PXCache<POVendorInventory>.CreateCopy(vi);
					rec.AddLeadTimeDays = current.AddLeadTimeDays;
					rec.ERQ = current.ERQ;
					rec.VLeadTime = current.VLeadTime;
					rec.LotSize = current.LotSize;
					rec.MaxOrdQty = current.MaxOrdQty;
					rec.MinOrdFreq = current.MinOrdFreq;
					rec.MinOrdQty = current.MinOrdQty;
					sender.Update(rec);
				}
			}

			if (!sender.Graph.IsCopyPasteContext && !IsEqualByItemXRef(current, old))
			{
				if (!ExistRelatedPOVendorInventory(old)) DeleteXRef(old);
				UpdateXRef(current);
			}
		}

		protected virtual void OnRowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			var row = (Table)e.Row;

			if (!ExistRelatedPOVendorInventory(row)) DeleteXRef(row);
		}

		protected virtual void OnRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if(e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				InventoryItem item = ReadInventory(e.Row);
				PXDefaultAttribute.SetPersistingCheck<POVendorInventory.subItemID>(sender, e.Row, item == null || item.StkItem == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			}
		}

		static bool IsEqualByItemXRef(Table op1, Table op2)
		{
			return (op1.VendorID == op2.VendorID
				&& op1.InventoryID == op2.InventoryID
				&& op1.SubItemID == op2.SubItemID
				&& op1.VendorInventoryID == op2.VendorInventoryID);
		}

		private void DeleteXRef(Table doc)
		{
			if (!CanProcessXRef(doc, out int? subItemID))
			{
				return;
			}

			PXCache cache = _Graph.Caches[typeof(INItemXRef)];

				foreach (INItemXRef it in PXSelect<INItemXRef,
										Where<INItemXRef.alternateID, Equal<Required<POVendorInventory.vendorInventoryID>>,
										And<INItemXRef.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
										And<INItemXRef.subItemID, Equal<Required<POVendorInventory.subItemID>>,
										And<INItemXRef.bAccountID, Equal<Required<POVendorInventory.vendorID>>,
										And<INItemXRef.alternateType, Equal<INAlternateType.vPN>>>>>>>.
									Select(_Graph, doc.VendorInventoryID, doc.InventoryID, subItemID, doc.VendorID))
				{
					cache.Delete(it);
				}
			}

		private bool ExistRelatedPOVendorInventory(Table doc)
		{
			if(!doc.InventoryID.HasValue || !doc.VendorID.HasValue || String.IsNullOrEmpty(doc.VendorInventoryID))
			{
				return false;
			}

			var cmd = new PXSelect<POVendorInventory,
				Where<POVendorInventory.vendorInventoryID, Equal<Required<POVendorInventory.vendorInventoryID>>,
					And<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
					And<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>,
							And<POVendorInventory.recordID, NotEqual<Required<POVendorInventory.recordID>>>>>>>(_Graph);

			if (doc.SubItemID.HasValue)
			{
				cmd.WhereAnd<Where<POVendorInventory.subItemID, Equal<Required<POVendorInventory.subItemID>>>>();
				return cmd.SelectWindowed(0, 1, doc.VendorInventoryID, doc.InventoryID, doc.VendorID, doc.RecordID, doc.SubItemID).Any();
			}
			else
			{
				InventoryItem item = ReadInventory(doc);

				if (item == null || item.StkItem != false) return false;

				return cmd.SelectWindowed(0, 1, doc.VendorInventoryID, doc.InventoryID, doc.VendorID, doc.RecordID).Any();
			}
		}

		private void UpdateXRef(Table doc)
		{
			if (!CanProcessXRef(doc, out int? subItemID))
			{
				return;
			}

			PXCache cache = _Graph.Caches[typeof(INItemXRef)];
				INItemXRef itemXRef = null;
				INItemXRef globalXRef = null;
				foreach (INItemXRef it in PXSelect<INItemXRef,
										Where<INItemXRef.alternateID, Equal<Required<POVendorInventory.vendorInventoryID>>,
										And<INItemXRef.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
										And<INItemXRef.subItemID, Equal<Required<POVendorInventory.subItemID>>,
										And<Where2<Where<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>,
											And<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>>>,
										Or<Where<INItemXRef.alternateType, Equal<INAlternateType.vPN>,
										And<INItemXRef.bAccountID, Equal<Required<POVendorInventory.vendorID>>
										>>>>>>>>, OrderBy<Asc<INItemXRef.alternateType>>>.
									Select(_Graph, doc.VendorInventoryID, doc.InventoryID, subItemID, doc.VendorID))
				{
					if (it.AlternateType == INAlternateType.VPN)
					{
						itemXRef = it;
					}
					else
					{
						if (globalXRef == null)
							globalXRef = it;
					}
				}
				if (itemXRef == null)
				{
					if (globalXRef == null)
					{
						itemXRef = new INItemXRef();
					Copy(itemXRef, doc, subItemID);
						itemXRef = (INItemXRef)cache.Insert(itemXRef);
					}
				}
				else
				{
					INItemXRef itemXRef2 = (INItemXRef)cache.CreateCopy(itemXRef);
				Copy(itemXRef2, doc, subItemID);
					itemXRef = (INItemXRef)cache.Update(itemXRef2);
				}
			}

		private bool CanProcessXRef(Table doc, out int? subItemID)
		{
			subItemID = doc.SubItemID;

			if (doc.InventoryID.HasValue && doc.VendorID.HasValue && !String.IsNullOrEmpty(doc.VendorInventoryID))
			{
				if (!doc.SubItemID.HasValue)
				{
					InventoryItem item = ReadInventory(doc);

					if (item == null || item.StkItem != false)
					{
						return false;
					}

					PXCache cache = _Graph.Caches[typeof(INItemXRef)];
					cache.RaiseFieldDefaulting<INItemXRef.subItemID>(null, out var defSubItemID);
					subItemID = (int?)defSubItemID;
				}

				return true;
			}

			return false;
		}

		static void Copy(INItemXRef dest, Table src, int? subItemID)
		{
			dest.InventoryID = src.InventoryID;
			if(PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			dest.SubItemID = subItemID;
			dest.BAccountID = src.VendorID;
			dest.AlternateType = INAlternateType.VPN;
			dest.AlternateID = src.VendorInventoryID;
			dest.UOM = src.PurchaseUnit;
		}
	}


    /// <summary>
    /// Specialized for Landed Cost version of VendorAttribute.
    /// Displayes only Vendors having LandedCostVendor = true.
    /// Employee and non-active vendors are filtered out
    /// <example>
    /// [LandedCostVendorActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
    /// </example>
    /// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Vendor", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<AP.Vendor.landedCostVendor,Equal<boolTrue>>), Messages.VendorIsNotLandedCostVendor)]
	public class LandedCostVendorActiveAttribute : AP.VendorNonEmployeeActiveAttribute
	{
        /// <summary>
        /// Default ctor.
        /// </summary>
		public LandedCostVendorActiveAttribute()
			: base()
		{
		}

		//public override void Verify(PXCache sender, Vendor item)
		//{
		//    if (item.LandedCostVendor == false)
		//    {
		//        throw new PXException(Messages.VendorIsNotLandedCostVendor);
		//    }
		//}
	}

	public class POLocationAvailAttribute : LocationAvailAttribute
	{
		public POLocationAvailAttribute(Type InventoryType, Type SubItemType, Type costCenterType, Type SiteIDType, Type TranType, Type InvtMultType)
			: base(InventoryType, SubItemType, costCenterType, SiteIDType, TranType, InvtMultType)
		{
		}

		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			POReceiptLine row = e.Row as POReceiptLine;
			if (row == null) return;

			if (POLineType.IsStock(row.LineType) && row.POType != null && row.PONbr != null && row.POLineNbr != null && row.POType != POOrderType.ProjectDropShip)
			{
				POLine poLine = PXSelect<POLine, Where<POLine.orderType, Equal<Required<POLine.orderType>>,
						And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
						And<POLine.lineNbr, Equal<Required<POLine.lineNbr>>>>>>.Select(sender.Graph, row.POType, row.PONbr, row.POLineNbr);

				if (poLine != null && poLine.TaskID != null)
				{
					INLocation selectedLocation = PXSelect<INLocation, Where<INLocation.siteID, Equal<Required<INLocation.siteID>>,
						And<INLocation.taskID, Equal<Required<INLocation.taskID>>>>>.Select(sender.Graph, row.SiteID, poLine.TaskID);

					if (selectedLocation != null )
					{
						e.NewValue = selectedLocation.LocationID;
						return;
					}
					else
					{
						e.NewValue = null;
						return;
					}
				}
			}

			base.FieldDefaulting(sender, e);
		}
	}

	public class POLotSerialNbrAttribute : INLotSerialNbrAttribute
	{
		public POLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType, Type ParentLotSerialNbrType, Type CostCenterType)
			: base(InventoryType, SubItemType, LocationType, ParentLotSerialNbrType, CostCenterType)
		{
		}

		public POLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType, Type CostCenterType)
			: base(InventoryType, SubItemType, LocationType, CostCenterType)
		{
		}

		protected POLotSerialNbrAttribute()
		{
		}

		protected override bool IsTracked(ILSMaster row, INLotSerClass lotSerClass, string tranType, int? invMult)
		{
			POReceiptLineSplit split = row as POReceiptLineSplit;

			if (tranType == INTranType.Issue && lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed)
			{
				return false;
			}
			else if (split != null && split.LineType == POLineType.GoodsForDropShip)
			{
				return true;
			}
			else
			{
				return base.IsTracked(row, lotSerClass, tranType, invMult);
			}
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
			if (item == null || ((ILSMaster)e.Row).SubItemID == null || ((ILSMaster)e.Row).LocationID == null)
			{
				return;
			}

			string lineType = (string)sender.GetValue<POReceiptLineSplit.lineType>(e.Row);

			if (lineType == POLineType.GoodsForDropShip)
			{
				if ( ((INLotSerClass)item).RequiredForDropship == true && IsTracked((ILSMaster)e.Row, item, ((ILSMaster)e.Row).TranType, ((ILSMaster)e.Row).InvtMult) && ((ILSMaster)e.Row).Qty != 0)
				{
					((IPXRowPersistingSubscriber)_Attributes[_DefAttrIndex]).RowPersisting(sender, e);
				}
			}
			else
			{
				base.RowPersisting(sender, e);
			}


			if (IsTracked((ILSMaster)e.Row, item, ((ILSMaster)e.Row).TranType, ((ILSMaster)e.Row).InvtMult) && ((ILSMaster)e.Row).Qty != 0)
			{
				((IPXRowPersistingSubscriber)_Attributes[_DefAttrIndex]).RowPersisting(sender, e);
			}
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			string lineType = (string)sender.GetValue<POReceiptLineSplit.lineType>(e.Row);
			if (POLineType.IsProjectDropShip(lineType))
			{
				PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
				if (item != null && ((INLotSerClass)item).LotSerTrack != INLotSerTrack.NotNumbered && ((INLotSerClass)item).RequiredForDropship == true)
				{
					return;
				}
			}

			base.FieldVerifying(sender, e);
		}
	}

	public class POTransferLotSerialNbrAttribute : POLotSerialNbrAttribute
	{
		public POTransferLotSerialNbrAttribute(Type inventoryType, Type subItemType, Type locationType, Type costCenterType, Type tranType, Type transferNbrType, Type transferLineNbrType)
			: this(inventoryType, subItemType, locationType, costCenterType, tranType, transferNbrType, transferLineNbrType, null)
		{
		}

		public POTransferLotSerialNbrAttribute(Type inventoryType, Type subItemType, Type locationType, Type costCenterType, Type tranType, Type transferNbrType, Type transferLineNbrType,
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

	public class POExpireDateAttribute : INExpireDateAttribute
	{
		public POExpireDateAttribute(Type InventoryType)
			: base(InventoryType)
		{
		}


		protected override bool IsTrackExpiration(PXCache sender, ILSMaster row)
		{
			string lineType = (string) sender.GetValue<POReceiptLineSplit.lineType>(row);

			if (lineType == POLineType.GoodsForDropShip)
			{
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);

				if (item == null)
					return false;
				else
					return ((INLotSerClass)item).LotSerTrackExpiration == true;
			}
			else
			{
				return base.IsTrackExpiration(sender, row);
			}
		}

		protected override bool IsFieldEnabled(PXCache sender, ILSMaster row)
		{
			if (row is POReceiptLine)
				return base.IsFieldEnabled(sender, row) && ((POReceiptLine)row).Released != true;

			return base.IsFieldEnabled(sender, row);
		}
	}

	public class POReports
	{
		public const string PurchaseOrderReportID = "PO641000";
	}
}
