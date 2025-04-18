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
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using SP.Objects.SP;

namespace SP.Objects.CR.DAC
{
    // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
    [System.SerializableAttribute()]
    [CRCacheIndependentPrimaryGraphList(new Type[]{
        typeof(PX.Objects.CR.BusinessAccountMaint),
		typeof(PX.Objects.EP.EmployeeMaint),
		typeof(PX.Objects.AP.VendorMaint),
		typeof(PX.Objects.AP.VendorMaint),
		typeof(PX.Objects.AR.CustomerMaint),
		typeof(PX.Objects.AR.CustomerMaint),
		typeof(PX.Objects.AP.VendorMaint),
		typeof(PX.Objects.AR.CustomerMaint),
		typeof(PX.Objects.CR.BusinessAccountMaint)},
        new Type[]{
            typeof(Select<PX.Objects.CR.BAccount, Where<PX.Objects.CR.BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
                    And<Current<BAccount.viewInCrm>, Equal<True>>>>),
			typeof(Select<PX.Objects.EP.EPEmployee, Where<PX.Objects.EP.EPEmployee.bAccountID, Equal<Current<BAccount.bAccountID>>>>),
			typeof(Select<PX.Objects.AP.VendorR, Where<PX.Objects.AP.VendorR.bAccountID, Equal<Current<BAccount.bAccountID>>>>), 
			typeof(Select<PX.Objects.AP.Vendor, Where<PX.Objects.AP.Vendor.bAccountID, Equal<Current<BAccountR.bAccountID>>>>), 
			typeof(Select<PX.Objects.AR.Customer, Where<PX.Objects.AR.Customer.bAccountID, Equal<Current<BAccount.bAccountID>>>>),
			typeof(Select<PX.Objects.AR.Customer, Where<PX.Objects.AR.Customer.bAccountID, Equal<Current<BAccountR.bAccountID>>>>),
			typeof(Where<PX.Objects.CR.BAccountR.bAccountID, Less<Zero>,
					And<BAccountR.type, Equal<BAccountType.vendorType>>>), 
			typeof(Where<PX.Objects.CR.BAccountR.bAccountID, Less<Zero>,
					And<BAccountR.type, Equal<BAccountType.customerType>>>), 
			typeof(Select<PX.Objects.CR.BAccount, 
				Where<PX.Objects.CR.BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>, 
					Or<Current<BAccount.bAccountID>, Less<Zero>>>>)
		})]
    [PXCacheName(PX.Objects.CR.Messages.BusinessAccount)]
    [CREmailContactsView(typeof(Select2<Contact,
        LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
        Where<Contact.bAccountID, Equal<Optional<BAccount.bAccountID>>,
                Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>))]
    [PXEMailSource]//NOTE: for assignment map
	public class BAccountExt : PXCacheExtension<BAccount>
	{
        #region ClassID
        public abstract class classID : PX.Data.IBqlField { }

        [PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXMassMergableField]
        [PXMassUpdatableField]
        [PXSelector(typeof(Search<CRCustomerClass.cRCustomerClassID,
                Where<CRCustomerClass.isInternal, Equal<False>>>),
                DescriptionField = typeof(CRCustomerClass.description), CacheGlobal = true)]
        [PXUIField(DisplayName = "Class ID")]
        public virtual String ClassID { get; set; }
        #endregion 
	}
}
