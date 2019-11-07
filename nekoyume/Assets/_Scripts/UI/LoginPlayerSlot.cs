﻿using Assets.SimpleLocalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    public class LoginPlayerSlot : MonoBehaviour
    {
        public GameObject NameView;
        public TextMeshProUGUI LabelLevel;
        public Image Icon;
        public TextMeshProUGUI LabelName;
        public GameObject CreateView;
        public TextMeshProUGUI CreateViewText;
        public GameObject DeleteView;
        public TextMeshProUGUI DeleteViewButtonText;

        private void Awake()
        {
            CreateViewText.text = LocalizationManager.Localize("UI_CREATE_CHARACTER");
            DeleteViewButtonText.text = LocalizationManager.Localize("UI_DELETE");
        }
    }
}
