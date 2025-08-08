using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System;
using System.Collections;
using System.Linq;
using ANCafe;

namespace ANCafe
{
    public class ZaloTemplateMaint : PXGraph<ZaloTemplateMaint, ZaloTemplate>
    {
        #region Views
        [PXViewName("Zalo Templates")]
        public SelectFrom<ZaloTemplate>.View Templates;

        public PXSave<ZaloTemplate> Save;
        public PXCancel<ZaloTemplate> Cancel;
        public PXInsert<ZaloTemplate> Insert;
        public PXDelete<ZaloTemplate> Delete;
        public PXFirst<ZaloTemplate> First;
        public PXPrevious<ZaloTemplate> Previous;
        public PXNext<ZaloTemplate> Next;
        public PXLast<ZaloTemplate> Last;
        #endregion

        #region Actions
        public PXAction<ZaloTemplate> showPreview;
        [PXUIField(DisplayName = "Show Preview Message", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ShowPreview(PXAdapter adapter)
        {
            var template = Templates.Current;
            if (template != null && !string.IsNullOrEmpty(template.Body))
            {
                try
                {
                    // Validate template trước khi tạo preview
                    var validationResult = ZaloMessage.ValidateTemplate(template.Body);

                    if (!validationResult.IsValid)
                    {
                        Templates.Ask(LocalizableMessages.ValidationErrorTitle,
                            string.Format(LocalizableMessages.TemplateInvalidCannotPreview, validationResult.ErrorMessage),
                            MessageButtons.OK, MessageIcon.Error);
                        return adapter.Get();
                    }

                    // Chỉ tạo preview nếu template hợp lệ
                    var previewMessage = ZaloMessage.BuildPreviewMessage(template.Body);

                    Templates.Ask(LocalizableMessages.PreviewMessageTitle, previewMessage, MessageButtons.OK, MessageIcon.Information);
                }
                catch (Exception ex)
                {
                    Templates.Ask(LocalizableMessages.ErrorTitle, string.Format(LocalizableMessages.PreviewError, ex.Message), MessageButtons.OK, MessageIcon.Error);
                }
            }
            else
            {
                Templates.Ask(LocalizableMessages.WarningTitle, LocalizableMessages.EnterTemplateFirst, MessageButtons.OK, MessageIcon.Warning);
            }

            return adapter.Get();
        }

        public PXAction<ZaloTemplate> insertMergeField;
        [PXUIField(DisplayName = "Insert Merge Field", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable InsertMergeField(PXAdapter adapter)
        {
            var mergeFields = ZaloMessage.GetValidMergeFields();
            var fieldList = string.Join(", ", mergeFields.Select(f => $"{{{{{f}}}}}"));

            Templates.Ask(LocalizableMessages.AvailableMergeFieldsTitle,
                string.Format(LocalizableMessages.AvailableMergeFields, fieldList),
                MessageButtons.OK,
                MessageIcon.Information);

            return adapter.Get();
        }

        public PXAction<ZaloTemplate> validateTemplate;
        [PXUIField(DisplayName = "Validate Template", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ValidateTemplate(PXAdapter adapter)
        {
            var template = Templates.Current;
            if (template != null && !string.IsNullOrEmpty(template.Body))
            {
                var validationResult = ZaloMessage.ValidateTemplate(template.Body);

                if (validationResult.IsValid)
                {
                    Templates.Ask(LocalizableMessages.ValidationResultTitle, LocalizableMessages.TemplateValid, MessageButtons.OK, MessageIcon.Information);
                }
                else
                {
                    Templates.Ask(LocalizableMessages.ValidationErrorTitle, string.Format(LocalizableMessages.TemplateInvalid, validationResult.ErrorMessage), MessageButtons.OK, MessageIcon.Error);
                }
            }
            else
            {
                Templates.Ask(LocalizableMessages.WarningTitle, LocalizableMessages.EnterTemplateToValidate, MessageButtons.OK, MessageIcon.Warning);
            }

            return adapter.Get();
        }
        #endregion

        #region Events
        protected virtual void _(Events.RowSelected<ZaloTemplate> e)
        {
            if (e.Row == null) return;

            var hasBody = !string.IsNullOrEmpty(e.Row.Body);
            showPreview.SetEnabled(hasBody);
            validateTemplate.SetEnabled(hasBody);
            insertMergeField.SetEnabled(true);
        }

        protected virtual void _(Events.RowPersisting<ZaloTemplate> e)
        {
            if (e.Row == null) return;

            // Validate template trước khi save
            if (!string.IsNullOrEmpty(e.Row.Body))
            {
                var validationResult = ZaloMessage.ValidateTemplate(e.Row.Body);

                if (!validationResult.IsValid)
                {
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    // Acuminator disable once PX1051 NonLocalizableString [Justification]
                    e.Cache.RaiseExceptionHandling<ZaloTemplate.body>(e.Row,
                        e.Row.Body,
                        new PXSetPropertyException(validationResult.ErrorMessage, PXErrorLevel.Error));
                }
            }

            // Check duplicate Description (Template Code)
            var existing = SelectFrom<ZaloTemplate>
                .Where<ZaloTemplate.description.IsEqual<P.AsString>
                    .And<ZaloTemplate.noteID.IsNotEqual<P.AsGuid>>>
                .View.Select(this, e.Row.Description, e.Row.NoteID);

            if (existing.Count > 0)
            {
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                e.Cache.RaiseExceptionHandling<ZaloTemplate.description>(e.Row,
                    e.Row.Description,
                    new PXSetPropertyException("Template Code '{0}' đã tồn tại. Vui lòng sử dụng mã khác.", PXErrorLevel.Error, e.Row.Description));
            }
        }

        protected virtual void _(Events.FieldUpdated<ZaloTemplate, ZaloTemplate.description> e)
        {
            if (e.Row == null || string.IsNullOrEmpty(e.NewValue?.ToString())) return;

            // Auto-generate subject nếu trống
            if (string.IsNullOrEmpty(e.Row.Subject))
            {
                e.Row.Subject = $"Zalo Template {e.NewValue}";
            }
        }

        protected virtual void _(Events.FieldVerifying<ZaloTemplate, ZaloTemplate.description> e)
        {
            if (e.Row == null || e.NewValue == null) return;

            string desc = e.NewValue.ToString();

            // Tự động loại bỏ khoảng trắng thay vì throw exception
            if (desc.Contains(" "))
            {
                desc = desc.Replace(" ", "");
                e.NewValue = desc;
            }

            if (desc.Length > 100)
            {
                throw new PXSetPropertyException(LocalizableMessages.TemplateCodeTooLong);
            }
        }

        protected virtual void _(Events.FieldUpdating<ZaloTemplate, ZaloTemplate.description> e)
        {
            if (e.NewValue is string desc && !string.IsNullOrEmpty(desc))
            {
                // Loại bỏ toàn bộ khoảng trắng
                e.NewValue = desc.Replace(" ", "");
            }
        }

        protected virtual void _(Events.RowInserting<ZaloTemplate> e)
        {
            if (e.Row == null) return;

            // Không set NotificationID = null cho PXDBIdentity - để system tự sinh
            // PXDBIdentity sẽ tự động tạo ID dương

            // Auto-fill Description nếu cần
            if (string.IsNullOrEmpty(e.Row.Description))
            {
                e.Row.Description = $"ZaloTemplate{DateTime.Now:yyyyMMddHHmmss}";
            }

            // Tự động generate SubscriberID nếu chưa có
            if (e.Row.SubscriberID == null || e.Row.SubscriberID == Guid.Empty)
            {
                e.Row.SubscriberID = Guid.NewGuid();
                PXTrace.WriteInformation("Generated new SubscriberID for ZaloTemplate: {0}", e.Row.SubscriberID);
            }

            // Đảm bảo Screen field có giá trị hợp lệ
            if (string.IsNullOrEmpty(e.Row.Screen))
            {
                e.Row.Screen = null; // Cho phép null để template có thể dùng cho mọi screen
            }

            // Đảm bảo ActivityType có giá trị
            if (string.IsNullOrEmpty(e.Row.ActivityType))
            {
                e.Row.ActivityType = "Zalo";
            }
        }
        #endregion
    }
}