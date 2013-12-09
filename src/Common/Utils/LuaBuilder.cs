using LuaInterface;

namespace Common.Utils
{
    /// <summary>
    /// Creates and configures Lua instances.
    /// </summary>
    public static class LuaBuilder
    {
        /// <summary>
        /// Creates a new <see cref="Lua"/> instance with default static helper methods already registered.
        /// </summary>
        /// <returns>The newly created <see cref="Lua"/> instance.</returns>
        public static Lua Default()
        {
            var lua = new Lua();
            LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(Log));
            LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(StringUtils));
            LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(MathUtils));
            LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(RandomUtils));
            LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(ColorUtils));
            return lua;
        }
    }
}
