
using System.Collections;
using Mirror;

public struct PlayableMessage : NetworkMessage
{
    public IPlayableContext context;
}

public static class ContextSerializationHelper
{

    public static void WriteSkill(this NetworkWriter writer, ISkill skill)
    {
        writer.WriteString(skill.GetType().Name);
        writer.WriteString(skill.GetType().Assembly.GetName().Name);
        skill.Serialize(writer);
    }

    public static ISkill ReadSkill(this NetworkReader reader)
    {
        var type = reader.ReadString();
        var asm = reader.ReadString();
        var inst = (ISkill)System.Activator.CreateInstance(asm, type).Unwrap();
        return inst.Deserialize(reader);
    }

    public static void WriteSkillInfo(this NetworkWriter writer, SkillInfo info)
    {
        writer.Write((byte)info.SourceTileInput);
        writer.Write((byte)info.TileOutput);
        writer.Write((byte)info.Trigger);
        if (info.Conditions != null)
        {
            writer.Write(info.Conditions.Length);
            foreach (var cond in info.Conditions)
                writer.Write((byte)cond);
        }
        else writer.WriteInt(0);
        writer.WriteSkill(info.Skill);
    }

    public static SkillInfo ReadSkillInfo(this NetworkReader reader)
    {
        var info = new SkillInfo
        {
            SourceTileInput = (SkillChain)reader.ReadByte(),
            TileOutput = (SkillChain)reader.ReadByte(),
            Trigger = (SkillTrigger)reader.ReadByte()
        };
        var arr = new SkillChain[reader.ReadInt()];
        for (int i = 0; i < arr.Length; i++)
            arr[i] = (SkillChain)reader.ReadByte();
        info.Conditions = arr;
        info.Skill = reader.ReadSkill();
        return info;
    }

    public static void WriteCardContext(this NetworkWriter writer, CardContext context) => context.SerializeWithoutUnit(writer);
    public static CardContext ReadCardContext(this NetworkReader reader) => (CardContext)new CardContext().DeserializeWithoutUnit(reader);

    public static void WritePlayable(this NetworkWriter writer, IPlayableContext context)
    {
        writer.WriteString(context.GetType().Name);
        writer.WriteString(context.GetType().Assembly.GetName().Name);
        context.Serialize(writer);
    }

    public static IPlayableContext ReadPlayable(this NetworkReader reader)
    {
        var type = reader.ReadString();
        var asm = reader.ReadString();
        var inst = (IPlayableContext)System.Activator.CreateInstance(asm, type).Unwrap();
        return inst.Deserialize(reader);
    }
}

public interface IPlayableContextWithUnit : IPlayableContext
{
    Unit Unit { get; }
}

public interface IClientPlayable
{
    bool ManualSending => false;
    IEnumerator PlayOnClient();
}


public interface IServerPlayable
{
    bool PlayOnServer();
}

public interface IServerWaitPlayable
{
    NetworkConnection Owner { get; }
    IEnumerator PlayOnServer();
}

public interface IPlayableContext
{
    bool Queueable { get; }
    string Name { get; }

    void Serialize(NetworkWriter writer);
    IPlayableContext Deserialize(NetworkReader reader);
}