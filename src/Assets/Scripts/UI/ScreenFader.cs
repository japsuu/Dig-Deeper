using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Singletons;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Fades the screen in and out on scene transitions.
    /// </summary>
    public class ScreenFader : SingletonBehaviour<ScreenFader>
    {
        [SerializeField]
        private Image _fadeImage;
        
        [SerializeField]
        private float _fadeSpeed = 2f;

        private TweenerCore<Color, Color, ColorOptions> _tweener;
        
        
        private IEnumerator Start()
        {
            yield return StartScene();
        }


        private IEnumerator StartScene()
        {
            _tweener?.Kill();
            _fadeImage.enabled = true;
            _fadeImage.color = Color.black;
            yield return new WaitForSeconds(1f);

            _tweener = _fadeImage.DOFade(0f, _fadeSpeed);
            _tweener.onComplete += () => _fadeImage.enabled = false;
        }


        public void EndScene(int sceneNumber)
        {
            _tweener?.Kill();
            _fadeImage.enabled = true;
            _fadeImage.color = Color.clear;
            _tweener = _fadeImage.DOFade(1f, _fadeSpeed);
            _tweener.onComplete += () =>
            {
                SceneManager.LoadScene(sceneNumber);
                StartCoroutine(StartScene());
            };
        }
    }
}