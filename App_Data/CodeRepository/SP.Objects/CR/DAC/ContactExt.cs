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

using PX.Data;
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using System;
using PX.Objects.SP.DAC;

namespace SP.Objects.CR
{
    // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
    [Serializable]
    [CRContactCacheName("Lead/Contact")]
    [PXEMailSource]
	[CRPrimaryGraphRestricted(
		new[]{
			typeof(ProductContactMaint)
		},
		new[]{
			typeof(Select<
				Contact,
				Where<
					Contact.contactID, Equal<Current<Contact.contactID>>,
					And<MatchWithBAccountNotNull<Contact.bAccountID>>>>)
		})]
	public class ContactExt : PXCacheExtension<Contact>
    {
        #region ClassID
        public abstract class classID : PX.Data.IBqlField { }

        [PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXMassMergableField]
        [PXMassUpdatableField]
        [PXDefault(typeof(PortalSetup.defaultContactClassID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<CRContactClass.classID,
                Where<CRContactClass.isInternal, Equal<False>>>),
                DescriptionField = typeof(CRContactClass.description), CacheGlobal = true)]
        [PXUIField(DisplayName = "Class ID")]
        public virtual String ClassID { get; set; }
        #endregion

        #region FirstName
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "First Name")]
        [PXMassMergableField]
        public virtual string FirstName { get; set; }
        #endregion


        #region HasRigths
        public abstract class hasRigths : PX.Data.IBqlField { }
        [PXBool()]
        [PXUIField(Visible = false)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? HasRigths { get; set; }
        #endregion        
    }
}
