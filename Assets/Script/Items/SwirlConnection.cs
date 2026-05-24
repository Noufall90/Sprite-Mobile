using System;
using UnityEngine;

/// <summary>
/// SwirlConnection — script permanen yang selalu aktif di scene.
/// Mendengarkan static event dari SwirlItem dan meneruskannya
/// ke SwirlController (parent dari shiruken) untuk diaktifkan.
///
/// Pola ini sama dengan DefenseItem → DefenseCanvas.
///
/// Setup di Unity:
/// 1. Buat GameObject kosong "SwirlConnection" (selalu aktif)
/// 2. Attach script ini ke sana
/// 3. Assign field 'swirlController' → drag SwirlPlayer dari Hierarchy
/// </summary>
public class SwirlConnection : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag SwirlPlayer (yang punya SwirlController) dari Hierarchy ke sini.")]
    [SerializeField] private SwirlController swirlController;

    private void OnEnable()
    {
        SwirlItem.OnSwirlStart += HandleSwirlStart;
    }

    private void OnDisable()
    {
        SwirlItem.OnSwirlStart -= HandleSwirlStart;
    }

    private void HandleSwirlStart(Transform player, float duration)
    {
        if (swirlController == null)
        {
            Debug.LogError("[SwirlConnection] swirlController belum di-assign di Inspector!");
            return;
        }

        swirlController.Activate(player, duration);
    }
}
