using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Quick.EntityFrameworkCore.Plus;

namespace YiWuLian.Server.Core.NoticeTypes;

public class NoticeTypeManager
{
    public static NoticeTypeManager Instance { get; } = new();
    private Dictionary<string, INoticeType> noticeTypeDict;
    private NoticeTypeManager()
    {
        noticeTypeDict = new();
        AddNoticeType(new SmsNoticeType.NoticeType());
    }

    public INoticeType[] GetAll() => noticeTypeDict.Values.ToArray();

    public INoticeType Get(string noticeTypeId)
    {
        noticeTypeDict.TryGetValue(noticeTypeId, out var noticeType);
        return noticeType;
    }

    public void AddNoticeType(INoticeType noticeType)
    {
        noticeTypeDict[noticeType.Id] = noticeType;
    }

    public T GetNoticeTypeConfig<T>(string noticeTypeId, JsonTypeInfo<T> jsonTypeInfo, out Models.YIS_NoticeTypeConfig configModel)
        where T : new()
    {
        using (var dbContext = new ConfigDbContext())
        {
            configModel = dbContext.Find<Models.YIS_NoticeTypeConfig>(noticeTypeId);
            if (configModel == null)
                return new();
            var configJson = configModel.Config;
            try
            {
                return JsonSerializer.Deserialize(configJson, jsonTypeInfo);
            }
            catch
            {
                return new();
            }
        }
    }

    public void Start()
    {
        using (var dbContext = new ConfigDbContext())
        {
            foreach (var noticeType in noticeTypeDict.Values)
            {
                var configModel = dbContext.Find<Models.YIS_NoticeTypeConfig>(noticeType.Id);
                if (configModel == null || !configModel.Enable)
                    continue;
                noticeType.Start();
            }
        }
    }

    public void Stop()
    {
        foreach (var noticeType in noticeTypeDict.Values)
            noticeType.Stop();
    }

    public void SaveNoticeTypeConfig(Models.YIS_NoticeTypeConfig configModel)
    {
        using (var dbContext = new ConfigDbContext())
        {
            var existConfigModel = dbContext.Find<Models.YIS_NoticeTypeConfig>(configModel.Id);
            if (existConfigModel == null)
            {
                dbContext.Add(configModel);
            }
            else
            {
                existConfigModel.Enable = configModel.Enable;
                existConfigModel.Config = configModel.Config;
                dbContext.Update(existConfigModel);
            }
            dbContext.SaveChanges();
        }
        var noticeType = Get(configModel.Id);
        if (configModel.Enable)
            noticeType.Start();
        else
            noticeType.Stop();
    }

    public void SaveNoticeLog(Models.YIS_NoticeLog noticeLog)
    {
        using (var dbContext = new ConfigDbContext())
        {
            dbContext.Add(noticeLog);
            dbContext.SaveChanges();    
        }
    }
}
