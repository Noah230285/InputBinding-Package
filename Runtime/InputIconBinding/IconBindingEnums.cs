namespace UtilEssentials.InputIconBinding
{
    /// <summary>
    /// The type of an icon bind, which defines whether its animated, and if so what type of animation
    /// </summary>
    public enum IconType
    {
        Static = 0,
        Animated_ReverseLoop,
        Animated_RolloverLoop
    }

    /// <summary>
    /// Input categories set up for icon binding
    /// </summary>
    public enum InputBindingCategories
    {
        Keyboard = 0,
        Mouse,
        Gamepad,
        Custom
    }

    /// <summary>
    /// The infomation that is going to be used to search for an input binding icon
    /// </summary>
    public enum InputBindingSearchType
    {
        None = -1,
        Category,
        NameID,
        Tags,
        CategoryAndNameID,
        NameIDAndTags,
        CategoryAndTags,
        CategoryAndNameIDAndTags,
    }
}
