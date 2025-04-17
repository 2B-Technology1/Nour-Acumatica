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
using System;
using System.Runtime.Serialization;

namespace PX.Objects.IN
{
	[Serializable]
	public class AlternatieIDNotUniqueException : PXSetPropertyException
	{
		public AlternatieIDNotUniqueException(string newAltID) : base(Messages.AlternatieIDNotUnique, PXErrorLevel.Error, newAltID)
		{
		}

		public AlternatieIDNotUniqueException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public AlternatieIDNotUniqueException(string alternateID, string inventoryCD, string uom) : base(Messages.AlternateIDExistsUOMIsDifferent, PXErrorLevel.Error, alternateID, inventoryCD, uom)
		{
		}
	}
}
