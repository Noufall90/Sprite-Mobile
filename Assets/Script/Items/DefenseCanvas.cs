using UnityEngine;

/// <summary>
/// DefenseCanvas — attach script ini ke GameObject Canvas yang menampilkan
/// visual shield defense. Script ini stay di scene selamanya dan
/// mendengarkan static event dari DefenseItem untuk show/hide canvas.
///
/// Setup di Unity:
/// 1. Buat/pilih Canvas GameObject untuk visual defense
/// 2. Attach script ini ke GameObject tersebut
/// 3. Assign field 'canvasObject' ke Canvas/Panel yang ingin ditampilkan
///    (bisa diri sendiri atau child-nya)
/// </summary>
public class DefenseCanvas : MonoBehaviour
{
    [Header("Canvas Target")]
    [Tooltip("GameObject canvas/panel yang akan ditampilkan saat defense aktif. " +
             "Kosongkan = gunakan GameObject ini sendiri.")]
    [SerializeField] private GameObject canvasObject;

    private void Awake()
    {
        // Default: gunakan GameObject ini sendiri jika tidak diisi
        if (canvasObject == null)
            canvasObject = gameObject;

        // Pastikan canvas tersembunyi di awal
        canvasObject.SetActive(false);
    }

    private void OnEnable()
    {
        DefenseItem.OnDefenseStart += ShowCanvas;
        DefenseItem.OnDefenseEnd   += HideCanvas;
    }

    private void OnDisable()
    {
        DefenseItem.OnDefenseStart -= ShowCanvas;
        DefenseItem.OnDefenseEnd   -= HideCanvas;
    }

    private void ShowCanvas()
    {
        if (canvasObject != null)
            canvasObject.SetActive(true);
    }

    private void HideCanvas()
    {
        if (canvasObject != null)
            canvasObject.SetActive(false);
    }
}
