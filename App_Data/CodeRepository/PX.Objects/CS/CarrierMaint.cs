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
using System.Text;
using PX.Data;
using PX.Objects.CS;
using System.Web.Compilation;
using PX.Objects.IN;
using PX.SM;
using PX.Data.Update;
using System.Security.Permissions;
using System.Reflection;
using PX.CarrierService;

namespace PX.Objects.CS
{
	public class CarrierMaint : PXGraph<CarrierMaint, Carrier>
	{
		public PXSelect<Carrier> Carrier;
		public PXSelect<Carrier, Where<Carrier.carrierID, Equal<Current<Carrier.carrierID>>>> CarrierCurrent;
		public PXSelect<FreightRate, Where<FreightRate.carrierID, Equal<Current<Carrier.carrierID>>>> FreightRates;
		public PXSelectJoin<CarrierPackage, 
			InnerJoin<CSBox, On<CSBox.boxID, Equal<CarrierPackage.boxID>> ,CrossJoin<CommonSetup>>,           
			Where<CarrierPackage.carrierID, Equal<Current<Carrier.carrierID>>>> CarrierPackages;
		public PXSelect<CSBox, Where<CSBox.activeByDefault, Equal<boolTrue>>> DefaultBoxes;
		

		
		protected virtual void FreightRate_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			FreightRate doc = (FreightRate)e.Row;

			if (doc.Weight < 0)
			{
				if (sender.RaiseExceptionHandling<FreightRate.weight>(e.Row, null, new PXSetPropertyException(Messages.FieldShouldNotBeNegative, $"[{nameof(FreightRate.weight)}]")))
				{
					throw new PXRowPersistingException(typeof(FreightRate.weight).Name, null, Messages.FieldShouldNotBeNegative, nameof(FreightRate.weight));
				}
				e.Cancel = true;
			}
			if (doc.Volume < 0)
			{
				if (sender.RaiseExceptionHandling<FreightRate.volume>(e.Row, null, new PXSetPropertyException(Messages.FieldShouldNotBeNegative, $"[{nameof(FreightRate.volume)}]")))
				{
					throw new PXRowPersistingException(typeof(FreightRate.volume).Name, null, Messages.FieldShouldNotBeNegative, nameof(FreightRate.volume));
				}
				e.Cancel = true;
			}
			if (doc.Rate < 0)
			{
				if (sender.RaiseExceptionHandling<FreightRate.rate>(e.Row, null, new PXSetPropertyException(Messages.FieldShouldNotBeNegative, $"[{nameof(FreightRate.rate)}]")))
				{
					throw new PXRowPersistingException(typeof(FreightRate.rate).Name, null, Messages.FieldShouldNotBeNegative, nameof(FreightRate.rate));
				}
				e.Cancel = true;
			}
		}

