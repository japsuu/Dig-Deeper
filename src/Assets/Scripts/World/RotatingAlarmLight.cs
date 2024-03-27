using UnityEngine;

namespace World
{
    [ExecuteInEditMode]
    public class RotatingAlarmLight : MonoBehaviour
    {
        [SerializeField]
        private GameObject _lightRoot;
        
        public float speed = 6f;
        
        
        public void SetEnabled(bool enable)
        {
            enabled = enable;
            _lightRoot.SetActive(enable);
        }

        
        private void Update()
        {
            transform.Rotate(0f, 0f, Time.deltaTime * speed * 40f);
        }
    }
}