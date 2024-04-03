using Audio;
using UI;
using UnityEngine;

public static class SceneChanger
{
    public const int MAIN_MENU_SCENE_INDEX = 1;
    public const int GAMEPLAY_SCENE_INDEX = 2;


    public static void LoadMainMenuScene() => ChangeSceneFaded(MAIN_MENU_SCENE_INDEX);
    public static void LoadGameplayScene() => ChangeSceneFaded(GAMEPLAY_SCENE_INDEX);
    public static void ChangeSceneInstant(int sceneIndexToLoad) => UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndexToLoad);


    public static void ChangeSceneFaded(int sceneIndexToLoad)
    {
        AudioLayer.StopAllMusic(false);
        ScreenFader.Instance.EndScene(sceneIndexToLoad);
    }
}