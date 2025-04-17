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

namespace PX.Objects.AM
{
    /// <summary>
    /// Schedule Detail Interface
    /// </summary>
    public interface ISchdDetail
    {
        Int64? RecordID { get; set; }
        string ResourceID { get; set; }
        DateTime? SchdDate { get; set; }
        DateTime? StartTime { get; set; }
        DateTime? EndTime { get; set; }
        /// <summary>
        /// Required due to overnight times. We need to know what is morning of or still apart of an overnight shift (next day but still apart of the previous date)
        /// </summary>
        DateTime? OrderByDate { get; set; }
        int? RunTimeBase { get; set; }
        int? RunTime { get; set; }
        int? SchdBlocks { get; set; }
        bool? IsBreak { get; set; }
    }

    /// <summary>
    /// Schedule Detail Interface
    /// </summary>
    public interface ISchdDetail<T> : ISchdDetail
    {
        T Copy();
    }

    public interface ISchdReference
    {
        Guid? SchdKey { get; set; }
    }
}