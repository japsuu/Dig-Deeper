using JSAM;
using UnityEngine;

namespace Audio
{
    /// <summary>
    /// Wrapper around the JSAM AudioManager.
    /// Exists because JSAM enum names might change if audio files are renamed.
    /// </summary>
    public static class AudioLayer
    {
        public static void PlaySoundOneShot(OneShotSoundType clip, Vector3 position = default)
        {
            JSAMSound? jsamSound = OneShotSoundTypeToJsamSound(clip);
            
            if (jsamSound == null)
                return;
            
            if (position != default)
                AudioManager.PlaySound(jsamSound.Value, position);
            else
                AudioManager.PlaySound(jsamSound.Value);
        }
        
        
        public static void PlaySoundOneShot(OneShotSoundType clip, Transform parent)
        {
            JSAMSound? jsamSound = OneShotSoundTypeToJsamSound(clip);
            
            if (jsamSound == null)
                return;
            
            if (parent != null)
                AudioManager.PlaySound(jsamSound.Value, parent);
            else
                AudioManager.PlaySound(jsamSound.Value);
        }
        
        
        public static void PlaySoundLoop(LoopingSoundType clip, Transform parent = null)
        {
            JSAMSound? jsamSound = LoopingSoundTypeToJsamSound(clip);
            
            if (jsamSound == null)
                return;
            
            if (parent != null)
                AudioManager.PlaySound(jsamSound.Value, parent);
            else
                AudioManager.PlaySound(jsamSound.Value);
        }
        
        
        public static void StopSoundLoop(LoopingSoundType clip, Transform parent = null)
        {
            JSAMSound? jsamSound = LoopingSoundTypeToJsamSound(clip);
            
            if (jsamSound == null)
                return;
            
            if (parent != null)
                AudioManager.StopSound(jsamSound.Value, parent);
            else
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
                OneShotSoundType.DRILL_DAMAGED => JSAMSound.Drill_Damaged,
                OneShotSoundType.UI_BUTTON_CLICK => JSAMSound.UI_Menu_Open,
                OneShotSoundType.MISC_EXPLOSION => JSAMSound.Explosion,
                OneShotSoundType.MISC_HEAL => JSAMSound.Heal,
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