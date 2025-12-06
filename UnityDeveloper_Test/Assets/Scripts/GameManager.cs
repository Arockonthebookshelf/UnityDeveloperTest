using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Cube[] cubes;
    public bool hasCollectedAllCube;
    [SerializeField] private GameObject Gameover;
    [SerializeField] private GameObject GameWin;
    [SerializeField]private float fallTimer;
    [SerializeField] private float Timer = 120;
    [SerializeField] private float curfallTimer;
    [SerializeField] private bool fall;
    [SerializeField] private TMP_Text timer;
    private void OnEnable()
    {
        Player.OnLand += OnLand;
        Player.Onfall += OnFall;
    }
    private void OnDisable()
    {
        Player.OnLand -= OnLand;
        Player.Onfall -= OnFall;
    }
    private void OnLevelWasLoaded(int level)
    {
        Time.timeScale = 1;
    }
    void Start()
    {
        if(cubes.Length == 0)
        {
            Debug.Log("Cube cannot be zero");
        }
    }

    private void Update()
    {
        if (Timer > 0)
        {
            Timer -= Time.deltaTime;
            timer.text = ((int)Timer).ToString();
        }
        else
        {
            Gameover.SetActive(true);
        }
        if (fall)
        {
            curfallTimer -= Time.deltaTime;
        }
        else
        {
            curfallTimer = fallTimer;
        }
        if(curfallTimer < 0)
        {
            Gameover.SetActive(true) ;
            PauseGame();
        }
    }
    void FixedUpdate()
    {
        hasCollectedAllCube = true;
        foreach (Cube cube in cubes)
        {
            hasCollectedAllCube = hasCollectedAllCube && cube.collected;
        }
        if(hasCollectedAllCube)
        {
            GameWin.SetActive(true);
            PauseGame() ;
        }
    }
    private void PauseGame()
    {
        Time.timeScale = 0;
    }
    public void RestartLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void Exit()
    {
        Application.Quit();
    }
    private void OnFall()
    {
        fall = true;
        Debug.Log("fall");
    }
    private void OnLand()
    {
        fall = false;
        Debug.Log("land");
    }
}
