using Quick.Sms.CMCC.CloudMAS;
using YiQiDong.Agent;
using YiQiDong.Core.Utils;

namespace YiWuLian.Server.Core.NoticeTypes.SmsNoticeType;

public class NoticeType : INoticeType
{
    public const string NOTICE_TYPE_ID = "sms";
    public string Id => NOTICE_TYPE_ID;

    public string Name => "短信";

    public bool HasTarget => true;

    public string TargetTypeId => "mobile";

    public string TargetTypeName => "电话号码";

    public bool Enable => config.Enable;

    private HttpApiClient httpApiClient;
    private SmsConfigModel config;    

    public NoticeType()
    {
        config = Agent.Instance.Config.SmsConfig;
    }

    public void Start()
    {
        Stop();        
        httpApiClient = new HttpApiClient(new HttpApiClientOptions()
        {
            url = config.ApiAddress,
            addSerial = config.AddSerial,
            apId = config.ApId,
            ecName = config.EcName,
            secretKey = config.SecretKey,
            sign = config.Sign
        });
        AgentContext.LogInfo($"[短信通知类型]已启动");
    }

    public void Stop()
    {
        if (httpApiClient != null)
        {
            httpApiClient = null;
            AgentContext.LogInfo($"[短信通知类型]已停止");
        }
    }

    public void SendNotice(Models.YIS_Device device, YlIotProtocol.V1.Commands.SendNotice.Request request)
    {
        var client = httpApiClient;
        if (client == null)
            throw new ApplicationException("短信通知类型当前未启动！");
        var noticeResult = "成功";
        try
        {
            httpApiClient.SendTemplateSmsAsync(
                request.Target,
                config.TemplateId,
                [
                    request.Content
                ],
                CancellationToken.None).Wait();
            AgentContext.LogTrace($"[短信通知类型]{device}向[{request.Target}]发送短信成功。");
        }
        catch (Exception ex)
        {
            AgentContext.LogTrace($"[短信通知类型]{device}向[{request.Target}]发送短信失败，原因：{ExceptionUtils.GetExceptionMessage(ex)}");
            noticeResult = $"失败，原因：{ExceptionUtils.GetExceptionMessage(ex)}";
            throw new IOException($"向[{request.Target}]发送短信时出错。", ex);
        }
        finally
        {
            if (device != null)
            {
                NoticeTypeManager.Instance.SaveNoticeLog(new()
                {
                    DeviceId = device.Id,
                    DeviceName = device.Name,
                    NoticeTarget = request.Target,
                    NoticeContent = request.Content,
                    NoticeResult = noticeResult,
                    NoticeType = Name,
                    Time = DateTime.Now
                });
            }
        }
    }
}
