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
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.Descriptor.Attributes;

namespace PX.Objects.PJ.DrawingLogs.PJ.DAC
{
    [Serializable]
    [PXCacheName(CacheNames.DrawingLogDiscipline)]
    public class DrawingLogDiscipline : IBqlTable, ISortOrder
    {
        [PXDBIdentity(IsKey = true)]
        public virtual int? DrawingLogDisciplineId
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [Unique(ErrorMessage = DrawingLogMessages.DisciplineNameUniqueConstraint)]
        [PXUIField(DisplayName = "Discipline", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Name
        {
            get;
            set;
        }

        [PXDBBool]
        public virtual bool? IsDefault
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Active")]
        public virtual bool? IsActive
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "Sort Order", Visible = false, Enabled = false)]
        public int? SortOrder
        {
            get;
            set;
        }

        [PXInt]
        [PXFormula(typeof(drawingLogDisciplineId))]
        [PXUIField(DisplayName = "Line Nbr", Visible = false, Enabled = false)]
        public int? LineNbr
        {
            get;
            set;
        }

        public abstract class drawingLogDisciplineId : BqlInt.Field<drawingLogDisciplineId>
        {
        }

        public abstract class name : BqlString.Field<name>
        {
        }

        public abstract class isDefault : BqlBool.Field<isDefault>
        {
        }

        public abstract class isActive : BqlBool.Field<isActive>
        {
        }

        public abstract class sortOrder : BqlInt.Field<sortOrder>
        {
        }

        public abstract class lineNbr : BqlInt.Field<lineNbr>
        {
        }
    }
}
