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
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.EP;

namespace SP.Objects.CR
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CRActivityExt : PXCacheExtension<CRActivity>
	{
		#region Type
		public abstract class type : IBqlField { }
		protected string _Type;
		[PXDBString(5, IsFixed = true, IsUnicode = false)]
		[PXUIField(DisplayName = "Type", Required = true)]
		[PXSelector(typeof(EPActivityType.type), DescriptionField = typeof(EPActivityType.description))]
		[PXRestrictor(typeof(Where<EPActivityType.active, Equal<True>>), PX.Objects.CR.Messages.InactiveActivityType, typeof(EPActivityType.type))]
		[PXRestrictor(typeof(Where<EPActivityType.isInternal, NotEqual<True>>), PX.Objects.CR.Messages.ExternalActivityType, typeof(EPActivityType.type))]
		[PXDefault(typeof(Search<EPActivityType.type,
			Where<EPActivityType.isInternal, NotEqual<True>,
			And<EPActivityType.isDefault, Equal<True>,
			And<Current<CRActivity.classID>, Equal<CRActivityClass.activity>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String Type { get; set; }
		#endregion

		#region CommentType
		public abstract class commenttype : IBqlField { }

		[PXString(5, IsFixed = true, IsUnicode = false)]
		[PXUIField(DisplayName = "Type", Required = true, Visible = false)]
		[PXSelector(typeof(EPActivityType.type), DescriptionField = typeof(EPActivityType.description))]
		[PXRestrictor(typeof(Where<EPActivityType.active, Equal<True>>), PX.Objects.CR.Messages.InactiveActivityType, typeof(EPActivityType.type))]
		[PXRestrictor(typeof(Where<EPActivityType.isInternal, NotEqual<True>>), PX.Objects.CR.Messages.InternalActivityType, typeof(EPActivityType.type))]
		[PXDefault(typeof(Search<EPActivityType.type,
			Where<EPActivityType.isInternal, NotEqual<True>,
			And<EPActivityType.isDefault, Equal<True>,
			And<Current<CRActivity.classID>, Equal<CRActivityClass.activity>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string CommentType { get; set; }
		#endregion

        #region CommentSubject
        public abstract class commentSubject :IBqlField { }
		
        [PXString(255, InputMask = "", IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String CommentSubject { get; set; }
        #endregion

        #region CommentBody
        public abstract class commentBody : IBqlField { }

        [PXString(IsUnicode = true)]
        [PXUIField(DisplayName = "Activity Details")]
        public virtual String CommentBody { get; set; }
        #endregion

        #region StartDate
        public abstract class commentStartDate : PX.Data.IBqlField { }

        [PXDateAndTime]
        [PXUIField(DisplayName = "Start Date", Required = true, Visible = false)]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual DateTime? CommentStartDate { get; set; }
        #endregion
	}
}
