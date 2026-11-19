namespace Stilargo.Infrastructure.Input
{
    /// <summary>
    /// Input System control paths and composite names in one place, so a rebinding
    /// or a typo is fixed here instead of being hunted down across the action setup.
    /// </summary>
    public static class InputBindingPaths
    {
        public const string Vector2DComposite = "2DVector";
        public const string CompositeUp = "Up";
        public const string CompositeDown = "Down";
        public const string CompositeLeft = "Left";
        public const string CompositeRight = "Right";

        public const string KeyW = "<Keyboard>/w";
        public const string KeyA = "<Keyboard>/a";
        public const string KeyS = "<Keyboard>/s";
        public const string KeyD = "<Keyboard>/d";
        public const string KeyUpArrow = "<Keyboard>/upArrow";
        public const string KeyDownArrow = "<Keyboard>/downArrow";
        public const string KeyLeftArrow = "<Keyboard>/leftArrow";
        public const string KeyRightArrow = "<Keyboard>/rightArrow";
        public const string KeyLeftShift = "<Keyboard>/leftShift";
        public const string KeyRightShift = "<Keyboard>/rightShift";
        public const string KeyF = "<Keyboard>/f";

        public const string MouseScrollY = "<Mouse>/scroll/y";
        public const string MouseLeftButton = "<Mouse>/leftButton";
        public const string MouseRightButton = "<Mouse>/rightButton";
        public const string PointerPosition = "<Pointer>/position";
    }
}
