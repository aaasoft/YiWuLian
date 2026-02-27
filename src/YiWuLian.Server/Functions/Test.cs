using Quick.Fields;
using Quick.Utils;
using YiQiDong.Agent;
using YiQiDong.Core;
using YiQiDong.Protocol.V1.Model;
using YiWuLian.Server.Core.NoticeTypes;

namespace YiWuLian.Server.Functions;

public class Test : AbstractFunction
{
    public override string Name => "测试";
    public override bool IsVisiable() => AgentContext.Container.AutoStart;

    private const string TXT_NOTICE_TYPE = nameof(TXT_NOTICE_TYPE);
    private const string TXT_NOTICE_TARGET = nameof(TXT_NOTICE_TARGET);
    private const string TXT_NOTICE_CONTENT = nameof(TXT_NOTICE_CONTENT);

    private const string BTN_SEND = nameof(BTN_SEND);

    public override FieldForGet[] Execute(FunctionRequest request)
    {
        var list = new List<FieldForGet>();
        if (request != null)
        {
            if (request.IsFieldIdsMatch(BTN_SEND))
            {
                var noticeTypeId = request.GetFieldValue(TXT_NOTICE_TYPE);
                var noticeTarget = request.GetFieldValue(TXT_NOTICE_TARGET);
                var noticeContent = request.GetFieldValue(TXT_NOTICE_CONTENT);
                var noticeType = NoticeTypeManager.Instance.Get(noticeTypeId);
                try
                {
                    noticeType.SendNotice(new Models.YIS_Device()
                    {
                        Id = "test",
                        Name = "测试"
                    },
                    new YlIotProtocol.V1.Commands.SendNotice.Request()
                    {
                        Target = noticeTarget,
                        NoticeTypeId = noticeTypeId,
                        Content = noticeContent
                    });
                    list.Add(new()
                    {
                        Name = "成功",
                        Type = FieldType.Alert,
                        Theme = FieldTheme.Success,
                        Description = $"对[{noticeTarget}]发送[{noticeType.Name}]通知成功。"
                    });
                }
                catch (Exception ex)
                {
                    list.Add(new()
                    {
                        Name = "错误",
                        Type = FieldType.Alert,
                        Theme = FieldTheme.Danger,
                        Description = $"对[{noticeTarget}]发送[{noticeType.Name}]通知时出错，原因：{ExceptionUtils.GetExceptionMessage(ex)}"
                    });
                }
            }
        }
        list.AddRange([
            new ()
            {
                Id = TXT_NOTICE_TYPE,
                Name = "通知类型",
                Type = FieldType.InputSelect,
                InputSelect_Options = Core.NoticeTypes.NoticeTypeManager.Instance.GetAll().ToDictionary(t => t.Id, t => t.Name),
                Value = request != null ? request.GetFieldValue(TXT_NOTICE_TYPE) : null ?? Core.NoticeTypes.NoticeTypeManager.Instance.GetAll().FirstOrDefault()?.Id
            },
            new ()
            {
                Id = TXT_NOTICE_TARGET,
                Name = "通知目标",
                Type = FieldType.InputText,
                Value = request != null ? request.GetFieldValue(TXT_NOTICE_TARGET) : null
            },
            new ()
            {
                Id = TXT_NOTICE_CONTENT,
                Name = "通知内容",
                Type = FieldType.InputTextArea,
                Value = request != null ? request.GetFieldValue(TXT_NOTICE_CONTENT) : null
            },
            new ()
            {
                Id= BTN_SEND,
                Name="发送",
                Type =  FieldType.Button
            }
        ]);
        return list.ToArray();
    }
}
