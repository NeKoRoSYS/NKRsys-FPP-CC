using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace NeKoRoSYS.InputManagement
{
    public static class EnhancedTouchInitializer
    {
    	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => EnhancedTouchSupport.Enable();
    }
}