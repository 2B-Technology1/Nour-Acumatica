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

using System.Collections;
using System.Collections.Generic;
using PX.Objects.CS;
using PX.SM;
using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Data.BQL.Fluent;
using System.Linq;
using PX.Data.BQL;

namespace PX.Objects.EP
{
	public class EmployeeClassMaint : PXGraph<EmployeeClassMaint>
    {
		#region Selects Declartion

		public PXSelect<EPVendorClass> DummyVendorClass;
		public PXSelect<EPEmployeeClass> EmployeeClass;
		public PXSelect<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Current<EPEmployeeClass.vendorClassID>>>> CurEmployeeClassRecord;

        [PXViewName(CR.Messages.Attributes)]
        public CSAttributeGroupList<EPEmployeeClass, EPEmployee> Mapping;

        #endregion

        #region Buttons Declaration
        public PXSave<EPEmployeeClass> Save;
        [PXCancelButton]
        [PXUIField(MapEnableRights = PXCacheRights.Select)]
        protected virtual System.Collections.IEnumerable Cancel(PXAdapter a)
        {
            foreach (EPEmployeeClass e in (new PXCancel<EPEmployeeClass>(this, "Cancel")).Press(a))
            {
                if (EmployeeClass.Cache.GetStatus(e) == PXEntryStatus.Inserted)
                {
                    EPVendorClass e1 = PXSelect<EPVendorClass, Where<EPVendorClass.vendorClassID, Equal<Required<EPVendorClass.vendorClassID>>>>.Select(this, e.VendorClassID);              
                    if (e1 != null)
                    {
                        EmployeeClass.Cache.RaiseExceptionHandling<EPEmployeeClass.vendorClassID>(e, e.VendorClassID, new PXSetPropertyException(Messages.VendorClassExists));
                    }
                }
                yield return e;
            }
        }

        public PXAction<EPEmployeeClass> cancel;
        public PXInsert<EPEmployeeClass> Insert;
    	public PXCopyPasteAction<EPEmployeeClass> Edit; 
        public PXDelete<EPEmployeeClass> Delete;
        public PXFirst<EPEmployeeClass> First;
        public PXPrevious<EPEmployeeClass> Prev;
        public PXNext<EPEmployeeClass> Next;
        public PXLast<EPEmployeeClass> Last;
        #endregion

        #region Events

        protected virtual void EPEmployeeClass_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
			var mcFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
			PXUIFieldAttribute.SetVisible<EPEmployeeClass.curyID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<EPEmployeeClass.curyRateTypeID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<EPEmployeeClass.allowOverrideCury>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<EPEmployeeClass.allowOverrideRate>(cache, null, mcFeatureInstalled);
			if (e.Row != null)
			{
				EPEmployeeClass row = (EPEmployeeClass)e.Row;
				PXUIFieldAttribute.SetEnabled<EPEmployeeClass.cashAcctID>(cache, e.Row, String.IsNullOrEmpty(row.PaymentMethodID) == false);
			}
        }
        
		public virtual void EPEmployeeClass_PaymentMethodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			cache.SetDefaultExt<EPEmployeeClass.cashAcctID>(e.Row);
		}


		protected virtual void EPEmployeeClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			EPEmployeeClass row = e.Row as EPEmployeeClass;
			if (row != null)
			{
				EPEmployee refEmployee = PXSelect<EPEmployee, Where<EPEmployee.vendorClassID, Equal<Current<EPEmployeeClass.vendorClassID>>>>.SelectWindowed(this, 0, 1);
				if (refEmployee != null)
				{
					e.Cancel = true;
					throw new PXException(CS.Messages.ReferencedByEmployee, refEmployee.AcctCD);
				}
			}
		}

		protected virtual void EPEmployeeClass_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (EmployeeClass.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted && e.Operation != PXDBOperation.Delete)
			{
				EPVendorClass e1 = PXSelect<EPVendorClass, Where<EPVendorClass.vendorClassID, Equal<Current<EPEmployeeClass.vendorClassID>>>>.SelectSingleBound(this, new object[]{e.Row});
				if (e1 != null)
				{
					sender.IsDirty = false;
					e.Cancel = true;
					throw new PXRowPersistingException(typeof(EPEmployeeClass.vendorClassID).Name, null, Messages.VendorClassExists);
				}
			}
		}

		protected virtual void EPEmployeeClass_OvertimeMultiplier_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ( (decimal?)(e.NewValue) <= 0m)
			{
				throw new PXSetPropertyException(Messages.ValueMustBeGreaterThanZero);
			}
		}

		protected virtual void _(Events.FieldVerifying<EPEmployeeClass, EPEmployeeClass.probationPeriodMonths> e)
		{
			EPEmployeeClass row = e.Row;
			if (row == null || e.NewValue == e.OldValue)
			{
				return;
			}

			bool isEmployeeClassUsed = SelectFrom<EPEmployee>.Where<EPEmployee.vendorClassID.IsEqual<P.AsString>>.View.Select(this, row.VendorClassID).Any();
			if (isEmployeeClassUsed)
			{
				e.Cache.RaiseExceptionHandling<EPEmployeeClass.probationPeriodMonths>(row, null, new PXSetPropertyException(Messages.ProbationPeriodMonthsChanged, PXErrorLevel.Warning));
			}
		}

		#endregion

		#region Default Instance Accessors

		public PXSelectReadonly<CMSetup> cmsetup;

		public CMSetup CMSETUP
		{
			get
			{
				CMSetup setup = cmsetup.Select();
				if (setup == null)
				{
					setup = new CMSetup();
				}
				return setup;
			}
		}

		#endregion
	}
}
