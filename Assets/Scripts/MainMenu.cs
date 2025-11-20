using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sebbe
{
    public class MainMenu : MonoBehaviour
    {
        public void StartGame()
        {
            SceneManager.LoadScene("Level 1");
        }

        public void QuitGame()
        {
            Application.Quit();
        }
        
    }   
}