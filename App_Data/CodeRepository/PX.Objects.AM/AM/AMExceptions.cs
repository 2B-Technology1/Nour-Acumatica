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
using System.Runtime.Serialization;
using PX.Common;

namespace PX.Objects.AM
{
    /// <summary>
    /// Indicates work calendar is invalid
    /// </summary>
    [Serializable]
    public class InvalidWorkCalendarException : PX.Data.PXException
    {
        public InvalidWorkCalendarException()
        {
            this._Message = Messages.GetLocal(Messages.InvalidWorkCalendar, string.Empty);
        }

        public InvalidWorkCalendarException(string calendarID)
        {
            this._Message = Messages.GetLocal(Messages.InvalidWorkCalendar, calendarID);
        }

        public InvalidWorkCalendarException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// Indicates BOM is invalid
    /// </summary>
    [Serializable]
    public class InvalidBOMException : PX.Data.PXException
    {
        public InvalidBOMException()
        {
            this._Message = Messages.GetLocal(Messages.InvalidBOM);
        }

        public InvalidBOMException(string bomID)
        {
            this._Message = $"{Messages.GetLocal(Messages.InvalidBOM)} '{bomID}'";
        }

        public InvalidBOMException(string bomID, string message)
        {
            _Message = Messages.GetLocal(Messages.InvalidBOM);

            if (!string.IsNullOrWhiteSpace(bomID))
            {
                _Message = $"{_Message} '{bomID}'";
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                _Message = $"{_Message}. {message}";
            }
        }

        public InvalidBOMException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public sealed class BOMSetupNotEnteredException : PX.Data.PXSetupNotEnteredException
    {
        public BOMSetupNotEnteredException() : this(Messages.GetLocal(Messages.SetupNotEntered))
        {
        }

        public BOMSetupNotEnteredException(string format) : base(format, typeof(AMBSetup), Messages.GetLocal(Messages.BOMSetup))
        {
        }

        public BOMSetupNotEnteredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public sealed class ProductionSetupNotEnteredException : PX.Data.PXSetupNotEnteredException
    {
        public ProductionSetupNotEnteredException() : this(Messages.GetLocal(Messages.SetupNotEntered))
        {
        }

        public ProductionSetupNotEnteredException(string format) : base(format, typeof(AMPSetup), Messages.GetLocal(Messages.ProductionSetup))
        {
        }

        public ProductionSetupNotEnteredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public sealed class EstimatingSetupNotEnteredException : PX.Data.PXSetupNotEnteredException
    {
        public EstimatingSetupNotEnteredException() : this(Messages.GetLocal(Messages.SetupNotEntered))
        {
        }

        public EstimatingSetupNotEnteredException(string format) : base(format, typeof(AMEstimateSetup), Messages.GetLocal(Messages.EstimateSetup))
        {
        }

        public EstimatingSetupNotEnteredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class MRPRegenException : PX.Data.PXException
    {
        public MRPRegenException(string message) : base(message)
        {
        }

        public MRPRegenException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public MRPRegenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class SchedulingOrderDatesException : PX.Data.PXException
    {
        public readonly DateTime LastAttemptDateTime;
        public readonly DateTime NextAttemptDateTime;
        public readonly int? PlanDateAdjustDays;

        public SchedulingOrderDatesException(DateTime lastAttemptDateTime, DateTime nextAttemptDateTime) : base(Messages.GetLocal(Messages.ErrorSchedulingOrderDates))
        {
            LastAttemptDateTime = lastAttemptDateTime;
            NextAttemptDateTime = nextAttemptDateTime;
        }

        public SchedulingOrderDatesException(DateTime lastAttemptDateTime, DateTime nextAttemptDateTime, string message) : base(message)
        {
            LastAttemptDateTime = lastAttemptDateTime;
            NextAttemptDateTime = nextAttemptDateTime;
        }

        public SchedulingOrderDatesException(DateTime lastAttemptDateTime, DateTime nextAttemptDateTime, string message, Exception innerException) : base(message, innerException)
        {
            LastAttemptDateTime = lastAttemptDateTime;
            NextAttemptDateTime = nextAttemptDateTime;
        }

        public SchedulingOrderDatesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            PXReflectionSerializer.RestoreObjectProps(this, info);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PXReflectionSerializer.GetObjectData(this, info);
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class MaterialScheduleShortageException : PX.Data.PXException
    {
        public MaterialScheduleShortageException(string message) : base(message)
        {
        }

        public MaterialScheduleShortageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public MaterialScheduleShortageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class MaxLowLevelException : PX.Data.PXException
    {
        public int Level;

        public MaxLowLevelException() : this(Messages.GetLocal(Messages.MaxLevelsReached))
        {
        }

        public MaxLowLevelException(string message) : base(message)
        {
        }

        public MaxLowLevelException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            PXReflectionSerializer.RestoreObjectProps(this, info);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PXReflectionSerializer.GetObjectData(this, info);
            base.GetObjectData(info, context);
        }

        public MaxLowLevelException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public MaxLowLevelException(string format, params object[] args) : base(format, args)
        {
        }

        public MaxLowLevelException(Exception innerException, string format, params object[] args) : base(innerException, format, args)
        {
        }
    }

    /// <summary>
    /// Used when checking transaction for material, move qty, etc.
    /// </summary>
    [Serializable]
    public class AMTransactionFailedCheckException : PX.Data.PXException
    {
        /// <summary>
        /// Is the exception based on a warning (which might allow for transactions to process)
        /// </summary>
        public bool IsWarning;

        public AMTransactionFailedCheckException(string message) : base(message)
        {
        }

        public AMTransactionFailedCheckException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AMTransactionFailedCheckException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            PXReflectionSerializer.RestoreObjectProps(this, info);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PXReflectionSerializer.GetObjectData(this, info);
            base.GetObjectData(info, context);
        }
    }
}