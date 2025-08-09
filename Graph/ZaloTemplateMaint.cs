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
        public PXAction<ZaloTemplate> insertMergeField;
        [PXUIField(DisplayName = "Insert Merge Field", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable InsertMergeField(PXAdapter adapter)
        {
            var template = @"
🏪 Chi nhánh: {{Branch}}
📅 Ngày kiểm kê: {{CheckDate}}
👤 Người kiểm: {{CheckedBy}}
📝 Số phiếu: {{DocumentNbr}}

💰 Tổng chênh lệch: {{TotalDifference}}

📋 Chi tiết chênh lệch:
{{DifferenceDetails}}

Trạng thái: {{Status}}
Tổng số lượng kiểm: {{TotalPhysicalQty}}
Tổng chênh lệch số lượng: {{TotalVarQty}}";

            Templates.Ask(
                "Template Mẫu",
                "Copy template mẫu dưới đây:\n\n" + template,
                MessageButtons.OK,
                MessageIcon.Information
            );

            return adapter.Get();
        }
        #endregion

        #region Events
        protected virtual void _(Events.RowSelected<ZaloTemplate> e)
        {
            var row = e.Row;
            if (row == null) return;

            if (!string.IsNullOrEmpty(row.ReferenceNbr))
            {
                row.PreviewMessage = ZaloMessage.BuildPreviewMessage(row.Body, this, row.ReferenceNbr);
            }
        }

        protected virtual void _(Events.RowPersisting<ZaloTemplate> e)
        {
            if (e.Row == null) return;

            // Validate template as before
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