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
using PX.Data;
using PX.Objects.GL;
using PX.SM;

namespace PX.Objects.AP
{
    [Serializable]
	public partial class APIntegrityCheckFilter : PX.Data.IBqlTable
	{
		#region VendorClassID
		public abstract class vendorClassID : PX.Data.BQL.BqlString.Field<vendorClassID> { }
		protected String _VendorClassID;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(VendorClass.vendorClassID), DescriptionField = typeof(VendorClass.descr), CacheGlobal = true)]
		[PXUIField(DisplayName = "Vendor Class")]
		public virtual String VendorClassID
		{
			get
			{
				return this._VendorClassID;
			}
			set
			{
				this._VendorClassID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[FinPeriodNonLockedSelector]
		[PXDefault]
		[PXUIField(DisplayName = GL.Messages.FinPeriod)]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion	
	}



	[TableAndChartDashboardType]
	public class APIntegrityCheck : PXGraph<APIntegrityCheck>
	{
		public PXFilter<APIntegrityCheckFilter> Filter;
		public PXCancel<APIntegrityCheckFilter> Cancel;
		[PXFilterable]
		public PXFilteredProcessing<Vendor, APIntegrityCheckFilter,
			Where<Match<Current<AccessInfo.userName>>>> APVendorList;
		public PXSelect<Vendor,
			Where<Vendor.vendorClassID, Equal<Current<APIntegrityCheckFilter.vendorClassID>>,
			And<Match<Current<AccessInfo.userName>>>>>
			APVendorList_ByVendorClassID;

		public APIntegrityCheck()
		{
			APSetup setup = APSetup.Current;

			APVendorList.SetProcessTooltip(Messages.RecalculateBalanceTooltip);
			APVendorList.SetProcessAllTooltip(Messages.RecalculateBalanceTooltip);
		}

		protected virtual void APIntegrityCheckFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			bool errorsOnForm = PXUIFieldAttribute.GetErrors(sender, null, PXErrorLevel.Error, PXErrorLevel.RowError).Count > 0;
			APVendorList.SetProcessEnabled(!errorsOnForm);
			APVendorList.SetProcessAllEnabled(!errorsOnForm);
			APVendorList.SuppressMerge = true;
			APVendorList.SuppressUpdate = true;

			APIntegrityCheckFilter filter = Filter.Current;

			APVendorList.SetProcessDelegate<APReleaseProcess>(
				delegate(APReleaseProcess re, Vendor vend)
				{
					re.Clear(PXClearOption.PreserveTimeStamp);
					re.IntegrityCheckProc(vend, filter.FinPeriodID);
					ReopenDocumentsHavingPendingApplications(re, vend, filter.FinPeriodID);
				}
			);

			//For perfomance recomended select not more than maxVendorCount vendors, 
			//because the operation is performed for a long time.
			const int maxVendorCount = 5;
			APVendorList.SetParametersDelegate(delegate(List<Vendor> list)
			{
				bool processing = true;
				if (PX.Common.PXContext.GetSlot<AUSchedule>() == null && list.Count > maxVendorCount)
				{
					WebDialogResult wdr = APVendorList.Ask(Messages.ContinueValidatingBalancesForMultipleVendors, MessageButtons.OKCancel);
					processing = wdr == WebDialogResult.OK;
				}
				return processing;
			});
		}

		public virtual IEnumerable apvendorlist()
		{
			if (Filter.Current != null && Filter.Current.VendorClassID != null)
			{
				return APVendorList_ByVendorClassID.Select();
			}
			return null;
		}

		public PXSetup<APSetup> APSetup;

		public PXAction<APIntegrityCheckFilter> viewVendor;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewVendor(PXAdapter adapter)
		{
			VendorMaint graph = CreateInstance<VendorMaint>();
			graph.BAccount.Current = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Current<Vendor.bAccountID>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, "View Vendor") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}


		private static void ReopenDocumentsHavingPendingApplications(PXGraph graph, Vendor vendor, string finPeriod)
		{
			PXUpdate<Set<APRegister.openDoc, True>,
				APRegister,
				Where<APRegister.openDoc, Equal<False>,
					And<APRegister.vendorID, Equal<Required<APRegister.vendorID>>,
					And<APRegister.tranPeriodID, GreaterEqual<Required<APRegister.tranPeriodID>>,
					And<Exists<Select<APAdjust, Where<
						APAdjust.released, Equal<False>,
						And<APAdjust.adjgDocType, Equal<APRegister.docType>,
						And<APAdjust.adjgRefNbr, Equal<APRegister.refNbr>>>
						>>>>>>>>
				.Update(graph, vendor.BAccountID, finPeriod);


			PXUpdate<Set<APRegister.status, APDocStatus.open>,
				APRegister,
				Where<APRegister.status, Equal<APDocStatus.closed>,
					And<APRegister.vendorID, Equal<Required<APRegister.vendorID>>,
					And<APRegister.tranPeriodID, GreaterEqual<Required<APRegister.tranPeriodID>>,
					And<APRegister.openDoc, Equal<True>>>>>>
				.Update(graph, vendor.BAccountID, finPeriod);
		}
	}
}
