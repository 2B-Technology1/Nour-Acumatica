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
using System.Linq;
using PX.Data;

namespace PX.Objects.AM
{
	public static class PXTreeHelper
	{
		internal static string FormatCommandArguments(string s, string xMsg = "")
		{
			if(string.IsNullOrWhiteSpace(s))
			{
				return null;
			}

			var sb = new System.Text.StringBuilder();
			var cntr = 0;
			if(!string.IsNullOrWhiteSpace(xMsg))
			{
				sb.AppendLine(xMsg);
			}
			foreach (var item in ParseCommandArgument(s))
			{
				sb.AppendLine($"[{(++cntr).ToString().PadLeft(2,'0')}] {item}");
			}
			return sb.ToString();
		}

		internal static string FormatCommandArguments(List<TreeNodeEventResult> treeNodeResults, string xMsg = "")
		{
			if(treeNodeResults == null)
			{
				return null;
			}

			var sb = new System.Text.StringBuilder();
			var cntr = 0;
			if(!string.IsNullOrWhiteSpace(xMsg))
			{
				sb.AppendLine(xMsg);
			}
			foreach (var item in treeNodeResults)
			{
				sb.AppendLine($"[{(++cntr).ToString().PadLeft(2,'0')}] {item.DebuggerDisplay}");
			}
			return sb.ToString();
		}

		public static List<TreeNodeEventResult> CommandAsTreeNodeEventResults(string command)
		{
			var rtn = new List<TreeNodeEventResult>();
			if(string.IsNullOrWhiteSpace(command))
			{
				return rtn;
			}
		
			foreach (var node in ParseCommandArgument(command))
			{
				var treeNodeResults = new TreeNodeEventResult();
				foreach (var fieldValue in SplitNodeFields(node))
				{
					if(TrySplitFieldValue(fieldValue, out var field, out var value) && !string.IsNullOrWhiteSpace(field))
					{
						switch (field.ToLower())
						{
							case "id":
								treeNodeResults.ID = value;
								break;
							case "parentid":
								treeNodeResults.ParentID = value;
								break;
							case "oldparentid":
								treeNodeResults.OldParentID = value;
								break;
							case "text":
								treeNodeResults.Text = value;
								break;
							case "prevnodeid":
								treeNodeResults.PrevNodeID = value;
								break;
						}
					}
				}

				if(!string.IsNullOrWhiteSpace(treeNodeResults.ID))
				{
					rtn.Add(treeNodeResults);
				}
			}
			return rtn;
		}

		public static List<string> ParseCommandArgument(string s)
        {
            var nodes = new List<string>();
            var currentNode = new System.Text.StringBuilder();
            var position = 0;
            var sArray = s.ToCharArray();
            for (int i = 0; i < sArray.Length; i++)
            {
                var c = sArray[i];
                if(c == '{')
                {
                    position++;
                    if(position == 1)
                    {
                        continue;
                    }
                }
                else if(c == '}')
                {
                    position--;
                    if(position == 0)
                    {
                        nodes.Add(currentNode.ToString());
                        currentNode.Clear();
                        continue;
                    }
                }

                if(position == 0)
                {
                    continue;
                }

                currentNode.Append(c);
            }

			return nodes;
        }

		public static string[] SplitNodeFields(string node)
		{
			return string.IsNullOrEmpty(node) ? new string[0] : node.Split(',');
		}

		public static bool TrySplitFieldValue(string fieldValue, out string field, out string value)
		{
			field = null;
			value = null;

			if(string.IsNullOrWhiteSpace(fieldValue))
			{
				return false;
			}

			var split = fieldValue.Split(':');
			if(split == null || split.Length < 2)
			{
				return false;
			}

			field = split[0]?.Replace("\"", string.Empty);
			value = split[1]?.Replace("\"", string.Empty);

			return !string.IsNullOrWhiteSpace(field);
		}

		public const string NODETYPE_OPER = "OPER";
		public const string NODETYPE_MATL = "MATL";
		private const string TREENODESEPERATOR = "+";
		private const char TREENODESEPERATORCHAR = '+';

		public static string GetBomItemKey(AMBomItem bomItem)
			=> string.Join(TREENODESEPERATOR, bomItem?.BOMID, bomItem?.RevisionID);

		public static string GetBomOperKey(AMBomOper bomOper)
			=> string.Join(TREENODESEPERATOR, bomOper?.BOMID, bomOper?.RevisionID, bomOper?.OperationCD, bomOper?.OperationID, NODETYPE_OPER);
		
