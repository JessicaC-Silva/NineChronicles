﻿using System;
using System.Collections.Generic;
using Nekoyume.UI.Scroller;
using Nekoyume.Model.Item;
using UniRx;
using Nekoyume.State;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI.Module
{
    public class EquipmentRecipe : MonoBehaviour
    {
        public EquipmentRecipeCellView cellViewPrefab;
        public EquipmentRecipeCellView[] cellViews;
        public TabButton weaponTabButton;
        public TabButton armorTabButton;
        public TabButton beltTabButton;
        public TabButton necklaceTabButton;
        public TabButton ringTabButton;
        public Transform cellViewParent;
        public ScrollRect scrollRect;
        public EquipmentRecipeCellView selectedRecipe;

        private readonly ToggleGroup _toggleGroup = new ToggleGroup();

        private readonly ReactiveProperty<ItemSubType> _filterType =
            new ReactiveProperty<ItemSubType>(ItemSubType.Weapon);

        private readonly List<IDisposable> _disposablesAtLoadRecipeList = new List<IDisposable>();

        private void Awake()
        {
            _toggleGroup.OnToggledOn.Subscribe(SubscribeOnToggledOn).AddTo(gameObject);
            _toggleGroup.RegisterToggleable(weaponTabButton);
            _toggleGroup.RegisterToggleable(armorTabButton);
            _toggleGroup.RegisterToggleable(beltTabButton);
            _toggleGroup.RegisterToggleable(necklaceTabButton);
            _toggleGroup.RegisterToggleable(ringTabButton);

            LoadRecipes();
            _filterType.Subscribe(SubScribeFilterType).AddTo(gameObject);
        }

        private void OnEnable()
        {
            if (States.Instance.CurrentAvatarState is null)
                return;

            UpdateRecipes();
        }

        private void OnDestroy()
        {
            _filterType.Dispose();
            _disposablesAtLoadRecipeList.DisposeAllAndClear();
        }

        private void LoadRecipes()
        {
            _disposablesAtLoadRecipeList.DisposeAllAndClear();

            var recipeSheet = Game.Game.instance.TableSheets.EquipmentItemRecipeSheet;
            var totalCount = recipeSheet.Count;
            cellViews = new EquipmentRecipeCellView[totalCount];

            var idx = 0;
            foreach (var recipeRow in recipeSheet)
            {
                var cellView = Instantiate(cellViewPrefab, cellViewParent);
                cellView.Set(recipeRow);
                cellView.OnClick.Subscribe(SubscribeOnClickCellView).AddTo(_disposablesAtLoadRecipeList);
                cellViews[idx] = cellView;
                ++idx;
            }
            
            UpdateRecipes();
        }

        public void UpdateRecipes()
        {
            var avatarState = States.Instance.CurrentAvatarState;
            if (avatarState is null)
                return;
            
            foreach (var cellView in cellViews)
            {
                cellView.Set(avatarState);
            }
        }

        private void SubScribeFilterType(ItemSubType itemSubType)
        {
            scrollRect.normalizedPosition = new Vector2(0.5f, 1.0f);

            // FIXME : 테이블이 완성된 후 대응시켜야 함.
            foreach (var cellView in cellViews)
            {
                if (cellView.itemSubType == itemSubType)
                {
                    cellView.Show();
                }
                else
                {
                    cellView.Hide();
                }
            }

            switch (itemSubType)
            {
                case ItemSubType.Weapon:
                    _toggleGroup.SetToggledOn(weaponTabButton);
                    break;
                case ItemSubType.Armor:
                    _toggleGroup.SetToggledOn(armorTabButton);
                    break;
                case ItemSubType.Belt:
                    _toggleGroup.SetToggledOn(beltTabButton);
                    break;
                case ItemSubType.Necklace:
                    _toggleGroup.SetToggledOn(necklaceTabButton);
                    break;
                case ItemSubType.Ring:
                    _toggleGroup.SetToggledOn(ringTabButton);
                    break;
            }
        }

        private void SubscribeOnToggledOn(IToggleable toggleable)
        {
            if (toggleable.Name.Equals(weaponTabButton.Name))
            {
                _filterType.SetValueAndForceNotify(ItemSubType.Weapon);
            }
            else if (toggleable.Name.Equals(armorTabButton.Name))
            {
                _filterType.SetValueAndForceNotify(ItemSubType.Armor);
            }
            else if (toggleable.Name.Equals(beltTabButton.Name))
            {
                _filterType.SetValueAndForceNotify(ItemSubType.Belt);
            }
            else if (toggleable.Name.Equals(necklaceTabButton.Name))
            {
                _filterType.SetValueAndForceNotify(ItemSubType.Necklace);
            }
            else if (toggleable.Name.Equals(ringTabButton.Name))
            {
                _filterType.SetValueAndForceNotify(ItemSubType.Ring);
            }
        }

        private void SubscribeOnClickCellView(EquipmentRecipeCellView cellView)
        {
            selectedRecipe = cellView;
            Widget.Find<Combination>().State.SetValueAndForceNotify(Combination.StateType.CombinationConfirm);
        }
    }
}
