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
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using System;

namespace PX.Objects.FS
{
    public class FSxAPTran : PXCacheExtension<APTran>, IFSRelatedDoc
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
        [PXDBString(4, IsFixed = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Service Order Type", FieldClass = FSSetup.ServiceManagementFieldClass)]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region AppointmentRefNbr
        public abstract class appointmentRefNbr : PX.Data.BQL.BqlString.Field<appointmentRefNbr> { }
		[PXDBString(20, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Appointment Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = true, FieldClass = FSSetup.ServiceManagementFieldClass)]
        public virtual string AppointmentRefNbr { get; set; }
        #endregion
        #region AppointmentLineNbr
        public abstract class appointmentLineNbr : PX.Data.BQL.BqlInt.Field<appointmentLineNbr> { }
        [PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Appointment Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, FieldClass = FSSetup.ServiceManagementFieldClass)]
        public virtual Int32? AppointmentLineNbr { get; set; }
        #endregion
        #region ServiceOrderRefNbr
        public abstract class serviceOrderRefNbr : PX.Data.BQL.BqlString.Field<serviceOrderRefNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Service Order Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = true, FieldClass = FSSetup.ServiceManagementFieldClass)]
        public virtual string ServiceOrderRefNbr { get; set; }
        #endregion
        #region ServiceOrderLineNbr
        public abstract class serviceOrderLineNbr : PX.Data.BQL.BqlInt.Field<serviceOrderLineNbr> { }
        [PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Service Order Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, FieldClass = FSSetup.ServiceManagementFieldClass)]
        public virtual Int32? ServiceOrderLineNbr { get; set; }
        #endregion
        #region RelatedEntityType
        public abstract class relatedEntityType : PX.Data.BQL.BqlString.Field<relatedEntityType> { }
        [PXDBString(40)]
        [PXDefault(typeof(CreateAPFilter.relatedEntityType), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXStringList(new string[] { ID.FSEntityType.ServiceOrder, ID.FSEntityType.Appointment }, new string[] { TX.TableName.SERVICE_ORDER, TX.TableName.APPOINTMENT })]
        [PXUIField(DisplayName = "Related Svc. Doc. Type", Visible = false)]
        public virtual string RelatedEntityType { get; set; }
        #endregion
        #region RelatedDocNoteID
        public abstract class relatedDocNoteID : PX.Data.BQL.BqlGuid.Field<relatedDocNoteID> { }
        [FSEntityIDAPInvoiceSelector(typeof(relatedEntityType))]
        [PXDBGuid()]
        [PXDefault(typeof(CreateAPFilter.relatedDocNoteID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Related Svc. Doc. Nbr.", Visible = false)]
        [PXFormula(typeof(Default<FSxAPTran.relatedEntityType>))]
        public virtual Guid? RelatedDocNoteID { get; set; }
        #endregion

        #region Mem_PreviousPostID
        public abstract class mem_PreviousPostID : PX.Data.BQL.BqlInt.Field<mem_PreviousPostID> { }
        [PXInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? Mem_PreviousPostID { get; set; }
        #endregion
        #region Mem_TableSource
        public abstract class mem_TableSource : PX.Data.BQL.BqlString.Field<mem_TableSource> { }
        [PXString]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string Mem_TableSource { get; set; }
		#endregion

		#region ServiceContractPeriodID
		public abstract class serviceContractPeriodID : PX.Data.BQL.BqlInt.Field<serviceContractPeriodID> { }
		[PXInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public int? ServiceContractPeriodID
        {
            get
            {
                return null;
            }
        }
		#endregion
		#region ServiceContractRefNbr
		public abstract class serviceContractRefNbr : PX.Data.BQL.BqlString.Field<serviceContractRefNbr> { }
		[PXString]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public string ServiceContractRefNbr
        {
            get 
            {
                return string.Empty;
            }
        }
        #endregion
        #region IsDocBilledOrClosed
        public abstract class isDocBilledOrClosed : PX.Data.BQL.BqlBool.Field<isDocBilledOrClosed> { }
        [PXBool]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? IsDocBilledOrClosed { get; set; }
        #endregion
    }
}