		protected virtual void Carrier_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Carrier row = e.Row as Carrier;
			if (row != null)
			{
				foreach (CSBox box in DefaultBoxes.Select())
				{
					CarrierPackage package = new CarrierPackage();
					package.CarrierID = row.CarrierID;
					package.BoxID = box.BoxID;

					CarrierPackages.Insert(package);
				}
				CarrierPackages.Cache.IsDirty = false;
			}
		}
				
		protected virtual void Carrier_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Carrier doc = (Carrier)e.Row;

			if (doc.BaseRate < 0)
			{
				if (sender.RaiseExceptionHandling<Carrier.baseRate>(e.Row, null, new PXSetPropertyException(Messages.FieldShouldNotBeNegative, $"[{nameof(CS.Carrier.baseRate)}]")))
				{
					throw new PXRowPersistingException(typeof(Carrier.baseRate).Name, null, Messages.FieldShouldNotBeNegative, nameof(CS.Carrier.baseRate));
				}
				e.Cancel = true;
			}

			if (doc.IsExternal == true)
			{
				if (string.IsNullOrEmpty(doc.CarrierPluginID))
				{
					if (sender.RaiseExceptionHandling<Carrier.carrierPluginID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(CS.Carrier.carrierPluginID)}]")))
					{
						throw new PXRowPersistingException(typeof(Carrier.carrierPluginID).Name, null, ErrorMessages.FieldIsEmpty, nameof(CS.Carrier.carrierPluginID));
					}
					e.Cancel = true;
				}

				if (string.IsNullOrEmpty(doc.PluginMethod))
				{
					if (sender.RaiseExceptionHandling<Carrier.pluginMethod>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(CS.Carrier.pluginMethod)}]")))
					{
						throw new PXRowPersistingException(typeof(Carrier.pluginMethod).Name, null, ErrorMessages.FieldIsEmpty, nameof(CS.Carrier.pluginMethod));
					}
					e.Cancel = true;
				}
			}

			if (PXAccess.FeatureInstalled<FeaturesSet.advancedFulfillment>() && doc.IsExternalShippingApplication == true && String.IsNullOrEmpty(doc.ShippingApplicationType))
			{
				if (sender.RaiseExceptionHandling<Carrier.shippingApplicationType>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(CS.Carrier.shippingApplicationType)}]")))
				{
					throw new PXRowPersistingException(typeof(Carrier.shippingApplicationType).Name, null, ErrorMessages.FieldIsEmpty, nameof(CS.Carrier.shippingApplicationType));
				}
			}
		}

        protected virtual void Carrier_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            Carrier row = e.Row as Carrier;
            if (row != null)
            {
				if (row.CarrierPluginID != null)
				{
					CarrierPlugin plugin = (CarrierPlugin)PXSelectorAttribute.Select<CarrierPlugin.carrierPluginID>(sender, row);
					bool isValidType = !string.IsNullOrEmpty(plugin?.PluginTypeName) ? CarrierPluginMaint.IsValidType(plugin.PluginTypeName) : false;
					PXUIFieldAttribute.SetWarning<Carrier.carrierPluginID>(sender, row, isValidType ? null : string.Format(Messages.FailedToCreateCarrierService, row?.CarrierPluginID));
				}

                PXUIFieldAttribute.SetVisible<Carrier.calcMethod>(sender, row, row.IsExternal != true);
                PXUIFieldAttribute.SetVisible<Carrier.baseRate>(sender, row, row.IsExternal != true);
				PXUIFieldAttribute.SetEnabled<Carrier.calcFreightOnReturn>(sender, row, row.IsExternal != true);

				PXUIFieldAttribute.SetVisible<Carrier.carrierPluginID>(sender, row, row.IsExternal == true);
                PXUIFieldAttribute.SetVisible<Carrier.pluginMethod>(sender, row, row.IsExternal == true);
                PXUIFieldAttribute.SetVisible<Carrier.confirmationRequired>(sender, row, row.IsExternal == true);
                PXUIFieldAttribute.SetVisible<Carrier.packageRequired>(sender, row, row.IsExternal == true);

				PXUIFieldAttribute.SetEnabled<Carrier.baseRate>(sender, row, row.CalcMethod != CarrierCalcMethod.Manual);

				if (PXAccess.FeatureInstalled<FeaturesSet.advancedFulfillment>())
	            {
		            // Shipping application integration is mutually exclusive with external carrier plug-in. You can't use both at the same time.
		            PXUIFieldAttribute.SetEnabled<Carrier.isExternal>(sender, row, row.IsExternalShippingApplication == false);
		            PXUIFieldAttribute.SetVisible<Carrier.returnLabel>(sender, row, row.IsExternal == true);
		            PXUIFieldAttribute.SetEnabled<Carrier.isExternalShippingApplication>(sender, row, row.IsExternal == false);
		            PXUIFieldAttribute.SetEnabled<Carrier.shippingApplicationType>(sender, row, row.IsExternal == false);
		            PXUIFieldAttribute.SetEnabled<Carrier.shippingApplicationType>(sender, row, row.IsExternalShippingApplication == true);
				}
            }
        }

        protected virtual void Carrier_CarrierPluginID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            Carrier row = e.Row as Carrier;
            if (row == null) return;
            row.PluginMethod = null;
        }

        protected virtual void Carrier_CalcMethod_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            Carrier row = e.Row as Carrier;
            if (row.CalcMethod == CarrierCalcMethod.Manual) 
                row.BaseRate = 0;
        }

		protected virtual void Carrier_IsExternal_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Carrier row = e.Row as Carrier;
			if (row == null)
				return;

				sender.SetDefaultExt<Carrier.calcFreightOnReturn>(row);
			if (row.IsExternal == true)
				sender.SetDefaultExt<Carrier.calcMethod>(row);
		}

		public static ICarrierService CreateCarrierService(PXGraph graph, string carrierID)
		{
			if (string.IsNullOrEmpty(carrierID))
				throw new ArgumentNullException();

			ICarrierService service = null;
			Carrier carrier = CS.Carrier.PK.Find(graph, carrierID);
			if (carrier != null && carrier.IsExternal == true && !string.IsNullOrEmpty(carrier.CarrierPluginID))
			{
				CarrierResult<ICarrierService> serviceResult = CarrierPluginMaint.CreateCarrierService(graph, carrier.CarrierPluginID, true);
				service = serviceResult.Result;
                service.Method = carrier.PluginMethod;

            }

			return service;
		}
	}

	
    [Serializable]
	public class CarrierMethodSelectorAttribute : PXCustomSelectorAttribute
	{
		public CarrierMethodSelectorAttribute()
			: base(typeof(CarrierPluginMethod.code))
		{
			DescriptionField = typeof(CarrierPluginMethod.description);
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		protected virtual IEnumerable GetRecords()
		{
			PXCache cache = this._Graph.Caches[typeof(Carrier)];
			Carrier row = cache.Current as Carrier;
			if (row != null && row.IsExternal == true && !string.IsNullOrEmpty(row.CarrierPluginID))
			{
				CarrierResult<ICarrierService> serviceResult = CarrierPluginMaint.CreateCarrierService(this._Graph, row.CarrierPluginID);

				if (serviceResult.IsSuccess)
				{
					foreach (CarrierMethod cm in serviceResult.Result.AvailableMethods)
					{
						CarrierPluginMethod cpm = new CarrierPluginMethod();
						cpm.Code = cm.Code;
						cpm.Description = cm.Description;

						yield return cpm;
					}
				}
			}


		}

        [Serializable]
        [PXHidden]
		public partial class CarrierPluginMethod : IBqlTable
		{
			#region Code
			public abstract class code : PX.Data.BQL.BqlString.Field<code> { }
			protected String _Code;
			[PXDefault()]
			[PXString(50, IsUnicode = false, IsKey = true)]
			[PXUIField(DisplayName = "Method Code", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String Code
			{
				get
				{
					return this._Code;
				}
				set
				{
					this._Code = value;
				}
			}
			#endregion
			#region Description
			public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
			protected String _Description;
			[PXString(255, IsUnicode = true)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String Description
			{
				get
				{
					return this._Description;
				}
				set
				{
					this._Description = value;
				}
			}
			#endregion
		}
	}

}
