using Audio;
using UnityEngine;

namespace Entities.Enemies
{
    public class WormBody : WormPart
    {
        private bool _awaitingDestruction;    // This object may be set as head the same frame it is destroyed.
        
        [HideInInspector]
        public WormHead HeadRef;
        
        
        public void SetAsHead()
        {
            if (_awaitingDestruction)
                return;
            
            if (HeadRef == null)
            {
                OnKilled();
                return;
            }
            
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
            _awaitingDestruction = true;
            if (TailLink != null)
                TailLink.SetAsHead();
            
            DestroySelf();
        }
    }
}