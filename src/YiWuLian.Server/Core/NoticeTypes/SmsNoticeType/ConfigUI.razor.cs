using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using MudBlazor;
using YiQiDong.Core.Utils;

namespace YiWuLian.Server.Core.NoticeTypes.SmsNoticeType;

public partial class ConfigUI : ComponentBase
{
    private Models.YIS_NoticeTypeConfig configModel;
    private SmsConfigModel config;
    private string targets;
    private string content = @"2025-07-16_16:35:00在数据中心,1栋,1楼开发机房的温湿度1发生温度过高,值为30";

    public bool Enable
    {
        get
        {
            return configModel.Enable;
        }
        set
        {
            configModel.Enable = value;
            configModel.Config = JsonSerializer.Serialize(config, SmsConfigModelSerializerContext.Default2.SmsConfigModel);
            NoticeTypeManager.Instance.SaveNoticeTypeConfig(configModel);
        }
    }

    protected override void OnInitialized()
    {
        config = NoticeTypeManager.Instance.GetNoticeTypeConfig(NoticeType.NOTICE_TYPE_ID, SmsConfigModelSerializerContext.Default2.SmsConfigModel, out configModel);
        if (configModel == null)
            configModel = new()
            {
                Id = NoticeType.NOTICE_TYPE_ID,
                Enable = false
            };
    }
    private bool isSending = false;
    [Inject]
    public IDialogService DialogService { get; set; }

    private async Task Send()
    {
        var noticeTypeiId = "sms";
        var noticeType = NoticeTypeManager.Instance.Get(noticeTypeiId);
        isSending = true;
        await Task.Run(async () =>
        {
            try
            {
                noticeType.SendNotice(null, new YlIotProtocol.V1.Commands.SendNotice.Request()
                {
                    Target = targets,
                    NoticeTypeId = noticeTypeiId,
                    Content = content
                });
                await DialogService.ShowMessageBox(new MessageBoxOptions()
                {
                    Title = "成功",
                    Message = $"发送成功！"
                });
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessageBox(new MessageBoxOptions()
                {
                    Title = "错误",
                    Message = $"发送时出错，原因：" + ExceptionUtils.GetExceptionMessage(ex)
                });
            }
            finally
            {
                isSending = false;
                await InvokeAsync(StateHasChanged);
            }
        });
    }
}
