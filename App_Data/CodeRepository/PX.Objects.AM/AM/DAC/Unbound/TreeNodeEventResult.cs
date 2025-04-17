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
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
	[Serializable]
	[PXHidden]
	[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class TreeNodeEventResult : IBqlTable
	{
		internal string DebuggerDisplay => $"ID = {ID}, Text = {Text}, Parent = {ParentID}, OldParentID = {OldParentID}, PrevNodeID = {PrevNodeID}";

		#region ID
		public abstract class iD : PX.Data.BQL.BqlString.Field<iD> { }

		[PXString]
		public virtual string ID { get; set; }

		#endregion
		#region ParentID
		public abstract class parentID : PX.Data.BQL.BqlString.Field<parentID> { }

		[PXString]
		public virtual string ParentID { get; set; }

		#endregion
		#region OldParentID
		public abstract class oldParentID : PX.Data.BQL.BqlString.Field<oldParentID> { }

		[PXString]
		public virtual string OldParentID { get; set; }

		#endregion
		#region Text
		public abstract class text : PX.Data.BQL.BqlString.Field<text> { }

		[PXString]
		public virtual string Text { get; set; }

		#endregion
		#region PrevNodeID
		public abstract class prevNodeID : PX.Data.BQL.BqlString.Field<prevNodeID> { }

		[PXString]
		public virtual string PrevNodeID { get; set; }

		#endregion
	}

	[Serializable]
	[PXHidden]
	public class TreeNodeSelected : TreeNodeEventResult
	{
		#region BOMID (key)
		public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

		[BomID(IsKey = true, Enabled = false)]
		public virtual string BOMID { get; set; }

		#endregion
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

		[RevisionIDField(Enabled = false)]
		public virtual string RevisionID { get; set; }

		#endregion
		#region OperationID
		public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

		[OperationIDField(Enabled = false)]
		public virtual int? OperationID { get; set; }

		#endregion
		#region OperationCD
		public abstract class operationCD : PX.Data.BQL.BqlString.Field<operationCD> { }

		[OperationCDField]
		public virtual string OperationCD { get; set; }

		#endregion
		#region LineID
		public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Line Nbr.", Enabled = false)]
		public virtual Int32? LineID { get; set; }

		#endregion
		#region IsOperation
		public abstract class isOperation : PX.Data.BQL.BqlBool.Field<isOperation> { }

		[PXBool]
		[PXUIField]
		public virtual bool? IsOperation { get; set; }
		#endregion
		#region OperationDescription
		public abstract class operationDescription : PX.Data.BQL.BqlString.Field<operationDescription> { }

		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Operation Description")]
		public virtual String OperationDescription { get; set; }
		#endregion
		#region WcID
		public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

		protected String _WcID;
		[WorkCenterIDField]
		[PXSelector(typeof(Search<AMWC.wcID>), ValidateValue = false)]
		public virtual String WcID { get; set; }
		#endregion

		#region IsSubassembly
		public abstract class isSubassembly : PX.Data.BQL.BqlBool.Field<isSubassembly> { }

		[PXBool]
		[PXUIField]
		public virtual bool? IsSubassembly { get; set; }
		#endregion
		#region SubassemblyBOMID
		public abstract class subassemblyBOMID : PX.Data.BQL.BqlString.Field<subassemblyBOMID> { }

		[BomID(IsKey = true, Enabled = false)]
		public virtual string SubassemblyBOMID { get; set; }

		#endregion
		#region SubassemblyRevisionID
		public abstract class subassemblyRevisionID : PX.Data.BQL.BqlString.Field<subassemblyRevisionID> { }

		[RevisionIDField(Enabled = false)]
		public virtual string SubassemblyRevisionID { get; set; }

		#endregion
	}
}
