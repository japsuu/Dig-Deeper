using JSAM;

namespace Audio
{
    public enum OneShotSoundType
    {
        STATION_SELL_MATERIALS,
        STATION_RECEIVE_CREDITS,
        STATION_ENTER,
        DRILL_CANNON_FIRE,
        TYPEWRITER_TYPE,
        WORM_HIT_NORMAL,
        WORM_HIT_CRITICAL,
        WORM_SPAWN,
        WORM_DEATH,
        WORM_ATTACK,
        THUMP_SMALL,
        THUMP_LARGE,
        DRILL_CRASH_WARNING,
        DRILL_REBOOTING,
        DRILL_EXPLOSION,
        UI_BUTTON_CLICK,
        UI_BUTTON_HOVER,
    }
    
    public enum LoopingSoundType
    {
        DRILL_CANNON_ROTATE,
        DRILL_DRILLING,
        DRILL_FALLING,
        WORM_DIGGING,
    }
    
    public static class AudioLayer
    {
        public static void PlaySoundOneShot(OneShotSoundType clip)
        {
            JSAMSound? jsamSound = OneShotSoundTypeToJsamSound(clip);
            
            if (jsamSound == null)
                return;
            
            AudioManager.PlaySound(jsamSound.Value);
        }
        
        
        public static void PlaySoundLoop(LoopingSoundType clip)
        {
            JSAMSound? jsamSound = LoopingSoundTypeToJsamSound(clip);
            
            if (jsamSound == null)
                return;
            
            AudioManager.PlaySound(jsamSound.Value);
        }
        
        
        public static void StopSoundLoop(LoopingSoundType clip)
        {
            JSAMSound? jsamSound = LoopingSoundTypeToJsamSound(clip);
            
            if (jsamSound == null)
                return;
            
            AudioManager.StopSound(jsamSound.Value);
        }
        
        
        public static void StopAllMusic(bool instantly)
        {
            AudioManager.StopAllMusic(instantly);
        }
        
        
        private static JSAMSound? OneShotSoundTypeToJsamSound(OneShotSoundType soundType)
        {
            return soundType switch
            {
                OneShotSoundType.STATION_RECEIVE_CREDITS => JSAMSound.Station_Coins,
                OneShotSoundType.STATION_ENTER => JSAMSound.Station_Enter,
                OneShotSoundType.DRILL_CANNON_FIRE => JSAMSound.Cannon_Fire,
                OneShotSoundType.WORM_HIT_NORMAL => JSAMSound.Worm_Damaged_Normal,
                OneShotSoundType.WORM_HIT_CRITICAL => JSAMSound.Worm_Damaged_Critical,
                OneShotSoundType.WORM_SPAWN => JSAMSound.Worm_Spawn,
                OneShotSoundType.WORM_DEATH => JSAMSound.Worm_Death,
                OneShotSoundType.WORM_ATTACK => JSAMSound.Worm_Attack,
                OneShotSoundType.THUMP_SMALL => JSAMSound.Drill_Thump_Small,
                OneShotSoundType.THUMP_LARGE => JSAMSound.Drill_Thump_Large,
                OneShotSoundType.DRILL_CRASH_WARNING => JSAMSound.Drill_Warning,
                OneShotSoundType.DRILL_REBOOTING => JSAMSound.Drill_Reboot,
                OneShotSoundType.DRILL_EXPLOSION => JSAMSound.Drill_Explosion,
                OneShotSoundType.UI_BUTTON_CLICK => JSAMSound.UI_Menu_Open,
                _ => null
            };
        }
        
        
        private static JSAMSound? LoopingSoundTypeToJsamSound(LoopingSoundType soundType)
        {
            return soundType switch
            {
                LoopingSoundType.DRILL_CANNON_ROTATE => JSAMSound.Cannon_Turn_Loop,
                LoopingSoundType.DRILL_DRILLING => JSAMSound.Drill_Drilling,
                LoopingSoundType.DRILL_FALLING => JSAMSound.Drill_Falling,
                LoopingSoundType.WORM_DIGGING => JSAMSound.Worm_Dig,
                _ => null
            };
        }
    }
}