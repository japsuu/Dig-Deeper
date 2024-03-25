using UnityEngine;

namespace Entities.Enemies
{
    public abstract class WormPart : DamageableEntity
    {
        [SerializeField]
        public SpriteRenderer SpriteRenderer;

        [SerializeField]
        [Tooltip("Reference to the \"previous\" link in the chain.")]
        protected WormBody TailLink;

        [SerializeField]
        [Tooltip("Changes the attachment radius of the link.")]
        public float LinkRadius = 0.5f;

        
        protected override void Awake()
        {
            base.Awake();

            if (SpriteRenderer == null)
                SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }


        public void SetTailLink(WormBody tailLink)
        {
            TailLink = tailLink;
        }
        
        
        public void SetOrderInLayer(int orderInLayer)
        {
            SpriteRenderer.sortingOrder = orderInLayer;
        }


        protected void DestroyRecursive()
        {
            if (TailLink != null)
                TailLink.DestroyRecursive();
            
            DestroySelf();
        }


        protected void DestroySelf()
        {
            Destroy(gameObject);
        }
        
        
        protected virtual void Update()
        {
            if (TailLink != null)
            {
                MoveAndRotateTailLink();
            }
        }


        private void MoveAndRotateTailLink()
        {
            // Move the tail link to the correct position
            Vector2 thisLinkPos = transform.position;
            Vector2 previousLinkPos = TailLink.transform.position;
            Vector2 direction = thisLinkPos - previousLinkPos;
            direction.Normalize();
            Vector2 targetPosition = thisLinkPos - direction * LinkRadius;
            
            // Rotate the tail link to face this link
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            TailLink.transform.position = targetPosition;
            TailLink.transform.rotation = Quaternion.Euler(0, 0, angle);
        }


        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, LinkRadius);
        }
    }
}