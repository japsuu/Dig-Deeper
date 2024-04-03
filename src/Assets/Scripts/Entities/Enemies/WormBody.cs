using Audio;
using UnityEngine;

namespace Entities.Enemies
{
    public class WormBody : WormPart
    {
        [HideInInspector]
        public WormHead HeadRef;
        
        
        public void SetAsHead()
        {
            WormHead head = Instantiate(HeadRef, transform.position, Quaternion.identity);
            head.SetTailLink(TailLink);
            
            Destroy(gameObject);
        }


        public override void Damage(int amount)
        {
            AudioLayer.PlaySoundOneShot(OneShotSoundType.WORM_HIT_NORMAL);
            
            base.Damage(amount);
        }


        protected override void OnKilled()
        {
            if (TailLink != null)
                TailLink.SetAsHead();
            
            DestroySelf();
        }
    }
}