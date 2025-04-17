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

namespace PX.Objects.FS 
{
    public class SkillMaint : PXGraph<SkillMaint, FSSkill>
    {
        [PXImport(typeof(FSSkill))]
        public PXSelect<FSSkill> SkillRecords;

		protected virtual void _(Events.RowDeleting<FSSkill> e)
		{
			if (e.Row == null)
				return;

			FSEmployeeSkill link = PXSelect<FSEmployeeSkill, Where<FSEmployeeSkill.skillID, Equal<Required<FSSkill.skillID>>>>.SelectWindowed(this, 0, 1, e.Row.SkillID);
			FSServiceSkill service = PXSelect<FSServiceSkill, Where<FSServiceSkill.skillID, Equal<Required<FSSkill.skillID>>>>.SelectWindowed(this, 0, 1, e.Row.SkillID);
			if (link != null || service != null)
			{
				throw new PXException(TX.Error.RecordIsReferencedAtEmployee);
			}
		}
	}
}
