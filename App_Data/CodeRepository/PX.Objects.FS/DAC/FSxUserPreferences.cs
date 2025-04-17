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
using PX.Objects.CS;
using PX.SM;
using System;

namespace PX.Objects.FS
{
    public class FSxUserPreferences : PXCacheExtension<UserPreferences>
    {
        #region DfltBranchLocationID
        public abstract class dfltBranchLocationID : PX.Data.BQL.BqlInt.Field<dfltBranchLocationID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Default Branch Location", FieldClass = "SERVICEMANAGEMENT")]
        [PXSelector(typeof(Search<FSBranchLocation.branchLocationID, Where<FSBranchLocation.branchID, Equal<Current<UserPreferences.defBranchID>>>>), SubstituteKey = typeof(FSBranchLocation.branchLocationCD), DescriptionField = typeof(FSBranchLocation.descr))]
        [PXFormula(typeof(Default<UserPreferences.defBranchID>))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? DfltBranchLocationID { get; set; }
        #endregion
        #region DfltSrvOrdType
        public abstract class dfltSrvOrdType : PX.Data.BQL.BqlString.Field<dfltSrvOrdType> { }
        [PXDBString(4, IsUnicode = true, IsFixed = true)]
        [PXUIField(DisplayName = "Default Service Order Type", FieldClass = "SERVICEMANAGEMENT")]
        [FSSelectorSrvOrdTypeNOTQuote]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string DfltSrvOrdType { get; set; }
        #endregion
        #region AskForSrvOrdTypeInCalendars
        public abstract class askForSrvOrdTypeInCalendars : PX.Data.BQL.BqlBool.Field<askForSrvOrdTypeInCalendars> { }
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Select Service Order Type on Creation from Calendars", FieldClass = "SERVICEMANAGEMENT")]
        public virtual bool? AskForSrvOrdTypeInCalendars { get; set; }
		#endregion
		#region TrackLocation
		public abstract class trackLocation : PX.Data.IBqlField { }
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Track Location")]
        public virtual bool? TrackLocation { get; set; }
		#endregion
		#region Interval
		public abstract class interval : PX.Data.IBqlField { }
        [PXDBShort(MinValue = 1)]
        [PXDefault((short)5, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Tracking Frequency", Enabled = false)]
        public virtual Int16? Interval { get; set; }
		#endregion
		#region Distance
		public abstract class distance : PX.Data.IBqlField { }
        [PXDBShort(MinValue = 1)]
        [PXDefault((short)250, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Distance Frequency", Enabled = false)]
        public virtual Int16? Distance { get; set; }
        #endregion
    }
}
