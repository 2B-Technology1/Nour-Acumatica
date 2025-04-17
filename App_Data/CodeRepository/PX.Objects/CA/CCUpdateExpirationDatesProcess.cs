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

using PX.CCProcessingBase;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing;
using System;
using PX.Objects.AR.CCPaymentProcessing.Helpers;

namespace PX.Objects.CA
{

	[Serializable]
	public class CCUpdateExpirationDatesProcess : PXGraph<CCUpdateExpirationDatesProcess>
	{

		public partial class CPMFilter : IBqlTable
		{
			#region ProcessingCenterID
			public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }
			protected String _ProcessingCenterID;
			[PXString(10, IsUnicode = true, IsKey = true)]
			[PXSelector(typeof(Search<CCProcessingCenter.processingCenterID>))]
			[PXUIField(DisplayName = "Proc. Center ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual String ProcessingCenterID
			{
				get
				{
					return this._ProcessingCenterID;
				}
				set
				{
					this._ProcessingCenterID = value;
				}
			}
			#endregion
		}

		public PXCancel<CPMFilter> Cancel;
		public PXFilter<CPMFilter> Filter;
		public PXFilteredProcessing<CustomerPaymentMethod, CPMFilter, Where<CustomerPaymentMethod.cCProcessingCenterID, 
			Equal<Current<CPMFilter.processingCenterID>>, And<CustomerPaymentMethod.expirationDate, IsNull>>> CustomerPaymentMethods;

		public CCUpdateExpirationDatesProcess()
		{
			this.CustomerPaymentMethods.SetProcessDelegate(cpm => DoDateUpdateProcess(cpm));
		}

		public static void DoDateUpdateProcess(CustomerPaymentMethod cpm)
		{
			CustomerPaymentMethodMaint cpmGraph = PXGraph.CreateInstance<CustomerPaymentMethodMaint>();
			cpmGraph.CustomerPaymentMethod.Current = cpm;
			var graph = PXGraph.CreateInstance<CCCustomerInformationManagerGraph>();
			ICCPaymentProfileAdapter paymentProfile = new GenericCCPaymentProfileAdapter<CustomerPaymentMethod>(cpmGraph.CustomerPaymentMethod);
			ICCPaymentProfileDetailAdapter profileDetail = new GenericCCPaymentProfileDetailAdapter<CustomerPaymentMethodDetail, 
				PaymentMethodDetail>(cpmGraph.DetailsAll, cpmGraph.PMDetails);
			graph.GetPaymentProfile(cpmGraph, paymentProfile, profileDetail);
			if (cpmGraph.CustomerPaymentMethod.Current.ExpirationDate == null)
			{
				throw new PXException(Messages.ExpDateRetrievalFailed);
			}
			cpmGraph.Save.Press();
		}

	}
}
