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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.PR.AUF
{
	public class AufFormatter
	{
		public AufFormatter()
		{
			_Ver = new VerRecord(AufConstants.AufVersionNumber);
		}

		public byte[] GenerateAufFile()
		{
			if (_Ver == null || Dat == null || Cmp == null)
			{
				throw new PXException(Messages.AatrixReportMissingAufInfo);
			}

			StringBuilder builder = new StringBuilder(_Ver.ToString());
			builder.Append(Dat.ToString());
			PimList?.ForEach(pim => builder.Append(pim.ToString()));
			EmpList?.SelectMany(emp => emp.EffList).Distinct(new EffIDComparer()).ToList().ForEach(eff => builder.Append(new FfdRecord(eff.FfdID).ToString()));
			builder.Append(Cmp.ToString());
			EmpList?.ForEach(emp => builder.Append(emp.ToString()));

			return Encoding.UTF8.GetBytes(builder.ToString());
		}

		private VerRecord _Ver;
		public DatRecord Dat { private get; set; }
		public List<PimRecord> PimList { private get; set; }
		public CmpRecord Cmp { private get; set; }
		public List<EmpRecord> EmpList { private get; set; }
	}
}
