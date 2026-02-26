using Quick.EntityFrameworkCore.Plus;

namespace YiWuLian.Server.Core.NoticeTypes;

public class NoticeTypeManager
{
    public static NoticeTypeManager Instance { get; } = new();
    private Dictionary<string, INoticeType> noticeTypeDict;

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

    public void Start()
    {
        noticeTypeDict = new();
        AddNoticeType(new SmsNoticeType.NoticeType());

        using (var dbContext = new ConfigDbContext())
        {
            foreach (var noticeType in noticeTypeDict.Values)
            {
                if (noticeType.Enable)
                    noticeType.Start();
            }
        }
    }

    public void Stop()
    {
        foreach (var noticeType in noticeTypeDict.Values)
            if (noticeType.Enable)
                noticeType.Stop();
        noticeTypeDict = null;
    }

    /// <summary>
    /// 保存通知日志
    /// </summary>
    /// <param name="noticeLog"></param>
    public void SaveNoticeLog(Models.YIS_NoticeLog noticeLog)
    {
        using (var dbContext = new ConfigDbContext())
        {
            dbContext.Add(noticeLog);
            dbContext.SaveChanges();
        }
    }

    /// <summary>
    /// 保存连接日志
    /// </summary>
    /// <param name="systemLog"></param>
    public void SaveConnectionLog(Models.YIS_ConnectionLog systemLog)
    {
        using (var dbContext = new ConfigDbContext())
        {
            dbContext.Add(systemLog);
            dbContext.SaveChanges();
        }
    }
}
