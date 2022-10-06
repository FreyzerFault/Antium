using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CinematicAnt : MonoBehaviour
{
    private Ant ant;
    private Camera cam;

    public float maxCamSize = 10;

    public DialogueBox dialogueBox;

    private IEnumerator fadeInRoutine;
    private IEnumerator fadeOutRoutine;
    
    public GameObject crown;

    private void Awake()
    {
        ant = GetComponent<Ant>();
        cam = Camera.main;
        dialogueBox.gameObject.SetActive(false);
        
        crown.SetActive(AntController.win1time);
    }

    private void Start()
    {
        ant.Speed = 5;
        cam.orthographicSize = 0;

        fadeInRoutine = FadeInIntro();
        fadeOutRoutine = IntroEnd();
        
        StartCoroutine(fadeInRoutine);
        
        dialogueBox.OnEnd += () => StopCoroutine(fadeInRoutine);
        dialogueBox.OnEnd += () => StartCoroutine(fadeOutRoutine);
        dialogueBox.OnEnd += () => dialogueBox.gameObject.SetActive(false);
    }


    private IEnumerator FadeInIntro()
    {
        while (true)
        {
            ant.targetSpeed = 5;
            ant.targetRotation = Quaternion.Euler(0, 0, -90);
        
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, maxCamSize, Time.deltaTime * 0.1f);
        
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, new Vector3(0, -5, -10), Time.deltaTime * 0.1f);

            if (cam.orthographicSize > maxCamSize / 2)
            {
                dialogueBox.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    private IEnumerator IntroEnd()
    {
        while (cam.transform.localPosition.y < 20)
        {
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, cam.transform.localPosition + Vector3.up * 10, Time.deltaTime);

            yield return new WaitForSeconds(Time.deltaTime);
        }

        SceneManager.LoadScene(1);
    }
}
