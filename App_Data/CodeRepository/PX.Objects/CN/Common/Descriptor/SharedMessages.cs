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

using PX.Common;

namespace PX.Objects.CN.Common.Descriptor
{
    [PXLocalizable]
    public static class SharedMessages
    {
	    public const string Warning = "Warning";

	    public const string DefaultValue = "Default Value";
	    public const string DescriptionFieldPostfix = "_description";

	    public const string FieldIsEmpty = "Error: '{0}' cannot be empty.";
	    public const string CannotBeFound = "Error: '{0}' cannot be found in the system.";
	    public const string RequiredAttributesAreEmpty = "There are empty required attributes: {0}";
		public const string CorrectProformaInvoice =  "Correct Pro Forma Invoice";
	}
}