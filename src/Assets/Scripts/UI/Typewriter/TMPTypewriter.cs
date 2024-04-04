using System.Collections;
using Audio;
using TMPro;
using UnityEngine;

namespace UI.Typewriter
{
    /// <summary>
    /// Writes text to a text field letter by letter with a typewriter effect.
    /// </summary>
    public class TMPTypewriter : MonoBehaviour
    {
        private const string FIRST_SKIP_TEXT = "Press [SPACE] to skip";
        private const string SECOND_SKIP_TEXT = "Press [SPACE] to continue";
        
        [SerializeField]
        [TextArea]
        private string _textToWrite;

        [SerializeField]
        [Tooltip("First press shows all the text, second press loads the specified scene.")]
        private KeyCode _skipKey;

        [SerializeField]
        private TMP_Text _skipText;
        
        [SerializeField]
        private TMP_Text _targetTextField;

        [SerializeField]
        private TypeWriterPauseInfo _pauseInfo;
        
        private int _index;
        private string _textFieldText = "";


        private void Start()
        {
            _targetTextField.text = "";
            _skipText.text = FIRST_SKIP_TEXT;
            
            ReproduceText();
        }


        private void Update()
        {
            if (!Input.GetKeyDown(_skipKey))
                return;
            
            if (_index < _textToWrite.Length)
            {
                _index = _textToWrite.Length;
                _targetTextField.text = _textToWrite;
                _skipText.text = SECOND_SKIP_TEXT;
            }
            else
            {
                OnComplete();
            }
        }


        protected virtual void OnComplete() { }


        private void ReproduceText()
        {
            if (_index >= _textToWrite.Length)
                return;

            //get one letter
            char letter = _textToWrite[_index];

            //Actualize on screen
            _targetTextField.text = Write(letter);
            
            // Play sound
            AudioLayer.PlaySoundOneShot(OneShotSoundType.TYPEWRITER_TYPE);

            //set to go to the next
            _index += 1;
            StartCoroutine(PauseBetweenChars(letter));
        }


        private string Write(char letter)
        {
            _textFieldText += letter;
            return _textFieldText;
        }


        private IEnumerator PauseBetweenChars(char letter)
        {
            switch (letter)
            {
                case '.':
                    yield return new WaitForSeconds(_pauseInfo.DotPause);
                    ReproduceText();
                    yield break;
                case ',':
                    yield return new WaitForSeconds(_pauseInfo.CommaPause);
                    ReproduceText();
                    yield break;
                case ' ':
                    yield return new WaitForSeconds(_pauseInfo.SpacePause);
                    ReproduceText();
                    yield break;
                default:
                    yield return new WaitForSeconds(_pauseInfo.NormalPause);
                    ReproduceText();
                    yield break;
            }
        }
    }
}