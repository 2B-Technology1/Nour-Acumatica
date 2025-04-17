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

namespace PX.Objects.CR
{
	public class CRRoleTypeList
	{
		public class FullListAttribute : PXStringListAttribute
		{
			public FullListAttribute() : base(new[]
				{
					BusinessUser,
					Child,
					Derivative,
					DecisionMaker,
					Evaluator,
					Parent,
					RelatedEntity,
					Referrer,
					SupportEngineer,
					Source,
					Supervisor,
					TechnicalExpert,
				},
				new[]
				{
					Messages.Role_BusinessUser,
					Messages.Role_Child,
					Messages.Role_Derivative,
					Messages.Role_DecisionMaker,
					Messages.Role_Evaluator,
					Messages.Role_Parent,
					Messages.Role_RelatedEntity,
					Messages.Role_Referrer,
					Messages.Role_SupportEngineer,
					Messages.Role_Source,
					Messages.Role_Supervisor,
					Messages.Role_TechnicalExpert,
				}) { }
		}

		public class ShortListAttribute : PXStringListAttribute
		{
			public ShortListAttribute() : base(new[]
				{
					Child,
					Derivative,
					Parent,
					RelatedEntity,
					Source,
				},
				new[]
				{
					Messages.Role_Child,
					Messages.Role_Derivative,
					Messages.Role_Parent,
					Messages.Role_RelatedEntity,
					Messages.Role_Source,
				})
			{ }
		}

		public const string BusinessUser = "BU";
		public const string DecisionMaker = "DM";
		public const string Evaluator = "EV";
		public const string SupportEngineer = "SE";
		public const string Supervisor = "SV";
		public const string TechnicalExpert = "TE";
		public const string RelatedEntity = "RE";
		public const string Referrer = "RF";
		public const string Source = "SR";
		public const string Derivative = "DE";
		public const string Parent = "PR";
		public const string Child = "CH";
		public const string Licensee = "AL";

		#region BQL constant

		public sealed class referrer : PX.Data.BQL.BqlString.Constant<referrer>
		{
			public referrer() : base(Referrer) { }
		}

		public sealed class supervisor : PX.Data.BQL.BqlString.Constant<supervisor>
		{
			public supervisor() : base(Supervisor) { }
		}

		public sealed class businessUser : PX.Data.BQL.BqlString.Constant<businessUser>
		{
			public businessUser() : base(BusinessUser) { }
		}

		public sealed class decisionMaker : PX.Data.BQL.BqlString.Constant<decisionMaker>
		{
			public decisionMaker() : base(DecisionMaker) { }
		}

		public sealed class relatedEntity : PX.Data.BQL.BqlString.Constant<relatedEntity>
		{
			public relatedEntity() : base(RelatedEntity) { }
		}

		public sealed class technicalExpert : PX.Data.BQL.BqlString.Constant<technicalExpert>
		{
			public technicalExpert() : base(TechnicalExpert) { }
		}

		public sealed class supportEngineer : PX.Data.BQL.BqlString.Constant<supportEngineer>
		{
			public supportEngineer() : base(SupportEngineer) { }
		}

		public sealed class evaluator : PX.Data.BQL.BqlString.Constant<evaluator>
		{
			public evaluator() : base(Evaluator) { }
		}

		public sealed class licensee : PX.Data.BQL.BqlString.Constant<licensee>
		{
			public licensee() : base(Licensee) { }
		}

		public sealed class source : PX.Data.BQL.BqlString.Constant<source>
		{
			public source() : base(Source) { }
		}

		public sealed class derivative : PX.Data.BQL.BqlString.Constant<derivative>
		{
			public derivative() : base(Derivative) { }
		}

		public sealed class parent : PX.Data.BQL.BqlString.Constant<parent>
		{
			public parent() : base(Parent) { }
		}

		public sealed class child : PX.Data.BQL.BqlString.Constant<child>
		{
			public child() : base(Child) { }
		}

		#endregion
	}
}
