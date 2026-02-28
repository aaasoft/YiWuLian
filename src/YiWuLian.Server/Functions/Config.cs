using Quick.EntityFrameworkCore.Plus;
using Quick.Fields;
using YiQiDong.Agent;
using YiQiDong.Core.Functions;
using YiQiDong.Protocol.V1.Model;
using YiWuLian.Server.Models;

namespace YiWuLian.Server.Functions;

public class Config : ModelJsonConfig<ConfigModel>
{
    public static Config Instance { get; private set; }
    public override string Name => "配置";
    public override int ExecuteTimeout => 5 * 60 * 1000;

    public Config()
     : base(
        ConfigModelSerializerContext.Default2.ConfigModel,
        AgentContext.Container?.ContainerFolder ?? string.Empty,
        () => AgentContext.Container == null ? false : AgentContext.Container.AutoStart)
    {
        Instance = this;
    }

    private IDbContextConfigHandler appConfigHandler;

    public override ConfigModel ReadConfig()
    {
        var config = base.ReadConfig();
        appConfigHandler = DbUtils.GetDbContextConfigHandler(config.AppDb.DbType, t => ModelsJsonSerializerContext.Default2, config.AppDb.DbConnectionParameter);
        return config;
    }

    public override void WriteConfig(ConfigModel model)
    {
        if (appConfigHandler != null)
            model.AppDb.DbConnectionParameter = DbUtils.SerializerConfigHandler(appConfigHandler);
        base.WriteConfig(model);
    }

    protected FieldForGet getBasicConfigGroup(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        var model = requestModel ?? Model;

        return new FieldForGet()
        {
            Type = FieldType.ContainerGroup,
            Name = "基础配置",
            Children =
            [
                new()
                    {
                        Id =  nameof(model.Urls),
                        Name = "Web服务地址",
                        Description = null,
                        Input_AllowBlank = false,
                        Input_RegularExpression = "^http://((\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])\\.(\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])\\.(\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])\\.(\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])|\\*)(\\:([0-9]|[1-9]\\d{1,3}|[1-5]\\d{4}|6[0-5]{2}[0-3][0-5]))?$",
                        Type =  FieldType.InputText,
                        Value = model.Urls,
                        Input_ReadOnly = isReadOnly
                    },
                    new()
                    {
                        Id =  nameof(model.Password),
                        Name = "管理密码",
                        Description = "默认密码：123456",
                        Input_AllowBlank = false,
                        Type =  FieldType.InputText,
                        Value = model.Password,
                        Input_ReadOnly = isReadOnly
                    },
                    new()
                    {
                        Id =  nameof(model.NoticeConnectionChangedDurationMinutes),
                        Name = "通知连接变化持续分钟数",
                        Description = null,
                        Input_AllowBlank = false,
                        Type =  FieldType.InputNumber,
                        Value = model.NoticeConnectionChangedDurationMinutes.ToString(),
                        Input_ReadOnly = isReadOnly
                    },
            ]
        };
    }

    protected FieldForGet getSmsServiceGroup(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        var model = requestModel ?? Model;

        return new FieldForGet()
        {
            Id = nameof(model.SmsConfig),
            Type = FieldType.ContainerGroup,
            Name = "短信服务",
            Children =
            [
                new()
                {
                    Id = nameof(model.SmsConfig.Enable),
                    Name = "启用",
                    Input_AllowBlank = false,
                    Type = FieldType.InputSelect,
                    InputSelect_Options = new Dictionary<string,string>()
                    {
                        [true.ToString()] = "是",
                        [false.ToString()] = "否"
                    },
                    PostOnChanged = true,
                    Value = model.SmsConfig.Enable.ToString(),
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id =  nameof(model.SmsConfig.ApiAddress),
                    Name = "API地址",
                    Description = null,
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.SmsConfig.ApiAddress,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id =  nameof(model.SmsConfig.EcName),
                    Name = "企业名称",
                    Description = null,
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.SmsConfig.EcName,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id =  nameof(model.SmsConfig.AddSerial),
                    Name = "扩展码",
                    Description = null,
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.SmsConfig.AddSerial,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id =  nameof(model.SmsConfig.ApId),
                    Name = "接口账号用户名",
                    Description = null,
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.SmsConfig.ApId,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id =  nameof(model.SmsConfig.SecretKey),
                    Name = "接口账号密码",
                    Description = null,
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.SmsConfig.SecretKey,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id =  nameof(model.SmsConfig.Sign),
                    Name = "签名编码",
                    Description = null,
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.SmsConfig.Sign,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id =  nameof(model.SmsConfig.TemplateId),
                    Name = "模板ID",
                    Description = null,
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.SmsConfig.TemplateId,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id =  nameof(model.SmsConfig.AdminNoticeTarget),
                    Name = "管理员通知目标",
                    Description = "如果为空，则不通知",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.SmsConfig.AdminNoticeTarget,
                    Input_ReadOnly = isReadOnly
                }
            ]
        };
    }

    protected override List<FieldForGet> innerGet(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        var model = requestModel ?? Model;
        var defaultModel = ConfigModel.Default;
        return new List<FieldForGet>()
            {
                new FieldForGet()
                {
                    Type = FieldType.ContainerTab,
                    Children =
                    [
                        getBasicConfigGroup(request,requestModel,isReadOnly),
                        model.AppDb.GetDbConfigGroup(request,isReadOnly,nameof(ConfigModel.AppDb),"数据库",
                            t=>new ConfigDbContext(t),
                            t => ModelsJsonSerializerContext.Default2,
                            ()=> appConfigHandler,
                            t=>appConfigHandler=t),
                        model.DeviceServiceConfig.GetConfigGroup(isReadOnly,nameof(model.DeviceServiceConfig),"设备服务", defaultModel.DeviceServiceConfig),
                        getSmsServiceGroup(request,requestModel,isReadOnly)
                    ]
                }
            };
    }
}
