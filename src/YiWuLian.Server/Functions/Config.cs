using YiWuLian.Server.Utils;
using Quick.EntityFrameworkCore.Plus;
using Quick.Fields;
using YiQiDong.Agent;
using YiQiDong.Core.Functions;
using YiQiDong.Protocol.V1.Model;
using System.Text.Json;
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
        appConfigHandler = DbUtils.AppDbUtils.GetDbContextConfigHandler(config.AppDbType, config.AppDbConfig);
        appConfigHandler.GetModelsJsonSerializerContextFunc = t => ModelsJsonSerializerContext.Default2;
        return config;
    }

    public override void WriteConfig(ConfigModel model)
    {
        if (appConfigHandler != null)
            model.AppDbConfig = DbUtils.AppDbUtils.SerializerConfigHandler(appConfigHandler);
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

    protected FieldForGet getAppDbGroup(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        var model = requestModel ?? Model;
        var appDbConfigRequest = new FieldsForPostContainer();
        //准备Children
        var appDbConfigRequestFieldList = new List<FieldForPost>();
        if (isReadOnly)
        {
            appDbConfigRequestFieldList.Add
            (
                new()
                {
                    Id = AbstractDbContextConfigHandler.Quick_EntityFrameworkCore_Plus_AbstractDbContextConfigHandler_IsReadOnly,
                    Value = isReadOnly.ToString()
                }
            );
        }
        if (request != null)
        {
            var prefixFieldIds = new string[] { nameof(Model.AppDbConfig) };
            //准备FieldIds
            if (request.IsFieldIdsMatch(prefixFieldIds))
            {
                appDbConfigRequest.FieldIds = request.FieldIds.Skip(prefixFieldIds.Length).ToArray();
            }
            else
            {
                //设置FieldIds不为null，代表是Post请求
                appDbConfigRequest.FieldIds = [];
            }
            var otherChildren = request.GetField(nameof(Model.AppDbConfig)).Children;
            if (otherChildren != null)
                appDbConfigRequestFieldList.AddRange(otherChildren);
        }
        appDbConfigRequest.Fields = appDbConfigRequestFieldList.ToArray();
        appConfigHandler = DbUtils.AppDbUtils.GetDbContextConfigHandler(model.AppDbType);
        appConfigHandler.GetModelsJsonSerializerContextFunc = t => ModelsJsonSerializerContext.Default2;
        var list = new List<FieldForGet>
            {
                new ()
                {
                    Id=nameof(Model.AppDbType),
                    Name="数据库类型",
                    Type= FieldType.InputSelect,
                    InputSelect_Options = DbUtils.AppDbUtils.GetDbTypeDict(),
                    PostOnChanged=true,
                    Value = model.AppDbType,
                    Input_ReadOnly = isReadOnly
                }
            };
        if (model.AppDbType == "Quick.EntityFrameworkCore.Plus.SQLite.SQLiteDbContextConfigHandler")
        {
            list.Add(new()
            {
                Name = "警告",
                Input_AllowBlank = false,
                Type = FieldType.Alert,
                Theme = FieldTheme.Danger,
                Description = "一般只在开发和调试的情况下使用SQLite数据库，生产环境建议使用其他数据库！",
                Input_ReadOnly = isReadOnly
            });
        }
        list.AddRange(
        [
            new ()
            {
                Id=nameof(Model.AppDbConfig),
                Type = FieldType.ContainerRow,
                Children=
                [
                    new ()
                    {
                        Type = FieldType.HtmlDiv,
                        ColumnWidth = 0,
                        Children =  appConfigHandler.QuickFields_Request(appDbConfigRequest)
                    }
                ]
            },
            new FieldForGet()
            {
                Type = FieldType.ContainerRow,
                Margin = 1
            }
        ]);
        return new FieldForGet()
        {
            Type = FieldType.ContainerGroup,
            Name = "数据库连接",
            Children = list.ToArray()
        };
    }

    protected FieldForGet getDeviceServiceGroup(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        var model = requestModel ?? Model;
        return new FieldForGet()
        {
            Id = nameof(model.DeviceServiceConfig),
            Type = FieldType.ContainerGroup,
            Name = "设备服务",
            Children =
            [
                new()
                {
                    Id = nameof(model.DeviceServiceConfig.Password),
                    Name = "密码",
                    Description = "默认密码：123456",
                    Input_AllowBlank = false,
                    Type = FieldType.InputText,
                    Value = model.DeviceServiceConfig.Password,
                    Input_ReadOnly = isReadOnly
                },
                new ()
                {
                    Name = "管道",
                    Type = FieldType.ContainerGroup,
                    MarginBottom = 1,
                    Children =
                    [
                        new()
                        {
                            Id = nameof(model.DeviceServiceConfig.PipeEnable),
                            Name = "启用",
                            Description = "接口地址示例：qp.pipe://./YiWuLian.Server.ClientInterface",
                            Input_AllowBlank = false,
                            Type = FieldType.InputSelect,
                            InputSelect_Options = new Dictionary<string,string>()
                            {
                                [true.ToString()] = "是",
                                [false.ToString()] = "否"
                            },
                            PostOnChanged = true,
                            Value = model.DeviceServiceConfig.PipeEnable.ToString(),
                            Input_ReadOnly = isReadOnly
                        },
                        new()
                        {
                            Id = nameof(model.DeviceServiceConfig.PipeName),
                            Name = "管道名称",
                            Input_AllowBlank = false,
                            Type = model.DeviceServiceConfig.PipeEnable ? FieldType.InputText: FieldType.InputHidden,
                            Value = model.DeviceServiceConfig.PipeName,
                            Input_ReadOnly = isReadOnly
                        },
                    ]
                },
                new ()
                {
                    Name = "WebSocket",
                    Type = FieldType.ContainerGroup,
                    MarginBottom = 1,
                    Children =
                    [
                        new()
                        {
                            Id = nameof(model.DeviceServiceConfig.WebSocketEnable),
                            Name = "启用",
                            Description = "接口地址示例：qp.ws://127.0.0.1:8097/ws/device",
                            Input_AllowBlank = false,
                            Type = FieldType.InputSelect,
                            InputSelect_Options = new Dictionary<string,string>()
                            {
                                [true.ToString()] = "是",
                                [false.ToString()] = "否"
                            },
                            PostOnChanged = true,
                            Value = model.DeviceServiceConfig.WebSocketEnable.ToString(),
                            Input_ReadOnly = isReadOnly
                        }      
                    ]
                },
                new ()
                {
                    Name = "TCP",
                    Type = FieldType.ContainerGroup,
                    MarginBottom = 1,
                    Children =
                    [
                        new()
                        {
                            Id = nameof(model.DeviceServiceConfig.TcpEnable),
                            Name = "启用",
                            Description = "接口地址示例：qp.tcp://127.0.0.1:8097",
                            Input_AllowBlank = false,
                            Type = FieldType.InputSelect,
                            InputSelect_Options = new Dictionary<string,string>()
                            {
                                [true.ToString()] = "是",
                                [false.ToString()] = "否"
                            },
                            PostOnChanged = true,
                            Value = model.DeviceServiceConfig.TcpEnable.ToString(),
                            Input_ReadOnly = isReadOnly
                        },
                        new()
                        {
                            Id = nameof(model.DeviceServiceConfig.TcpListenAddress),
                            Name = "监听地址",
                            Input_AllowBlank = false,
                            Type = model.DeviceServiceConfig.TcpEnable ? FieldType.InputText: FieldType.InputHidden,
                            Value = model.DeviceServiceConfig.TcpListenAddress,
                            Input_ReadOnly = isReadOnly
                        },
                        new()
                        {
                            Id = nameof(model.DeviceServiceConfig.TcpListenPort),
                            Name = "监听端口",
                            Input_AllowBlank = false,
                            Type = model.DeviceServiceConfig.TcpEnable ? FieldType.InputText: FieldType.InputHidden,
                            Value = model.DeviceServiceConfig.TcpListenPort.ToString(),
                            Input_ReadOnly = isReadOnly
                        }
                    ]
                }
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
        return new List<FieldForGet>()
            {
                new FieldForGet()
                {
                    Type = FieldType.ContainerTab,
                    Children =
                    [
                        getBasicConfigGroup(request,requestModel,isReadOnly),
                        getAppDbGroup(request,requestModel,isReadOnly),
                        getDeviceServiceGroup(request,requestModel,isReadOnly),
                        getSmsServiceGroup(request,requestModel,isReadOnly)
                    ]
                }
            };
    }
}
