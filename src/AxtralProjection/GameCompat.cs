using System;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;

namespace AxtralProjection;
internal static class GameCompat
{
    private static MethodInfo? message;
    private static ParameterInfo[]? parameters;
    public static ManualLogSource? Log { get; set; }
    public static void Message(Character character, MessageHud.MessageType type, string text)
    {
        if (!character) return;
        try
        {
            if (message == null)
            {
                message = typeof(Character).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == "Message" && m.GetParameters().Length >= 2
                        && m.GetParameters()[0].ParameterType == typeof(MessageHud.MessageType)
                        && m.GetParameters()[1].ParameterType == typeof(string))
                    .OrderBy(m => m.GetParameters().Length).First();
                parameters = message.GetParameters();
            }
            var args = new object?[parameters!.Length]; args[0] = type; args[1] = text;
            for (int i = 2; i < args.Length; i++)
                args[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue
                    : parameters[i].ParameterType.IsValueType ? Activator.CreateInstance(parameters[i].ParameterType) : null;
            message.Invoke(character, args);
        }
        catch (Exception e) { Log?.LogWarning("HUD message failed: " + (e.InnerException ?? e).Message); }
    }
    public static void RaiseWoodCutting(Player player, float amount)
    {
        if (amount > 0) player.RaiseSkill(Skills.SkillType.WoodCutting, amount);
    }
}