		public static string GetBomMatlKey(AMBomOper bomOper, AMBomMatl bomMatl)
			=> string.Join(TREENODESEPERATOR, bomMatl?.BOMID, bomMatl?.RevisionID, bomOper?.OperationCD, bomOper?.OperationID, NODETYPE_MATL, PadLineID(bomMatl?.SortOrder), PadLineID(bomMatl?.LineID));

		public static string GetBomMatlKeyWithSubBomID(AMBomOper bomOper, AMBomMatl bomMatl, AMBomItem bomMatlSubBomItem)
			=> bomMatlSubBomItem?.BOMID == null
			? GetBomMatlKey(bomOper, bomMatl)
			: string.Join(TREENODESEPERATOR, bomMatl?.BOMID, bomMatl?.RevisionID, bomOper?.OperationCD, bomOper?.OperationID, NODETYPE_MATL, PadLineID(bomMatl?.SortOrder), PadLineID(bomMatl?.LineID), bomMatlSubBomItem.BOMID, bomMatlSubBomItem.RevisionID);

		public static string AppendDuplicateIDSegment(string id, int count) => string.Join(TREENODESEPERATOR, id, $"R{_PadInt(2, count)}");

		private static string PadLineID(int? lineID)
		{
			return _PadInt(3, lineID);
		}

		private static string _PadInt(int totalWidth, int? paddingInt) => paddingInt == null ? "0" : paddingInt.GetValueOrDefault().ToString().PadLeft(totalWidth, '0');

		/// <summary>
		/// Translates the tree node ID in reverse to it's <see cref="AMBomOper"/> record
		/// </summary>
		public static AMBomOper GetBomOperFromTreeNodeID(PXGraph graph, string id)
		{
			var operKeys = GetOperKeysFromTreeNodeID(id);
			if(operKeys == null || !operKeys.IsValidID)
			{
				return null;
			}

			return AMBomOper.PK.Find(graph, operKeys.BOMID, operKeys.RevisionID, operKeys.OperationID)
				// PK.Find will not find an inserted oper
				?? graph.Caches[typeof(AMBomOper)].Inserted.RowCast<AMBomOper>()
					.Where(r => r.BOMID == operKeys.BOMID && r.RevisionID == operKeys.RevisionID && r.OperationID == operKeys.OperationID).FirstOrDefault();
		}

		public static BomOperTreeNode GetOperKeysFromTreeNodeID(string id)
		{
			return new BomOperTreeNode(id);
		}

		public class BomOperTreeNode : IBomOper
		{
			public BomOperTreeNode(string id)
			{
				ID = id;
				if (string.IsNullOrWhiteSpace(ID))
				{
					return;
				}

				var split = id.Split(TREENODESEPERATORCHAR);
				if (split.Length < 5)
				{
					return;
				}

				BOMID = split[0];
				RevisionID = split[1];
				OperationCD = split[2];
				OperationID = int.Parse(split[3] ?? "0");

				IsValidID = !string.IsNullOrWhiteSpace(BOMID) && !string.IsNullOrWhiteSpace(RevisionID) && OperationID != 0;
			}

			public string ID { get; private set; }
			public bool IsValidID { get; private set; }
			public string BOMID { get; set; }
			public string RevisionID { get; set; }
			public int? OperationID { get; set; }
			public string OperationCD { get; set; }
		}

		/// <summary>
		/// Is the given tree node ID an operation node
		/// </summary>
		public static bool IsOperationTreeNode(string id)
		{
			return TryGetNodeTypeFromID(id, out var type) && type == NODETYPE_OPER;
		}

		public static AMBomItem GetBomMatlSubBomItemFromTreeNodeID(PXGraph graph, string id)
		{
			var matlKeys = GetMatlKeysFromTreeNodeID(id);
			if (matlKeys == null || !matlKeys.IsSubassembly)
			{
				return null;
			}

			return AMBomItem.PK.Find(graph, matlKeys.SubassemblyBOMID, matlKeys.SubassemblyRevisionID);
		}

		/// <summary>
		/// Translates the tree node ID in reverse to it's <see cref="AMBomMatl"/> record
		/// </summary>
		public static AMBomMatl GetBomMatlFromTreeNodeID(PXGraph graph, string id)
		{
			var matlKeys = GetMatlKeysFromTreeNodeID(id);
			if(matlKeys == null || !matlKeys.IsValidID)
			{
				return null;
			}

			return AMBomMatl.PK.Find(graph, matlKeys.BOMID, matlKeys.RevisionID, matlKeys.OperationID, matlKeys.LineID);
		}

