using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUIController : Singleton<GameUIController>
{
    [SerializeField] private Image thumbnailImage;
    [Header("Dial Properties")]
    [SerializeField] private Image dialBackdropImage;
    [SerializeField] private GameObject dialEntryPrefab;
    [SerializeField] private float demoSpinSpeed = 180f;
    [SerializeField] private float spinDuration = 4f;
    [SerializeField] private AnimationCurve spinCurve;
    [Header("Text Fields")]
    [SerializeField] private TMP_Text titleContent;
    [SerializeField] private TMP_Text genreContent;
    [SerializeField] private TMP_Text playerContent;
    [SerializeField] private TMP_Text devsContent;
    [SerializeField] private TMP_Text descriptionContent;

    private List<float> entryAngles;
    private List<Sprite> thumbnails;

    private float radStep;
    private int randomGameIndex;

    private void Start()
    {
        StartDemoSpin();
    }

    public void SetTitles(string[] titles)
    {
        radStep = 2f * Mathf.PI / titles.Length;
        float distance = 24f * titles.Length;

        Vector2 sizeDelta = dialBackdropImage.rectTransform.sizeDelta;
        sizeDelta.y = 55f * titles.Length;
        dialBackdropImage.rectTransform.sizeDelta = sizeDelta;

        entryAngles = new();

        for (int i = 0; i < titles.Length; i++)
        {
            GameObject dialEntry = Instantiate(dialEntryPrefab, transform);

            Vector3 position = new Vector3(0, Mathf.Sin(radStep * i), -Mathf.Cos(radStep * i)) * distance;
            Quaternion rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * radStep * i, Vector3.right);
            dialEntry.transform.SetLocalPositionAndRotation(position, rotation);

            dialEntry.GetComponentInChildren<TMP_Text>().text = titles[i];

            float angle = NormalizeAngle(Mathf.Rad2Deg * radStep * i);

            entryAngles.Add(angle);
        }
    }

    public void SetThumbnails(Sprite[] sprites)
    {
        thumbnails = sprites.ToList();
        thumbnailImage.sprite = thumbnails[0];
    }

    [ContextMenu("Demo Spin")]
    public void StartDemoSpin()
    {
        StartCoroutine(DemoSpin());
    }

    private IEnumerator DemoSpin()
    {
        transform.Rotate(Vector3.right, demoSpinSpeed * Time.deltaTime);
        yield return null;
        StartCoroutine(DemoSpin());
    }

    public void OnSpin(InputAction.CallbackContext context)
    {
        if (context.started && !LaunchManager.Instance.Playing)
        {
            StopAllCoroutines();
            SpinTheDial();
        }
    }

    private void SpinTheDial()
    {
        LaunchManager.Instance.Playing = true;
        float randomExtraRotation = Random.Range(5, 10) * 360f;

        randomGameIndex = Random.Range(0, entryAngles.Count);
        float randomEndAngle = -radStep * randomGameIndex * Mathf.Rad2Deg;
        float targetAngle = randomExtraRotation + randomEndAngle;

        ResetGameInfo();
        StartCoroutine(SpinToTargetAngle(targetAngle, randomEndAngle));
    }

    private IEnumerator SpinToTargetAngle(float targetAngle, float endAngle)
    {
        float startAngle = transform.rotation.eulerAngles.x;

        float elapsedTime = 0f;
        int index = 0;

        while (elapsedTime < spinDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = spinCurve.Evaluate(elapsedTime / spinDuration);
            float currentAngle = Mathf.Lerp(startAngle, targetAngle, t);

            float closestAngle = entryAngles
                .Select(a => NormalizeAngle(a)) // Normalize the array angles
                .OrderBy(a => Mathf.Abs(NormalizeAngle(-currentAngle) - a)) // Find the closest
                .First();

            index = entryAngles.IndexOf(closestAngle);
            thumbnailImage.sprite = thumbnails[index];

            transform.localEulerAngles = new Vector3(currentAngle, 0, 0);

            yield return null;
        }

        transform.localEulerAngles = new Vector3(endAngle, 0, 0);
        thumbnailImage.sprite = thumbnails[index];
        SetGameInfo();

        yield return new WaitForSeconds(LaunchManager.Instance.DelayBeforeStart);

        LaunchManager.Instance.LaunchGame(FolderScanner.Instance.GameExeFiles[randomGameIndex].FullName);
    }

    private float NormalizeAngle(float angle)
    {
        return ((angle + 180) % 360 + 360) % 360 - 180;
    }

    private void SetGameInfo()
    {
        GameData[] array = FolderScanner.Instance.DataArray;

        titleContent.text = array[randomGameIndex].title;
        genreContent.text = array[randomGameIndex].genre;
        playerContent.text = array[randomGameIndex].players.ToString();
        devsContent.text = string.Join(", ", array[randomGameIndex].team);
        descriptionContent.text = array[randomGameIndex].desc;
    }

    private void ResetGameInfo()
    {
        titleContent.text = string.Empty;
        genreContent.text = string.Empty;
        playerContent.text = string.Empty;
        devsContent.text = string.Empty;
        descriptionContent.text = string.Empty;
    }
}