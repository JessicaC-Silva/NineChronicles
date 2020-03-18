using Assets.SimpleLocalization;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI.Module
{
    public class EquipmentOptionRecipeView : EquipmentOptionView
    {
        [SerializeField]
        private TextMeshProUGUI unlockConditionText = null;

        [SerializeField]
        private RequiredItemRecipeView requiredItemRecipeView = null;

        [SerializeField]
        private Button button = null;

        [SerializeField]
        private GameObject lockParent = null;

        [SerializeField]
        private GameObject header = null;

        [SerializeField]
        private GameObject options = null;

        private EquipmentItemSubRecipeSheet.Row _rowData;

        public readonly Subject<EquipmentOptionRecipeView> OnClick = new Subject<EquipmentOptionRecipeView>();

        private bool IsLocked => lockParent.activeSelf;
        private bool NotEnoughMaterials { get; set; } = true;

        private void Awake()
        {
            button.OnClickAsObservable().Subscribe(_ =>
            {
                if (IsLocked || NotEnoughMaterials)
                {
                    return;
                }

                OnClick.OnNext(this);
            }).AddTo(gameObject);
        }

        private void OnDestroy()
        {
            OnClick.Dispose();
        }

        public void Show(string recipeName, int subRecipeId, EquipmentItemSubRecipeSheet.MaterialInfo baseMaterialInfo)
        {
            if (Game.Game.instance.TableSheets.EquipmentItemSubRecipeSheet.TryGetValue(subRecipeId, out _rowData))
            {
                requiredItemRecipeView.SetData(baseMaterialInfo, _rowData.Materials);
            }
            else
            {
                Debug.LogWarning($"SubRecipe ID not found : {subRecipeId}");
                Hide();
                return;
            }

            SetLocked(false);
            Show(recipeName, subRecipeId);
        }

        public void Set(AvatarState avatarState)
        {
            // 해금 검사.
            if (avatarState.worldInformation.TryGetLastClearedStageId(out var stageId))
            {
                if (_rowData.UnlockStage > stageId)
                {
                    SetLocked(true);
                    return;
                }

                SetLocked(false);
            }
            else
            {
                SetLocked(true);
                return;
            }

            // 재료 검사.
            var materialSheet = Game.Game.instance.TableSheets.MaterialItemSheet;
            var inventory = avatarState.inventory;
            var shouldDimmed = false;
            foreach (var info in _rowData.Materials)
            {
                if (materialSheet.TryGetValue(info.Id, out var materialRow) &&
                    inventory.TryGetFungibleItem(materialRow.ItemId, out var fungibleItem) &&
                    fungibleItem.count >= info.Count)
                {
                    continue;
                }

                shouldDimmed = true;
                break;
            }

            SetDimmed(shouldDimmed);
        }

        public void ShowLocked()
        {
            SetLocked(true);
            Show();
        }

        private void SetLocked(bool value)
        {
            lockParent.SetActive(value);
            unlockConditionText.text = value
                ? string.Format(LocalizationManager.Localize("UI_UNLOCK_CONDITION_STAGE"),
                    _rowData.UnlockStage > 50
                        ? "???"
                        : _rowData.UnlockStage.ToString())
                : string.Empty;

            header.SetActive(!value);
            options.SetActive(!value);
            requiredItemRecipeView.gameObject.SetActive(!value);
            SetPanelDimmed(value);
        }

        public override void SetDimmed(bool value)
        {
            base.SetDimmed(value);
            NotEnoughMaterials = value;
        }
    }
}
