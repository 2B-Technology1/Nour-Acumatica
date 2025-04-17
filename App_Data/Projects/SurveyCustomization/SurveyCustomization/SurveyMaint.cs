using System;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace SurveyCustomization
{
  public class SurveyMaint : PXGraph<SurveyMaint>
  {
        // Primary View
        public SelectFrom<SurveyTemplate>.View SurveyTemplateMaster;

        // Secondary View
        //public SelectFrom<SurveyQuestion>.View QuestionGrid;
        public SelectFrom<SurveyQuestion>
           .Where<SurveyQuestion.surveyTemplateID
               .IsEqual<SurveyTemplate.surveyTemplateID.FromCurrent>>
           .View QuestionGrid;

        public PXSave<SurveyTemplate> Save;
        public PXCancel<SurveyTemplate> Cancel;



        #region
        protected virtual void _(Events.RowSelected<SurveyQuestion> e)
        {
            SurveyQuestion row = e.Row;

            PXUIFieldAttribute.SetEnabled<SurveyQuestion.answertext>(e.Cache, row, false);
            PXUIFieldAttribute.SetVisible<SurveyQuestion.answertext>(e.Cache, row, false);

            PXUIFieldAttribute.SetEnabled<SurveyQuestion.answerYesNo>(e.Cache, row, false);
            PXUIFieldAttribute.SetVisible<SurveyQuestion.answerYesNo>(e.Cache, row, false);

            PXUIFieldAttribute.SetEnabled<SurveyQuestion.answerRating>(e.Cache, row, false);
            PXUIFieldAttribute.SetVisible<SurveyQuestion.answerRating>(e.Cache, row, false);

            if (row.QuestionType == 0)
            {
                PXUIFieldAttribute.SetEnabled<SurveyQuestion.answertext>(e.Cache, row, true);
                PXUIFieldAttribute.SetVisible<SurveyQuestion.answertext>(e.Cache, row, true);
            }
            else if (row.QuestionType == 1)
            {
                PXUIFieldAttribute.SetEnabled<SurveyQuestion.answerYesNo>(e.Cache, row, true);
                PXUIFieldAttribute.SetVisible<SurveyQuestion.answerYesNo>(e.Cache, row, true);
            }
            else if (row.QuestionType == 2)
            {
                PXUIFieldAttribute.SetEnabled<SurveyQuestion.answerRating>(e.Cache, row, true);
                PXUIFieldAttribute.SetVisible<SurveyQuestion.answerRating>(e.Cache, row, true);
            }



        }
        #endregion


  }
}