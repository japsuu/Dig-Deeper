using UnityEngine;

namespace Utilities
{
    [ExecuteInEditMode]
    public class RotatingObject : MonoBehaviour
    {
        [SerializeField]
        private GameObject _root;
        
        public float speed = 6f;
        
        
        public void SetEnabled(bool enable)
        {
            enabled = enable;
            _root.SetActive(enable);
        }

        
        private void Update()
        {
            transform.Rotate(0f, 0f, Time.deltaTime * speed * 40f);
        }
    }
}