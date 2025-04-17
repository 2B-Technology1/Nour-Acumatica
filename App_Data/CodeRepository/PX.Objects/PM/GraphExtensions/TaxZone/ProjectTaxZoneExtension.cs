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

namespace PX.Objects.PM.TaxZoneExtension
{
	public abstract class ProjectRevenueTaxZoneExtension<T> : PXGraphExtension<T>
		where T : PXGraph
	{
		#region Mappings

		protected abstract DocumentMapping GetDocumentMapping();

		public PXSelectExtension<Document> Document;

		#endregion

		protected virtual void _(Events.FieldUpdated<Document, Document.projectID> e)
		{
			SetDefaultShipToAddress(e.Cache, e.Row);
		}

		protected abstract void SetDefaultShipToAddress(PXCache sender, Document row);
	}

	public class Document : PXMappedCacheExtension
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
	}

	public class DocumentMapping : IBqlMapping
	{
		public Type Extension => typeof(Document);

		protected Type _table;
		public Type Table => _table;
		public DocumentMapping(Type table)
		{
			_table = table;
		}

		public Type ProjectID = typeof(Document.projectID);
	}
}
