using System.Collections;
using DG.Tweening;
using Singletons;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class ScreenFader : SingletonBehaviour<ScreenFader>
    {
        [SerializeField]
        private Image _fadeImage;
        
        [SerializeField]
        private float _fadeSpeed = 2f;
        
        
        private IEnumerator Start()
        {
            yield return StartScene();
        }


        private IEnumerator StartScene()
        {
            _fadeImage.enabled = true;
            _fadeImage.color = Color.black;
            yield return new WaitForSeconds(1f);
         
            _fadeImage.DOFade(0f, _fadeSpeed).onComplete += () => _fadeImage.enabled = false;
        }


        public void EndScene(int sceneNumber)
        {
            _fadeImage.enabled = true;
            _fadeImage.color = Color.clear;
            _fadeImage.DOFade(1f, _fadeSpeed).onComplete += () =>
            {
                SceneManager.LoadScene(sceneNumber);
                StartCoroutine(StartScene());
            };
        }
    }
}