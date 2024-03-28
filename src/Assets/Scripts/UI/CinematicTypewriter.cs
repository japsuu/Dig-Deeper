namespace UI
{
    public class CinematicTypewriter : TMPTypewriter
    {
        // [SerializeField]
        // [Tooltip("The scene to load after the text has been displayed.")]
        // private int _sceneIndexToLoad = -1;
        
        
        /*private void Awake()
        {
            if (Cinematics.HasPlayerSeenCinematic())
                SceneChanger.ChangeSceneInstant(_sceneIndexToLoad);
            else
                AudioListener.volume = 0.5f;
        }*/

        protected override void OnComplete()
        {
            base.OnComplete();
            
            // Cinematics.SetPlayerHasSeenCinematic(true);
            // SceneChanger.ChangeSceneFaded(_sceneIndexToLoad);
        }
    }
}