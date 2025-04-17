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

using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Payroll;
using PX.Payroll.Data;
using PX.Payroll.Data.Vertex;
using PX.Payroll.GovernmentSlips;
using PX.Payroll.Proxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace PX.Objects.PR
{
	public class PRTaxFormGenerator : PXGraph<PRTaxFormGenerator>
	{
		public const string T4AmountsTag = "T4_AMT";
		public const string RL1AmountsTag = "Montants";

		public const string CanFITTaxUniqueCode = "478_700000000";
		public const string QCPITTaxUniqueCode = "500_700190000";
		public const string CanCPPTaxUniqueCode = "474_700000000";
		public const string CanQPPTaxUniqueCode = "476_700000000";
		public const string CanEITaxUniqueCode = "472_700000000";
		public const string CanPPIPTaxUniqueCode = "448_700000000";

		#region Views

		public SelectFrom<PRGovernmentSlip>
			.Where<PRGovernmentSlip.slipName.IsEqual<P.AsString>
				.And<PRGovernmentSlip.year.IsEqual<P.AsInt>>>.View GovernmentSlips;

		public SelectFrom<PRGovernmentSlipField>
			.Where<PRGovernmentSlipField.slipName.IsEqual<P.AsString>
				.And<PRGovernmentSlipField.year.IsEqual<P.AsInt>>>.View GovernmentSlipFields;

		public SelectFrom<PREmployeeAttribute>
			.Where<MatchPRCountry<PREmployeeAttribute.countryID>
				.And<PREmployeeAttribute.bAccountID.IsEqual<P.AsInt>>
				.And<PREmployeeAttribute.canadaReportMapping.IsEqual<P.AsInt>>>.View EmployeeAttributes;

		public SelectFrom<PRCompanyTaxAttribute>
			.Where<MatchPRCountry<PREmployeeAttribute.countryID>
				.And<PRCompanyTaxAttribute.canadaReportMapping.IsEqual<P.AsInt>>>.View CompanyAttributes;

		#endregion Views

		public static List<TaxFormFile> GetTaxFormFiles(TaxFormRequestParameters parameters)
		{
			PRTaxFormGenerator taxFormGenerator = PXGraph.CreateInstance<PRTaxFormGenerator>();

			Slip slip;
			switch (parameters.FormName)
			{
				case GovernmentSlipTypes.T4:
					slip = new T4Slip(taxFormGenerator, parameters);
					break;
				case GovernmentSlipTypes.RL1:
					slip = new RL1Slip(taxFormGenerator, parameters);
					break;
				default:
					throw new PXException(Messages.TaxFormIsNotSupported, parameters.FormName);
			}

			List<TaxFormFile> result = new List<TaxFormFile>();

			foreach (string formFileType in parameters.FormFileTypes)
			{
				if (formFileType == FormFileType.PDF)
				{
					result.Add(new TaxFormFile(FormFileType.PDF, taxFormGenerator.GetTaxFormPdfFile(slip, parameters)));
				}
				else if (formFileType == FormFileType.XML)
				{
					if (parameters.FormName == GovernmentSlipTypes.T4 && slip is T4Slip t4Slip)
					{
						result.Add(new TaxFormFile(FormFileType.XML, GetEmployeeT4SlipXmlTag(t4Slip)));
					}
					else if (parameters.FormName == GovernmentSlipTypes.RL1 && slip is RL1Slip rl1Slip)
					{
						result.Add(new TaxFormFile(FormFileType.XML, GetEmployeeRL1SlipXmlTag(rl1Slip)));
					}
				}
				else
				{
					throw new PXException(Messages.ImpossibleToGenerateTaxForm, parameters.FormName, formFileType);
				}
			}

			return result;
		}

		public static PX.SM.FileInfo GetCombinedPdfFile(PRTaxFormBatch taxFormBatch, PREmployeeTaxFormData[] employeeTaxForms)
		{
			PdfDocument combinedPdfDocument = new PdfDocument();

			foreach (PREmployeeTaxFormData employeeTaxForm in employeeTaxForms)
			{
				if (employeeTaxForm.FormFileType != FormFileType.PDF)
				{
					continue;
				}

				PdfDocument currentPdfDocument;
				using (MemoryStream memoryStream = new MemoryStream())
				{
					byte[] documentData = Convert.FromBase64String(employeeTaxForm.FormData);
					memoryStream.Write(documentData, 0, documentData.Length);
					currentPdfDocument = PdfReader.Open(memoryStream, PdfDocumentOpenMode.Import);
				}

				foreach (PdfPage page in currentPdfDocument.Pages)
				{
					combinedPdfDocument.AddPage(page);
				}
			}

			string errorMessage = null;
			if (combinedPdfDocument.CanSave(ref errorMessage))
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					combinedPdfDocument.Save(memoryStream, false);
					byte[] data = memoryStream.ToArray();
					return new PX.SM.FileInfo(string.Format("{0}-{1}.pdf", taxFormBatch.FormType, taxFormBatch.BatchID), null, data);
				}
			}

			throw new Exception(errorMessage);
		}

		public static PX.SM.FileInfo GetCombinedT4XmlFile(PRTaxFormBatch taxFormBatch, PREmployeeTaxFormData[] employeeTaxForms)
		{
			PRTaxFormGenerator taxFormGenerator = PXGraph.CreateInstance<PRTaxFormGenerator>();

			T619 t619 = new T619(taxFormGenerator, taxFormBatch);
			XmlDocument xmlDocument = new XmlDocument();

			XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", "");
			xmlDocument.AppendChild(xmlDeclaration);

			XmlElement submissionTag = xmlDocument.CreateElement("Submission");

			submissionTag.SetAttribute("xmlns:ccms", "http://www.cra-arc.gc.ca/xmlns/ccms/1-0-0");
			submissionTag.SetAttribute("xmlns:sdt", "http://www.cra-arc.gc.ca/xmlns/sdt/2-2-0");
			submissionTag.SetAttribute("xmlns:ols", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols/1-0-1");
			submissionTag.SetAttribute("xmlns:ols1", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols1/1-0-1");
			submissionTag.SetAttribute("xmlns:ols10", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols10/1-0-1");
			submissionTag.SetAttribute("xmlns:ols100", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols100/1-0-1");
			submissionTag.SetAttribute("xmlns:ols12", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols12/1-0-1");
			submissionTag.SetAttribute("xmlns:ols125", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols125/1-0-1");
			submissionTag.SetAttribute("xmlns:ols140", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols140/1-0-1");
			submissionTag.SetAttribute("xmlns:ols141", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols141/1-0-1");
			submissionTag.SetAttribute("xmlns:ols2", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols2/1-0-1");
			submissionTag.SetAttribute("xmlns:ols5", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols5/1-0-1");
			submissionTag.SetAttribute("xmlns:ols50", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols50/1-0-1");
			submissionTag.SetAttribute("xmlns:ols52", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols52/1-0-1");
			submissionTag.SetAttribute("xmlns:ols6", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols6/1-0-1");
			submissionTag.SetAttribute("xmlns:ols8", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols8/1-0-1");
			submissionTag.SetAttribute("xmlns:ols8-1", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols8-1/1-0-1");
			submissionTag.SetAttribute("xmlns:ols9", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols9/1-0-1");
			submissionTag.SetAttribute("xmlns:olsbr", "http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/olsbr/1-0-1");

			// five lines below are needed to add xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" and xsi: noNamespaceSchemaLocation = "layout-topologie.xsd"
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
			xmlNamespaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
			XmlAttribute noNamespaceSchemaLocationAttribute = xmlDocument.CreateAttribute("xsi", "noNamespaceSchemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
			noNamespaceSchemaLocationAttribute.Value = "layout-topologie.xsd";
			submissionTag.SetAttributeNode(noNamespaceSchemaLocationAttribute);

			XmlElement t619Tag = xmlDocument.CreateElement("T619");

			AppendNode(xmlDocument, t619Tag, "sbmt_ref_id", t619.SubmissionRefID);
			AppendNode(xmlDocument, t619Tag, "rpt_tcd", t619.ReportTypeCode);
			AppendNode(xmlDocument, t619Tag, "trnmtr_nbr", t619.TransmitterNumber);
			AppendNode(xmlDocument, t619Tag, "trnmtr_tcd", t619.TransmitterTypeIndicator);
			AppendNode(xmlDocument, t619Tag, "summ_cnt", t619.SummaryRecordsNumber);
			AppendNode(xmlDocument, t619Tag, "lang_cd", t619.Language);

			XmlElement transmitterNameTag = xmlDocument.CreateElement("TRNMTR_NM");
			AppendNode(xmlDocument, transmitterNameTag, "l1_nm", t619.TransmitterNameLine1);
			AppendNode(xmlDocument, transmitterNameTag, "l2_nm", t619.TransmitterNameLine2, false);
			t619Tag.AppendChild(transmitterNameTag);

			XmlElement transmitterAddressTag = xmlDocument.CreateElement("TRNMTR_ADDR");
			AppendNode(xmlDocument, transmitterAddressTag, "addr_l1_txt", t619.TransmitterAddressLine1, false);
			AppendNode(xmlDocument, transmitterAddressTag, "addr_l2_txt", t619.TransmitterAddressLine2, false);
			AppendNode(xmlDocument, transmitterAddressTag, "cty_nm", t619.TransmitterCity);
			AppendNode(xmlDocument, transmitterAddressTag, "prov_cd", t619.TransmitterProvinceCode);
			AppendNode(xmlDocument, transmitterAddressTag, "cntry_cd", t619.TransmitterCountryCode);
			AppendNode(xmlDocument, transmitterAddressTag, "pstl_cd", t619.TransmitterPostalCode);
			t619Tag.AppendChild(transmitterAddressTag);

			XmlElement contactNameTag = xmlDocument.CreateElement("CNTC");
			AppendNode(xmlDocument, contactNameTag, "cntc_nm", t619.ContactName);
			AppendNode(xmlDocument, contactNameTag, "cntc_area_cd", t619.ContactAreaCode);
			AppendNode(xmlDocument, contactNameTag, "cntc_phn_nbr", t619.ContactTelephoneNumber);
			AppendNode(xmlDocument, contactNameTag, "cntc_extn_nbr", t619.ContactExtensionNumber, false);
			AppendNode(xmlDocument, contactNameTag, "cntc_email_area", t619.ContactEmailAddress);
			AppendNode(xmlDocument, contactNameTag, "sec_cntc_email_area", t619.ContactEmailAddress2, false);
			t619Tag.AppendChild(contactNameTag);

			submissionTag.AppendChild(t619Tag);

			XmlElement returnTag = xmlDocument.CreateElement("Return");
			XmlElement t4Tag = xmlDocument.CreateElement("T4");

			Dictionary<string, decimal> t4SummaryAccomulatedTotals = new Dictionary<string, decimal>
			{
				{ "tot_empt_incamt", 0m },
				{ "tot_empe_cpp_amt", 0m },
				{ "tot_empe_eip_amt", 0m },
				{ "tot_rpp_cntrb_amt", 0m },
				{ "tot_itx_ddct_amt", 0m },
				{ "tot_padj_amt", 0m }
			};

			List<int?> employeeIDs = new List<int?>();
			foreach (PREmployeeTaxFormData employeeTaxForm in employeeTaxForms)
			{
				if (employeeTaxForm.FormFileType != FormFileType.XML)
				{
					continue;
				}

				string t4SlipEmployeeData = employeeTaxForm.FormData;
				XmlNode t4SlipNode = xmlDocument.ReadNode(new XmlTextReader(new System.IO.StringReader(t4SlipEmployeeData)));
				t4Tag.AppendChild(t4SlipNode);

				foreach (XmlNode sectionNode in t4SlipNode.ChildNodes)
				{
					if (sectionNode.Name == T4AmountsTag)
					{
						foreach (XmlNode amountsNode in sectionNode.ChildNodes)
						{
							string accomulatedTag = "tot_" + amountsNode.Name;
							if (t4SummaryAccomulatedTotals.ContainsKey(accomulatedTag) && Decimal.TryParse(amountsNode.InnerText, out decimal employeeAmount))
							{
								t4SummaryAccomulatedTotals[accomulatedTag] += employeeAmount;
							}							
						}
					}
				}
				employeeIDs.Add(employeeTaxForm.EmployeeID);
			}

			T4SummaryInfo t4Summary = new T4SummaryInfo(taxFormGenerator, taxFormBatch, employeeIDs);
			XmlElement t4SummaryTag = xmlDocument.CreateElement("T4Summary");
			AppendNode(xmlDocument, t4SummaryTag, "bn", t4Summary.PayrollAccountNumber);

			XmlElement employerNameTag = xmlDocument.CreateElement("EMPR_NM");
			AppendNode(xmlDocument, employerNameTag, "l1_nm", t4Summary.EmployerNameLine1);
			AppendNode(xmlDocument, employerNameTag, "l2_nm", t4Summary.EmployerNameLine2, false);
			AppendNode(xmlDocument, employerNameTag, "l3_nm", t4Summary.EmployerNameLine3, false);
			t4SummaryTag.AppendChild(employerNameTag);

			XmlElement employerAddressTag = xmlDocument.CreateElement("EMPR_ADDR");
			AppendNode(xmlDocument, employerAddressTag, "addr_l1_txt", t4Summary.EmployerAddressLine1, false);
			AppendNode(xmlDocument, employerAddressTag, "addr_l2_txt", t4Summary.EmployerAddressLine2, false);
			AppendNode(xmlDocument, employerAddressTag, "cty_nm", t4Summary.EmployerCity, false);
			AppendNode(xmlDocument, employerAddressTag, "prov_cd", t4Summary.EmployerProvince, false);
			AppendNode(xmlDocument, employerAddressTag, "cntry_cd", t4Summary.EmployerCountry, false);
			AppendNode(xmlDocument, employerAddressTag, "pstl_cd", t4Summary.EmployerPostalCode, false);
			t4SummaryTag.AppendChild(employerAddressTag);

			XmlElement summaryContactNameTag = xmlDocument.CreateElement("CNTC");
			AppendNode(xmlDocument, summaryContactNameTag, "cntc_nm", t4Summary.ContactName);
			AppendNode(xmlDocument, summaryContactNameTag, "cntc_area_cd", t4Summary.ContactAreaCode);
			AppendNode(xmlDocument, summaryContactNameTag, "cntc_phn_nbr", t4Summary.ContactTelephoneNumber);
			AppendNode(xmlDocument, summaryContactNameTag, "cntc_extn_nbr", t4Summary.ContactExtension, false);
			t4SummaryTag.AppendChild(summaryContactNameTag);

			AppendNode(xmlDocument, t4SummaryTag, "tx_yr", t4Summary.TaxationYear);
			AppendNode(xmlDocument, t4SummaryTag, "slp_cnt", t4Summary.T4SlipRecordsNumber.ToString());

			XmlElement proprietorTag = xmlDocument.CreateElement("CNTC");
			AppendNode(xmlDocument, proprietorTag, "pprtr_1_sin", t4Summary.Proprietor1SIN);
			AppendNode(xmlDocument, proprietorTag, "pprtr_2_sin", t4Summary.Proprietor2SIN, false);
			t4SummaryTag.AppendChild(proprietorTag);

			AppendNode(xmlDocument, t4SummaryTag, "rpt_tcd", t4Summary.ReportTypeCode);
			AppendNode(xmlDocument, t4SummaryTag, "fileramendmentnote", t4Summary.FilerAmendmentNote, false);

			XmlElement t4AmountsTag = xmlDocument.CreateElement("T4_TAMT");
			foreach (KeyValuePair<string, decimal> tagValuePair in t4SummaryAccomulatedTotals)
			{
				AppendNode(xmlDocument, t4AmountsTag, tagValuePair.Key, FormatDecimal(tagValuePair.Value));
			}

			AppendNode(xmlDocument, t4AmountsTag, "tot_empr_cpp_amt", FormatDecimal(t4Summary.TotalEmployersCPPContributions));
			AppendNode(xmlDocument, t4AmountsTag, "tot_empr_eip_amt", FormatDecimal(t4Summary.TotalEmployersEIremiums));
			t4SummaryTag.AppendChild(t4AmountsTag);

			t4Tag.AppendChild(t4SummaryTag);

			returnTag.AppendChild(t4Tag);
			submissionTag.AppendChild(returnTag);

			xmlDocument.AppendChild(submissionTag);
			byte[] xmlData = Encoding.UTF8.GetBytes(xmlDocument.OuterXml);

			return new PX.SM.FileInfo(string.Format("{0}-{1}.xml", taxFormBatch.FormType, taxFormBatch.BatchID), null, xmlData);
		}

		public static PX.SM.FileInfo GetCombinedRL1XmlFile(PRTaxFormBatch taxFormBatch, PREmployeeTaxFormData[] employeeTaxForms)
		{
			PRTaxFormGenerator taxFormGenerator = PXGraph.CreateInstance<PRTaxFormGenerator>();
			XmlDocument xmlDocument = new XmlDocument();

			XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", "");
			xmlDocument.AppendChild(xmlDeclaration);

			XmlElement transmissionTag = xmlDocument.CreateElement("Transmission");
			transmissionTag.SetAttribute("VersionSchema", "2020.1");
			transmissionTag.SetAttribute("xmlns", "http://www.mrq.gouv.qc.ca/T5");

			Production production = new Production(taxFormGenerator, taxFormBatch);
			XmlElement productionTag = xmlDocument.CreateElement("P");
			AppendNode(xmlDocument, productionTag, "Annee", production.SubmissionYear);
			string transmissionType;
			if (production.ReportTypeCode == TaxFormBatchType.OriginalRL1)
			{
				transmissionType = "1";
			}
			else if (production.ReportTypeCode == TaxFormBatchType.AmendmentRL1)
			{
				transmissionType = "4";
			}
			else
			{
				transmissionType = "6";
			}
			AppendNode(xmlDocument, productionTag, "TypeEnvoi", transmissionType);

			RL1EmployerInfo employerInfo = new RL1EmployerInfo(taxFormGenerator, taxFormBatch);
			XmlElement transmitterTag = xmlDocument.CreateElement("Preparateur");
			AppendNode(xmlDocument, transmitterTag, "No", employerInfo.TransmitterNumber);
			AppendNode(xmlDocument, transmitterTag, "Type", production.TransmitterTypeIndicator, false);
			AppendNode(xmlDocument, transmitterTag, "Nom1", production.TransmitterNameLine1);
			AppendNode(xmlDocument, transmitterTag, "Nom2", production.TransmitterNameLine2, false);

			if (!string.IsNullOrWhiteSpace(production.TransmitterAddressLine1))
			{
				XmlElement transmitterAddressTag = xmlDocument.CreateElement("Adresse");
				AppendNode(xmlDocument, transmitterAddressTag, "Ligne1", production.TransmitterAddressLine1);
				AppendNode(xmlDocument, transmitterAddressTag, "Ligne2", production.TransmitterAddressLine2, false);
				AppendNode(xmlDocument, transmitterAddressTag, "Ville", production.TransmitterCity, false);
				AppendNode(xmlDocument, transmitterAddressTag, "Province", production.TransmitterProvinceCode, false);
				AppendNode(xmlDocument, transmitterAddressTag, "CodePostal", production.TransmitterPostalCode, false);
				transmitterTag.AppendChild(transmitterAddressTag);
			}

			productionTag.AppendChild(transmitterTag);

			if (!string.IsNullOrWhiteSpace(production.ContactName))
			{
				XmlElement informationTag = xmlDocument.CreateElement("Informatique");
				AppendNode(xmlDocument, informationTag, "Nom", production.ContactName, false);
				AppendNode(xmlDocument, informationTag, "IndRegional", production.ContactAreaCode, false);
				AppendNode(xmlDocument, informationTag, "Tel", production.ContactTelephoneNumber, false);
				productionTag.AppendChild(informationTag);
			}

			AppendNode(xmlDocument, productionTag, "NoCertification", "RQ-20-99-999");
			AppendNode(xmlDocument, productionTag, "NomLogiciel", "Acumatica");
			AppendNode(xmlDocument, productionTag, "VersionLogiciel", PXVersionInfo.AcumaticaBuildVersion);
			AppendNode(xmlDocument, productionTag, "IdPartenaireReleves", "XXXXXXXXXXXXXXXX");
			AppendNode(xmlDocument, productionTag, "IdProduitsReleves", "XXXXXXXXXXXXXXXX");

			transmissionTag.AppendChild(productionTag);

			XmlElement rl1Tag = xmlDocument.CreateElement("Groupe01");

			foreach (PREmployeeTaxFormData employeeTaxForm in employeeTaxForms)
			{
				if (employeeTaxForm.FormFileType != FormFileType.XML)
				{
					continue;
				}

				XmlNode oldEmployeeDataNode = null;
				XmlNode rl1SlipNode = xmlDocument.ReadNode(new XmlTextReader(new System.IO.StringReader(employeeTaxForm.FormData)));
				XmlElement batchTypeTag = xmlDocument.CreateElement(production.ReportTypeCode);

				if (production.ReportTypeCode == TaxFormBatchType.OriginalRL1)
				{
					rl1Tag.AppendChild(rl1SlipNode);
				}
				else
				{
					string oldBatchID = PRTaxFormBatchMaint
						.GetEmployeeBatches(taxFormGenerator, taxFormBatch.Year, taxFormBatch.OrgBAccountID, employeeTaxForm.EmployeeID, taxFormBatch.FormType)
						.FirstOrDefault()?.BatchID;
					if (oldBatchID != null)
					{
						PREmployeeTaxFormData oldTaxForm = SelectFrom<PREmployeeTaxFormData>
							.Where<PREmployeeTaxFormData.batchID.IsEqual<P.AsString>
								.And<PREmployeeTaxFormData.employeeID.IsEqual<P.AsInt>>
								.And<PREmployeeTaxFormData.formFileType.IsEqual<P.AsString>>>.View.Select(taxFormGenerator, oldBatchID, employeeTaxForm.EmployeeID, FormFileType.XML).FirstOrDefault();
						oldEmployeeDataNode = oldTaxForm != null ? xmlDocument.ReadNode(new XmlTextReader(new System.IO.StringReader(oldTaxForm.FormData))) : null;
					}
					if (oldEmployeeDataNode != null)
					{
						if (production.ReportTypeCode == TaxFormBatchType.AmendmentRL1)
						{
							string oldFileNumber = ((XmlNode)oldEmployeeDataNode.SelectNodes("NoReleve").FirstOrDefault_()).Value;
							AppendNode(xmlDocument, rl1SlipNode, "NoReleveDerniereTrans", oldFileNumber);
							batchTypeTag.InnerXml = rl1SlipNode.InnerXml;
						}
						else
						{
							XmlNode amountsNode = (XmlNode)oldEmployeeDataNode.SelectNodes(RL1AmountsTag).FirstOrDefault_();
							oldEmployeeDataNode.RemoveChild(amountsNode);
							batchTypeTag.InnerXml = oldEmployeeDataNode.InnerXml;
						}
					}
					rl1Tag.AppendChild(batchTypeTag);
				}
			}

			XmlElement totalsTag = xmlDocument.CreateElement("T");
			AppendNode(xmlDocument, totalsTag, "Annee", production.SubmissionYear);
			AppendNode(xmlDocument, totalsTag, "NbReleves", taxFormBatch.NumberOfEmployees.ToString());

			XmlElement employerTag = xmlDocument.CreateElement("Employeur");
			AppendNode(xmlDocument, employerTag, "NoId", employerInfo.EmployerID);
			AppendNode(xmlDocument, employerTag, "TypeDossier", employerInfo.FileType);
			AppendNode(xmlDocument, employerTag, "NoDossier", employerInfo.FileNumber);
			AppendNode(xmlDocument, employerTag, "NEQ", employerInfo.NEQ);
			AppendNode(xmlDocument, employerTag, "Nom1", employerInfo.EmployerNameLine1);

			XmlElement addressTag = xmlDocument.CreateElement("Adresse");
			AppendNode(xmlDocument, addressTag, "Ligne1", employerInfo.EmployerAddressLine1);
			AppendNode(xmlDocument, addressTag, "Ligne2", employerInfo.EmployerAddressLine2, false);
			AppendNode(xmlDocument, addressTag, "Ville", employerInfo.EmployerCity);
			AppendNode(xmlDocument, addressTag, "Province", employerInfo.EmployerProvince);
			AppendNode(xmlDocument, addressTag, "CodePostal", employerInfo.EmployerPostalCode);

			employerTag.AppendChild(addressTag);
			totalsTag.AppendChild(employerTag);
			rl1Tag.AppendChild(totalsTag);
			transmissionTag.AppendChild(rl1Tag);
			xmlDocument.AppendChild(transmissionTag);

			byte[] xmlData = Encoding.UTF8.GetBytes(xmlDocument.OuterXml);
			return new PX.SM.FileInfo(string.Format("{0}-{1}.xml", taxFormBatch.FormType, taxFormBatch.BatchID), null, xmlData);
		}

		/// <summary>
		/// Retrieves BranchIDs related to the Business Account.
		/// </summary>
		/// <returns>Depending on the settings the method will return one BranchID or all BranchIDs in the Organization</returns>
		public static int[] GetBranchIDsByBusinessAccount(int? bAccountID)
		{
			List<PXAccess.MasterCollection.Branch> organizationBranches = PXAccess.GetOrganizationByBAccountID(bAccountID)?.ChildBranches;
			if (organizationBranches != null)
			{
				return organizationBranches.Select(item => item.BranchID).ToArray();
			}

			PXAccess.MasterCollection.Branch branch = PXAccess.GetBranchByBAccountID(bAccountID);

			return branch != null ? new[] { branch.BranchID } : new int[0];
		}

		public static IEnumerable<PropertyInfo> GetT4Properties()
		{
			return typeof(T4Slip).GetProperties().Where(p => Attribute.IsDefined(p, typeof(T4XmlNodeAttribute)));
		}

		private string GetTaxFormPdfFile(Slip slip, TaxFormRequestParameters parameters)
		{
			SlipInfo slipInfo = GetSlipInfo(parameters.FormName, parameters.Year);
			Dictionary<string, string> slipFieldValues = slip.GetPdfFieldValuePairs();
			Dictionary<string, Dictionary<int, string>> additionalSlipFieldValues = slip.GetAdditionalPdfFieldValuePairs();

			PdfDocument pdfDocument;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				memoryStream.Write(slipInfo.SlipData, 0, slipInfo.SlipData.Length);
				pdfDocument = PdfReader.Open(memoryStream, PdfDocumentOpenMode.Modify);
			}

			bool fillableFieldsExist = false;

			foreach (SlipField slipField in slipInfo.SlipFields)
			{
				if (!slipFieldValues.TryGetValue(slipField.FieldName, out string valueToAdd))
				{
					continue;
				}

				if (slipField.Fillable)
				{
					fillableFieldsExist = true;
				}

				using (XGraphics slipPage = XGraphics.FromPdfPage(pdfDocument.Pages[slipField.Page]))
				{
					FillSlipPageField(pdfDocument, slipPage, slipField, valueToAdd);
				}
			}

			if (slip.PagesToInsert > 0)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					pdfDocument.Save(memoryStream);
					pdfDocument = PdfReader.Open(memoryStream, PdfDocumentOpenMode.Import);
					for (int i = 0; i < slip.PagesToInsert; i++)
					{
						pdfDocument.InsertPage(i + 1, (PdfPage)pdfDocument.Pages[0].Clone());
					}
					pdfDocument.Save(memoryStream);
					pdfDocument = PdfReader.Open(memoryStream, PdfDocumentOpenMode.Modify);
				}
			}

			foreach (SlipField slipField in slipInfo.SlipFields)
			{
				if (!additionalSlipFieldValues.TryGetValue(slipField.FieldName, out Dictionary<int, string> pageIndexesAndValues))
				{
					continue;
				}

				foreach (KeyValuePair<int, string> pageIndexAndValueToAdd in pageIndexesAndValues)
				{					
					if (slipField.Fillable)
					{
						fillableFieldsExist = true;
					}

					using (XGraphics slipPage = XGraphics.FromPdfPage(pdfDocument.Pages[slipField.Page + pageIndexAndValueToAdd.Key]))
					{
						FillSlipPageField(pdfDocument, slipPage, slipField, pageIndexAndValueToAdd.Value);
					}
				}
			}

			if (parameters.IncludeUnreleasedPaychecks)
			{
				AddWatermark(pdfDocument);
			}

			if (fillableFieldsExist)
			{
				pdfDocument.Flatten();
			}

			string errorMessage = null;
			if (pdfDocument.CanSave(ref errorMessage))
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					pdfDocument.Save(memoryStream, false);
					byte[] data = memoryStream.ToArray();
					return Convert.ToBase64String(data);
				}
			}

			throw new Exception(errorMessage);
		}

		private SlipInfo GetSlipInfo(string governmentSlipName, int year)
		{
			PRGovernmentSlip governmentSlip = GovernmentSlips.SelectSingle(governmentSlipName, year);
			PRGovernmentSlipField[] slipFields = GovernmentSlipFields.Select<PRGovernmentSlipField>(governmentSlipName, year);

			PRWebServiceRestClient restClient = new PRWebServiceRestClient();
			DateTime actualTimestamp = restClient.GetSlipTimestamp(governmentSlipName, year);

			SlipInfo slipInfo;
			if (governmentSlip == null || governmentSlip.Timestamp != actualTimestamp)
			{
				slipInfo = restClient.GetSlipInfo(governmentSlipName, year);

				if (governmentSlip != null)
				{
					foreach (PRGovernmentSlipField slipField in slipFields)
					{
						GovernmentSlipFields.Delete(slipField);
					}

					GovernmentSlips.Delete(governmentSlip);
				}

				PRGovernmentSlip newGovernmentSlip = slipInfo.GetPRGovernmentSlip(year);
				GovernmentSlips.Insert(newGovernmentSlip);

				foreach (SlipField slipField in slipInfo.SlipFields)
				{
					PRGovernmentSlipField governmentSlipField = slipField.GetPRGovernmentSlipField(slipInfo.SlipName, year);
					GovernmentSlipFields.Insert(governmentSlipField);
				}
				Actions.PressSave();
			}
			else
			{
				slipInfo = GovernmentSlipExtensions.GetSlipInfo(governmentSlip, slipFields);
			}

			return slipInfo;
		}

		private static void FillSlipPageField(PdfDocument pdfDocument, XGraphics slipPage, SlipField slipField, string valueToAdd)
		{
			if (slipField.Fillable)
			{
				PdfAcroForm pdfAcroForm = pdfDocument.AcroForm;

				PdfAcroField pdfAcroField = pdfAcroForm.Fields[slipField.FieldCode];
				pdfAcroField.Value = new PdfString(valueToAdd);
			}
			else
			{
				XRect fieldRectangle = new XRect(slipField.LeftX.Value, slipField.TopY.Value, slipField.Width.Value, slipField.Height.Value);
				XBrush brush = Enum.TryParse(slipField.Color, out XKnownColor knownColor) ? new XSolidBrush(XColor.FromKnownColor(knownColor)) : XBrushes.Black;

				if (slipField.DataType == SlipFieldType.CheckBox)
				{
					// \u0050 is a tick symbol (check mark) for the Check Box field
					valueToAdd = bool.TryParse(valueToAdd, out bool boolValue) && boolValue ? "\u0050" : null;
				}

				if (valueToAdd != null)
				{
					XPdfFontOptions fontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode);
					XFont font = new XFont(slipField.FontName, slipField.FontSize.Value, XFontStyle.Regular, fontOptions);
					XStringFormat stringFormat = new XStringFormat();
					if (slipField.Alignment == SlipFieldAlignment.Left)
					{
						stringFormat.Alignment = XStringAlignment.Near;
					}
					else if (slipField.Alignment == SlipFieldAlignment.Center)
					{
						stringFormat.Alignment = XStringAlignment.Center;
					}
					else
					{
						stringFormat.Alignment = XStringAlignment.Far;
					}

					if (slipField.Multiline)
					{
						XTextFormatter textFormatter = new XTextFormatter(slipPage);
						textFormatter.DrawString(valueToAdd, font, brush, fieldRectangle, stringFormat);
					}
					else
					{
						slipPage.DrawString(valueToAdd, font, brush, fieldRectangle, stringFormat);
					}
				}
			}
		}

		private static void AddWatermark(PdfDocument pdfDocument)
		{
			string watermarkMessage = Messages.IncludesUnreleasedPaychecks;
			XFont watermarkFont = new XFont("Arial", 45, XFontStyle.Bold);
			XStringFormat format = new XStringFormat();
			format.Alignment = XStringAlignment.Near;
			format.LineAlignment = XLineAlignment.Near;
			XBrush brush = new XSolidBrush(XColor.FromArgb(128, 255, 0, 0));

			foreach (PdfPage page in pdfDocument.Pages)
			{
				using (XGraphics graphics = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append))
				{
					XSize size = graphics.MeasureString(watermarkMessage, watermarkFont);
					graphics.TranslateTransform(page.Width / 2, page.Height / 2);
					graphics.RotateTransform(Math.Atan(page.Height / page.Width) * 180 / Math.PI);
					graphics.TranslateTransform(-page.Width / 2, -page.Height / 2);
					graphics.DrawString(watermarkMessage, watermarkFont, brush,
						new XPoint((page.Width - size.Width) / 2, (page.Height - size.Height) / 2), format);
				}
			}
		}

		private static string GetEmployeeT4SlipXmlTag(T4Slip t4Slip)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement t4SlipTag = xmlDocument.CreateElement("T4Slip");

			XmlElement employeeNameTag = xmlDocument.CreateElement("EMPE_NM");
			AppendNode(xmlDocument, employeeNameTag, "snm", t4Slip.EmployeeLastName);
			AppendNode(xmlDocument, employeeNameTag, "gvn_nm", t4Slip.EmployeeFirstName, false);
			AppendNode(xmlDocument, employeeNameTag, "init", t4Slip.EmployeeInitial, false);
			t4SlipTag.AppendChild(employeeNameTag);

			XmlElement employeeAddressTag = xmlDocument.CreateElement("EMPE_ADDR");
			AppendNode(xmlDocument, employeeAddressTag, "addr_l1_txt", t4Slip.EmployeeAddressLine1, false);
			AppendNode(xmlDocument, employeeAddressTag, "addr_l2_txt", t4Slip.EmployeeAddressLine2, false);
			AppendNode(xmlDocument, employeeAddressTag, "cty_nm", t4Slip.EmployeeCity);
			AppendNode(xmlDocument, employeeAddressTag, "prov_cd", t4Slip.EmployeeProvince);
			AppendNode(xmlDocument, employeeAddressTag, "cntry_cd", t4Slip.EmployeeCountry);
			AppendNode(xmlDocument, employeeAddressTag, "pstl_cd", t4Slip.EmployeePostalCode);
			t4SlipTag.AppendChild(employeeAddressTag);

			AppendNode(xmlDocument, t4SlipTag, "sin", t4Slip.SocialInsuranceNumberBox12);
			AppendNode(xmlDocument, t4SlipTag, "empe_nbr", t4Slip.EmployeeNumber, false);
			AppendNode(xmlDocument, t4SlipTag, "bn", t4Slip.EmployersAccountNumberBox54);
			AppendNode(xmlDocument, t4SlipTag, "rpp_dpsp_rgst_nbr", t4Slip.RPPDPSPRegistrationNumberBox50, false);
			AppendNode(xmlDocument, t4SlipTag, "cpp_qpp_xmpt_cd", t4Slip.ExemptCPPQPPBox28 ? "1" : "0");
			AppendNode(xmlDocument, t4SlipTag, "ei_xmpt_cd", t4Slip.ExemptEIBox28 ? "1" : "0");
			AppendNode(xmlDocument, t4SlipTag, "prov_pip_xmpt_cd", t4Slip.ExemptPPIPBox28 ? "1" : "0");
			AppendNode(xmlDocument, t4SlipTag, "empt_cd", t4Slip.EmploymentCodeBox29, false);
			AppendNode(xmlDocument, t4SlipTag, "rpt_tcd", t4Slip.ReportTypeCode);
			AppendNode(xmlDocument, t4SlipTag, "empt_prov_cd", t4Slip.ProvinceOfEmploymentBox10);

			XmlElement t4AmountsTag = xmlDocument.CreateElement(T4AmountsTag);
			foreach (PropertyInfo t4Property in GetT4Properties())
			{
				T4XmlNodeAttribute attr = (T4XmlNodeAttribute)t4Property.GetCustomAttribute(typeof(T4XmlNodeAttribute));
				AppendNode(xmlDocument, t4AmountsTag, attr.NodeName, FormatDecimal((Decimal?)typeof(T4Slip).GetProperty(t4Property.Name).GetValue(t4Slip, null)));
			}
			t4SlipTag.AppendChild(t4AmountsTag);

			XmlElement otherInfoTag = xmlDocument.CreateElement("OTH_INFO");
			foreach (KeyValuePair<string, decimal> tagValuePair in t4Slip.GetOtherInfoSectionXmlNodes())
			{
				AppendNode(xmlDocument, otherInfoTag, tagValuePair.Key, FormatDecimal(tagValuePair.Value));
			}
			t4SlipTag.AppendChild(otherInfoTag);

			return t4SlipTag.OuterXml;
		}

		private static string GetEmployeeRL1SlipXmlTag(RL1Slip rl1Slip)
		{
			XmlDocument xmlDocument = new XmlDocument();

			XmlElement rl1SlipTag = xmlDocument.CreateElement("R");
			AppendNode(xmlDocument, rl1SlipTag, "Annee", rl1Slip.Year);
			AppendNode(xmlDocument, rl1SlipTag, "NoReleve", rl1Slip.ReferenceNumber);

			XmlElement identificationTag = xmlDocument.CreateElement("Identification");

			XmlElement employeeTag = xmlDocument.CreateElement("Employe");
			AppendNode(xmlDocument, employeeTag, "NAS", rl1Slip.SocialInsuranceNumber);
			AppendNode(xmlDocument, employeeTag, "NomFamille", rl1Slip.EmployeeLastName);
			AppendNode(xmlDocument, employeeTag, "Prenom", rl1Slip.EmployeeFirstName);

			identificationTag.AppendChild(employeeTag);
			rl1SlipTag.AppendChild(identificationTag);

			if (!string.IsNullOrWhiteSpace(rl1Slip.EmployeeApartementHouseStreet))
			{
				XmlElement addressTag = xmlDocument.CreateElement("Adresse");
				AppendNode(xmlDocument, addressTag, "Ligne1", rl1Slip.EmployeeApartementHouseStreet);
				AppendNode(xmlDocument, addressTag, "Ville", rl1Slip.EmployeeCity, false);
				AppendNode(xmlDocument, addressTag, "Province", rl1Slip.EmployeeProvince, false);
				AppendNode(xmlDocument, addressTag, "CodePostal", rl1Slip.EmployeePostalCode, false);
				rl1SlipTag.AppendChild(addressTag);
			}

			XmlElement amountsTag = xmlDocument.CreateElement(RL1AmountsTag);
			AppendNode(xmlDocument, amountsTag, "A_RevenuEmploi", FormatDecimal(rl1Slip.EmploymentIncomeBoxA), false);
			AppendNode(xmlDocument, amountsTag, "B_CotisationRRQ", FormatDecimal(rl1Slip.QPPContributionsBoxB), false);
			AppendNode(xmlDocument, amountsTag, "C_CotisationAssEmploi", FormatDecimal(rl1Slip.EIPremiumsBoxC), false);
			AppendNode(xmlDocument, amountsTag, "D_CotisationRPA", FormatDecimal(rl1Slip.RPPContributionsBoxD), false);
			AppendNode(xmlDocument, amountsTag, "E_ImpotQue", FormatDecimal(rl1Slip.QuebecIncomeTaxBoxE), false);
			AppendNode(xmlDocument, amountsTag, "F_CotisationSyndicale", FormatDecimal(rl1Slip.UnionDuesBoxF), false);
			AppendNode(xmlDocument, amountsTag, "G_SalaireAdmisRRQ", FormatDecimal(rl1Slip.PensionableSalaryQPPBoxG), false);
			AppendNode(xmlDocument, amountsTag, "H_CotisationRQAP", FormatDecimal(rl1Slip.QPIPPremiumBoxH), false);
			AppendNode(xmlDocument, amountsTag, "I_SalaireAdmisRQAP", FormatDecimal(rl1Slip.EligibleSalaryQPIPBoxI), false);
			AppendNode(xmlDocument, amountsTag, "J_RegimeAssMaladie", FormatDecimal(rl1Slip.PrivateHealthServicesPlanBoxJ), false);
			AppendNode(xmlDocument, amountsTag, "K_Voyage", FormatDecimal(rl1Slip.RemoteAreaTripsBoxK), false);
			AppendNode(xmlDocument, amountsTag, "L_AutreAvantage", FormatDecimal(rl1Slip.OtherBenefitsBoxL), false);
			AppendNode(xmlDocument, amountsTag, "M_Commission", FormatDecimal(rl1Slip.CommissionsBoxM), false);
			AppendNode(xmlDocument, amountsTag, "N_DonBienfaisance", FormatDecimal(rl1Slip.CharitableDonationsBoxN), false);
			if (rl1Slip.OtherIncomeBoxO != null)
			{
				XmlElement otherIncomeTag = xmlDocument.CreateElement("O_AutreRevenu");
				AppendNode(xmlDocument, otherIncomeTag, "MontantCaseO", FormatDecimal(rl1Slip.OtherIncomeBoxO));
				AppendNode(xmlDocument, otherIncomeTag, "SourceCaseO", rl1Slip.CodeCaseO);
				amountsTag.AppendChild(otherIncomeTag);
			}
			AppendNode(xmlDocument, amountsTag, "P_RegimeAssInterEntr", FormatDecimal(rl1Slip.ContrToMultiEmployerInsPlanBoxP), false);
			AppendNode(xmlDocument, amountsTag, "Q_SalaireDiffere", FormatDecimal(rl1Slip.DeferredSalaryBoxQ), false);
			AppendNode(xmlDocument, amountsTag, "R_RevenuIndien", FormatDecimal(rl1Slip.IncomeSituatedOnAReserveBoxR), false);
			AppendNode(xmlDocument, amountsTag, "S_PourboireRecu", FormatDecimal(rl1Slip.TipsBoxS), false);
			AppendNode(xmlDocument, amountsTag, "T_PourboireAttribue", FormatDecimal(rl1Slip.TipsAllocatedByTheEmployerBoxT), false);
			AppendNode(xmlDocument, amountsTag, "U_RetraiteProgressive", FormatDecimal(rl1Slip.PhasedRetirementBoxU), false);
			AppendNode(xmlDocument, amountsTag, "V_NourritureLogement", FormatDecimal(rl1Slip.MealsAndLodgingBoxV), false);
			AppendNode(xmlDocument, amountsTag, "W_Vehicule", FormatDecimal(rl1Slip.MotorVehicleBoxW), false);
			rl1SlipTag.AppendChild(amountsTag);
			foreach (KeyValuePair<string, decimal> tagValuePair in rl1Slip.GetOtherInfoSectionXmlNodes())
			{
				XmlElement otherInfoTag = xmlDocument.CreateElement("CaseRensCompl");
				AppendNode(xmlDocument, otherInfoTag, "CodeRensCompl", tagValuePair.Key);
				AppendNode(xmlDocument, otherInfoTag, "DonneeRensCompl", FormatDecimal(tagValuePair.Value));
				rl1SlipTag.AppendChild(otherInfoTag);
			}

			return rl1SlipTag.OuterXml;
		}

		#region Helper Methods

		private static void AppendNode(XmlDocument xmlDocument, XmlNode parentNode, string nodeName, string nodeValue, bool required = true)
		{
			if (!required && string.IsNullOrWhiteSpace(nodeValue))
			{
				return;
			}

			XmlNode xmlNode = xmlDocument.CreateElement(nodeName);
			xmlNode.InnerText = nodeValue;
			parentNode.AppendChild(xmlNode);
		}

		private static string FormatDecimal(decimal? value)
		{
			if (value == null)
			{
				return string.Empty;
			}

			return value.Value.ToString("0.00");
		}

		private static string GetAddressLine1AndLine2(Address address)
		{
			StringBuilder result = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(address.AddressLine1))
			{
				result.Append(address.AddressLine1);
			}
			if (!string.IsNullOrWhiteSpace(address.AddressLine1) && !string.IsNullOrWhiteSpace(address.AddressLine2))
			{
				result.Append(", ");
			}
			if (!string.IsNullOrWhiteSpace(address.AddressLine2))
			{
				result.Append(address.AddressLine2);
			}

			return result.ToString();
		}

		private static string GetProvinceCode(string province, string countryID)
		{
			if (countryID == LocationConstants.CanadaCountryCode || countryID == LocationConstants.USCountryCode)
			{
				return province;
			}

			return "ZZ";
		}

		private static string GetCountryCode(string countryID)
		{
			if (countryID == LocationConstants.CanadaCountryCode)
			{
				return "CAN";
			}
			else if (countryID == LocationConstants.USCountryCode)
			{
				return "USA";
			}

			return countryID;
		}

		#endregion Helper Methods

		#region Helper Classes

		// Class for the T619 section of the T4 XML document
		private class T619
		{
			public string SubmissionRefID { get; private set; }
			public string ReportTypeCode { get; private set; }
			public string TransmitterNumber { get; private set; }
			public string TransmitterTypeIndicator { get; private set; }
			public string SummaryRecordsNumber { get; private set; }
			public string Language { get; private set; }
			public string TransmitterNameLine1 { get; private set; }
			public string TransmitterNameLine2 { get; private set; }

			public string TransmitterAddressLine1 { get; private set; }
			public string TransmitterAddressLine2 { get; private set; }
			public string TransmitterCity { get; private set; }
			public string TransmitterProvinceCode { get; private set; }
			public string TransmitterCountryCode { get; private set; }
			public string TransmitterPostalCode { get; private set; }

			public string ContactName  { get; private set; }
			public string ContactAreaCode { get; private set; }
			public string ContactTelephoneNumber { get; private set; }
			public string ContactExtensionNumber { get; private set; }
			public string ContactEmailAddress { get; private set; }
			public string ContactEmailAddress2 { get; private set; }

			public T619(PXGraph graph, PRTaxFormBatch taxFormBatch)
			{
				SubmissionRefID = taxFormBatch.BatchID;
				ReportTypeCode = taxFormBatch.DocType == TaxFormBatchType.Original ? TaxFormBatchType.Original : TaxFormBatchType.Amendment;
				TransmitterNumber = "MM555555";
				TransmitterTypeIndicator = "3";
				SummaryRecordsNumber = "1";
				Language = "E";

				BAccount businessAccount = SelectFrom<BAccountR>.Where<BAccountR.bAccountID.IsEqual<P.AsInt>>.View.Select(graph, taxFormBatch.OrgBAccountID);
				Address businessAddress = SelectFrom<Address>.Where<Address.addressID.IsEqual<P.AsInt>>.View.Select(graph, businessAccount.DefAddressID);
				Contact businessContact = SelectFrom<Contact>.Where<Contact.contactID.IsEqual<P.AsInt>>.View.Select(graph, businessAccount.DefContactID);

				int accountNameMaxLength = 30;
				string accountName = businessAccount.AcctCD.Trim();
				TransmitterNameLine1 = accountName;
				TransmitterNameLine2 = string.Empty;

				if (TransmitterNameLine1.Length > accountNameMaxLength)
				{
					TransmitterNameLine1 = accountName.Substring(0, accountNameMaxLength);
					TransmitterNameLine2 = accountName.Substring(accountNameMaxLength);
					if (TransmitterNameLine2.Length > accountNameMaxLength)
					{
						TransmitterNameLine2 = TransmitterNameLine2.Substring(0, accountNameMaxLength);
					}
				}

				TransmitterAddressLine1 = businessAddress.AddressLine1;
				TransmitterAddressLine2 = businessAddress.AddressLine2;
				TransmitterCity = businessAddress.City;
				TransmitterProvinceCode = GetProvinceCode(businessAddress.State, businessAddress.CountryID);
				TransmitterCountryCode = GetCountryCode(businessAddress.CountryID);
				TransmitterPostalCode = businessAddress.PostalCode;

				ContactName = businessContact.FullName;
				if (businessContact.Phone1?.Length >= 3)
				{
					ContactAreaCode = businessContact.Phone1.Substring(0, 3);
				}
				ContactTelephoneNumber = businessContact.Phone1;
				ContactEmailAddress = businessContact.EMail;
			}
		}

		private class Production
		{
			public string SubmissionYear { get; private set; }
			public string ReportTypeCode { get; private set; }
			public string TransmitterNumber { get; private set; }
			public string TransmitterTypeIndicator { get; private set; }
			public string TransmitterNameLine1 { get; private set; }
			public string TransmitterNameLine2 { get; private set; }

			public string TransmitterAddressLine1 { get; private set; }
			public string TransmitterAddressLine2 { get; private set; }
			public string TransmitterCity { get; private set; }
			public string TransmitterProvinceCode { get; private set; }
			public string TransmitterPostalCode { get; private set; }

			public string ContactName { get; private set; }
			public string ContactAreaCode { get; private set; }
			public string ContactTelephoneNumber { get; private set; }

			public Production(PXGraph graph, PRTaxFormBatch taxFormBatch)
			{
				SubmissionYear = taxFormBatch.Year;
				ReportTypeCode = TaxFormBatchType.GetRL1TypeCode(taxFormBatch.DocType);
				TransmitterNumber = "NPXXXXXX";
				TransmitterTypeIndicator = "3";

				BAccount businessAccount = SelectFrom<BAccountR>.Where<BAccountR.bAccountID.IsEqual<P.AsInt>>.View.Select(graph, taxFormBatch.OrgBAccountID);
				Address businessAddress = SelectFrom<Address>.Where<Address.addressID.IsEqual<P.AsInt>>.View.Select(graph, businessAccount.DefAddressID);
				Contact businessContact = SelectFrom<Contact>.Where<Contact.contactID.IsEqual<P.AsInt>>.View.Select(graph, businessAccount.DefContactID);

				int accountNameMaxLength = 30;
				string accountName = businessAccount.AcctCD.Trim();
				TransmitterNameLine1 = accountName;
				TransmitterNameLine2 = string.Empty;

				if (TransmitterNameLine1.Length > accountNameMaxLength)
				{
					TransmitterNameLine1 = accountName.Substring(0, accountNameMaxLength);
					TransmitterNameLine2 = accountName.Substring(accountNameMaxLength);
					if (TransmitterNameLine2.Length > accountNameMaxLength)
					{
						TransmitterNameLine2 = TransmitterNameLine2.Substring(0, accountNameMaxLength);
					}
				}
				TransmitterAddressLine1 = businessAddress.AddressLine1;
				TransmitterAddressLine2 = businessAddress.AddressLine2;
				TransmitterCity = businessAddress.City;
				TransmitterProvinceCode = GetProvinceCode(businessAddress.State, businessAddress.CountryID);
				TransmitterPostalCode = businessAddress.PostalCode;

				ContactName = businessContact.FullName;
				if (businessContact.Phone1?.Length >= 3)
				{
					ContactAreaCode = businessContact.Phone1.Substring(0, 3);
				}
				ContactTelephoneNumber = businessContact.Phone1;
			}
		}

		private abstract class Slip
		{
			public abstract int AdditionalFieldsPerPage { get; }
			public abstract string OtherInformationBoxName { get; }
			public abstract string OtherInformationAmountName { get; }
			public int PagesToInsert { get; private set; }

			public virtual Dictionary<string, string> GetPdfFieldValuePairs()
			{
				Dictionary<string, string> result = new Dictionary<string, string>();
				System.Reflection.PropertyInfo[] properties = this.GetType().GetProperties();

				foreach (System.Reflection.PropertyInfo property in properties)
				{
					object value = property.GetValue(this);
					string valueToDisplay;
					if (property.PropertyType == typeof(decimal?) && value != null)
					{
						valueToDisplay = FormatDecimal((decimal)value);
					}
					else if (value != null)
					{
						valueToDisplay = value.ToString();
					}
					else
					{
						valueToDisplay = string.Empty;
					}

					result.Add(property.Name, valueToDisplay);
				}

				return result;
			}

			public virtual Dictionary<string, Dictionary<int, string>> GetAdditionalPdfFieldValuePairs()
			{
				Dictionary<string, Dictionary<int, string>> result = new Dictionary<string, Dictionary<int, string>>();

				int otherFieldIndex = 0;
				int pageIndex = -1;
				foreach (KeyValuePair<string, decimal> boxWithAmount in GetOtherInfoSectionPdfFields())
				{
					otherFieldIndex++;

					if (otherFieldIndex == 1)
					{
						pageIndex++;
					}

					string otherInformationBox = OtherInformationBoxName + otherFieldIndex;
					string otherInformationAmount = OtherInformationAmountName + otherFieldIndex;

					if (!result.TryGetValue(otherInformationBox, out Dictionary<int, string> pageIndexAndBoxName))
					{
						pageIndexAndBoxName = new Dictionary<int, string>();
						result[otherInformationBox] = pageIndexAndBoxName;
					}
					if (!result.TryGetValue(otherInformationAmount, out Dictionary<int, string> pageIndexAndBoxValue))
					{
						pageIndexAndBoxValue = new Dictionary<int, string>();
						result[otherInformationAmount] = pageIndexAndBoxValue;
					}

					pageIndexAndBoxName.Add(pageIndex, boxWithAmount.Key);
					pageIndexAndBoxValue.Add(pageIndex, FormatDecimal(boxWithAmount.Value));

					if (otherFieldIndex >= AdditionalFieldsPerPage)
					{
						otherFieldIndex = 0;
					}
				}

				PagesToInsert = pageIndex;

				return result;
			}

			public abstract Dictionary<string, decimal> GetOtherInfoSectionPdfFields();

			public abstract Dictionary<string, decimal> GetOtherInfoSectionXmlNodes();

			protected abstract string FormatSIN(string socialInsuranceNumber);
		}

		// Class for the T4Slip section of the T4 XML document and for all the fields of the T4 PDF document
		private class T4Slip : Slip
		{
			public override int AdditionalFieldsPerPage => 6;
			public override string OtherInformationBoxName => T4FieldNames.OtherInformationBox;
			public override string OtherInformationAmountName => T4FieldNames.OtherInformationAmount;

			public int Year { get; private set; }
			public string EmployerNameAndAddress { get; private set; }
			public string EmployeeLastName { get; private set; }
			public string EmployeeFirstName { get; private set; }
			public string EmployeeInitial { get; private set; }
			public string EmployeeAddress { get; private set; }
			public string EmployeeAddressLine1 { get; private set; }
			public string EmployeeAddressLine2 { get; private set; }
			public string EmployeeCity { get; private set; }
			public string EmployeeProvince { get; private set; }
			public string EmployeeCountry { get; private set; }
			public string EmployeePostalCode { get; private set; }
			public string SocialInsuranceNumberBox12 { get; private set; }
			public string EmployeeNumber { get; private set; }
			public string EmployersAccountNumberBox54 { get; private set; }
			public string RPPDPSPRegistrationNumberBox50 { get; private set; }
			public bool ExemptCPPQPPBox28 { get; private set; }
			public bool ExemptEIBox28 { get; private set; }
			public bool ExemptPPIPBox28 { get; private set; }
			public string EmploymentCodeBox29 { get; private set; }
			public string ReportTypeCode { get; private set; }
			public string ProvinceOfEmploymentBox10 { get; private set; }
			[T4XmlNode(NodeName = "empt_incamt", BoxName = "Box 14: Employment Income")]
			public decimal? EmploymentIncomeBox14 { get; private set; }
			[T4XmlNode(NodeName = "cpp_cntrb_amt", BoxName = "Box 16: CPP Contributions")]
			public decimal? CPPContributionsBox16 { get; private set; }
			[T4XmlNode(NodeName = "qpp_cntrb_amt", BoxName = "Box 17: QPP Contributions")]
			public decimal? QPPContributionsBox17 { get; private set; }
			[T4XmlNode(NodeName = "empe_eip_amt", BoxName = "Box 18: EI Premiums")]
			public decimal? EIPremiumsBox18 { get; private set; }
			[T4XmlNode(NodeName = "rpp_cntrb_amt", BoxName = "Box 20: RPP Contributions")]
			public decimal? RPPContributionsBox20 { get; private set; }
			[T4XmlNode(NodeName = "itx_ddct_amt", BoxName = "Box 22: Income Tax Deducted")]
			public decimal? IncomeTaxDeductedBox22 { get; private set; }
			[T4XmlNode(NodeName = "ei_insu_ern_amt", BoxName = "Box 24: EI Insurable Earnings")]
			public decimal? EIInsurableEarningsBox24 { get; private set; }
			[T4XmlNode(NodeName = "cpp_qpp_ern_amt", BoxName = "Box 26: Pensionable Earnings")]
			public decimal? PensionableEarningsBox26 { get; private set; }
			[T4XmlNode(NodeName = "unn_dues_amt", BoxName = "Box 44: Union Dues")]
			public decimal? UnionDuesBox44 { get; private set; }
			[T4XmlNode(NodeName = "chrty_dons_amt", BoxName = "Box 46: Charitable Donations")]
			public decimal? CharitableDonationsBox46 { get; private set; }
			[T4XmlNode(NodeName = "padj_amt", BoxName = "Box 52: Pension Adjustment")]
			public decimal? PensionAdjustmentBox52 { get; private set; }
			[T4XmlNode(NodeName = "prov_pip_amt", BoxName = "Box 55: Employee PPIP Premiums")]
			public decimal? EmployeesPPIPPremiumsBox55 { get; private set; }
			[T4XmlNode(NodeName = "prov_insu_ern_amt", BoxName = "Box 56: PPIP Insurable Earnings")]
			public decimal? PPIPInsurableEarningsBox56 { get; private set; }

			private Dictionary<string, decimal> _OtherInfoSectionXmlNodes = new Dictionary<string, decimal>();
			private Dictionary<string, decimal> _OtherInfoSectionPdfFields = new Dictionary<string, decimal>();

			public T4Slip(PRTaxFormGenerator graph, TaxFormRequestParameters parameters)
			{
				PREmployee employee = PREmployee.PK.Find(graph, parameters.EmployeeID);
				Contact employeeContact = PXSelectorAttribute.Select<PREmployee.defContactID>(graph.Caches[typeof(PREmployee)], employee) as Contact;
				Address residenceAddress = PXSelectorAttribute.Select<PREmployee.defAddressID>(graph.Caches[typeof(PREmployee)], employee) as Address;
				int? businessAccountID = employee.ParentBAccountID;

				Branch branch = SelectFrom<Branch>.Where<Branch.bAccountID.IsEqual<P.AsInt>>.View.Select(graph, businessAccountID);
				if (branch != null)
				{
					Organization organization = Organization.PK.Find(graph, branch.OrganizationID);
					if (organization?.FileTaxesByBranches == false)
					{
						businessAccountID = organization?.BAccountID;
					}
				}

				BAccount businessAccount = SelectFrom<BAccountR>.Where<BAccountR.bAccountID.IsEqual<P.AsInt>>.View.Select(graph, businessAccountID);
				Address employmentAddress = PXSelectorAttribute.Select<BAccountR.defAddressID>(graph.Caches[typeof(BAccountR)], businessAccount) as Address;

				StringBuilder employeeAddress = new StringBuilder();

				employeeAddress.AppendLine(GetAddressLine1AndLine2(residenceAddress));
				employeeAddress.AppendLine(residenceAddress.AddressLine3);

				employeeAddress.AppendLine();
				employeeAddress.Append(residenceAddress.City);
				employeeAddress.Append(", ");
				employeeAddress.Append(residenceAddress.State);
				employeeAddress.Append(", ");
				employeeAddress.Append(residenceAddress.CountryID);
				employeeAddress.Append(", ");
				employeeAddress.Append(residenceAddress.PostalCode);

				string payrollAccountNumber = PRTaxReportingAccount.PK.Find(graph, businessAccount.BAccountID)?.CRAPayrollAccountNumber;

				string socialInsuranceNumber = graph.EmployeeAttributes.Select(employee.BAccountID, CanadaReportField.EMP.SocialInsuranceNumber).TopFirst?.Value;
				string employmentCode = graph.EmployeeAttributes.Select(employee.BAccountID, CanadaReportField.EMP.EmploymentCode).TopFirst?.Value;

				int[] branchIDs = GetBranchIDsByBusinessAccount(parameters.OrgBAccountID);
				PRPayment[] allPayments = SelectFrom<PRPayment>
					.Where<PRPayment.branchID.IsIn<P.AsInt>
						.And<PRPayment.employeeID.IsEqual<P.AsInt>>
						.And<P.AsBool.IsEqual<True>
							.Or<PRPayment.released.IsEqual<True>
								.And<PRPayment.voided.IsEqual<False>>
								.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>>>
						.And<Where<DatePart<DatePart.year, PRPayment.transactionDate>, Equal<P.AsInt>>>>
						.View.Select(graph, branchIDs, employee.BAccountID, parameters.IncludeUnreleasedPaychecks, parameters.Year).FirstTableItems.ToArray();

				decimal totalIncome = allPayments.Sum(p => p.TotalEarnings.Value);
				decimal totalTax = 0;

				decimal totalCPP = 0;
				decimal totalQPP = 0;
				decimal totalEI = 0;
				decimal totalEIEarnings = 0;
				decimal totalPPIPPremiums = 0;
				decimal totalPPIPEarnings = 0;
				decimal totalCPPEarnings = 0;
				decimal totalQPPEarnings = 0;

				Dictionary<int, ReportingType> reportingTypes = PRTaxWebServiceDataSlot.GetData(LocationConstants.CanadaCountryCode).ReportingTypes;

				foreach (PRPayment payment in allPayments)
				{
					var paymentTaxes = SelectFrom<PRPaymentTax>
						.InnerJoin<PRTaxCode>
							.On<PRPaymentTax.FK.TaxCode>
						.Where<PRPaymentTax.docType.IsEqual<P.AsString>
							.And<PRPaymentTax.refNbr.IsEqual<P.AsString>>
							.And<PRTaxCode.taxUniqueCode.IsIn<P.AsString>>>
						.View.Select(graph, payment.DocType, payment.RefNbr, new string[] { CanFITTaxUniqueCode, CanCPPTaxUniqueCode, CanQPPTaxUniqueCode, CanEITaxUniqueCode, CanPPIPTaxUniqueCode }).ToArray();

					var paymentEarnings = SelectFrom<PRPaymentEarning>
						.InnerJoin<EPEarningType>
							.On<PRPaymentEarning.FK.EarningType>
						.Where<PRPaymentEarning.docType.IsEqual<P.AsString>
							.And<PRPaymentEarning.refNbr.IsEqual<P.AsString>>
							.And<PREarningType.reportTypeCAN.IsGreater<Zero>>>
						.View.Select(graph, payment.DocType, payment.RefNbr).ToArray();

					var paymentDeductionsContributions = SelectFrom<PRPaymentDeduct>
						.InnerJoin<PRDeductCode>
							.On<PRPaymentDeduct.FK.DeductionCode>
						.Where<PRPaymentDeduct.docType.IsEqual<P.AsString>
							.And<PRPaymentDeduct.refNbr.IsEqual<P.AsString>>
							.And<PRDeductCode.cntReportTypeCAN.IsGreater<Zero>
								.Or<PRDeductCode.dedReportTypeCAN.IsGreater<Zero>>>>
						.View.Select(graph, payment.DocType, payment.RefNbr).ToArray();

					foreach (PXResult<PRPaymentTax, PRTaxCode> record in paymentTaxes)
					{
						PRTaxCode taxCode = record;
						PRPaymentTax paymentTax = record;

						if (taxCode.TaxUniqueCode == CanFITTaxUniqueCode)
						{
							totalTax += (paymentTax.TaxAmount ?? 0m);
						}
						if (taxCode.TaxUniqueCode == CanCPPTaxUniqueCode)
						{
							totalCPP += (paymentTax.TaxAmount ?? 0m);
							totalCPPEarnings += (paymentTax.WageBaseAmount ?? 0m);
						}
						if (taxCode.TaxUniqueCode == CanQPPTaxUniqueCode)
						{
							totalQPP += (paymentTax.TaxAmount ?? 0m);
							totalQPPEarnings += (paymentTax.WageBaseAmount ?? 0m);
						}
						if (taxCode.TaxUniqueCode == CanEITaxUniqueCode)
						{
							totalEI += (paymentTax.TaxAmount ?? 0m);
							totalEIEarnings += (paymentTax.WageBaseAmount ?? 0m);
						}
						if (taxCode.TaxUniqueCode == CanPPIPTaxUniqueCode)
						{
							totalPPIPEarnings += (paymentTax.TaxAmount ?? 0m);
							totalPPIPPremiums += (paymentTax.WageBaseGrossAmt ?? 0m);
						}
					}

					foreach (PXResult<PRPaymentEarning, EPEarningType> record in paymentEarnings)
					{
						PRPaymentEarning paymentEarning = record;
						EPEarningType earningType = record;

						PREarningType prEarningType = earningType?.GetExtension<PREarningType>();

						if (reportingTypes.TryGetValue(prEarningType.ReportTypeCAN.Value, out ReportingType reportingType))
						{
							if (!string.IsNullOrWhiteSpace(reportingType.T4OtherInfoXmlTag))
							{
								if (!_OtherInfoSectionXmlNodes.ContainsKey(reportingType.T4OtherInfoXmlTag))
								{
									_OtherInfoSectionXmlNodes[reportingType.T4OtherInfoXmlTag] = 0;
								}

								string boxNumber = reportingType.TypeName.Replace("Box", string.Empty);
								if (!_OtherInfoSectionPdfFields.ContainsKey(boxNumber))
								{
									_OtherInfoSectionPdfFields[boxNumber] = 0;
								}

								_OtherInfoSectionXmlNodes[reportingType.T4OtherInfoXmlTag] += paymentEarning.Amount.GetValueOrDefault();
								_OtherInfoSectionPdfFields[boxNumber] += paymentEarning.Amount.GetValueOrDefault();
							}
							if (reportingType.TypeName == ReportingType.PensionAdjustment)
							{
								PensionAdjustmentBox52 = PensionAdjustmentBox52.GetValueOrDefault() + paymentEarning.Amount.GetValueOrDefault();
							}
						}
					}

					foreach (PXResult<PRPaymentDeduct, PRDeductCode> record in paymentDeductionsContributions)
					{
						PRPaymentDeduct paymentDeduct = record;
						PRDeductCode deductCode = record;						

						if (deductCode.CntReportTypeCAN > 0 && reportingTypes.TryGetValue(deductCode.CntReportTypeCAN.Value, out ReportingType cntReportingType))
						{
							if (!string.IsNullOrWhiteSpace(cntReportingType.T4OtherInfoXmlTag))
							{
								if (!_OtherInfoSectionXmlNodes.ContainsKey(cntReportingType.T4OtherInfoXmlTag))
								{
									_OtherInfoSectionXmlNodes[cntReportingType.T4OtherInfoXmlTag] = 0;
								}

								string boxNumber = cntReportingType.TypeName.Replace("Box", string.Empty);
								if (!_OtherInfoSectionPdfFields.ContainsKey(boxNumber))
								{
									_OtherInfoSectionPdfFields[boxNumber] = 0;
								}

								_OtherInfoSectionXmlNodes[cntReportingType.T4OtherInfoXmlTag] += paymentDeduct.CntAmount.GetValueOrDefault();
								_OtherInfoSectionPdfFields[boxNumber] += paymentDeduct.CntAmount.GetValueOrDefault();

								List<string> listOfReportingTypesWhichAffectEmploymentIncome = new List<string>
								{
									ReportingCodesForT4OtherSection.BoardAndLodging,
									ReportingCodesForT4OtherSection.PersonalUseOfVehicle,
									ReportingCodesForT4OtherSection.OtherTaxableAllowances
								};

								if (listOfReportingTypesWhichAffectEmploymentIncome.Contains(cntReportingType.TypeName))
								{
									totalIncome += paymentDeduct.CntAmount.GetValueOrDefault();
								}
							}

							if (cntReportingType.TypeName == ReportingType.PensionAdjustment)
							{
								PensionAdjustmentBox52 = PensionAdjustmentBox52.GetValueOrDefault() + paymentDeduct.CntAmount.GetValueOrDefault();
							}
						}

						if (deductCode.DedReportTypeCAN > 0 && reportingTypes.TryGetValue(deductCode.DedReportTypeCAN.Value, out ReportingType dedReportingType))
						{
							if (dedReportingType.TypeName == ReportingType.RPPcontributions)
							{
								RPPContributionsBox20 = RPPContributionsBox20.GetValueOrDefault() + paymentDeduct.DedAmount.GetValueOrDefault();
							}
							else if (dedReportingType.TypeName == ReportingType.UnionDues)
							{
								UnionDuesBox44 = UnionDuesBox44.GetValueOrDefault() + paymentDeduct.DedAmount.GetValueOrDefault();
							}
							else if (dedReportingType.TypeName == ReportingType.CharitableDonations)
							{
								CharitableDonationsBox46 = CharitableDonationsBox46.GetValueOrDefault() + paymentDeduct.DedAmount.GetValueOrDefault();
							}
							else if (dedReportingType.TypeName == ReportingType.PensionAdjustment)
							{
								PensionAdjustmentBox52 = PensionAdjustmentBox52.GetValueOrDefault() + paymentDeduct.DedAmount.GetValueOrDefault();
							}
							else if (!string.IsNullOrWhiteSpace(dedReportingType.T4OtherInfoXmlTag))
							{
								if (!_OtherInfoSectionXmlNodes.ContainsKey(dedReportingType.T4OtherInfoXmlTag))
								{
									_OtherInfoSectionXmlNodes[dedReportingType.T4OtherInfoXmlTag] = 0;
								}

								string boxNumber = dedReportingType.TypeName.Replace("Box", string.Empty);
								if (!_OtherInfoSectionPdfFields.ContainsKey(boxNumber))
								{
									_OtherInfoSectionPdfFields[boxNumber] = 0;
								}

								_OtherInfoSectionXmlNodes[dedReportingType.T4OtherInfoXmlTag] += paymentDeduct.DedAmount.GetValueOrDefault();
								_OtherInfoSectionPdfFields[boxNumber] += paymentDeduct.DedAmount.GetValueOrDefault();
							}
						}
					}
				}

				Year = parameters.Year;

				EmployerNameAndAddress = businessAccount.AcctName;

				EmployeeLastName = employeeContact.LastName;
				EmployeeFirstName = employeeContact.FirstName;
				EmployeeInitial = employeeContact.MidName;

				EmployeeAddress = employeeAddress.ToString();
				EmployeeAddressLine1 = residenceAddress.AddressLine1;
				EmployeeAddressLine2 = residenceAddress.AddressLine2;
				EmployeeCity = residenceAddress.City;
				EmployeeProvince = GetProvinceCode(residenceAddress.State, residenceAddress.CountryID);
				EmployeeCountry = GetCountryCode(residenceAddress.CountryID);
				EmployeePostalCode = residenceAddress.PostalCode;

				ProvinceOfEmploymentBox10 = GetProvinceCode(employmentAddress?.State, employmentAddress?.CountryID);

				SocialInsuranceNumberBox12 = FormatSIN(socialInsuranceNumber);
				EmployeeNumber = employee.AcctCD;

				EmployersAccountNumberBox54 = payrollAccountNumber;
				RPPDPSPRegistrationNumberBox50 = graph.EmployeeAttributes.Select(employee.BAccountID, CanadaReportField.EMP.RPPDPSPRegistrationNumber).TopFirst?.Value;

				EmploymentCodeBox29 = employmentCode;

				ReportTypeCode = parameters.ReportTypeCode;
				bool exemptEmployment =
					employmentCode == EmploymentCodes.PlacementAgencyWorker ||
					employmentCode == EmploymentCodes.TaxiDrivers ||
					employmentCode == EmploymentCodes.Barbers ||
					employmentCode == EmploymentCodes.Fishers;
				if (!exemptEmployment)
				{
					EmploymentIncomeBox14 = totalIncome;
				}

				if (totalCPP > 0)
				{
					CPPContributionsBox16 = totalCPP;
				}
				else if (totalQPP > 0)
				{
					QPPContributionsBox17 = totalQPP;
				}

				EIPremiumsBox18 = totalEI;

				IncomeTaxDeductedBox22 = totalTax;
				EIInsurableEarningsBox24 = 0m;
				PensionableEarningsBox26 = 0m;
				if (!exemptEmployment)
				{
					EIInsurableEarningsBox24 = totalEIEarnings;
					PensionableEarningsBox26 = totalCPPEarnings == 0 ? totalQPPEarnings : totalCPPEarnings;
				}

				EmployeesPPIPPremiumsBox55 = totalPPIPEarnings;
				PPIPInsurableEarningsBox56 = totalPPIPPremiums;
				ExemptCPPQPPBox28 = !(CPPContributionsBox16 > 0 || QPPContributionsBox17 > 0 || PensionableEarningsBox26 > 0);
				ExemptEIBox28 = !(EIPremiumsBox18 > 0 || EIInsurableEarningsBox24 > 0);
				ExemptPPIPBox28 = !(EmployeesPPIPPremiumsBox55 > 0 || PPIPInsurableEarningsBox56 > 0);
			}

			protected override string FormatSIN(string socialInsuranceNumber)
			{
				if (!string.IsNullOrWhiteSpace(socialInsuranceNumber) && socialInsuranceNumber.Length == 9)
				{
					return socialInsuranceNumber.Insert(6, " ").Insert(3, " ");
				}

				return socialInsuranceNumber;
			}

			public override Dictionary<string, decimal> GetOtherInfoSectionPdfFields()
			{
				return _OtherInfoSectionPdfFields;
			}

			public override Dictionary<string, decimal> GetOtherInfoSectionXmlNodes()
			{
				return _OtherInfoSectionXmlNodes;
			}
		}

		// Class for the T4Summary section of the T4 XML document
		private class T4SummaryInfo
		{
			private const string CanCPPEmployerTaxUniqueCode = "475_700000000";
			private const string CanEIEmployerTaxUniqueCode = "473_700000000";

			public string PayrollAccountNumber { get; private set; }

			public string EmployerNameLine1 { get; private set; }
			public string EmployerNameLine2 { get; private set; } // No mapping
			public string EmployerNameLine3 { get; private set; } // No mapping

			public string EmployerAddressLine1 { get; private set; }
			public string EmployerAddressLine2 { get; private set; }
			public string EmployerCity { get; private set; }
			public string EmployerProvince { get; private set; }
			public string EmployerCountry { get; private set; }
			public string EmployerPostalCode { get; private set; }

			public string ContactName { get; private set; }
			public string ContactAreaCode { get; private set; }
			public string ContactTelephoneNumber { get; private set; }
			public string ContactExtension { get; private set; }    // No mapping

			public string TaxationYear { get; private set; }
			public int T4SlipRecordsNumber { get; private set; }

			public string Proprietor1SIN { get; private set; }
			public string Proprietor2SIN { get; private set; }

			public string ReportTypeCode { get; private set; }
			public string FilerAmendmentNote { get; private set; }  // No mapping

			#region Total Amounts
			public decimal TotalEmploymentIncome { get; private set; }  // No mapping
			public decimal TotalCPPContributions { get; private set; }  // No mapping
			public decimal TotalEIPremiums { get; private set; }        // No mapping
			public decimal TotalRPPContributions { get; private set; }  // No mapping
			public decimal TotalIncomeTaxDeducted { get; private set; } // No mapping
			public decimal TotalPensionAdjustment { get; private set; } // No mapping
			public decimal TotalEmployersCPPContributions { get; private set; }
			public decimal TotalEmployersEIremiums { get; private set; }
			#endregion Total Amounts

			public T4SummaryInfo(PRTaxFormGenerator graph, PRTaxFormBatch taxFormBatch, List<int?> employeeIDs)
			{
				BAccount businessAccount = SelectFrom<BAccountR>.Where<BAccountR.bAccountID.IsEqual<P.AsInt>>.View.Select(graph, taxFormBatch.OrgBAccountID);
				string payrollAccountNumber = PRTaxReportingAccount.PK.Find(graph, businessAccount.BAccountID)?.CRAPayrollAccountNumber;
				Address employerAddress = PXSelectorAttribute.Select<BAccountR.defAddressID>(graph.Caches[typeof(BAccountR)], businessAccount) as Address;
				Contact businessContact = SelectFrom<Contact>.Where<Contact.contactID.IsEqual<P.AsInt>>.View.Select(graph, businessAccount.DefContactID);

				PayrollAccountNumber = payrollAccountNumber;

				EmployerNameLine1 = businessAccount.AcctName;

				EmployerAddressLine1 = employerAddress.AddressLine1;
				EmployerAddressLine2 = employerAddress.AddressLine2;
				EmployerCity = employerAddress.City;
				EmployerProvince = GetProvinceCode(employerAddress.State, employerAddress.CountryID);
				EmployerCountry = GetCountryCode(employerAddress.CountryID);
				EmployerPostalCode = employerAddress.PostalCode;

				ContactName = businessContact.FullName;
				if (businessContact.Phone1?.Length >= 3)
				{
					ContactAreaCode = businessContact.Phone1.Substring(0, 3);
				}
				ContactTelephoneNumber = businessContact.Phone1;

				TaxationYear = taxFormBatch.Year;
				T4SlipRecordsNumber = employeeIDs.Count;

				Proprietor1SIN = graph.CompanyAttributes.Select(CanadaReportField.CMP.SocialInsuranceNumberProprietor1).TopFirst?.Value;
				Proprietor2SIN = graph.CompanyAttributes.Select(CanadaReportField.CMP.SocialInsuranceNumberProprietor2).TopFirst?.Value;

				ReportTypeCode = taxFormBatch.DocType == TaxFormBatchType.Original ? TaxFormBatchType.Original : TaxFormBatchType.Amendment;

				var paymentTaxes = SelectFrom<PRPaymentTax>
					.InnerJoin<PRTaxCode>
						.On<PRPaymentTax.FK.TaxCode>
					.InnerJoin<PRPayment>
						.On<PRPaymentTax.FK.Payment>
					.Where<PRPayment.released.IsEqual<True>
						.And<PRPayment.voided.IsEqual<False>>
						.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
						.And<Where<DatePart<DatePart.year, PRPayment.transactionDate>, Equal<P.AsInt>>>
						.And<PRPayment.employeeID.IsIn<P.AsInt>>
						.And<PRTaxCode.taxUniqueCode.IsIn<P.AsString>>>
					.View.Select(graph, taxFormBatch.Year, employeeIDs, new string[] { CanCPPEmployerTaxUniqueCode, CanEIEmployerTaxUniqueCode }).ToArray();

				foreach (PXResult<PRPaymentTax, PRTaxCode> record in paymentTaxes)
				{
					PRTaxCode taxCode = record;
					PRPaymentTax paymentTax = record;

					if (taxCode.TaxUniqueCode == CanCPPEmployerTaxUniqueCode)
					{
						TotalEmployersCPPContributions += (paymentTax.TaxAmount ?? 0m);
					}
					if (taxCode.TaxUniqueCode == CanEIEmployerTaxUniqueCode)
					{
						TotalEmployersEIremiums += (paymentTax.TaxAmount ?? 0m);
					}
				}
			}
		}

		private class RL1Slip : Slip
		{
			public override int AdditionalFieldsPerPage => 4;
			public override string OtherInformationBoxName => RL1FieldNames.AdditionalInformationBox;
			public override string OtherInformationAmountName => RL1FieldNames.AdditionalInformationAmount;

			public string Year { get; private set; }		
			public string StatementCode { get; private set; }
			public string SocialInsuranceNumber { get; private set; }
			public string ReferenceNumber { get; private set; }

			public string EmployeeLastAndFirstName { get; private set; }
			public string EmployeeLastName { get; private set; }
			public string EmployeeFirstName { get; private set; }
			public string EmployeeApartementHouseStreet { get; private set; }
			public string EmployeeCity { get; private set; }
			public string EmployeeProvinceAndPostalCode { get; private set; }
			public string EmployeeProvince { get; private set; }
			public string EmployeePostalCode { get; private set; }

			public string EmployerLastAndFirstName { get; private set; }
			public string EmployerApartementHouseStreet { get; private set; }
			public string EmployerCity  { get; private set; }
			public string EmployerProvinceAndPostalCode { get; private set; }

			public decimal? EmploymentIncomeBoxA { get; private set; }
			public decimal? QPPContributionsBoxB { get; private set; }
			public decimal? EIPremiumsBoxC { get; private set; }
			public decimal? RPPContributionsBoxD { get; private set; }
			public decimal? QuebecIncomeTaxBoxE { get; private set; }
			public decimal? UnionDuesBoxF { get; private set; }
			public decimal? PensionableSalaryQPPBoxG { get; private set; }
			public decimal? QPIPPremiumBoxH { get; private set; }
			public decimal? EligibleSalaryQPIPBoxI { get; private set; }
			public decimal? CommissionsBoxM { get; private set; }
			public decimal? CharitableDonationsBoxN { get; private set; }
			public decimal? OtherIncomeBoxO { get; private set; }
			public decimal? DeferredSalaryBoxQ { get; private set; }
			public decimal? IncomeSituatedOnAReserveBoxR { get; private set; }
			public decimal? TipsBoxS { get; private set; }
			public decimal? TipsAllocatedByTheEmployerBoxT { get; private set; }
			public decimal? PhasedRetirementBoxU { get; private set; }

			public decimal? PrivateHealthServicesPlanBoxJ { get; private set; }
			public decimal? RemoteAreaTripsBoxK { get; private set; }
			public decimal? OtherBenefitsBoxL { get; private set; }
			public decimal? ContrToMultiEmployerInsPlanBoxP { get; private set; }
			public decimal? MealsAndLodgingBoxV { get; private set; }
			public decimal? MotorVehicleBoxW { get; private set; }
			public string CodeCaseO { get; private set; }

			private Dictionary<string, decimal> _AdditionalInformationBoxFields = new Dictionary<string, decimal>();

			private Regex letterDashDigitRegex = new Regex("^[A-Z][-][0-9]+$");
			private Regex threeDigitRegex = new Regex("^[0-9]{3}$");
			private Regex codeCaseORegex = new Regex("^[A-Z]{2}$");

			private const string codeCaseOPrefix = "RZ-";

			public RL1Slip(PRTaxFormGenerator graph, TaxFormRequestParameters parameters)
			{
				PREmployee employee = PREmployee.PK.Find(graph, parameters.EmployeeID);
				Contact employeeContact = PXSelectorAttribute.Select<PREmployee.defContactID>(graph.Caches[typeof(PREmployee)], employee) as Contact;
				Address residenceAddress = PXSelectorAttribute.Select<PREmployee.defAddressID>(graph.Caches[typeof(PREmployee)], employee) as Address;
				BAccount businessAccount = SelectFrom<BAccountR>.Where<BAccountR.bAccountID.IsEqual<P.AsInt>>.View.Select(graph, employee.ParentBAccountID);
				Address employmentAddress = PXSelectorAttribute.Select<BAccountR.defAddressID>(graph.Caches[typeof(BAccountR)], businessAccount) as Address;

				string payrollAccountNumber = PRTaxReportingAccount.PK.Find(graph, businessAccount.BAccountID)?.CRAPayrollAccountNumber;

				string socialInsuranceNumber = graph.EmployeeAttributes.Select(employee.BAccountID, CanadaReportField.EMP.SocialInsuranceNumber).TopFirst?.Value;
				string employmentCode = graph.EmployeeAttributes.Select(employee.BAccountID, CanadaReportField.EMP.EmploymentCode).TopFirst?.Value;

				int[] branchIDs = GetBranchIDsByBusinessAccount(parameters.OrgBAccountID);
				PRPayment[] allPayments = SelectFrom<PRPayment>
					.Where<PRPayment.branchID.IsIn<P.AsInt>
						.And<PRPayment.employeeID.IsEqual<P.AsInt>>
						.And<P.AsBool.IsEqual<True>
							.Or<PRPayment.released.IsEqual<True>
								.And<PRPayment.voided.IsEqual<False>>
								.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>>>
						.And<Where<DatePart<DatePart.year, PRPayment.transactionDate>, Equal<P.AsInt>>>>
						.View.Select(graph, branchIDs, employee.BAccountID, parameters.IncludeUnreleasedPaychecks, parameters.Year).FirstTableItems.ToArray();

				EmploymentIncomeBoxA = allPayments.Sum(p => p.TotalEarnings.Value);

				Dictionary<int, ReportingType> reportingTypes = PRTaxWebServiceDataSlot.GetData(LocationConstants.CanadaCountryCode).ReportingTypes;
				Dictionary<int, ReportingType> qcReportingTypes = PRTaxWebServiceDataSlot.GetData(LocationConstants.CanadaCountryCode).QuebecReportingTypes;

				foreach (PRPayment payment in allPayments)
				{
					PXResult<PRPaymentTax, PRTaxCode>[] paymentTaxes = SelectFrom<PRPaymentTax>
						.InnerJoin<PRTaxCode>
							.On<PRPaymentTax.FK.TaxCode>
						.Where<PRPaymentTax.docType.IsEqual<P.AsString>
							.And<PRPaymentTax.refNbr.IsEqual<P.AsString>>
							.And<PRTaxCode.taxUniqueCode.IsIn<P.AsString>>>
						.View.Select(graph, payment.DocType, payment.RefNbr, new string[] { QCPITTaxUniqueCode, CanQPPTaxUniqueCode, CanEITaxUniqueCode, CanPPIPTaxUniqueCode })
						.Select(item => (PXResult<PRPaymentTax, PRTaxCode>)item).ToArray();

					PXResult<PRPaymentEarning, EPEarningType>[] paymentEarnings = SelectFrom<PRPaymentEarning>
						.InnerJoin<EPEarningType>
							.On<PRPaymentEarning.FK.EarningType>
						.Where<PRPaymentEarning.docType.IsEqual<P.AsString>
							.And<PRPaymentEarning.refNbr.IsEqual<P.AsString>>
							.And<PREarningType.reportTypeCAN.IsGreater<Zero>
								.Or<PREarningType.quebecReportTypeCAN.IsGreater<Zero>>>>
						.View.Select(graph, payment.DocType, payment.RefNbr)
						.Select(item => (PXResult<PRPaymentEarning, EPEarningType>)item).ToArray();

					PXResult<PRPaymentDeduct, PRDeductCode>[] paymentDeductionsContributions = SelectFrom<PRPaymentDeduct>
						.InnerJoin<PRDeductCode>
							.On<PRPaymentDeduct.FK.DeductionCode>
						.Where<PRPaymentDeduct.docType.IsEqual<P.AsString>
							.And<PRPaymentDeduct.refNbr.IsEqual<P.AsString>>
							.And<PRDeductCode.cntReportTypeCAN.IsGreater<Zero>
								.Or<PRDeductCode.dedReportTypeCAN.IsGreater<Zero>>
								.Or<PRDeductCode.cntQuebecReportTypeCAN.IsGreater<Zero>>
								.Or<PRDeductCode.dedQuebecReportTypeCAN.IsGreater<Zero>>>>
						.View.Select(graph, payment.DocType, payment.RefNbr)
						.Select(item => (PXResult<PRPaymentDeduct, PRDeductCode>)item).ToArray();

					ProcessPaymentTaxes(paymentTaxes);
					ProcessPaymentEarnings(paymentEarnings, reportingTypes, qcReportingTypes);
					ProcessPaymentDeductionsContributions(paymentDeductionsContributions, reportingTypes, qcReportingTypes);
				}

				Year = parameters.Year.ToString();

				StatementCode = TaxFormBatchType.GetRL1TypeCode(parameters.ReportTypeCode);
				SocialInsuranceNumber = FormatSIN(socialInsuranceNumber);

				EmployeeLastName = employeeContact.LastName;
				EmployeeFirstName = employeeContact.FirstName;
				EmployeeLastAndFirstName = GetFullName();
				EmployeeApartementHouseStreet = GetApartementHouseStreet(residenceAddress);
				EmployeeCity = residenceAddress.City;
				EmployeeProvince = residenceAddress.State;
				EmployeePostalCode = residenceAddress.PostalCode;
				EmployeeProvinceAndPostalCode = GetProvinceAndPostalCode(residenceAddress);

				EmployerLastAndFirstName = businessAccount.AcctName;
				EmployerApartementHouseStreet = GetApartementHouseStreet(employmentAddress);
				EmployerCity = employmentAddress.City;
				EmployerProvinceAndPostalCode = GetProvinceAndPostalCode(employmentAddress);
			}

			private void ProcessPaymentTaxes(PXResult<PRPaymentTax, PRTaxCode>[] paymentTaxes)
			{
				decimal qcIncomeTax = 0;

				decimal totalQPP = 0;
				decimal totalQPPEarnings = 0;
				decimal totalEI = 0;
				decimal totalEIEarnings = 0;
				decimal totalPPIPPremiums = 0;
				decimal totalPPIPEarnings = 0;

				foreach (PXResult<PRPaymentTax, PRTaxCode> record in paymentTaxes)
				{
					PRTaxCode taxCode = record;
					PRPaymentTax paymentTax = record;

					if (taxCode.TaxUniqueCode == QCPITTaxUniqueCode)
					{
						qcIncomeTax += (paymentTax.TaxAmount ?? 0m);
					}
					if (taxCode.TaxUniqueCode == CanQPPTaxUniqueCode)
					{
						totalQPP += (paymentTax.TaxAmount ?? 0m);
						totalQPPEarnings += (paymentTax.WageBaseGrossAmt ?? 0m);
					}
					if (taxCode.TaxUniqueCode == CanEITaxUniqueCode)
					{
						totalEI += (paymentTax.TaxAmount ?? 0m);
						totalEIEarnings += (paymentTax.WageBaseGrossAmt ?? 0m);
					}
					if (taxCode.TaxUniqueCode == CanPPIPTaxUniqueCode)
					{
						totalPPIPPremiums += (paymentTax.TaxAmount ?? 0m);
						totalPPIPEarnings += (paymentTax.WageBaseGrossAmt ?? 0m);
					}
				}

				QPPContributionsBoxB = totalQPP;
				EIPremiumsBoxC = totalEI;

				QuebecIncomeTaxBoxE = qcIncomeTax;
				PensionableSalaryQPPBoxG = totalQPPEarnings;
				QPIPPremiumBoxH = totalPPIPPremiums;
				EligibleSalaryQPIPBoxI = totalPPIPEarnings;
			}

			private void ProcessPaymentEarnings(PXResult<PRPaymentEarning, EPEarningType>[] paymentEarnings,
				Dictionary<int, ReportingType> reportingTypes,
				Dictionary<int, ReportingType> quebecReportingTypes)
			{
				foreach (PXResult<PRPaymentEarning, EPEarningType> record in paymentEarnings)
				{
					PRPaymentEarning paymentEarning = record;
					EPEarningType earningType = record;

					PREarningType prEarningType = earningType?.GetExtension<PREarningType>();

					if (prEarningType.QuebecReportTypeCAN > 0 &&
						quebecReportingTypes.TryGetValue(prEarningType.QuebecReportTypeCAN.Value, out ReportingType quebecReportingType))
					{
						if (quebecReportingType.TypeName.StartsWith("L-"))
						{
							OtherBenefitsBoxL = OtherBenefitsBoxL.GetValueOrDefault() + paymentEarning.Amount.GetValueOrDefault();
						}
						if (quebecReportingType.TypeName == ReportingType.Commissions)
						{
							CommissionsBoxM = CommissionsBoxM.GetValueOrDefault() + paymentEarning.Amount.GetValueOrDefault();
						}
						if (quebecReportingType.TypeName == ReportingType.DeferredSalaryOrWages)
						{
							DeferredSalaryBoxQ = DeferredSalaryBoxQ.GetValueOrDefault() + paymentEarning.Amount.GetValueOrDefault();
						}
						if (quebecReportingType.TypeName == ReportingType.IncomeReserveOrPremises)
						{
							IncomeSituatedOnAReserveBoxR = IncomeSituatedOnAReserveBoxR.GetValueOrDefault() + paymentEarning.Amount.GetValueOrDefault();
						}
						if (quebecReportingType.TypeName == ReportingType.Tips)
						{
							TipsBoxS = TipsBoxS.GetValueOrDefault() + paymentEarning.Amount.GetValueOrDefault();
						}
						if (quebecReportingType.TypeName == ReportingType.PhasedRetirement)
						{
							PhasedRetirementBoxU = PhasedRetirementBoxU.GetValueOrDefault() + paymentEarning.Amount.GetValueOrDefault();
						}
						if (quebecReportingType.TypeName == ReportingType.MealsAndLodging)
						{
							MealsAndLodgingBoxV = MealsAndLodgingBoxV.GetValueOrDefault() + paymentEarning.Amount.GetValueOrDefault();
						}
						if (quebecReportingType.TypeName == ReportingType.MotorVehicle)
						{
							MotorVehicleBoxW = MotorVehicleBoxW.GetValueOrDefault() + paymentEarning.Amount.GetValueOrDefault();
						}
						ProcessCodeCaseO(quebecReportingType);

						if (letterDashDigitRegex.IsMatch(quebecReportingType.TypeName) || threeDigitRegex.IsMatch(quebecReportingType.TypeName))
						{
							AddAdditionalInfoNode(quebecReportingType.TypeName, paymentEarning.Amount.GetValueOrDefault());
						}
						else if (codeCaseORegex.IsMatch(quebecReportingType.TypeName))
						{
							AddAdditionalInfoNode(string.Concat(codeCaseOPrefix, quebecReportingType.TypeName), paymentEarning.Amount.GetValueOrDefault());
						}
					}
				}
			}

			private void ProcessPaymentDeductionsContributions(PXResult<PRPaymentDeduct, PRDeductCode>[] paymentDeductionsContributions,
				Dictionary<int, ReportingType> reportingTypes,
				Dictionary<int, ReportingType> qcReportingTypes)
			{
				foreach (PXResult<PRPaymentDeduct, PRDeductCode> record in paymentDeductionsContributions)
				{
					PRPaymentDeduct paymentDeduct = record;
					PRDeductCode deductCode = record;

					if (deductCode.DedReportTypeCAN > 0 &&
						reportingTypes.TryGetValue(deductCode.DedReportTypeCAN.Value, out ReportingType dedReportingType))
					{
						if (dedReportingType.TypeName == ReportingType.RPPcontributions)
						{
							RPPContributionsBoxD = RPPContributionsBoxD.GetValueOrDefault() + paymentDeduct.DedAmount.GetValueOrDefault();
						}
						if (dedReportingType.TypeName == ReportingType.UnionDues)
						{
							UnionDuesBoxF = UnionDuesBoxF.GetValueOrDefault() + paymentDeduct.DedAmount.GetValueOrDefault();
						}
						if (dedReportingType.TypeName == ReportingType.CharitableDonations)
						{
							CharitableDonationsBoxN = CharitableDonationsBoxN.GetValueOrDefault() + paymentDeduct.DedAmount.GetValueOrDefault();
						}
					}

					if (deductCode.DedQuebecReportTypeCAN > 0 &&
						qcReportingTypes.TryGetValue(deductCode.DedQuebecReportTypeCAN.Value, out ReportingType dedQuebecReportingType))
					{
						if (dedQuebecReportingType.TypeName == ReportingType.RPPcontributions)
						{
							RPPContributionsBoxD = RPPContributionsBoxD.GetValueOrDefault() + paymentDeduct.DedAmount.GetValueOrDefault();
						}
						if (dedQuebecReportingType.TypeName == ReportingType.UnionDues)
						{
							UnionDuesBoxF = UnionDuesBoxF.GetValueOrDefault() + paymentDeduct.DedAmount.GetValueOrDefault();
						}

						if (letterDashDigitRegex.IsMatch(dedQuebecReportingType.TypeName) || threeDigitRegex.IsMatch(dedQuebecReportingType.TypeName))
						{
							AddAdditionalInfoNode(dedQuebecReportingType.TypeName, paymentDeduct.DedAmount.GetValueOrDefault());
						}
						else if (codeCaseORegex.IsMatch(dedQuebecReportingType.TypeName))
						{
							OtherIncomeBoxO = OtherIncomeBoxO.GetValueOrDefault() + paymentDeduct.DedAmount.GetValueOrDefault();
							AddAdditionalInfoNode(string.Concat(codeCaseOPrefix, dedQuebecReportingType.TypeName), paymentDeduct.DedAmount.GetValueOrDefault());
						}
					}

					if (deductCode.CntQuebecReportTypeCAN > 0 &&
						qcReportingTypes.TryGetValue(deductCode.CntQuebecReportTypeCAN.Value, out ReportingType cntQuebecReportType))
					{
						if (cntQuebecReportType.TypeName == ReportingType.PremiumPrivateHealthServicesPlan)
						{
							PrivateHealthServicesPlanBoxJ = PrivateHealthServicesPlanBoxJ.GetValueOrDefault() + paymentDeduct.CntAmount.GetValueOrDefault();
							AddBenefitToEmploymentIncome(paymentDeduct.CntAmount.GetValueOrDefault());
						}
						if (cntQuebecReportType.TypeName == ReportingType.TripsToRemoteArea)
						{
							RemoteAreaTripsBoxK = RemoteAreaTripsBoxK.GetValueOrDefault() + paymentDeduct.CntAmount.GetValueOrDefault();
							AddBenefitToEmploymentIncome(paymentDeduct.CntAmount.GetValueOrDefault());
						}
						if (cntQuebecReportType.TypeName.StartsWith("L-"))
						{
							OtherBenefitsBoxL = OtherBenefitsBoxL.GetValueOrDefault() + paymentDeduct.CntAmount.GetValueOrDefault();
							AddBenefitToEmploymentIncome(paymentDeduct.CntAmount.GetValueOrDefault());
						}
						if (cntQuebecReportType.TypeName == ReportingType.Commissions)
						{
							CommissionsBoxM = CommissionsBoxM.GetValueOrDefault() + paymentDeduct.CntAmount.GetValueOrDefault();
						}
						if (cntQuebecReportType.TypeName == ReportingType.CharitableDonationsAndGift)
						{
							CharitableDonationsBoxN = CharitableDonationsBoxN.GetValueOrDefault() + paymentDeduct.CntAmount.GetValueOrDefault();
						}
						if (cntQuebecReportType.TypeName == ReportingType.MultiEmployerInsurance)
						{
							ContrToMultiEmployerInsPlanBoxP = ContrToMultiEmployerInsPlanBoxP.GetValueOrDefault() + paymentDeduct.CntAmount.GetValueOrDefault();
							AddBenefitToEmploymentIncome(paymentDeduct.CntAmount.GetValueOrDefault());
						}
						if (cntQuebecReportType.TypeName == ReportingType.DeferredSalaryOrWages)
						{
							DeferredSalaryBoxQ = DeferredSalaryBoxQ.GetValueOrDefault() + paymentDeduct.CntAmount.GetValueOrDefault();
						}
						if (cntQuebecReportType.TypeName == ReportingType.Tips)
						{
							TipsBoxS = TipsBoxS.GetValueOrDefault() + paymentDeduct.CntAmount.GetValueOrDefault();
						}
						if (cntQuebecReportType.TypeName == ReportingType.TipsAllocatedByEmployer)
						{
							TipsAllocatedByTheEmployerBoxT = TipsAllocatedByTheEmployerBoxT.GetValueOrDefault() + paymentDeduct.CntAmount.GetValueOrDefault();
						}
						if (cntQuebecReportType.TypeName == ReportingType.PhasedRetirement)
						{
							PhasedRetirementBoxU = PhasedRetirementBoxU.GetValueOrDefault() + paymentDeduct.CntAmount.GetValueOrDefault();
						}
						if (cntQuebecReportType.TypeName == ReportingType.MealsAndLodging)
						{
							MealsAndLodgingBoxV = MealsAndLodgingBoxV.GetValueOrDefault() + paymentDeduct.CntAmount.GetValueOrDefault();
							AddBenefitToEmploymentIncome(paymentDeduct.CntAmount.GetValueOrDefault());
						}
						if (cntQuebecReportType.TypeName == ReportingType.MotorVehicle)
						{
							MotorVehicleBoxW = MotorVehicleBoxW.GetValueOrDefault() + paymentDeduct.CntAmount.GetValueOrDefault();
							AddBenefitToEmploymentIncome(paymentDeduct.CntAmount.GetValueOrDefault());
						}

						if (letterDashDigitRegex.IsMatch(cntQuebecReportType.TypeName) || threeDigitRegex.IsMatch(cntQuebecReportType.TypeName))
						{
							AddAdditionalInfoNode(cntQuebecReportType.TypeName, paymentDeduct.CntAmount.GetValueOrDefault());
						}
						else if (codeCaseORegex.IsMatch(cntQuebecReportType.TypeName))
						{
							OtherIncomeBoxO = OtherIncomeBoxO.GetValueOrDefault() + paymentDeduct.CntAmount.GetValueOrDefault();
							AddAdditionalInfoNode(string.Concat(codeCaseOPrefix, cntQuebecReportType.TypeName), paymentDeduct.CntAmount.GetValueOrDefault());
						}
					}
				}
			}

			private void ProcessCodeCaseO(ReportingType quebecReportingType)
			{
				if (quebecReportingType.TypeName.Length == 2 &&
					quebecReportingType.TypeName.StartsWith("C") &&
					quebecReportingType.TypeName.StartsWith("R"))
				{
					if (string.IsNullOrEmpty(CodeCaseO))
					{
						CodeCaseO = quebecReportingType.TypeName;
					}
					else if (CodeCaseO != quebecReportingType.TypeName)
					{
						CodeCaseO = "RZ";
					}
				}
			}

			private string GetFullName()
			{
				if (!string.IsNullOrWhiteSpace(EmployeeLastName) && !string.IsNullOrWhiteSpace(EmployeeFirstName))
				{
					return string.Format("{0} {1}", EmployeeLastName, EmployeeFirstName);
				}
				if (!string.IsNullOrWhiteSpace(EmployeeLastName))
				{
					return EmployeeLastName;
				}
				if (!string.IsNullOrWhiteSpace(EmployeeFirstName))
				{
					return EmployeeFirstName;
				}

				return string.Empty;
			}

			private string GetApartementHouseStreet(Address address)
			{
				string line1AndLine2 = GetAddressLine1AndLine2(address);

				if (!string.IsNullOrEmpty(line1AndLine2) && !string.IsNullOrWhiteSpace(address.AddressLine3))
				{
					return string.Format("{0}, {1}", line1AndLine2, address.AddressLine3);
				}
				if (!string.IsNullOrEmpty(line1AndLine2))
				{
					return line1AndLine2;
				}
				if (!string.IsNullOrWhiteSpace(address.AddressLine3))
				{
					return address.AddressLine3;
				}

				return string.Empty;
			}

			private string GetProvinceAndPostalCode(Address address)
			{
				string province = GetProvinceCode(address.State, address.CountryID);
				string postalCode = address.PostalCode;

				if (!string.IsNullOrEmpty(province) && !string.IsNullOrWhiteSpace(postalCode))
				{
					return string.Format("{0} {1}", province, postalCode);
				}
				if (!string.IsNullOrEmpty(province))
				{
					return province;
				}
				if (!string.IsNullOrWhiteSpace(postalCode))
				{
					return postalCode;
				}

				return string.Empty;
			}

			protected override string FormatSIN(string socialInsuranceNumber)
			{
				if (!string.IsNullOrWhiteSpace(socialInsuranceNumber) && socialInsuranceNumber.Length == 9)
				{
					string spaces = new string(' ', 9);
					return socialInsuranceNumber.Insert(6, spaces).Insert(3, spaces);
				}

				return socialInsuranceNumber;
			}

			public override Dictionary<string, decimal> GetOtherInfoSectionPdfFields()
			{
				return _AdditionalInformationBoxFields;
			}

			public override Dictionary<string, decimal> GetOtherInfoSectionXmlNodes()
			{
				return _AdditionalInformationBoxFields;
			}

			public void AddAdditionalInfoNode(string fieldName, decimal amount)
			{
				if (!_AdditionalInformationBoxFields.ContainsKey(fieldName))
				{
					_AdditionalInformationBoxFields[fieldName] = 0;
				}
				_AdditionalInformationBoxFields[fieldName] += amount;
			}

			public void AddBenefitToEmploymentIncome(decimal amount)
			{
				EmploymentIncomeBoxA += amount;
			}
		}

		private class RL1EmployerInfo
		{
			public string EmployerNameLine1 { get; private set; }

			public string EmployerAddressLine1 { get; private set; }
			public string EmployerAddressLine2 { get; private set; }
			public string EmployerCity { get; private set; }
			public string EmployerProvince { get; private set; }
			public string EmployerPostalCode { get; private set; }

			public string FileType = "RS";
			public string EmployerID { get; private set; }
			public string FileNumber { get; private set; }
			public string NEQ { get; private set; }
			public string TransmitterNumber { get; private set; }

			public RL1EmployerInfo(PRTaxFormGenerator graph, PRTaxFormBatch taxFormBatch)
			{
				BAccount businessAccount = SelectFrom<BAccountR>.Where<BAccountR.bAccountID.IsEqual<P.AsInt>>.View.Select(graph, taxFormBatch.OrgBAccountID);
				Address employerAddress = PXSelectorAttribute.Select<BAccountR.defAddressID>(graph.Caches[typeof(BAccountR)], businessAccount) as Address;
				PRTaxReportingAccount taxReportingAccount = PRTaxReportingAccount.PK.Find(graph, taxFormBatch.OrgBAccountID);

				EmployerNameLine1 = businessAccount.AcctCD;

				EmployerAddressLine1 = employerAddress.AddressLine1;
				EmployerAddressLine2 = employerAddress.AddressLine2;
				EmployerCity = employerAddress.City;
				EmployerProvince = GetProvinceCode(employerAddress.State, employerAddress.CountryID);
				EmployerPostalCode = employerAddress.PostalCode;

				if (taxReportingAccount != null)
				{
					EmployerID = taxReportingAccount.RL1IdentificationNumber;
					FileNumber = taxReportingAccount.RL1FileNumber;
					NEQ = taxReportingAccount.RL1QuebecEnterpriseNumber;
					TransmitterNumber = taxReportingAccount.RL1QuebecTransmitterNumber;
				}
			}
		}

		#endregion Helper Classes
	}

	public class TaxFormFile
	{
		public TaxFormFile(string fileType, string data)
		{
			FileType = fileType;
			Data = data;
		}

		public string FileType { get; private set; }
		public string Data { get; private set; }
	}

	public class TaxFormRequestParameters
	{
		public string FormName { get; private set; }
		public string[] FormFileTypes { get; private set; }
		public int Year { get; private set; }
		public int? OrgBAccountID { get; private set; }
		public int? EmployeeID { get; private set; }
		public bool IncludeUnreleasedPaychecks { get; private set; }
		public string ReportTypeCode { get; private set; }

		public TaxFormRequestParameters(string formName, string[] formFileTypes, int year, int? orgBAccountID, int? employeeID, bool includeUnreleasedPaychecks, string reportTypeCode = TaxFormBatchType.Original)
		{
			FormName = formName;
			FormFileTypes = formFileTypes;
			Year = year;
			OrgBAccountID = orgBAccountID;
			EmployeeID = employeeID;
			IncludeUnreleasedPaychecks = includeUnreleasedPaychecks;
			ReportTypeCode = reportTypeCode;
		}
	}

	public class T4XmlNodeAttribute : Attribute
	{
		public string NodeName;
		public string BoxName;
	}
}
