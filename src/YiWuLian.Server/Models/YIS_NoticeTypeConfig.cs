using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Quick.EntityFrameworkCore.Plus;
using Quick.EntityFrameworkCore.Plus.MySql;
using YiQiDong.Core.JsonConverters;

namespace YiWuLian.Server.Models;

[Table(nameof(YIS_NoticeTypeConfig))]
[MySqlCharSet(DbConsts.MYSQL_DEFAULT_CHARSET)]
[Comment("通知类型配置")]
public class YIS_NoticeTypeConfig : BaseModel
{
    [Comment("通知类型编号")]
    public override string Id { get; set; }
    [Comment("是否启用")]
    [JsonConverter(typeof(JsonBoolConverter))]
    public bool Enable { get; set; }
    [Comment("配置")]
    public string Config { get; set; }
}
