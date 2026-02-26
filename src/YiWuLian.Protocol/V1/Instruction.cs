using Quick.Protocol;

namespace YlIotProtocol.V1;

/// <summary>
/// 易物联协议V1架构
/// </summary>
public class Instruction
{
    public static QpInstruction Instance = new QpInstruction()
    {
        Id = typeof(Instruction).Namespace,
        Name = "易物联协议V1",
        CommandInfos =
        [
            QpCommandInfo.Create(new Commands.Register.Request()),
            QpCommandInfo.Create(new Commands.GetNoticeTypes.Request()),
            QpCommandInfo.Create(new Commands.SendNotice.Request())
        ]
    };
}
