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

namespace PX.Objects.FS
{
    [Serializable]
    public class FSActivationContractFilter : PX.Data.IBqlTable
    {
        #region ActivationDate
        public abstract class activationDate : PX.Data.BQL.BqlDateTime.Field<activationDate> { }

        [PXDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Activation Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ActivationDate { get; set; }
        #endregion
    }

    [Serializable]
    public class FSTerminateContractFilter : PX.Data.IBqlTable
    {
        #region CancelationDate
        public abstract class cancelationDate : PX.Data.BQL.BqlDateTime.Field<cancelationDate> { }

        [PXDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Cancellation Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? CancelationDate { get; set; }
        #endregion
    }

    [Serializable]
    public class FSSuspendContractFilter : PX.Data.IBqlTable
    {
        #region SuspensionDate
        public abstract class suspensionDate : PX.Data.BQL.BqlDateTime.Field<suspensionDate> { }

        [PXDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Suspension Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? SuspensionDate { get; set; }
        #endregion
    }

	[Serializable]
	public class FSCopyContractFilter : PX.Data.IBqlTable
	{
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

		[PXDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? StartDate { get; set; }
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty to be documented later
		[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault(typeof(FSServiceContract.refNbr))]
		[PXUIField(DisplayName = "Service Contract ID", Visible = true, Enabled = true)]
		public virtual string RefNbr { get; set; }
		#endregion
	}

	[Serializable]
    public class ActiveSchedule : FSSchedule
    {
        #region RefNbr
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXSelector(typeof(Search<FSSchedule.refNbr>))]
        public override string RefNbr { get; set; }
        #endregion   
        #region RecurrenceDescription
        [PXDBString(int.MaxValue, IsUnicode = false)]
        [PXUIField(DisplayName = "Recurrence Description", Enabled = false)]
        public override string RecurrenceDescription { get; set; }
        #endregion
        #region ChangeRecurrence
        public abstract class changeRecurrence : PX.Data.BQL.BqlBool.Field<changeRecurrence> { }

        [PXBool]
        [PXUIField(DisplayName = "Change Recurrence")]
        public virtual bool? ChangeRecurrence { get; set; }
        #endregion
        #region EffectiveRecurrenceStartDate
        public abstract class effectiveRecurrenceStartDate : PX.Data.BQL.BqlDateTime.Field<effectiveRecurrenceStartDate> { }

        [PXDate]
        [PXUIField(DisplayName = "Effective Recurrence Start Date", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
        [PXUIEnabled(typeof(Where<changeRecurrence, Equal<True>>))]
        public virtual DateTime? EffectiveRecurrenceStartDate { get; set; }
        #endregion
        #region NextExecution
        public abstract class nextExecution : PX.Data.BQL.BqlDateTime.Field<nextExecution> { }

        [PXDate]
        [PXUIField(DisplayName = "Next Execution", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual DateTime? NextExecution { get; set; }
        #endregion

        public ActiveSchedule()
        {
        }
    }
}
