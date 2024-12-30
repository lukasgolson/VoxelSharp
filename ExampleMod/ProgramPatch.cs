using HarmonyLib;
using VoxelSharp.Client;

namespace ExampleMod
{
    [HarmonyPatch(typeof(Program), "Main")]
    public static class ProgramPatch
    {
        public static Client ExposedClient { get; private set; }

        [HarmonyPostfix]
        public static void Postfix(Client __state)
        {
            // Capture the Client instance
            ExposedClient = __state;
        }
    }
}