		public static BomMatlTreeNode GetMatlKeysFromTreeNodeID(string id)
		{
			return new BomMatlTreeNode(id);
		}

		public static bool IsSubassemblyMaterialTreeNode(string id)
		{
			return GetMatlKeysFromTreeNodeID(id).IsSubassembly;
		}

		public static bool IsMaterialTreeNode(string id)
		{
			return TryGetNodeTypeFromID(id, out var type) && type == NODETYPE_MATL;
		}

		public class BomMatlTreeNode : IBomDetail
		{
			public BomMatlTreeNode(string id)
			{
				ID = id;
				if (string.IsNullOrWhiteSpace(ID))
				{
					return;
				}

				var split = id.Split(TREENODESEPERATORCHAR);
				if (split.Length < 7 || split[4] != NODETYPE_MATL)
				{
					return;
				}

				BOMID = split[0];
				RevisionID = split[1];
				OperationCD = split[2];
				OperationID = int.Parse(split[3] ?? "0");
				SortOrder = int.Parse(split[5] ?? "0");
				LineID = int.Parse(split[6] ?? "0");

				if(split.Length >= 9)
				{
					SubassemblyBOMID = split[7];
					SubassemblyRevisionID = split[8];
				}

				IsValidID = !string.IsNullOrWhiteSpace(BOMID) && !string.IsNullOrWhiteSpace(RevisionID);
				IsSubassembly = IsValidID && !string.IsNullOrWhiteSpace(SubassemblyBOMID) && !string.IsNullOrWhiteSpace(SubassemblyRevisionID);
			}

			public string ID { get; private set; }
			public bool IsValidID { get; private set; }
			public bool IsSubassembly { get; private set; }
			public string BOMID { get; set; }
			public string RevisionID { get; set; }
			public int? OperationID { get; set; }
			public string OperationCD { get; set; }
			public int? SortOrder { get; set; }
			public int? LineID { get; set; }
			public string SubassemblyBOMID { get; set; }
			public string SubassemblyRevisionID { get; set; }
		}

		private static bool TryGetNodeTypeFromID(string id, out string type)
		{
			type = null;
			if(string.IsNullOrWhiteSpace(id))
			{
				return false;
			}

			var split = id.Split(TREENODESEPERATORCHAR);
			if(split.Length < 5)
			{
				return false;
			}

			type = split[4];
			return !string.IsNullOrWhiteSpace(type);
		}

		public static AMBomItem GetBomItemFromTreeNodeID(PXGraph graph, string id)
		{
			var bomKeys = GetBomKeysFromTreeNodeID(id);
			if(bomKeys == null)
			{
				return null;
			}

			return AMBomItem.PK.Find(graph, (string)bomKeys[0], (string)bomKeys[1]);
		}

		public static object[] GetBomKeysFromTreeNodeID(string id)
		{
			if(string.IsNullOrWhiteSpace(id))
			{
				return null;
			}

			var segments = id.Split(TREENODESEPERATORCHAR);
			return segments != null && segments.Length >= 2 ? new object[2] { segments[0], segments[1] } : null;
		}

		public static string MakeActionsString(TreeNodeActions actions)
		{
			if(actions == null)
			{
				return "{\"rename\": false, \"createChild\": false, \"createSibling\": false, \"delete\": false, \"disableDropChild\": false, \"disableDrag\": false}";
			}

			var sb = new System.Text.StringBuilder();
			sb.Append("{\"rename\": ");
			sb.Append(ToLowerString(actions.Rename));

			sb.Append(", \"createChild\": ");
			sb.Append(ToLowerString(actions.CreateChild));

			sb.Append(", \"createSibling\": ");
			sb.Append(ToLowerString(actions.CreateSibling));

			sb.Append(", \"delete\": ");
			sb.Append(ToLowerString(actions.Delete));

			sb.Append(", \"disableDropChild\": ");
			sb.Append(ToLowerString(actions.DisableDropChild));

			sb.Append(", \"disableDrag\": ");
			sb.Append(ToLowerString(actions.DisableDrag));
			sb.Append("}");

			return sb.ToString();
		}

		private static string ToLowerString(bool val)
		{
			return val.ToString().ToLower();
		}
	}

	public class TreeNodeActions
	{
		public bool Rename;
		public bool Delete;
		public bool CreateChild;
		public bool CreateSibling;
		public bool DisableDropChild;
		public bool DisableDrag;
	}
}
