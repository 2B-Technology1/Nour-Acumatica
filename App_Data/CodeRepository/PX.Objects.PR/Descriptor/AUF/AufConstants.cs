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

using System.Collections.Generic;

namespace PX.Objects.PR.AUF
{
	public enum AufRecordType
	{
		Ver,
		Dat,
		Cmp,
		Emp,
		Cji,
		Gto,
		Gen,
		Ejw,
		Pim,
		Csi,
		Cli,
		Esi,
		Eli,
		Csp,
		Clp,
		Cfb,
		Efb,
		Ale,
		Agg,
		Dge,
		Ecv,
		Eci,
		Ffd,
		Eff
	}

	public static class AufConstants
	{
		public static Dictionary<AufRecordType, string> RecordNames = new Dictionary<AufRecordType, string>()
		{
			{ AufRecordType.Ver, "VER" },
			{ AufRecordType.Dat, "DAT" },
			{ AufRecordType.Cmp, "CMP" },
			{ AufRecordType.Emp, "EMP" },
			{ AufRecordType.Cji, "CJI" },
			{ AufRecordType.Gto, "GTO" },
			{ AufRecordType.Gen, "GEN" },
			{ AufRecordType.Ejw, "EJW" },
			{ AufRecordType.Pim, "PIM" },
			{ AufRecordType.Csi, "CSI" },
			{ AufRecordType.Cli, "CLI" },
			{ AufRecordType.Esi, "ESI" },
			{ AufRecordType.Eli, "ELI" },
			{ AufRecordType.Csp, "CSP" },
			{ AufRecordType.Clp, "CLP" },
			{ AufRecordType.Cfb, "CFB" },
			{ AufRecordType.Efb, "EFB" },
			{ AufRecordType.Ale, "ALE" },
			{ AufRecordType.Agg, "AGG" },
			{ AufRecordType.Dge, "DGE" },
			{ AufRecordType.Ecv, "ECV" },
			{ AufRecordType.Eci, "ECI" },
			{ AufRecordType.Ffd, "FFD" },
			{ AufRecordType.Eff, "EFF" },
		};

		public const object UnusedField = null;
		public const object ManualInput = null;

		public const char DefaultDelimiterCharacter = '\t';
		public const string DefaultEndline = "\r\n";
		public const string SelectedBox = "X";
		public const string NotSelectedBox = "N";

		public const int GenericLocalTaxMapping = 2000;

		#region VER records
		public const string VendorName = "Acumatica";
		public const string AufVersionNumber = "2.76";
		public const string SourceProgram = "Acumatica The Cloud ERP";
		#endregion
	}
}
